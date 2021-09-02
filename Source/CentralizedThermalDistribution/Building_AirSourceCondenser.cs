using System.Linq;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_AirSourceCondenser : Building_TempControl // Requires temp control!
    {
        public enum Status
        {
            Offline,
            AirBlocked,
            EfficiencyLow,
            TemperatureReached,
            Working,
        }

        //public CompPowerTrader compPowerTrader;
        public CompCoolantConditioner compCoolantConditioner;

        private const float WasteHeatMultiplier = 0.15f; // This percentage of work done is additionally output as waste heat, regardless of operation mode.
        private const float MinTotalEfficiency = 0.20f; // If operational efficiency drops below this percent, unit will go idle. (Eventually to be set in-game)
        private float MaxTemperatureDelta; // Positive degrees Celcius. Efficiency decreases, up to this limit, as the coolant is heated/chilled beyond ambient temp.
        private float ThermalWorkMultiplier; // Positive if heating coolant, negative if cooling. Heat output to both surroundings and and coolant is multiplied by this. 

        public Status status = Status.Offline;
        public bool IsAirBlocked = false;
        public bool IsEfficiencyLow = false;
        public bool IsTemperatureReached = false;

        public float AirFlowEfficiency = 0f;
        public float ThermalEfficiency = 0f;
        public float TotalEfficiency = 0f;

        /// <summary>
        ///     Building spawned on the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = GetComp<CompPowerTrader>();
            compCoolantConditioner = GetComp<CompCoolantConditioner>();
            compCoolantConditioner.Props.flowType = CoolantPipeColor.Any;

            ThermalWorkMultiplier = compCoolantConditioner.Props.ThermalWorkMultiplier;
            MaxTemperatureDelta = compCoolantConditioner.Props.ConditionerMaxTemperatureDelta;

            //if (!respawningAfterLoad)
            {
                compCoolantConditioner.CoolantTemperature = Position.GetTemperature(Map);
            }
            compCoolantConditioner.CoolantThermalMass = 1f; //TEMP
        }

        public override string GetInspectString()
        {
            string output = base.GetInspectString();
            output += "\nStatus: " + (IsAirBlocked ? "Air_Blocked, " : "Air_OK, ") + (IsEfficiencyLow ? "Efficiency_Low, " : "Efficiency_OK, ") + (IsTemperatureReached ? "Temp_Reached" : "Temp_OK");
            output += "\nAirFlowEfficiency: " + (int)(AirFlowEfficiency * 100) + "%";
            output += "\nThermalEfficiency: " + ThermalEfficiency;
            output += "\nCurrentEnergyDelta: " + compCoolantConditioner.CurrentEnergyDelta;
            output += "\nCoolantTemperature: " + compCoolantConditioner.CoolantTemperature;
            return output;
        }

        /// <summary>
        ///     Tick condenser unit. Check the surrondings for blockage. Calculate total efficiency and resulting output.
        /// </summary>
        public override void TickRare()
        {
            if (compPowerTrader.PowerOn)
            {
                int SignOfMode = System.Math.Sign(ThermalWorkMultiplier); // Positive if heating coolant, negative if cooling.

                // === Temperature setting check ===
                IsTemperatureReached = ((SignOfMode*compCoolantConditioner.CoolantTemperature) >= (SignOfMode*compTempControl.targetTemperature)); // True if coolant temp is at or beyond desired temp.

                // === Blockage check ===
                // Count number of blocked adjacent cells
                var adjList = GenAdj.CellsAdjacentCardinal(Position, Rotation, def.Size).ToList();
                int unblockedAdjacent = 0;
                foreach (var intVec in adjList)
                {
                    if (intVec.Impassable(Map))
                    {
                        continue;
                    }
                    unblockedAdjacent++;
                }
                // Air flow efficiency is ratio of adjacent open cells
                AirFlowEfficiency = (float)unblockedAdjacent / adjList.Count;
                IsAirBlocked = (AirFlowEfficiency == 0);  // True if all adjacent cells are blocked

                // === Thermal efficiency check ===
                // Temperature differential efficincy is parabolic
                // 0% efficiency when coolant temp is at or beyond MaxTemperatureDifference from ambient temp
                // 100% efficiency when coolant temp matches ambient temp
                // Bonus efficiency if ambient temp is closer to desired temp than coolant is (for example, 400% if delta is MaxTemperatureDifference in the opposite direction)
                // https://www.desmos.com/calculator/5uosg4hpca
                var tempDelta = SignOfMode * (Position.GetTemperature(Map) - compCoolantConditioner.CoolantTemperature);
                if (tempDelta > -MaxTemperatureDelta)
                {
                    // ThermalEfficiency = (tempDelta + MaxTemperatureDifference)^2 / MaxTemperatureDifference^2
                    ThermalEfficiency = ((tempDelta + MaxTemperatureDelta) * (tempDelta + MaxTemperatureDelta)) / (MaxTemperatureDelta * MaxTemperatureDelta); // Optimized
                }
                else
                {
                    ThermalEfficiency = 0f;
                }
                TotalEfficiency = AirFlowEfficiency * ThermalEfficiency;
                IsEfficiencyLow = (TotalEfficiency < MinTotalEfficiency); // True if total efficiency is below the efficiency limit

                compCoolantConditioner.ActiveOnNetwork = !IsAirBlocked; // Of the three idle cases, AirBlocked is the only one that should cause it to deactivate from the coolant network.

                if (IsAirBlocked || IsEfficiencyLow || IsTemperatureReached)
                {
                    // === Condenser is online and idle ===
                    compTempControl.operatingAtHighPower = false;
                    compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption * compTempControl.Props.lowPowerConsumptionFactor;
                    compCoolantConditioner.CurrentEnergyDelta = 0;

                    // This determines the priority of status in case muiltiple idle cases are active:
                    // AirBlocked > EfficiencyLow > TemperatureReached
                    // This code could be optimized to be branchless.
                    if (IsAirBlocked) { status = Status.AirBlocked; }
                    else if (IsEfficiencyLow) { status = Status.EfficiencyLow; }
                    else { status = Status.TemperatureReached; }
                }
                else
                {
                    // === Condenser is online and working ===
                    status = Status.Working;
                    compTempControl.operatingAtHighPower = true;
                    compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption; // Power consumption ignores current operating efficiency.
                    var ThermalWork = TotalEfficiency * ThermalWorkMultiplier;
                    compCoolantConditioner.CurrentEnergyDelta = ThermalWork; // Update coolant thermal energy change rate

                    compCoolantConditioner.TickRare();
                    GenTemperature.PushHeat(Position, base.Map, ThermalWork + (System.Math.Abs(ThermalWork) * WasteHeatMultiplier)); // Push exhaust
                }
            }
            else
            {
                // === Condenser is offline ===
                status = Status.Offline;
                compCoolantConditioner.ActiveOnNetwork = false;
                //compTempControl.operatingAtHighPower = false; // Vanilla coolers and heaters simply leave these values as-is.
                //compPowerTrader.PowerOutput = 0;
                compCoolantConditioner.CurrentEnergyDelta = 0;

            }
        }
    }
}