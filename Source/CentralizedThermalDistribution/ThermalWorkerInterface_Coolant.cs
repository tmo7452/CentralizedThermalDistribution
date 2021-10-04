using static CentralizedThermalDistribution.CompThermalWorker;

namespace CentralizedThermalDistribution
{
    class ThermalWorkerInterface_Coolant : ThermalWorkerInterface
    {
        public ThermalWorkerInterface_Coolant(CompThermalWorker thermalWorker, bool isInput, CompCoolantTrader compCoolant) : base(thermalWorker, isInput)
        {
            CoolantComp = compCoolant;
            if (isInput)
                CoolantConsumerComp = (compCoolant as CompCoolantConsumer);
            else
                CoolantProviderComp = (compCoolant as CompCoolantProvider);
        }

        private CompCoolantTrader CoolantComp = null;
        private CompCoolantConsumer CoolantConsumerComp = null;
        private CompCoolantProvider CoolantProviderComp = null;

        public override void Update()
        {
            base.Update();
            if (IsInput)
            {
                if (!CoolantConsumerComp.IsConnected)
                {
                    Status = ThermalWorkerStatus.offline;
                    StatusReason = "CoolantNotConnected";
                    return;
                }
                if (!CoolantConsumerComp.coolantNet.IsNetActive())
                {
                    Status = ThermalWorkerStatus.offline;
                    StatusReason = "CoolantNetInactive";
                    return;
                }
            }
            else
            {
                CoolantProviderComp.IsActiveOnNetwork = true;
            }
            Temperature = CoolantComp.GetTemp();
        }

        public override void PushWork(float work)
        {
            base.PushWork(work);
            CoolantComp.PushThermalLoad(work);
        }

        public override void DoOffline()
        {
            base.DoOffline();
            if (IsOutput)
                CoolantProviderComp.IsActiveOnNetwork = false;
        }
    }
}
