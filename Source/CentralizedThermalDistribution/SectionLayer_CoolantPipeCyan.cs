﻿using System.Linq;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    internal class SectionLayer_CoolantPipeCyan : SectionLayer_Things
    {
        public CoolantPipeColor FlowType;

        /// <summary>
        ///     Cyan Pipe Overlay Section Layer
        /// </summary>
        /// <param name="section">Section of the Map</param>
        public SectionLayer_CoolantPipeCyan(Section section) : base(section)
        {
            FlowType = CoolantPipeColor.Cyan;
            requireAddToMapMesh = false;
            relevantChangeTypes = (MapMeshFlag)4;
        }

        /// <summary>
        ///     Function which Checks if we need to Draw the Layer or not. If we do, we call the Base DrawLayer();
        ///     We Check if the Pipe is a Cyan Pipe and thus start a DrawLayer request.
        /// </summary>
        public override void DrawLayer()
        {
            var designatorBuild = Find.DesignatorManager.SelectedDesignator as Designator_Build;

            var thingDef = designatorBuild?.PlacingDef as ThingDef;

            if (thingDef?.comps.OfType<CompProperties_Coolant>().FirstOrDefault(x => x.flowType == FlowType) != null)
            {
                base.DrawLayer();
            }
        }

        /// <summary>
        ///     Called when a Draw is initiated from DrawLayer.
        /// </summary>
        /// <param name="thing">Thing that triggered the Draw Call</param>
        protected override void TakePrintFrom(Thing thing)
        {
            var building = thing as Building;
            if (building == null)
            {
                return;
            }

            var compAirFlow = building.GetComps<CompCoolant>()
                .FirstOrDefault(x => x.pipeColor == FlowType || x.pipeColor == CoolantPipeColor.Any);
            compAirFlow?.PrintForGrid(this, FlowType);
        }
    }
}