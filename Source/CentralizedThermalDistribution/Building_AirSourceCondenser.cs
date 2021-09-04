using System.Linq;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_AirSourceCondenser : Building_TempControl
    {
        public enum Status
        {
            Offline,
            AirBlocked,
            EfficiencyLow,
            TemperatureReached,
            Working,
        }

        public CompCoolantProvider compCoolant;

        private const float AirSourceCondenserMultiplier = 200.0f;
        private const float WasteHeatMultiplier = 0.15f; // This percentage of work done is additionally output as waste heat, regardless of operation mode.
        private const float MinEfficiencyCutoff = 0.25f; // If operational efficiency drops below this percent, unit will go idle. (Eventually to be set in-game)
        private float MaxTemperatureDelta; // Positive degrees Celcius. Efficiency decreases, up to this limit, as the coolant is heated/chilled beyond ambient temp.
        private float ThermalWorkMultiplier; // Positive if heating coolant, negative if cooling. Heat output to both surroundings and and coolant is multiplied by this. 

        public Status status = Status.Offline;
        public bool IsAirBlocked = false;
        public bool IsEfficiencyLow = false;
        public bool IsTemperatureReached = false;

        public float AirFlowEfficiency = 0f;
        public float ThermalEfficiency = 0f;
        public float TotalEfficiency = 0f;
        public float ThermalWork = 0f;

        /// <summary>
        ///     Building spawned on the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = GetComp<CompPowerTrader>();
            compCoolant = GetComp<CompCoolantProvider>();
            compCoolant.Props.flowType = CoolantPipeColor.Any;

            ThermalWorkMultiplier = compCoolant.Props.ThermalWorkMultiplier;
            MaxTemperatureDelta = compCoolant.Props.ProviderMaxTemperatureDelta * 2; // See comments for "Thermal efficiency check"

            compCoolant.CoolantTemperature = Position.GetTemperature(Map);

            compCoolant.CoolantThermalMass = 100; //TEMP
        }

        /// <summary>
        ///     Get the Gizmos for AirVent
        ///     Here, we generate the Gizmo for Chaning Pipe Priority
        /// </summary>
        /// <returns>List of Gizmos</returns>
        public override System.Collections.Generic.IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (compCoolant != null)
            {
                yield return CentralizedThermalDistributionUtility.GetPipeSwitchToggle(compCoolant);
            }
        }

        public override string GetInspectString()
        {
            string output = base.GetInspectString();
            output += "\nThermalWork: " + ThermalWork;
            output += "\nStatus: " + (IsAirBlocked ? "Air_Blocked, " : "Air_OK, ") + (IsEfficiencyLow ? "Efficiency_Low, " : "Efficiency_OK, ") + (IsTemperatureReached ? "Temp_Reached" : "Temp_OK");
            output += "\nAirFlowEfficiency: " + (int)(AirFlowEfficiency * 100) + "%";
            output += "\nTotalEfficiency: " + TotalEfficiency;
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
                IsTemperatureReached = ((SignOfMode*compCoolant.CoolantTemperature) >= (SignOfMode*compTempControl.targetTemperature)); // True if coolant temp is at or beyond desired temp.

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
                // The parabolic efficiency curve is deceptively agressive. We double the value of MaxTemperatureDifference (under SpawnSetup), so in-game the result is accurate with efficiency cutoff at 25%.
                // https://www.desmos.com/calculator/5uosg4hpca
                var tempDelta = SignOfMode * (Position.GetTemperature(Map) - compCoolant.CoolantTemperature);
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
                IsEfficiencyLow = (TotalEfficiency < MinEfficiencyCutoff); // True if total efficiency is below the efficiency limit

                compCoolant.ActiveOnNetwork = !IsAirBlocked; // Of the three idle cases, AirBlocked is the only one that should cause it to deactivate from the coolant network.

                if (IsAirBlocked || IsEfficiencyLow || IsTemperatureReached)
                {
                    // === Condenser is online and idle ===
                    compTempControl.operatingAtHighPower = false;
                    compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption * compTempControl.Props.lowPowerConsumptionFactor;
                    ThermalWork = 0;

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
                    ThermalWork = TotalEfficiency * AirSourceCondenserMultiplier * ThermalWorkMultiplier;

                    compCoolant.PushThermalLoad(ThermalWork); // Push to coolant
                    GenTemperature.PushHeat(Position, base.Map, (System.Math.Abs(ThermalWork) * WasteHeatMultiplier) - ThermalWork); // Push exhaust (negative ThermalWork)
                }
            }
            else
            {
                // === Condenser is offline ===
                status = Status.Offline;
                compCoolant.ActiveOnNetwork = false;
                //compTempControl.operatingAtHighPower = false; // Vanilla coolers and heaters simply leave these values as-is.
                //compPowerTrader.PowerOutput = 0;
                ThermalWork = 0;
            }
        }
    }
}