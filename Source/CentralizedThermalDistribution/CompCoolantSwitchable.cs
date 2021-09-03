using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompCoolantSwitchable : CompCoolant
    {

        public CoolantPipeColorSelection PipeColorSelection = CoolantPipeColorSelection.Auto;

        /// <summary>
        ///     Method called during Game Save/Load
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref PipeColorSelection, "pipeColorSelection", CoolantPipeColorSelection.Auto);
            CentralizedThermalDistributionUtility.GetNetManager(parent.Map).IsDirty = true;
        }

        /// <summary>
        ///     Set the Pipe Selection
        /// </summary>
        /// <param name="selection">Priority to Switch to.</param>
        public void SetSelection(CoolantPipeColorSelection selection)
        {
            PipeColorSelection = selection;
            coolantNet = null;
            CentralizedThermalDistributionUtility.GetNetManager(parent.Map).IsDirty = true;
        }
    }
}