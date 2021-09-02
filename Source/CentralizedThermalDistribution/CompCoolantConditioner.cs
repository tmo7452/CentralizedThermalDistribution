using System.Text;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    // Provides coolant flow and influences its temperature. At least one conditioner must be present on a network.
    public class CompCoolantConditioner : CompCoolant
    {
        public const string AirFlowOutputKey = "CentralizedThermalDistribution.AirFlowOutput";
        public const string IntakeTempKey = "CentralizedThermalDistribution.Producer.IntakeTemperature";
        public const string IntakeBlockedKey = "CentralizedThermalDistribution.Producer.IntakeBlocked";

        public bool ActiveOnNetwork = false; // Active on the coolant network.
        public float CoolantThermalMass;
        public float CoolantTemperature;
        public float CurrentEnergyDelta = 0f;

        /// <summary>
        ///     Debug String for a Air Flow Producer
        ///     Shows info about Air Flow etc.
        /// </summary>
        public string DebugString
        {
            get
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(parent.LabelCap + " CompAirFlow:");
                stringBuilder.AppendLine("   AirFlow IsOperating: " + IsConnected());
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        ///     Post Spawn for Component
        /// </summary>
        /// <param name="respawningAfterLoad">Unused Flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            CentralizedThermalDistributionUtility.GetNetManager(parent.Map).RegisterConditioner(this);
            base.PostSpawnSetup(respawningAfterLoad);
        }

        /// <summary>
        ///     Despawn Event for a Producer Component
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            CentralizedThermalDistributionUtility.GetNetManager(map).DeregisterConditioner(this);
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
        ///     Tick for Climate Control
        ///     Here we calculate the growth of Delta Temperature which is increased or decrased based on Intake and Target
        ///     Temperature.
        /// </summary>
        public void TickRare()
        {
            CoolantTemperature += CurrentEnergyDelta / CoolantThermalMass;
        }
    }
}