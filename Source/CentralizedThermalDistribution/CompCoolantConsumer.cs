using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompCoolantConsumer : CompCoolant
    {
        public const string AirFlowOutputKey = "CentralizedThermalDistribution.AirFlowOutput";
        public const string IntakeTempKey = "CentralizedThermalDistribution.Consumer.ConvertedTemperature";
        public const string FlowEfficiencyKey = "CentralizedThermalDistribution.Consumer.FlowEfficiencyKey";
        public const string ThermalEfficiencyKey = "CentralizedThermalDistribution.Consumer.ThermalEfficiencyKey";
        public const string DisconnectedKey = "CentralizedThermalDistribution.Consumer.Disconnected";
        public const string ClosedKey = "CentralizedThermalDistribution.Consumer.Closed";

        private bool _alertChange;
        public CoolantPipeColorPriority AirTypePriority = CoolantPipeColorPriority.Auto;

        public float ConvertedTemperature;
        protected CompFlickable FlickableComp;

        public float ExhaustAirFlow => Props.baseAirExhaust;

        public float FlowEfficiency => coolantNet.FlowEfficiency;

        public float ThermalEfficiency => coolantNet.ThermalEfficiency;

        /// <summary>
        ///     Debug String for AirFlow Consumer
        /// </summary>
        public string DebugString
        {
            get
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(parent.LabelCap + " CompAirFlowConsumer:");
                stringBuilder.AppendLine("   ConvertedTemperature: " + ConvertedTemperature);
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        ///     Post Spawn for Component
        /// </summary>
        /// <param name="respawningAfterLoad">Unused Flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            CentralizedThermalDistributionUtility.GetNetManager(parent.Map).RegisterConsumer(this);
            FlickableComp = parent.GetComp<CompFlickable>();

            base.PostSpawnSetup(respawningAfterLoad);
        }

        /// <summary>
        ///     Method called during Game Save/Load
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref AirTypePriority, "airTypePriority", CoolantPipeColorPriority.Auto);
#if DEBUG
            Debug.Log(parent + " - Air Priority Loaded: " + AirTypePriority);
#endif
            _alertChange = true;
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
            if (!FlickableComp.SwitchIsOn)
            {
                return ClosedKey.Translate() + "\n" + base.CompInspectStringExtra();
            }

            if (!IsConnected())
            {
                return base.CompInspectStringExtra();
            }

            if (!IsActive())
            {
                return DisconnectedKey.Translate() + "\n" + base.CompInspectStringExtra();
            }

            //var convertedTemp = ConvertedTemperature.ToStringTemperature("F0");
            //var str = IntakeTempKey.Translate(convertedTemp);
            var str = IntakeTempKey.Translate(ConvertedTemperature.ToStringTemperature("F0") + "\n");

            //var flowPercent = Mathf.FloorToInt(AirFlowNet.FlowEfficiency * 100) + "%";
            //str += "\n";
            //str += FlowEfficiencyKey.Translate(flowPercent);
            str += FlowEfficiencyKey.Translate(Mathf.FloorToInt(coolantNet.FlowEfficiency * 100) + "%" + "\n");

            //var thermalPercent = Mathf.FloorToInt(AirFlowNet.ThermalEfficiency * 100) + "%";
            //str += "\n";
            //str += ThermalEfficiencyKey.Translate(thermalPercent);
            str += ThermalEfficiencyKey.Translate(Mathf.FloorToInt(coolantNet.ThermalEfficiency * 100) + "%" + "\n");

            return str + base.CompInspectStringExtra();
        }

        /// <summary>
        ///     Set the Pipe Priority for Consumers
        /// </summary>
        /// <param name="priority">Priority to Switch to.</param>
        public void SetPriority(CoolantPipeColorPriority priority)
        {
            _alertChange = true;
            AirTypePriority = priority;
            coolantNet = null;
#if DEBUG
            Debug.Log("Setting Priority to: " + AirTypePriority);
#endif
        }

        /// <summary>
        ///     Tick for Consumers. Here:
        ///     - We Rebuild if Priority is Changed
        ///     - We take the Converted Temperature from Climate Units
        /// </summary>
        public void TickRare()
        {
            if (_alertChange)
            {
                //var manager = CentralizedThermalDistributionUtility.GetNetManager(parent.Map);
                //manager.IsDirty = true;

                // Direct access is given, so we should use it  --Brain
                CentralizedThermalDistributionUtility.GetNetManager(parent.Map).IsDirty = true;
                _alertChange = false;
            }

            if (!IsConnected())
            {
                return;
            }

            ConvertedTemperature = coolantNet.AverageConvertedTemperature;
        }

        public override bool IsConnected()
        {
            if (!FlickableComp.SwitchIsOn)
            {
                return false;
            }

            return base.IsConnected();
        }

        /// <summary>
        ///     Reset the Flow Variables and Forward the Control to Base class for more reset.
        /// </summary>
        public override void ResetCoolantVariables()
        {
            ConvertedTemperature = 0.0f;
            base.ResetCoolantVariables();
        }

        /// <summary>
        ///     Check if Consumer Can work.
        ///     This check is used after checking for Power.
        /// </summary>
        /// <returns>Boolean flag to show if Active</returns>
        public bool IsActive()
        {
            if (coolantNet == null)
            {
                return false;
            }

            return coolantNet.Producers.Count != 0 && coolantNet.Consumers.Count != 0;
        }
    }
}