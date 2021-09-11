using Verse;

namespace CentralizedThermalDistribution
{
    public class CompProperties_Coolant : CompProperties
    {
        public float baseAirExhaust;

        public float baseAirFlow;

        public CompCoolant.PipeColor flowType;

        public float thermalCapacity;
        public bool transmitsAir;

        public float ThermalWorkMultiplier = 1.00f;
        public float ProviderMaxTemperatureDelta;
        public int ProviderCoolantThermalMass = 20;
        public float ProviderLowFuelConsumptionFactor = 0.1f;
    }
}