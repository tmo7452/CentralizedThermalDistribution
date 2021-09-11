using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_CoolantPipe : Building
    {
        public CompCoolantPipe CompAirFlowPipe;
        public CompCoolant.PipeColor FlowType;

        /// <summary>
        ///     Building spawned on the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CompAirFlowPipe = GetComp<CompCoolantPipe>();
        }
    }
}