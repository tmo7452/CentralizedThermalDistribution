using Verse;

namespace CentralizedThermalDistribution
{
    // Member of a coolant network
    public class CompCoolant : ThingComp
    {
        public const string NotConnectedKey = "CentralizedThermalDistribution.AirFlowNetDisconnected";
        public const string ConnectedKey = "CentralizedThermalDistribution.AirFlowNetConnected";
        public const string AirTypeKey = "CentralizedThermalDistribution.AirType";
        public const string HotAirKey = "CentralizedThermalDistribution.HotAir";
        public const string ColdAirKey = "CentralizedThermalDistribution.ColdAir";
        public const string FrozenAirKey = "CentralizedThermalDistribution.FrozenAir";
        public const string TotalNetworkAirKey = "CentralizedThermalDistribution.TotalNetworkAir";

        public CoolantPipeColor pipeColor => Props.flowType;

        public int NetID { get; set; } = -2;

        public CoolantNet coolantNet { get; set; }

        public CompProperties_Coolant Props => (CompProperties_Coolant)props;

        /// <summary>
        ///     Reset the AirFlow Variables
        /// </summary>
        public virtual void ResetCoolantVariables()
        {
            coolantNet = null;
            NetID = -1;
        }

        /// <summary>
        ///     Component spawned on the map
        /// </summary>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            CentralizedThermalDistributionUtility.GetNetManager(parent.Map).RegisterPipe(this);
            base.PostSpawnSetup(respawningAfterLoad);
        }

        /// <summary>
        ///     Building de-spawned from the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            CentralizedThermalDistributionUtility.GetNetManager(map).DeregisterPipe(this);
            ResetCoolantVariables();

            base.PostDeSpawn(map);
        }

        /// <summary>
        ///     Check if connected to coolant network.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConnected()
        {
            return coolantNet != null;
        }

        /// <summary>
        ///     Inspect Component String
        /// </summary>
        /// <returns>String to be Displayed on the Component window</returns>
        public override string CompInspectStringExtra()
        {
            if (!IsConnected())
            {
                return NotConnectedKey.Translate();
            }

            string output; // = ConnectedKey.Translate();

            //if (pipeColor != CoolantPipeColor.Any)
            //{
            //    output += "\n";
            //    output += GetAirTypeString(pipeColor);
            //}

            if (coolantNet.IsNetActive())
                output = pipeColor + " net coolant temp: " + coolantNet.GetNetCoolantTemperature();
            else
                output = pipeColor + " net inactive";


            //res += "\n";
            //res += TotalNetworkAirKey.Translate(coolantNet.CurrentIntakeAir);

            return output;
        }

        /// <summary>
        ///     Print the Component for Overlay Grid
        /// </summary>
        /// <param name="layer">Section Layer that is being Printed</param>
        /// <param name="type">AirFlow Type</param>
        public void PrintForGrid(SectionLayer layer, CoolantPipeColor type)
        {
            switch (type)
            {
                case CoolantPipeColor.Red:
                    GraphicsLoader.GraphicHotPipeOverlay.Print(layer, parent, 0);
                    break;

                case CoolantPipeColor.Blue:
                    GraphicsLoader.GraphicColdPipeOverlay.Print(layer, parent, 0);
                    break;

                case CoolantPipeColor.Cyan:
                    GraphicsLoader.GraphicFrozenPipeOverlay.Print(layer, parent, 0);
                    break;

                case CoolantPipeColor.Any:
                    break;
            }
        }

        /// <summary>
        ///     Get the Type of Air as String. Hot Cold Frozen etc.
        /// </summary>
        /// <param name="type">Enum for AirFlow Type</param>
        /// <returns>Translated String</returns>
        protected string GetAirTypeString(CoolantPipeColor type)
        {
            var res = "";
            switch (type)
            {
                case CoolantPipeColor.Blue:
                    res += AirTypeKey.Translate(ColdAirKey.Translate());
                    break;

                case CoolantPipeColor.Red:
                    res += AirTypeKey.Translate(HotAirKey.Translate());
                    break;

                case CoolantPipeColor.Cyan:
                    res += AirTypeKey.Translate(FrozenAirKey.Translate());
                    break;

                default:
                    res += AirTypeKey.Translate("Unknown");
                    break;
            }

            return res;
        }
    }
}