using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_CoolantProvider : Building
    {
        public enum Status
        {
            Offline,
            Idle,
            Working,
        }
        private static readonly string[] statusStrings = { "Offline", "Idle", "Working" }; // TODO change to translation keys

        public const int tickRateInterval = 50; // Before this can change, the following methods would require rework: Tick(), TickRare()

        public CompPowerTrader compPowerTrader { get; private set; }
        public CompTempControlEx compTempControl { get; private set; }
        public CompCoolantProvider compCoolant { get; private set; }

        public Status status { get; private set; } = Status.Offline;
        public string statusString { get; private set; } = statusStrings[0];
        private System.Collections.Generic.List<Gizmo> Gizmos = new();
        public bool IsTemperatureReached { get; protected set; } = false;
        public float ThermalWork { get; protected set; } = 0f;
        public float ThermalWorkMultiplier; // Multiplier unique to the building type, set by the Def. Positive if heating coolant, negative if cooling.

        protected void SetStatus(Status newStatus, string reason = null)
        {
            status = newStatus;
            if (reason == null)
            {
                statusString = statusStrings[(int)status];
            }
            else
            {
                statusString = reason;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = GetComp<CompPowerTrader>();
            compTempControl = GetComp<CompTempControlEx>();
            compCoolant = GetComp<CompCoolantProvider>();

            compTempControl.targetTempString = "CentralizedThermalDistribution.Provider.TargetCoolantTemperature";
            compCoolant.Props.flowType = CoolantPipeColor.Any;

            ThermalWorkMultiplier = compCoolant.Props.ThermalWorkMultiplier;
            AddGizmo(CentralizedThermalDistributionUtility.GetPipeSwitchToggle(compCoolant));
        }

        public override string GetInspectString()
        {
            System.Text.StringBuilder output = new();
            output.AppendLine(base.GetInspectString());
            output.AppendLine("DEBUG Status: " + status + " (" + statusString + ")");
            output.AppendLine("DEBUG ThermalWork: " + ThermalWork);
            return output.ToString().Trim();
        }

        public void AddGizmo(Gizmo gizmo)
        {
            Gizmos.Add(gizmo);
        }

        public override System.Collections.Generic.IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            foreach (var gizmo in Gizmos)
                yield return gizmo;
        }

        public virtual void CheckStatus()
        {
            // === Temperature reached check ===
            // True if coolant temp is at or beyond desired temp, based on whether we are heating or cooling (from ThermalWorkMultiplier).
            IsTemperatureReached = ((System.Math.Sign(ThermalWorkMultiplier) * compCoolant.CoolantTemperature) >= (System.Math.Sign(ThermalWorkMultiplier) * compTempControl.targetTemperature)); 
        }

        public virtual void DoThermalWork(int tickMultiplier)
        {
            return;
        }

        private void CoolantTick(int tickMultiplier)
        {
            if (compPowerTrader.PowerOn)
            {
                CheckStatus();
                switch (status)
                {
                    case Status.Working:
                        // Active on coolant network and consuming high power.
                        compCoolant.ActiveOnNetwork = true;
                        compTempControl.operatingAtHighPower = true;
                        compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption;
                        DoThermalWork(tickMultiplier);
                        break;

                    case Status.Idle:
                        // Active on coolant network and consuming low power.
                        compCoolant.ActiveOnNetwork = true;
                        compTempControl.operatingAtHighPower = false;
                        compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption * compTempControl.Props.lowPowerConsumptionFactor;
                        ThermalWork = 0;
                        break;

                    case Status.Offline:
                        // Inactive on coolant network and consuming low power.
                        compCoolant.ActiveOnNetwork = false;
                        compTempControl.operatingAtHighPower = false;
                        compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption * compTempControl.Props.lowPowerConsumptionFactor;
                        ThermalWork = 0;
                        break;
                }
            }
            else
            {
                // Inactive on coolant network and consuming no power.
                SetStatus(Status.Offline, "NoPower");
                compCoolant.ActiveOnNetwork = false;
                // Vanilla coolers and heaters do not update the power state in this situation.
                //compTempControl.operatingAtHighPower = false;
                //compPowerTrader.PowerOutput = 0;
                ThermalWork = 0;
            }
        }

        public override void Tick()
        {
            if (this.IsHashIntervalTick(50))
                CoolantTick(1);
            base.Tick();
        }

        public override void TickRare()
        {
            CoolantTick(5);
            base.Tick();
        }
    }
}
