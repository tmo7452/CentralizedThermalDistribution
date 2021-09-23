using Verse;

namespace CentralizedThermalDistribution
{
    [StaticConstructorOnStartup]
    public class GraphicsLoader
    {
        // Overlays
        public static Graphic_Linked_CoolantPipe GraphicTraderPipeOverlay = new(GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeTrader_Overlay_Atlas", ShaderDatabase.MetaOverlay), CompCoolant.PipeColor.Trader);
        public static Graphic_Linked_CoolantPipe GraphicRedPipeOverlay = new(GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeRed_Overlay_Atlas", ShaderDatabase.MetaOverlay), CompCoolant.PipeColor.Red);
        public static Graphic_Linked_CoolantPipe GraphicBluePipeOverlay = new(GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeBlue_Overlay_Atlas", ShaderDatabase.MetaOverlay), CompCoolant.PipeColor.Blue);
        public static Graphic_Linked_CoolantPipe GraphicCyanPipeOverlay = new(GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeCyan_Overlay_Atlas", ShaderDatabase.MetaOverlay), CompCoolant.PipeColor.Cyan);
        public static Graphic_Linked_CoolantPipe GraphicGreenPipeOverlay = new(GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeGreen_Overlay_Atlas", ShaderDatabase.MetaOverlay), CompCoolant.PipeColor.Green);
    }
}