using System.Linq;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_AirSourceCondenser : Building_CoolantProvider
    {
        public enum MinEfficiencySelection
        {
            E10 = 10, // 10%
            E25 = 25, // 25%
            E50 = 50, // 50%
            E80 = 80, // 80%
        }

        private const float AirSourceCondenserMultiplier = 40.0f; // Multipler unique to this provider type
        private const float WasteHeatMultiplier = 0.15f; // This percentage of work done is additionally output as waste heat, regardless of operation mode.
        private float MaxTemperatureDelta; // Positive degrees Celcius. Efficiency decreases, up to this limit, as the coolant is heated/chilled beyond ambient temp.
        public MinEfficiencySelection minEfficiencySelection = MinEfficiencySelection.E25; // If operational efficiency drops below this percent, unit will go idle.

        public bool IsAirBlocked = false;
        public bool IsEfficiencyLow = false;
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
            MaxTemperatureDelta = compCoolant.Props.ProviderMaxTemperatureDelta * 2; // For why *2, see comments for "Thermal efficiency check"
            AddGizmo(() => CentralizedThermalDistributionUtility.GetMinEfficiencyToggle(this));
        }

        /// <summary>
        ///     Method called during Game Save/Load
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref minEfficiencySelection, "minEfficiencySelection", MinEfficiencySelection.E25);
        }

        public override string GetInspectString()
        {
            System.Text.StringBuilder output = new();
            output.AppendLine(base.GetInspectString());
            output.AppendLine("DEBUG AirFlowEfficiency: " + (int)(AirFlowEfficiency * 100) + "%");
            output.AppendLine("DEBUG TotalEfficiency: " + TotalEfficiency);
            return output.ToString().Trim();
        }

        public override void CheckStatus()
        {
            base.CheckStatus(); //Temperature reached check

            // === Air blockage check ===
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
            var tempDelta = System.Math.Sign(ThermalWorkMultiplier) * (Position.GetTemperature(Map) - compCoolant.CoolantTemperature);
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
            IsEfficiencyLow = (TotalEfficiency * 100) < (int)minEfficiencySelection; // True if total efficiency is below the efficiency limit

            // This determines the priority of status in case muiltiple idle cases are active:
            // AirBlocked > EfficiencyLow > TemperatureReached
            if (IsAirBlocked) { SetStatus(Status.Offline, "AirBlocked"); }
            else if (IsEfficiencyLow) { SetStatus(Status.Idle, "EfficiencyLow"); }
            else if (IsTemperatureReached) { SetStatus(Status.Idle, "TemperatureReached"); }
            else { SetStatus(Status.Working); }
        }

        public override void DoThermalWork(int tickMultiplier)
        {
            ThermalWork = TotalEfficiency * AirSourceCondenserMultiplier * ThermalWorkMultiplier * tickMultiplier;
            compCoolant.PushThermalLoad(ThermalWork); // Push to coolant
            GenTemperature.PushHeat(Position, base.Map, (System.Math.Abs(ThermalWork) * WasteHeatMultiplier) - ThermalWork); // Push exhaust (negative ThermalWork)
        }
    }
}