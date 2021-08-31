using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_CoolantPipeCyan : Building_CoolantPipe
    {
        public override Graphic Graphic
        {
            get
            {
                if (def.defName == "cyanAirPipeHidden")
                {
                    return GraphicsLoader.GraphicFrozenPipeHidden;
                }

                return GraphicsLoader.GraphicFrozenPipe;
            }
        }
    }
}