using System.Collections.Generic;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompProperties_Coolant : CompProperties
    {
        public CompProperties_Coolant() : base()
        {
            compClass = typeof(CompCoolant);
        }

        public CompCoolant.PipeColor pipeColor = CompCoolant.PipeColor.None;
        public bool pipeIsHidden = false;
        public int thermalMass = 0;

        public bool IsPipe
        {
            get => pipeColor != CompCoolant.PipeColor.None;
        }

        public bool IsProvider
        {
            get => thermalMass > 0;
        }

    }
}