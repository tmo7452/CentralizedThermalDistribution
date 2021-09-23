using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public abstract class CompCoolantTrader : CompCoolant
    {
        public enum PipeColorSelection
        {
            Red = 0,
            Blue = 1,
            Cyan = 2,
            Green = 4,
            Auto = 3,
        }

        public static bool PipeColorMatchesSelection(PipeColor color, PipeColorSelection selection)
        {
            return color switch
            {
                PipeColor.Red => (selection == PipeColorSelection.Red) || (selection == PipeColorSelection.Auto),
                PipeColor.Blue => (selection == PipeColorSelection.Blue) || (selection == PipeColorSelection.Auto),
                PipeColor.Cyan => (selection == PipeColorSelection.Cyan) || (selection == PipeColorSelection.Auto),
                PipeColor.Green => (selection == PipeColorSelection.Green) || (selection == PipeColorSelection.Auto),
                _ => false, // Default, I guess. Visual Studio generated this beauty.
            };
        }

        public PipeColorSelection pipeColorSelection = PipeColorSelection.Auto;
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
            AddGizmo(() => CentralizedThermalDistributionUtility.GetPipeSwitchToggle(this));
            RescanNets(CentralizedThermalDistributionUtility.GetNetManager(parent.Map));
        }

        /// <summary>
        ///     Method called during Game Save/Load
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref pipeColorSelection, "pipeColorSelection", PipeColorSelection.Auto);
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

        /// <summary>
        ///     Set the Pipe Selection
        /// </summary>
        /// <param name="selection">Priority to Switch to.</param>
        public void SetSelection(PipeColorSelection selection)
        {
            pipeColorSelection = selection;
            UpdateAttachedNet();
        }

        public abstract void PushThermalLoad(float ThermalLoad);

        public void RescanNets(CoolantNetManager manager)
        {
            
            AvailableNets = new();

            // For each cell we occupy, scan for any pipes and add their networks.
            foreach (var pos in parent.OccupiedRect().Cells)
            {
                for (int pipeColorIndex = 0; pipeColorIndex < PipeColorCount; pipeColorIndex++)
                {
                    if (manager.IsPipeAt(pos, pipeColorIndex))
                    {
                        CoolantNet net = manager.GetPipeAt(pos, pipeColorIndex).coolantNet;
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
    }
}