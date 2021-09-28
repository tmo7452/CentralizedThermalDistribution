using System.Linq;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompCoolantProviderAirSourceCondenser : CompCoolantProvider
    {
        public enum MinEfficiencySelection
        {
            E10 = 10, // 10%
            E25 = 25, // 25%
            E50 = 50, // 50%
            E80 = 80, // 80%
        }
        private const MinEfficiencySelection DefaultMinEfficiencySelection = MinEfficiencySelection.E25;

        private const float AirSourceCondenserMultiplier = 40.0f; // Multipler unique to this provider type
        private const float WasteHeatMultiplier = 0.15f; // This percentage of work done is additionally output as waste heat, regardless of operation mode.
        private float MaxTemperatureDelta; // Positive degrees Celcius. Efficiency decreases, up to this limit, as the coolant is heated/chilled beyond ambient temp.
        public MinEfficiencySelection minEfficiencySelection = DefaultMinEfficiencySelection; // If operational efficiency drops below this percent, unit will go idle.

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
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            MaxTemperatureDelta = Props.traderMaxTemperatureDelta * 2; // For why *2, see comments for "Thermal efficiency check"
            AddGizmo(() => GetMinEfficiencyToggle(this));
        }

        /// <summary>
        ///     Method called during Game Save/Load
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref minEfficiencySelection, "minEfficiencySelection", MinEfficiencySelection.E25);
        }

        public override string CompInspectStringExtra()
        {
            System.Text.StringBuilder output = new();
            output.AppendLine(base.CompInspectStringExtra());
            output.AppendLine("DEBUG AirFlowEfficiency: " + (int)(AirFlowEfficiency * 100) + "%");
            output.AppendLine("DEBUG TotalEfficiency: " + TotalEfficiency);
            return output.ToString().Trim();
        }

        public override void CheckStatus()
        {
            base.CheckStatus(); //Temperature reached check

            // === Air blockage check ===
            // Count number of blocked output cells
            var outputCells = OutputLocations;
            int openOutputCells = 0;
            foreach (var intVec in outputCells)
            {
                if (intVec.Impassable(parent.Map))
                {
                    continue;
                }
                openOutputCells++;
            }
            // Air flow efficiency is ratio of blocked to open output cells
            AirFlowEfficiency = (float)openOutputCells / outputCells.Count;
            IsAirBlocked = (AirFlowEfficiency == 0);  // True if all adjacent cells are blocked

            // === Thermal efficiency check ===
            // Temperature differential efficincy is parabolic
            // 0% efficiency when coolant temp is at or beyond MaxTemperatureDifference from ambient temp
            // 100% efficiency when coolant temp matches ambient temp
            // Bonus efficiency if ambient temp is closer to desired temp than coolant is (for example, 400% if delta is MaxTemperatureDifference in the opposite direction)
            // The parabolic efficiency curve is deceptively agressive. We double the value of MaxTemperatureDifference (under SpawnSetup), so in-game the result is accurate with efficiency cutoff at 25%.
            // https://www.desmos.com/calculator/5uosg4hpca
            var tempDelta = System.Math.Sign(ThermalWorkMultiplier) * (parent.Position.GetTemperature(parent.Map) - CoolantTemperature);
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
            ThermalWork = 0;
            float desiredWork = TotalEfficiency * AirSourceCondenserMultiplier * ThermalWorkMultiplier * tickMultiplier;
            // Push exhaust (negative ThermalWork) split between unique output cells
            var cells = UniqueOutputLocations; // Call once, because it's not a quick lookup
            var heatPerCell = ((System.Math.Abs(desiredWork) * WasteHeatMultiplier) - desiredWork) / cells.Count; // Includes waste heat
            foreach (var cell in cells)
            {
                if (cell.GetRoom(parent.Map) is null)
                    continue;
                GenTemperature.PushHeat(cell, parent.Map, heatPerCell);
                ThermalWork += desiredWork / cells.Count;
            }
            // Push to coolant
            PushThermalLoad(ThermalWork);
        }

        /// <summary>
        ///     Gizmo for changing condenser minimum efficiency setting.
        /// </summary>
        /// <param name="compCoolant">Component Asking for Gizmo</param>
        /// <returns>Action Button Gizmo</returns>
        public static Command_Action GetMinEfficiencyToggle(CompCoolantProviderAirSourceCondenser compCondenser)
        {
            UnityEngine.Texture2D icon;
            string label;

            switch (compCondenser.minEfficiencySelection)
            {

                case MinEfficiencySelection.E10:
                    //label = SwitchPipeRedKey.Translate();
                    label = "Efficiency Cutoff: 10%";
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/MinEfficiencySelect_E10");
                    break;

                case MinEfficiencySelection.E25:
                default:
                    //label = SwitchPipeBlueKey.Translate();
                    label = "Efficiency Cutoff: 25%";
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/MinEfficiencySelect_E25");
                    break;

                case MinEfficiencySelection.E50:
                    //label = SwitchPipeCyanKey.Translate();
                    label = "Efficiency Cutoff: 50%";
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/MinEfficiencySelect_E50");
                    break;

                case MinEfficiencySelection.E80:
                    //label = SwitchPipeAutoKey.Translate();
                    label = "Efficiency Cutoff: 80%";
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/MinEfficiencySelect_E80");
                    break;
            }

            return new Command_Action
            {
                defaultLabel = label,
                defaultDesc = "CentralizedThermalDistribution.Command.SwitchPipe.Desc".Translate(),
                //hotKey = KeyBindingDefOf.Misc4,
                icon = icon,
                action = delegate
                {
                    compCondenser.minEfficiencySelection = compCondenser.minEfficiencySelection switch
                    {
                        MinEfficiencySelection.E10 => MinEfficiencySelection.E25,
                        MinEfficiencySelection.E25 => MinEfficiencySelection.E50,
                        MinEfficiencySelection.E50 => MinEfficiencySelection.E80,
                        MinEfficiencySelection.E80 => MinEfficiencySelection.E10,
                        _ => DefaultMinEfficiencySelection
                    };
                }
            };
        }
    }
}