using Verse;

namespace CentralizedThermalDistribution
{
    public class CompProperties_Coolant : CompProperties
    {

        public CompCoolant.PipeColor pipeColor = CompCoolant.PipeColor.None;
        public bool pipeIsHidden = false;

        public float ThermalWorkMultiplier = 1.00f;
        public float ProviderMaxTemperatureDelta;
        public int ProviderCoolantThermalMass = 20;
        public float ProviderLowFuelConsumptionFactor = 0.1f;
    }
}