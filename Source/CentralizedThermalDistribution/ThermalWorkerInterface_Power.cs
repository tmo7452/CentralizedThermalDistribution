using Verse;
using RimWorld;
using static CentralizedThermalDistribution.CompThermalWorker;

namespace CentralizedThermalDistribution
{
    class ThermalWorkerInterface_Power : ThermalWorkerInterface
    {
        public ThermalWorkerInterface_Power(CompThermalWorker thermalWorker, bool isInput, CompPowerTrader powerTraderComp) : base(thermalWorker, isInput)
        {
            //PowerTraderComp = powerTraderComp;
            WattageSetting = 175; //TODO?
        }

        //private CompPowerTrader PowerTraderComp = null;
#if DEBUG
        [TweakValue("CTD", 0.0f, 0.5f)]
#endif
        private static float PowerMultiplier = 0.2f;
        private static int WattageSetting;

        public override void Update()
        {
            base.Update();
            Status = ThermalWorkerStatus.ready;
            StatusReason = null;
            PowerOutput = -WattageSetting;
            Multiplier = WattageSetting * PowerMultiplier;
        }

        public override void PushWork(float work)
        {
            base.PushWork(work);
        }

    }
}
