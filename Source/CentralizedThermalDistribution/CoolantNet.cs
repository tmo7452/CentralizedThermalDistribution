using System.Collections.Generic;
using System.Text;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CoolantNet
    {
        public uint NetID { get; set; } = 0;
        public CompCoolant.PipeColor pipeColor;

        private int ActiveProviders = 0;
        private float CoolantTemperature = 0f;

        public List<CompCoolantPipe> Pipes = new();
        public List<CompCoolantTrader> Traders = new();
        public List<CompCoolantProvider> Providers = new();
        public List<CompCoolantConsumer> Consumers = new();

        public CoolantNet(uint newNetID, CompCoolant.PipeColor newPipeColor)
        {
            NetID = newNetID;
            pipeColor = newPipeColor;
        }

        public void DestroyNet()
        {
            foreach (var pipe in Pipes)
                pipe.SetNet(null);
            Pipes = new();

            foreach (var trader in Traders)
                trader.RemoveNet(this);
            Traders = new();

            Providers = new();
            Consumers = new();
            ActiveProviders = 0;
        }

        public void CoolantNetTick()
        {
            ActiveProviders = 0;
            float massSum = 0;
            float energySum = 0f;
            float thermalLoadSum = 0f;

            foreach (var provider in Providers)
            {
                if (!provider.IsConnected || !provider.IsActiveOnNetwork) continue;

                massSum += provider.CoolantThermalMass;
                energySum += provider.CoolantThermalMass * provider.CoolantTemperature;
                ActiveProviders++;
            }

            if (ActiveProviders == 0) return;
            CoolantTemperature = energySum / massSum;

            foreach (var consumer in Consumers)
            {

                if (!consumer.IsConnected) continue;

                thermalLoadSum += consumer.PendingThermalLoad;
                consumer.PendingThermalLoad = 0;
            }

            float loadPerUnitMass = thermalLoadSum / massSum;

            foreach (var provider in Providers)
            {
                if (!provider.IsConnected || !provider.IsActiveOnNetwork) continue;

                provider.CoolantTemperature = CoolantTemperature;
                provider.PushThermalLoad(loadPerUnitMass * provider.CoolantThermalMass);
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
        ///     Get current tempature of the net coolant, or null if inactive.
        /// </summary>
        /// <returns>Float coolant temp or null</returns>
        public float? GetNetCoolantTemperature()
        {
            if (IsNetActive()) return CoolantTemperature;
            return null;
        }
    }
}