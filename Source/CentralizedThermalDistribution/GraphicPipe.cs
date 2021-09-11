using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class GraphicPipe : Graphic_Linked
    {
        public CompCoolant.PipeColor pipeColor;

        public GraphicPipe()
        {
        }

        /// <summary>
        ///     Graphic for Pipes Constructor
        /// </summary>
        /// <param name="graphic">Multi Graphic Object</param>
        /// <param name="flowType">Type of Pipe</param>
        public GraphicPipe(Graphic graphic, CompCoolant.PipeColor flowType)
        {
            subGraphic = graphic;
            pipeColor = flowType;
        }

        /// <summary>
        ///     Graphic for Pipes Constructor with Defaulted Red Pipe
        /// </summary>
        /// <param name="graphic">Multi Graphic Object</param>
        public GraphicPipe(Graphic graphic)
        {
            subGraphic = graphic;
            pipeColor = CompCoolant.PipeColor.Red;
        }

        /// <summary>
        ///     Overriden Function for Pipe Atlas. It Checks for Neighbouring tiles if it should be Linked to the target cell.
        ///     This Function specifies the condition that will be used.
        ///     Here we just check if the target cell that is asked for linkage has a Pipe of the same Color or not.
        /// </summary>
        /// <param name="vec">Target Cell</param>
        /// <param name="parent">Parent Object</param>
        /// <returns>Should Link with Same Color Pipe or not</returns>
        public override bool ShouldLinkWith(IntVec3 vec, Thing parent)
        {
            return vec.InBounds(parent.Map) &&
                   CentralizedThermalDistributionUtility.GetNetManager(parent.Map).IsPipeAt(vec, pipeColor);
        }


        /// <summary>
        ///     Main method to Print a Atlas Pipe Graphic
        /// </summary>
        /// <param name="layer">Section Layer calling this Print command</param>
        /// <param name="parent">Parent Object</param>
        /// <param name="extraRotation"></param>
        public override void Print(SectionLayer layer, Thing parent, float extraRotation)
        {
            if (parent.def.defName.Contains("Hidden"))
            {
                return;
            }

            //var material = LinkedDrawMatFrom(parent, parent.Position);
            //Printer_Plane.PrintPlane(layer, parent.TrueCenter(), Vector2.one, material, 0f);
            Printer_Plane.PrintPlane(
                layer,
                parent.TrueCenter(),
                Vector2.one,
                LinkedDrawMatFrom(parent, parent.Position)
            );

            foreach (var adjacentPos in GenAdj.CardinalDirections)
            {
                if (!adjacentPos.InBounds(parent.Map) ||
                    !CentralizedThermalDistributionUtility.GetNetManager(parent.Map).IsPipeAt(adjacentPos, pipeColor) ||
                    adjacentPos.GetTerrain(parent.Map).layerable)
                {
                    continue;
                }

                //var thingList = intVec.GetThingList(parent.Map);

                //if (thingList.Any(predicate))

                if (adjacentPos.GetThingList(parent.Map).OfType<Building_CoolantPipe>().Any()) //WTF????
                //if (CentralizedThermalDistributionUtility.GetNetManager(parent.Map).IsPipeAt(adjacentPos, pipeColor))
                {
                    continue;
                }

                //var material2 = LinkedDrawMatFrom(parent, intVec);
                //Printer_Plane.PrintPlane(layer, intVec.ToVector3ShiftedWithAltitude(parent.def.Altitude), Vector2.one, material2, 0f);
                Printer_Plane.PrintPlane(
                    layer,
                    adjacentPos.ToVector3ShiftedWithAltitude(parent.def.Altitude),
                    Vector2.one,
                    LinkedDrawMatFrom(parent, adjacentPos));
            }
        }
    }
}