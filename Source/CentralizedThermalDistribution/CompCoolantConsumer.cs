using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public abstract class CompCoolantConsumer : CompCoolantTrader
    {
        public const string AirFlowOutputKey = "CentralizedThermalDistribution.AirFlowOutput";
        public const string IntakeTempKey = "CentralizedThermalDistribution.Consumer.ConvertedTemperature";
        public const string FlowEfficiencyKey = "CentralizedThermalDistribution.Consumer.FlowEfficiencyKey";
        public const string ThermalEfficiencyKey = "CentralizedThermalDistribution.Consumer.ThermalEfficiencyKey";
        public const string DisconnectedKey = "CentralizedThermalDistribution.Consumer.Disconnected";
        public const string ClosedKey = "CentralizedThermalDistribution.Consumer.Closed";

        public float PendingThermalLoad = 0;

        /// <summary>
        ///     Post Spawn for Component
        /// </summary>
        /// <param name="respawningAfterLoad">Unused Flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            coolantNetManager.RegisterConsumer(this);
        }

        /// <summary>
        ///     Component De-spawned from Map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            coolantNetManager.DeregisterConsumer(this);
            base.PostDeSpawn(map);
        }

        public override void SetNet(CoolantNet newNet)
        {
            if (coolantNet != null)
                coolantNet.Consumers.Remove(this);
            base.SetNet(newNet);
            if (coolantNet != null)
                coolantNet.Consumers.Add(this);
        }

        /// <summary>
        ///     Adjusts the pending thermal load, to be processed by the coolant network.
        ///     Positive load to heat, negative load to cool.
        /// </summary>
        /// <param name="ThermalLoad">Float amount of thermal load to apply</param>
        public override void PushThermalLoad(float ThermalLoad)
        {
            PendingThermalLoad += ThermalLoad;
        }
    }
}