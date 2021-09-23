using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public abstract class CompCoolantTrader : CompCoolant
    {
        private const string SwitchPipeAutoKey = "CentralizedThermalDistribution.Command.SwitchPipe.Auto";
        private const string SwitchPipeRedKey = "CentralizedThermalDistribution.Command.SwitchPipe.Red";
        private const string SwitchPipeBlueKey = "CentralizedThermalDistribution.Command.SwitchPipe.Blue";
        private const string SwitchPipeCyanKey = "CentralizedThermalDistribution.Command.SwitchPipe.Cyan";
        private const string SwitchPipeGreenKey = "CentralizedThermalDistribution.Command.SwitchPipe.Green";
        public enum PipeColorSelection
        {
            Red = 0,
            Blue = 1,
            Cyan = 2,
            Green = 4,
            Auto = 3,
        }
        public const PipeColorSelection DefaultPipeColorSelection = PipeColorSelection.Auto;

        public static bool PipeColorMatchesSelection(PipeColor color, PipeColorSelection selection)
        {
            return color switch
            {
                PipeColor.Red => (selection == PipeColorSelection.Red) || (selection == PipeColorSelection.Auto),
                PipeColor.Blue => (selection == PipeColorSelection.Blue) || (selection == PipeColorSelection.Auto),
                PipeColor.Cyan => (selection == PipeColorSelection.Cyan) || (selection == PipeColorSelection.Auto),
                PipeColor.Green => (selection == PipeColorSelection.Green) || (selection == PipeColorSelection.Auto),
                _ => false
            };
        }

        private PipeColorSelection _pipeColorSelection = DefaultPipeColorSelection;
        public PipeColorSelection pipeColorSelection
        {
            get => _pipeColorSelection;
            set
            {
                _pipeColorSelection = value;
                UpdateAttachedNet();
            }
        }

        private System.Collections.Generic.List<CoolantNet> AvailableNets = new();
        private System.Collections.Generic.List<System.Func<Gizmo>> Gizmos = new(); // A list of lambda functions to be called during gizmo checks.

        public float ThermalWork { get; protected set; } = 0f;
        protected float ThermalWorkMultiplier; // Multiplier unique to the building type, set by the Def. Positive if heating coolant, negative if cooling.

        public void AddGizmo(System.Func<Gizmo> gizmo)
        {
            Gizmos.Add(gizmo);
        }

        public override System.Collections.Generic.IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            foreach (var gizmo in Gizmos)
                yield return gizmo();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Props.pipeColor = PipeColor.Trader;
            ThermalWorkMultiplier = Props.ThermalWorkMultiplier;
            AddGizmo(() => GetPipeSwitchToggle(this));
            RescanNets();
            pipeColorSelection = DefaultPipeColorSelection;
        }

        /// <summary>
        ///     Method called during Game Save/Load
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            var ExposedPipeColorSelection = pipeColorSelection;
            Scribe_Values.Look(ref ExposedPipeColorSelection, "pipeColorSelection", DefaultPipeColorSelection);
            if (ExposedPipeColorSelection != pipeColorSelection) pipeColorSelection = ExposedPipeColorSelection;
        }

        public override string CompInspectStringExtra()
        {
            System.Text.StringBuilder output = new();
            output.AppendLine(base.CompInspectStringExtra());
            output.AppendLine("DEBUG ThermalWork: " + ThermalWork);
            return output.ToString().Trim();
        }

        public override void SetNet(CoolantNet newNet)
        {
            if (coolantNet != null)
                coolantNet.Traders.Remove(this);
            base.SetNet(newNet);
            SetPipeColor(newNet == null ? PipeColor.None : newNet.pipeColor);
            if (coolantNet != null)
                coolantNet.Traders.Add(this);
        }

        public abstract void PushThermalLoad(float ThermalLoad);

        public void RescanNets()
        {

            AvailableNets = new();

            // For each cell we occupy, scan for any pipes and add their networks.
            foreach (var pos in parent.OccupiedRect().Cells)
            {
                for (int pipeColorIndex = 0; pipeColorIndex < PipeColorCount; pipeColorIndex++)
                {
                    if (coolantNetManager.IsPipeAt(pos, pipeColorIndex))
                    {
                        CoolantNet net = coolantNetManager.GetPipeAt(pos, pipeColorIndex).coolantNet;
                        if (AvailableNets.Contains(net))
                            continue;
                        AvailableNets.Add(net);
                    }
                }
            }
            UpdateAttachedNet();
        }

        private void UpdateAttachedNet()
        {
            SetNet(null);
            foreach (var net in AvailableNets)
            {
                if (PipeColorMatchesSelection(net.pipeColor, pipeColorSelection))
                {
                    SetNet(net);
                    break;
                }
            }
        }

        public void RemoveNet(CoolantNet net)
        {
            if (AvailableNets.Contains(net))
                AvailableNets.Remove(net);
        }

        /// <summary>
        ///     Gizmo for Changing Pipes
        /// </summary>
        /// <param name="compCoolant">Component Asking for Gizmo</param>
        /// <returns>Action Button Gizmo</returns>
        public static Command_Action GetPipeSwitchToggle(CompCoolantTrader compCoolant)
        {
            var currentSelection = compCoolant.pipeColorSelection;
            Texture2D icon;
            string label;

            switch (currentSelection)
            {

                case PipeColorSelection.Red:
                    label = SwitchPipeRedKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Red");
                    break;

                case PipeColorSelection.Blue:
                    label = SwitchPipeBlueKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Blue");
                    break;

                case PipeColorSelection.Cyan:
                    label = SwitchPipeCyanKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Cyan");
                    break;

                case PipeColorSelection.Green:
                    label = SwitchPipeGreenKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Green");
                    break;

                case PipeColorSelection.Auto:
                default:
                    label = SwitchPipeAutoKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Auto");
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
                    compCoolant.pipeColorSelection = currentSelection switch
                    {
                        PipeColorSelection.Auto => PipeColorSelection.Red,
                        PipeColorSelection.Red => PipeColorSelection.Blue,
                        PipeColorSelection.Blue => PipeColorSelection.Cyan,
                        PipeColorSelection.Cyan => PipeColorSelection.Green,
                        PipeColorSelection.Green => PipeColorSelection.Auto,
                        _ => DefaultPipeColorSelection
                    };
                }
            };
        }
    }
}