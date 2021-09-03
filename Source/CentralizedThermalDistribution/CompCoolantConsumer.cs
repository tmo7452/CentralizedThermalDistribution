using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompCoolantConsumer : CompCoolantSwitchable
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
            CentralizedThermalDistributionUtility.GetNetManager(parent.Map).RegisterConsumer(this);

            base.PostSpawnSetup(respawningAfterLoad);
        }

        /// <summary>
        ///     Component De-spawned from Map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            CentralizedThermalDistributionUtility.GetNetManager(map).DeregisterConsumer(this);
            ResetCoolantVariables();
            base.PostDeSpawn(map);
        }

        /// <summary>
        ///     Extra Component Inspection string
        /// </summary>
        /// <returns>String Containing information for Consumers</returns>
        public override string CompInspectStringExtra()
        {
            if (!IsConnected())
            {
                return base.CompInspectStringExtra();
            }

            //if (!IsActive())
            //{
            //    return DisconnectedKey.Translate() + "\n" + base.CompInspectStringExtra();
            //}

            return base.CompInspectStringExtra();
        }

        /// <summary>
        ///     Adjusts the pending thermal load, to be processed by the coolant network.
        ///     Positive load to heat, negative load to cool.
        /// </summary>
        /// <param name="ThermalLoad">Float amount of thermal load to apply</param>
        public void PushThermalLoad(float ThermalLoad)
        {
            PendingThermalLoad += ThermalLoad;
        }
    }
}