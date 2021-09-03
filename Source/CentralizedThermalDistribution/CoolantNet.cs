using System.Collections.Generic;
using System.Text;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CoolantNet
    {
        public int NetID { get; set; } = -2;
        public CoolantPipeColor PipeColor;

        private int ActiveProviders = 0;
        private float CoolantTemperature = 0f;

        public List<CompCoolant> Connectors = new();
        public List<CompCoolantProvider> Providers = new();
        public List<CompCoolantConsumer> Consumers = new();

        /// <summary>
        ///     Register a coolant provider in the Network.
        /// </summary>
        /// <param name="provider">The Provider's Component</param>
        public void RegisterProvider(CompCoolantProvider provider)
        {
            if (Providers.Contains(provider))
            {
                Log.Error("AirFlowNet registered provider it already had: " + provider);
                return;
            }

            Providers.Add(provider);
        }

        /// <summary>
        ///     De-register a coolant provider in the Network.
        /// </summary>
        /// <param name="provider">The Provider's Component</param>
        public void DeregisterProvider(CompCoolantProvider provider)
        {
            if (!Providers.Contains(provider))
            {
                Log.Error("AirFlowNet de-registered provider it already removed: " + provider);
                return;
            }

            Providers.Remove(provider);
        }

        /// <summary>
        ///     Register a coolant consumer in the Network.
        /// </summary>
        /// <param name="consumer">The Consumer's Component</param>
        public void RegisterConsumer(CompCoolantConsumer consumer)
        {
            if (Consumers.Contains(consumer))
            {
                Log.Error("CoolantNet registered consumer it already had: " + consumer);
                return;
            }

            Consumers.Add(consumer);
        }

        /// <summary>
        ///     De-register a coolant consumer in the Network.
        /// </summary>
        /// <param name="consumer">The Consumer's Component</param>
        public void DeregisterProvider(CompCoolantConsumer consumer)
        {
            if (!Consumers.Contains(consumer))
            {
                Log.Error("CoolantNet de-registered consumer it already removed: " + consumer);
                return;
            }

            Consumers.Remove(consumer);
        }

        /// <summary>
        ///     Process one Tick of the Air Flow Network. Here we process the Producers, Consumers and Climate Controllers.
        ///     We Calculate the Flow Efficiency (FE) and Thermal Efficiency (TE).
        ///     FE & TEs are recorded for each individual network.
        /// </summary>
        public void CoolantNetTick()
        {
            ActiveProviders = 0;
            float massSum = 0;
            float energySum = 0f;
            float thermalLoadSum = 0f;

            foreach (var provider in Providers)
            {
                if (!provider.IsConnected() || !provider.IsActive()) continue;

                massSum += provider.CoolantThermalMass;
                energySum += provider.CoolantThermalMass * provider.CoolantTemperature;
                ActiveProviders++;
            }

            if (ActiveProviders == 0) return;
            CoolantTemperature = energySum / massSum;

            foreach (var consumer in Consumers)
            {

                if (!consumer.IsConnected()) continue;

                thermalLoadSum += consumer.PendingThermalLoad;
                consumer.PendingThermalLoad = 0;
            }

            float splitThermalLoad = thermalLoadSum / ActiveProviders;

            foreach (var provider in Providers)
            {
                if (!provider.IsConnected() || !provider.IsActive()) continue;

                provider.CoolantTemperature = CoolantTemperature;
                provider.PushThermalLoad(splitThermalLoad);
            }
        }

        /// <summary>
        ///     Check if the coolant network is active. That is, if it has any active providers.
        /// </summary>
        /// <returns>Boolean Active State</returns>
        public bool IsNetActive()
        {
            return ActiveProviders > 0;
        }

        /// <summary>
        ///     Get current tempature of the net coolant. Always check IsNetActive and do not call if net is inactive.
        /// </summary>
        /// <returns>Float Coolant Temp</returns>
        public float GetNetCoolantTemperature()
        {
            if (IsNetActive()) return CoolantTemperature;
            Log.Error("GetNetCoolantTemperature called while net is inactive.");
            return 666f;
        }

        /// <summary>
        ///     Print the Debug String for this Network
        /// </summary>
        /// <returns>Multi-line String containing Output</returns>
        public string DebugString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("------------");
            stringBuilder.AppendLine("AIRFLOW NET:");
            stringBuilder.AppendLine("  Active Providers: " + ActiveProviders);
            stringBuilder.AppendLine("  Coolant Temperature: " + CoolantTemperature);

            stringBuilder.AppendLine("  Producers: ");
            foreach (var current in Providers)
            {
                stringBuilder.AppendLine("      " + current.parent);
            }

            stringBuilder.AppendLine("  Consumers: ");
            foreach (var current in Consumers)
            {
                stringBuilder.AppendLine("      " + current.parent);
            }

            stringBuilder.AppendLine("------------");
            return stringBuilder.ToString();
        }
    }
}