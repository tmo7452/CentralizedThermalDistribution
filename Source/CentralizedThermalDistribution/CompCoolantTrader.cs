using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompCoolantTrader : CompCoolant
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

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
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

        public virtual void PushThermalLoad(float ThermalLoad)
        {
            return;
        }

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