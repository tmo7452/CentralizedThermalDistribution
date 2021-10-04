using Verse;
using RimWorld;
using static CentralizedThermalDistribution.CompThermalWorker;

namespace CentralizedThermalDistribution
{
    class ThermalWorkerInterface_Fuel : ThermalWorkerInterface
    {
        public ThermalWorkerInterface_Fuel(CompThermalWorker thermalWorker, bool isInput, CompRefuelable refuelableComp, float idleFuelConsumptionFactor) : base(thermalWorker, isInput)
        {
            FuelComp = refuelableComp;
            WorkingFuelConsumptionRate = FuelComp.Props.fuelConsumptionRate;
            IdleFuelConsumptionRate = WorkingFuelConsumptionRate * idleFuelConsumptionFactor;
        }

        private CompRefuelable FuelComp = null;
#if DEBUG
        [TweakValue("CTD", 0.0f, 0.5f)]
#endif
        private static float FuelMultiplier = 0.1f;
        private float WorkingFuelConsumptionRate;
        private float IdleFuelConsumptionRate;
        private bool ConsumingFuel = false;

        public override void Update()
        {
            base.Update();
            if (!FuelComp.HasFuel)
            {
                Status = ThermalWorkerStatus.offline;
                StatusReason = "OutOfFuel";
                return;
            }

            Multiplier = FuelMultiplier;
        }

        public override void PushWork(float work)
        {
            base.PushWork(work);
            FuelComp.Props.fuelConsumptionRate = WorkingFuelConsumptionRate;
            ConsumingFuel = true;
        }

        public override void DoIdle()
        {
            base.DoIdle();
            FuelComp.Props.fuelConsumptionRate = IdleFuelConsumptionRate;
            ConsumingFuel = true;
        }

        public override void DoOffline()
        {
            base.DoOffline();
            ConsumingFuel = false;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (ConsumingFuel)
                FuelComp.Notify_UsedThisTick();
        }
    }
}
