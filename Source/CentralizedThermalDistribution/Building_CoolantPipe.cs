using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_CoolantPipe : Building
    {
        private Graphic activeGraphic;

        /// <summary>
        ///     Building spawned on the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var props = GetComp<CompCoolantPipe>().Props;
            if (props.pipeIsHidden)
            {
                activeGraphic = props.pipeColor switch
                {
                    CompCoolant.PipeColor.Red => GraphicsLoader.GraphicRedPipeHidden,
                    CompCoolant.PipeColor.Blue => GraphicsLoader.GraphicBluePipeHidden,
                    CompCoolant.PipeColor.Cyan => GraphicsLoader.GraphicCyanPipeHidden,
                    CompCoolant.PipeColor.Green => GraphicsLoader.GraphicGreenPipeHidden,
                    _ => throw new System.NotImplementedException(),
                };
            }
            else
            {
                activeGraphic = props.pipeColor switch
                {
                    CompCoolant.PipeColor.Red => GraphicsLoader.GraphicRedPipe,
                    CompCoolant.PipeColor.Blue => GraphicsLoader.GraphicBluePipe,
                    CompCoolant.PipeColor.Cyan => GraphicsLoader.GraphicCyanPipe,
                    CompCoolant.PipeColor.Green => GraphicsLoader.GraphicGreenPipe,
                    _ => throw new System.NotImplementedException(),
                };
            }
        }

        public override Graphic Graphic // This is currently neccesary to prevent parallel pipes of different colors from visually connecting.
        {
            get
            {
                return activeGraphic;
            }
        }
    }
}