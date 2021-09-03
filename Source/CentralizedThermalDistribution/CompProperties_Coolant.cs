using Verse;

namespace CentralizedThermalDistribution
{
    public class CompProperties_Coolant : CompProperties
    {
        public float baseAirExhaust;

        public float baseAirFlow;

        public CoolantPipeColor flowType;

        public float thermalCapacity;
        public bool transmitsAir;

        public float ThermalWorkMultiplier = 1.00f;
        public float ProviderMaxTemperatureDelta;
    }
}