using Verse;

namespace CentralizedThermalDistribution
{
    [StaticConstructorOnStartup]
    public class GraphicsLoader
    {
        // Actual Atlas
        public static Graphic BlankPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeBlank_Atlas", ShaderDatabase.Transparent);

        public static Graphic HotPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeRed_Atlas", ShaderDatabase.Transparent);

        public static Graphic ColdPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeBlue_Atlas", ShaderDatabase.Transparent);

        public static Graphic FrozenPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeCyan_Atlas", ShaderDatabase.Transparent);

        // Overlays
        public static Graphic HotPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeRed_Overlay_Atlas",
                ShaderDatabase.MetaOverlay);

        public static Graphic ColdPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeBlue_Overlay_Atlas",
                ShaderDatabase.MetaOverlay);

        public static Graphic FrozenPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeCyan_Overlay_Atlas",
                ShaderDatabase.MetaOverlay);

        public static Graphic AnyPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeAny_Overlay_Atlas",
                ShaderDatabase.MetaOverlay);

        public static GraphicPipe GraphicHotPipe = new GraphicPipe(HotPipeAtlas, CompCoolant.PipeColor.Red);
        public static GraphicPipe GraphicHotPipeHidden = new GraphicPipe(BlankPipeAtlas, CompCoolant.PipeColor.Red);
        public static GraphicPipe GraphicColdPipe = new GraphicPipe(ColdPipeAtlas, CompCoolant.PipeColor.Blue);
        public static GraphicPipe GraphicColdPipeHidden = new GraphicPipe(BlankPipeAtlas, CompCoolant.PipeColor.Blue);
        public static GraphicPipe GraphicFrozenPipe = new GraphicPipe(FrozenPipeAtlas, CompCoolant.PipeColor.Cyan);
        public static GraphicPipe GraphicFrozenPipeHidden = new GraphicPipe(BlankPipeAtlas, CompCoolant.PipeColor.Cyan);

        public static GraphicPipe_Overlay GraphicHotPipeOverlay =
            new GraphicPipe_Overlay(HotPipeOverlayAtlas, AnyPipeOverlayAtlas, CompCoolant.PipeColor.Red);

        public static GraphicPipe_Overlay GraphicColdPipeOverlay =
            new GraphicPipe_Overlay(ColdPipeOverlayAtlas, AnyPipeOverlayAtlas, CompCoolant.PipeColor.Blue);

        public static GraphicPipe_Overlay GraphicFrozenPipeOverlay =
            new GraphicPipe_Overlay(FrozenPipeOverlayAtlas, AnyPipeOverlayAtlas, CompCoolant.PipeColor.Cyan);
    }
}