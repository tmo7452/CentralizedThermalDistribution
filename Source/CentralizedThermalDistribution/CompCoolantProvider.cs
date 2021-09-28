using System.Text;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    // Provides coolant flow and influences its temperature. At least one provider must be present on a network.
    public abstract class CompCoolantProvider : CompCoolantTrader
    {
        public const string AirFlowOutputKey = "CentralizedThermalDistribution.AirFlowOutput";
        public const string IntakeTempKey = "CentralizedThermalDistribution.Producer.IntakeTemperature";
        public const string IntakeBlockedKey = "CentralizedThermalDistribution.Producer.IntakeBlocked";

        public enum Status
        {
            Offline,
            Idle,
            Working,
        }
        private static readonly string[] statusStrings = { "Offline", "Idle", "Working" }; // TODO change to translation keys

        public CompPowerTrader compPowerTrader { get; private set; }
        public CompTempControlEx compTempControl { get; private set; }

        public bool IsActiveOnNetwork => status != Status.Offline; // Active on the coolant network.
        public int CoolantThermalMass;
        public float CoolantTemperature;
        public Status status { get; private set; } = Status.Offline;
        public string statusString { get; private set; } = statusStrings[0];
        public bool IsTemperatureReached { get; protected set; } = false;

        public abstract void DoThermalWork(int tickMultiplier);

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

        /// <summary>
        ///     Post Spawn for Component
        /// </summary>
        /// <param name="respawningAfterLoad">Unused Flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            coolantNetManager.RegisterProvider(this);
            CoolantThermalMass = Props.providerCoolantThermalMass;
            CoolantTemperature = parent.Position.GetTemperature(parent.Map);
            compPowerTrader = parent.GetComp<CompPowerTrader>();
            compTempControl = parent.GetComp<CompTempControlEx>();

            compTempControl.targetTempString = "CentralizedThermalDistribution.Provider.TargetCoolantTemperature";
        }

        public override string CompInspectStringExtra()
        {
            System.Text.StringBuilder output = new();
            output.AppendLine(base.CompInspectStringExtra());
            output.AppendLine("DEBUG Status: " + status + " (" + statusString + ")");
            return output.ToString().Trim();
        }

        public virtual void CheckStatus()
        {
            // === Temperature reached check ===
            // True if coolant temp is at or beyond desired temp, based on whether we are heating or cooling (from ThermalWorkMultiplier).
            IsTemperatureReached = ((System.Math.Sign(ThermalWorkMultiplier) * CoolantTemperature) >= (System.Math.Sign(ThermalWorkMultiplier) * compTempControl.targetTemperature));
        }



        /// <summary>
        ///     Despawn Event for a Producer Component
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            coolantNetManager.DeregisterProvider(this);
            base.PostDeSpawn(map);
        }

        public override void SetNet(CoolantNet newNet)
        {
            if (coolantNet != null)
                coolantNet.Providers.Remove(this);
            base.SetNet(newNet);
            if (coolantNet != null)
                coolantNet.Providers.Add(this);
        }

        /// <summary>
        ///     Provided a thermal load value, this adjusts the internal coolant temp appropriately.
        ///     Positive load to heat, negative load to cool.
        /// </summary>
        /// <param name="ThermalLoad">Float amount Thermal Load to apply</param>
        public override void PushThermalLoad(float ThermalLoad)
        {
            CoolantTemperature += ThermalLoad / CoolantThermalMass;
        }

        protected override void CoolantTick(int tickMultiplier)
        {
            if (compPowerTrader.PowerOn)
            {
                CheckStatus();
                switch (status) // Could be converted to an if statement, but that might change down the line.
                {
                    case Status.Working:
                        // Active on coolant network and consuming high power.
                        compTempControl.operatingAtHighPower = true;
                        compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption;
                        DoThermalWork(tickMultiplier);
                        break;

                    case Status.Idle:
                        // Active on coolant network and consuming low power.

                        /* Redundant code. Just let it fall through.
                        compTempControl.operatingAtHighPower = false;
                        compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption * compTempControl.Props.lowPowerConsumptionFactor;
                        ThermalWork = 0;
                        break;
                        */

                    case Status.Offline:
                        // Inactive on coolant network and consuming low power.
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
                // Vanilla coolers and heaters do not update the power state in this situation.
                //compTempControl.operatingAtHighPower = false;
                //compPowerTrader.PowerOutput = 0;
                ThermalWork = 0;
            }
        }
    }
}