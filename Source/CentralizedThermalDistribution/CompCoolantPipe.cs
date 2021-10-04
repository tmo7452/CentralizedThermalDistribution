//using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompCoolantPipe : CompCoolant
    {
        /// <summary>
        ///     Component spawned on the map
        /// </summary>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            SetPipeColor(BasePipeColor); // Pipes spawn with a preset color
        }

        /// <summary>
        ///     Building de-spawned from the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            // SetPipeColor(PipeColor.None); // Can't use this because it assumes parent.map is valid.
            coolantNetManager.DeregisterPipe(this);
            base.SetPipeColor(PipeColor.None);
            base.PostDeSpawn(map);
        }

        public override void SetPipeColor(PipeColor newColor)
        {
            if (CurrentPipeColor != PipeColor.None)
            {
                coolantNetManager.DeregisterPipe(this);
            }
            base.SetPipeColor(newColor);
            if (CurrentPipeColor != PipeColor.None)
            {
                coolantNetManager.RegisterPipe(this);
            }

            if (parent.Graphic is Graphic_Wrapper_CoolantPipe)
            {
                (parent.Graphic as Graphic_Wrapper_CoolantPipe).pipeColor = BasePipeColor;
            }
        }

        /// <summary>
        ///     Look up the coolant network for this pipe.
        /// </summary>
        /// <returns>CoolantNet its connected to</returns>
        public CoolantNet GetNet()
        {
            return coolantNetManager.GetNet(NetID);
        }

        /*protected override void CoolantTick(int tickMultiplier)
        {
            return;
        }*/
    }
}