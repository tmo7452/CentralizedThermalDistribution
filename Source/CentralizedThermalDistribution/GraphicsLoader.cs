using Verse;

namespace CentralizedThermalDistribution
{
    [StaticConstructorOnStartup]
    public class GraphicsLoader
    {
        // Actual Atlas
        public static Graphic BlankPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/CoolantPipeBlank_Atlas", ShaderDatabase.Transparent);

        public static Graphic HotPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/CoolantPipeRed_Atlas", ShaderDatabase.Transparent);

        public static Graphic ColdPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/CoolantPipeBlue_Atlas", ShaderDatabase.Transparent);

        public static Graphic FrozenPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/CoolantPipeCyan_Atlas", ShaderDatabase.Transparent);

        // Overlays
        public static Graphic HotPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/CoolantPipeRed_Overlay_Atlas",
                ShaderDatabase.MetaOverlay);

        public static Graphic ColdPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/CoolantPipeBlue_Overlay_Atlas",
                ShaderDatabase.MetaOverlay);

        public static Graphic FrozenPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/CoolantPipeCyan_Overlay_Atlas",
                ShaderDatabase.MetaOverlay);

        public static Graphic AnyPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/CoolantPipeAny_Overlay_Atlas",
                ShaderDatabase.MetaOverlay);

        public static GraphicPipe GraphicHotPipe = new GraphicPipe(HotPipeAtlas, CoolantPipeColor.Red);
        public static GraphicPipe GraphicHotPipeHidden = new GraphicPipe(BlankPipeAtlas, CoolantPipeColor.Red);
        public static GraphicPipe GraphicColdPipe = new GraphicPipe(ColdPipeAtlas, CoolantPipeColor.Blue);
        public static GraphicPipe GraphicColdPipeHidden = new GraphicPipe(BlankPipeAtlas, CoolantPipeColor.Blue);
        public static GraphicPipe GraphicFrozenPipe = new GraphicPipe(FrozenPipeAtlas, CoolantPipeColor.Cyan);
        public static GraphicPipe GraphicFrozenPipeHidden = new GraphicPipe(BlankPipeAtlas, CoolantPipeColor.Cyan);

        public static GraphicPipe_Overlay GraphicHotPipeOverlay =
            new GraphicPipe_Overlay(HotPipeOverlayAtlas, AnyPipeOverlayAtlas, CoolantPipeColor.Red);

        public static GraphicPipe_Overlay GraphicColdPipeOverlay =
            new GraphicPipe_Overlay(ColdPipeOverlayAtlas, AnyPipeOverlayAtlas, CoolantPipeColor.Blue);

        public static GraphicPipe_Overlay GraphicFrozenPipeOverlay =
            new GraphicPipe_Overlay(FrozenPipeOverlayAtlas, AnyPipeOverlayAtlas, CoolantPipeColor.Cyan);
    }
}