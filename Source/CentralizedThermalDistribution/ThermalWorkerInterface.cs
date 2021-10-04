using static CentralizedThermalDistribution.CompThermalWorker;

namespace CentralizedThermalDistribution
{
    class ThermalWorkerInterface
    {
        public ThermalWorkerInterface(CompThermalWorker thermalWorker, bool isInput)
        {
            IsInput = isInput;
        }

        public ThermalWorkerStatus Status { get; protected set; } = ThermalWorkerStatus.offline;
        public string StatusReason { get; protected set; } = null;
        public bool IsInput { get; private set; }
        public bool IsOutput => !IsInput;
        public float? Efficiency { get; protected set; } = null;
        public float? Temperature { get; protected set; } = null;
        public float PowerOutput { get; protected set; } = 0f;
        public float Multiplier { get; protected set; } = 1.00f;

        public virtual void Update()
        {
            Status = ThermalWorkerStatus.ready;
            StatusReason = null;
        }

        public virtual void PushWork(float work)
        {
            Status = ThermalWorkerStatus.working;
        }

        public virtual void DoOffline()
        {
            return;
        }
        public virtual void DoIdle()
        {
            return;
        }
        public virtual void CompTick()
        {
            return;
        }
        public virtual void CompTickRare()
        {
            return;
        }
    }
}
