using Verse;

namespace CentralizedThermalDistribution
{
    public class CompProperties_CoolantFlow : CompProperties
    {
        public float baseAirExhaust;

        public float baseAirFlow;

        public CoolantPipeColor flowType;

        public float thermalCapacity;
        public bool transmitsAir;
    }
}