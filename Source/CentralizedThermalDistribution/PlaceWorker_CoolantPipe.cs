using System.Linq;
using Verse;

namespace CentralizedThermalDistribution
{
    public class PlaceWorker_CoolantPipe : PlaceWorker
    {
        /// <summary>
        ///     Place Worker for Air Pipes. Checks if Air Pipes are in a Suitable Location or not.
        ///     Checks:
        ///     - Current Cell shouldn't have an Air Flow Building (Since they already have a Pipe)
        ///     - If a pipe of the same color is built on the tile, dont allow it
        /// </summary>
        /// <param name="def">The Def Being Built</param>
        /// <param name="loc">Target Location</param>
        /// <param name="rot">Rotation of the Object to be Placed</param>
        /// <param name="map"></param>
        /// <param name="thingToIgnore">Unused field</param>
        /// <param name="thing"></param>
        /// <returns>Boolean/Acceptance Report if we can place the object of not.</returns>
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 loc, Rot4 rot, Map map,
            Thing thingToIgnore = null, Thing thing = null)
        {
            //var thingList = loc.GetThingList(map);
            //return thingList.OfType<Building_AirFlowControl>().Any() ? AcceptanceReport.WasRejected : AcceptanceReport.WasAccepted;
            //if (loc.GetThingList(map).OfType<Building_CoolantFlowControl>().Any())
            //{
            //    return AcceptanceReport.WasRejected;
            //}

            var pipeColor = (def as ThingDef).GetCompProperties<CompProperties_Coolant>().pipeColor;
            return map.GetComponent<CoolantNetManager>().IsPipeAt(loc, pipeColor) ? AcceptanceReport.WasRejected : AcceptanceReport.WasAccepted;
        }
    }
}