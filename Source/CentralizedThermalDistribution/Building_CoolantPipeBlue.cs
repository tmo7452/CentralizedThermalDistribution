using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_CoolantPipeBlue : Building_CoolantPipe
    {
        public override Graphic Graphic
        {
            get
            {
                if (def.defName == "blueAirPipeHidden")
                {
                    return GraphicsLoader.GraphicColdPipeHidden;
                }

                return GraphicsLoader.GraphicColdPipe;
            }
        }
    }
}