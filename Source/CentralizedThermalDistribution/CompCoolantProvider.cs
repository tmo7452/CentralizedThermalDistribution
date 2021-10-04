using System.Text;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    // Provides coolant flow and influences its temperature. At least one provider must be present on a network.
    public class CompCoolantProvider : CompCoolantTrader
    {
        public bool IsActiveOnNetwork = false; // Active on the coolant network.
        public int CoolantThermalMass;
        public float CoolantTemperature;

        /// <summary>
        ///     Post Spawn for Component
        /// </summary>
        /// <param name="respawningAfterLoad">Unused Flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            coolantNetManager.RegisterProvider(this);
            CoolantThermalMass = Props.thermalMass;
            CoolantTemperature = parent.Position.GetTemperature(parent.Map);

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
        public override float? GetTemp()
        {
            return CoolantTemperature;
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
    }
}