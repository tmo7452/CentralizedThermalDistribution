using Verse;

namespace CentralizedThermalDistribution
{
    [StaticConstructorOnStartup]
    public class GraphicsLoader
    {
        // Actual Atlas
        public static Graphic BlankPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeBlank_Atlas", ShaderDatabase.Transparent);

        public static Graphic RedPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeRed_Atlas", ShaderDatabase.Transparent);

        public static Graphic BluePipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeBlue_Atlas", ShaderDatabase.Transparent);

        public static Graphic CyanPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeCyan_Atlas", ShaderDatabase.Transparent);

        public static Graphic GreenPipeAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeGreen_Atlas", ShaderDatabase.Transparent);

        // Overlays
        public static Graphic RedPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeRed_Overlay_Atlas", ShaderDatabase.MetaOverlay);

        public static Graphic BluePipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeBlue_Overlay_Atlas", ShaderDatabase.MetaOverlay);

        public static Graphic CyanPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeCyan_Overlay_Atlas", ShaderDatabase.MetaOverlay);

        public static Graphic GreenPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeGreen_Overlay_Atlas", ShaderDatabase.MetaOverlay);

        public static Graphic AnyPipeOverlayAtlas =
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Pipes/CoolantPipeAny_Overlay_Atlas", ShaderDatabase.MetaOverlay);

        public static GraphicPipe GraphicRedPipe = new GraphicPipe(RedPipeAtlas, CompCoolant.PipeColor.Red);
        public static GraphicPipe GraphicRedPipeHidden = new GraphicPipe(BlankPipeAtlas, CompCoolant.PipeColor.Red);
        public static GraphicPipe GraphicBluePipe = new GraphicPipe(BluePipeAtlas, CompCoolant.PipeColor.Blue);
        public static GraphicPipe GraphicBluePipeHidden = new GraphicPipe(BlankPipeAtlas, CompCoolant.PipeColor.Blue);
        public static GraphicPipe GraphicCyanPipe = new GraphicPipe(CyanPipeAtlas, CompCoolant.PipeColor.Cyan);
        public static GraphicPipe GraphicCyanPipeHidden = new GraphicPipe(BlankPipeAtlas, CompCoolant.PipeColor.Cyan);
        public static GraphicPipe GraphicGreenPipe = new GraphicPipe(GreenPipeAtlas, CompCoolant.PipeColor.Green);
        public static GraphicPipe GraphicGreenPipeHidden = new GraphicPipe(BlankPipeAtlas, CompCoolant.PipeColor.Green);

        public static GraphicPipe_Overlay GraphicRedPipeOverlay =
            new GraphicPipe_Overlay(RedPipeOverlayAtlas, AnyPipeOverlayAtlas, CompCoolant.PipeColor.Red);

        public static GraphicPipe_Overlay GraphicBluePipeOverlay =
            new GraphicPipe_Overlay(BluePipeOverlayAtlas, AnyPipeOverlayAtlas, CompCoolant.PipeColor.Blue);

        public static GraphicPipe_Overlay GraphicCyanPipeOverlay =
            new GraphicPipe_Overlay(CyanPipeOverlayAtlas, AnyPipeOverlayAtlas, CompCoolant.PipeColor.Cyan);

        public static GraphicPipe_Overlay GraphicGreenPipeOverlay =
            new GraphicPipe_Overlay(GreenPipeOverlayAtlas, AnyPipeOverlayAtlas, CompCoolant.PipeColor.Green);
    }
}