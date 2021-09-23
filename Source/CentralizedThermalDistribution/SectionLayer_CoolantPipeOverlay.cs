using UnityEngine;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class SectionLayer_CoolantPipeOverlay : SectionLayer_Things
    {
        /// <summary>
        ///     Coolant Pipe Overlay Section Layer
        /// </summary>
        /// <param name="section">Section of the Map</param>
        public SectionLayer_CoolantPipeOverlay(Section section) : base(section)
        {
            requireAddToMapMesh = false;
            relevantChangeTypes = MapMeshFlag.Buildings;
        }

        /// <summary>
        ///     Function which Checks if we need to Draw the Layer or not.
        ///     If we do, we first call ForceDrawLayer for SectionLayer_CoolantTrader because those must be drawn UNDER the pipes. Then we call the Base DrawLayer();
        /// </summary>
        public override void DrawLayer()
        {
            if (!Visible) return;

            var designatorBuild = Find.DesignatorManager.SelectedDesignator as Designator_Build;

            var thingDef = (designatorBuild?.PlacingDef as ThingDef)?.GetCompProperties<CompProperties_Coolant>();

            if ((thingDef != null) && (thingDef.pipeColor != CompCoolant.PipeColor.None))
            {
                // This ensures pipes overlay is drawn over trader overlay. This took so much trial and error to figure out.
                (section.GetLayer(typeof(SectionLayer_CoolantTraderOverlay)) as SectionLayer_CoolantTraderOverlay)?.ForceDrawLayer(Vector3.zero);
                ForceDrawLayer(new(0f, 1f, 0f));
            }
        }

        public void ForceDrawLayer(Vector3 offset)
        {
            foreach (var layerSubMesh in subMeshes)
            {
                if (layerSubMesh.finalized && !layerSubMesh.disabled)
                {
                    Graphics.DrawMesh(layerSubMesh.mesh, offset, Quaternion.identity, layerSubMesh.material, 0);
                }
            }
        }

        /// <summary>
        ///     Called when a thing is going to be printed to the layer.
        ///     We draw all coolant pipes. I don't care about the order here.
        /// </summary>
        /// <param name="thing">Thing that triggered the Draw Call</param>
        protected override void TakePrintFrom(Thing thing)
        {
            var compCoolant = (thing as Building)?.GetComp<CompCoolantPipe>();
            if (compCoolant == null)
            {
                return;
            }

            compCoolant?.PrintOverlay(this);
        }
    }
}