using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class SectionLayer_CoolantTraderOverlay : SectionLayer_Things
    {
        /// <summary>
        ///     Coolant Trader Overlay Section Layer
        /// </summary>
        /// <param name="section">Section of the Map</param>
        public SectionLayer_CoolantTraderOverlay(Section section) : base(section)
        {
            requireAddToMapMesh = false;
            relevantChangeTypes = MapMeshFlag.Buildings;
        }

        /// <summary>
        ///     This layer is never drawn on its own. It must be drawn explicitly by SectionLayer_CoolantPipeOverlay.
        /// </summary>
        public override void DrawLayer()
        {
            return;
        }

        public void ForceDrawLayer(Vector3 offset)
        {
            if (!Visible) return;

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
        ///     Only draws coolant traders.
        /// </summary>
        /// <param name="thing">Thing that triggered the Draw Call</param>
        protected override void TakePrintFrom(Thing thing)
        {
            var compCoolant = (thing as Building)?.GetComp<CompCoolantTrader>();
            if (compCoolant == null)
            {
                return;
            }

            compCoolant?.PrintOverlay(this);
        }
    }
}