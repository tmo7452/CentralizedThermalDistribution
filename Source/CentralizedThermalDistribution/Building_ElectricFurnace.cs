using System.Linq;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_ElectricFurnace : Building_CoolantProvider
    {
        private const float ElectricFurnaceMultiplier = 0.04f; // Multipler unique to this provider type

        private int WattageSetting;

        /// <summary>
        ///     Building spawned on the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            WattageSetting = 800; //TEMP
        }

        public override void CheckStatus()
        {
            base.CheckStatus(); //Temperature reached check

            if (IsTemperatureReached) { SetStatus(Status.Idle, "TemperatureReached"); }
            else { SetStatus(Status.Working); }
        }

        public override void DoThermalWork(int tickMultiplier)
        {
            compPowerTrader.PowerOutput = -WattageSetting; //This will override what was set in Building_CoolantProvider.CoolantTick()
            ThermalWork = WattageSetting * ElectricFurnaceMultiplier * ThermalWorkMultiplier * tickMultiplier;
            compCoolant.PushThermalLoad(ThermalWork); // Push to coolant
        }
    }
}