using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_CoolantPipeRed : Building_CoolantPipe
    {
        public override Graphic Graphic
        {
            get
            {
                if (def.defName == "redAirPipeHidden")
                {
                    return GraphicsLoader.GraphicHotPipeHidden;
                }

                return GraphicsLoader.GraphicHotPipe;
            }
        }
    }
}