﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class PlaceWorker_CoilVent : PlaceWorker
    {
        /// <summary>
        ///     Draw Overlay when Selected or Placing.
        ///     Here we just draw a red/blue/cyan cell (based on Network flow type) towards the North. To indicate Exhaust.
        /// </summary>
        /// <param name="def">The Thing's Def</param>
        /// <param name="center">Location</param>
        /// <param name="rot">Rotation</param>
        /// <param name="ghostCol">Ghost Color</param>
        /// <param name="thing"></param>
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var type = CoolantPipeColor.Red;

            var map = Find.CurrentMap;

            //var list = center.GetThingList(map);
            //foreach (var thingType in list)
            foreach (var thingType in center.GetThingList(map))
            {
                if (!(thingType is Building_CoilVent))
                {
                    continue;
                }

                var airVent = thingType as Building_CoilVent;

                if (airVent.compCoolant.coolantNet != null)
                {
                    type = airVent.compCoolant.coolantNet.PipeColor;
                }

                break;
            }

            var intVec = center + IntVec3.North.RotatedBy(rot);

            var typeColor = type == CoolantPipeColor.Red ? Color.red : type == CoolantPipeColor.Blue ? Color.blue : Color.cyan;

            GenDraw.DrawFieldEdges(new List<IntVec3>
            {
                intVec
            }, typeColor);

            var roomGroup = intVec.GetRoomOrAdjacent(map);
            if (roomGroup == null)
            {
                return;
            }

            if (!roomGroup.UsesOutdoorTemperature)
            {
                GenDraw.DrawFieldEdges(roomGroup.Cells.ToList(), typeColor);
            }
        }

        /// <summary>
        ///     Place Worker for Air Vents.
        ///     Checks:
        ///     - North Cell from Center musn't be Impassable
        /// </summary>
        /// <param name="def">The Def Being Built</param>
        /// <param name="center">Target Location</param>
        /// <param name="rot">Rotation of the Object to be Placed</param>
        /// <param name="map"></param>
        /// <param name="thingToIgnore">Unused field</param>
        /// <param name="thing"></param>
        /// <returns>Boolean/Acceptance Report if we can place the object of not.</returns>
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map,
            Thing thingToIgnore = null, Thing thing = null)
        {
            //var vec = center + IntVec3.North.RotatedBy(rot);

            //if (vec.Impassable(map))
            if ((center + IntVec3.North.RotatedBy(rot)).Impassable(map))
            {
                return "CentralizedThermalDistribution.Consumer.AirVentPlaceError".Translate();
            }

            return true;
        }
    }
}