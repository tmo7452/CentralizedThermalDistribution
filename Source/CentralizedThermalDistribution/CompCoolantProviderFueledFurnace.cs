using System.Linq;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompCoolantProviderFueledFurnace : CompCoolantProvider
    {
        public CompRefuelable compRefuelable;

        private const float FueledFurnaceMultiplier = 4.0f; // Multipler unique to this provider type

        public bool IsOutOfFuel = true;
        private float highFuelConsumptionRate;
        private float lowFuelConsumptionRate;

        /// <summary>
        ///     Building spawned on the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compRefuelable = parent.GetComp<CompRefuelable>();

            compTempControl.consumptionModeString = null; // Disables printing the consumption mode, since it uses same power regardless.

            highFuelConsumptionRate = compRefuelable.Props.fuelConsumptionRate;
            lowFuelConsumptionRate = highFuelConsumptionRate * Props.ProviderLowFuelConsumptionFactor;
        }

        public override void CheckStatus()
        {
            base.CheckStatus(); //Temperature reached check

            // === Fuel status check ===
            IsOutOfFuel = !compRefuelable.HasFuel;

            if (IsOutOfFuel) { SetStatus(Status.Offline, "OutOfFuel"); }
            else if (IsTemperatureReached) { SetStatus(Status.Idle, "TemperatureReached"); }
            else { SetStatus(Status.Working); }

            // Set the current fuel consumption rate
            compRefuelable.Props.fuelConsumptionRate = IsTemperatureReached ? lowFuelConsumptionRate : highFuelConsumptionRate;
        }

        public override void DoThermalWork(int tickMultiplier)
        {
            ThermalWork = FueledFurnaceMultiplier * ThermalWorkMultiplier * tickMultiplier;
            PushThermalLoad(ThermalWork); // Push to coolant
        }

        public override void CompTick()
        {
            base.CompTick();

            //Consume fuel if online
            if (status != Status.Offline)
                compRefuelable.Notify_UsedThisTick();
        }
    }
}