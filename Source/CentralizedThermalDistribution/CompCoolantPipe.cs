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
            CentralizedThermalDistributionUtility.GetNetManager(map).DeregisterPipe(this);
            base.SetPipeColor(PipeColor.None);
            base.PostDeSpawn(map);
        }

        public override void SetPipeColor(PipeColor newColor)
        {
            if (CurrentPipeColor != PipeColor.None)
            {
                CentralizedThermalDistributionUtility.GetNetManager(parent.Map).DeregisterPipe(this);
            }
            base.SetPipeColor(newColor);
            if (CurrentPipeColor != PipeColor.None)
            {
                CentralizedThermalDistributionUtility.GetNetManager(parent.Map).RegisterPipe(this);
            }

            if (parent.Graphic is Graphic_Wrapper_CoolantPipe)
            {
                (parent.Graphic as Graphic_Wrapper_CoolantPipe).pipeColor = BasePipeColor;
            }
        }

        /*
        /// <summary>
        ///     Component Inspection for Pipes
        /// </summary>
        /// <returns>String to Display for Pipes</returns>
        public override string CompInspectStringExtra()
        {
            string output = GetAirTypeString(Props.flowType);
            output += "\nConnected: " + IsConnected();
            output += "\nNet ID: " + NetID;
            var net = GetNet();
            if (net != null)
            {
                output += "\nNet Coolant Temp: " + net.GetNetCoolantTemperature();
            }
            return output;
        }
        */
        /// <summary>
        ///     Look up the coolant network for this pipe.
        /// </summary>
        /// <returns>CoolantNet its connected to</returns>
        public CoolantNet GetNet()
        {
            return CentralizedThermalDistributionUtility.GetNetManager(parent.Map).GetNet(NetID);
        }

        protected override void CoolantTick(int tickMultiplier)
        {
            return;
        }
    }
}