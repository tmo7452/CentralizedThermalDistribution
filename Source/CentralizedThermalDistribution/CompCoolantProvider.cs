using System.Text;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    // Provides coolant flow and influences its temperature. At least one provider must be present on a network.
    public class CompCoolantProvider : CompCoolantSwitchable
    {
        public const string AirFlowOutputKey = "CentralizedThermalDistribution.AirFlowOutput";
        public const string IntakeTempKey = "CentralizedThermalDistribution.Producer.IntakeTemperature";
        public const string IntakeBlockedKey = "CentralizedThermalDistribution.Producer.IntakeBlocked";

        public bool ActiveOnNetwork = false; // Active on the coolant network.
        public float CoolantThermalMass;
        public float CoolantTemperature;

        /// <summary>
        ///     Post Spawn for Component
        /// </summary>
        /// <param name="respawningAfterLoad">Unused Flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            CentralizedThermalDistributionUtility.GetNetManager(parent.Map).RegisterProvider(this);
            base.PostSpawnSetup(respawningAfterLoad);
        }

        /// <summary>
        ///     Despawn Event for a Producer Component
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            CentralizedThermalDistributionUtility.GetNetManager(map).DeregisterProvider(this);
            ResetCoolantVariables();
            base.PostDeSpawn(map);
        }

        /// <summary>
        ///     Extra Component Inspection string
        /// </summary>
        /// <returns>String Containing information for Producers</returns>
        public override string CompInspectStringExtra()
        {
            
            var str = "";
            /*
            if (IsPoweredOff || IsBrokenDown)
            {
                return null;
            }

            if (IsBlocked)
            {
                str += IntakeBlockedKey.Translate();
                return str;
            }

            if (!IsConnected())
            {
                return str + base.CompInspectStringExtra();
            }

            str += IntakeTempKey.Translate(IntakeTemperature.ToStringTemperature("F0")) + "\n";
            */
            return str + base.CompInspectStringExtra();
            
        }

        /// <summary>
        ///     Check if Temperature Control is active or not. Needs Consumers and shouldn't be Blocked
        /// </summary>
        /// <returns>Boolean Active State</returns>
        public bool IsActive()
        {
            return ActiveOnNetwork;
        }

        /// <summary>
        ///     Provided a thermal load value, this adjusts the internal coolant temp appropriately.
        ///     Positive load to heat, negative load to cool.
        /// </summary>
        /// <param name="ThermalLoad">Float amount Thermal Load to apply</param>
        public void PushThermalLoad(float ThermalLoad)
        {
            CoolantTemperature += ThermalLoad / CoolantThermalMass;
        }
    }
}