using Verse;

namespace CentralizedThermalDistribution
{
    // Member of a coolant network
    public class CompCoolant : ThingComp
    {
        public enum PipeColor
        {
            Trader = -1,
            None = 0,
            Red = 1,
            Blue = 2,
            Cyan = 3,
            Green = 4,
        }
        public const int PipeColorCount = 4;
        public static int PipeColorToIndex(PipeColor color)
        {
            return (int)color - 1;
        }
        public static PipeColor PipeColorFromIndex(int index)
        {
            return (PipeColor)(index + 1);
        }

        public const string NotConnectedKey = "CentralizedThermalDistribution.AirFlowNetDisconnected";
        public const string ConnectedKey = "CentralizedThermalDistribution.AirFlowNetConnected";
        public const string AirTypeKey = "CentralizedThermalDistribution.AirType";
        public const string HotAirKey = "CentralizedThermalDistribution.HotAir";
        public const string ColdAirKey = "CentralizedThermalDistribution.ColdAir";
        public const string FrozenAirKey = "CentralizedThermalDistribution.FrozenAir";
        public const string TotalNetworkAirKey = "CentralizedThermalDistribution.TotalNetworkAir";

        public PipeColor BasePipeColor => Props.flowType;
        public PipeColor CurrentPipeColor { get; private set; } = PipeColor.None;

        public uint NetID { get; private set; } = 0;

        public CoolantNet coolantNet { get; private set; }

        public CompProperties_Coolant Props => (CompProperties_Coolant)props;

        public virtual void SetNet(CoolantNet newNet)
        {
            coolantNet = newNet;
            NetID = newNet == null ? 0 : newNet.NetID;
        }

        /// <summary>
        ///     Building de-spawned from the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            SetNet(null);
            base.PostDeSpawn(map);
        }

        public virtual void SetPipeColor(PipeColor newColor)
        {
            CurrentPipeColor = newColor;
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
                output = CurrentPipeColor + " net coolant temp: " + coolantNet.GetNetCoolantTemperature();
            else
                output = CurrentPipeColor + " net inactive";


            //res += "\n";
            //res += TotalNetworkAirKey.Translate(coolantNet.CurrentIntakeAir);

            return output;
        }

        /// <summary>
        ///     Print the Component for Overlay Grid
        /// </summary>
        /// <param name="layer">Section Layer that is being Printed</param>
        /// <param name="type">AirFlow Type</param>
        public void PrintForGrid(SectionLayer layer, PipeColor type)
        {
            switch (type)
            {
                case PipeColor.Red:
                    GraphicsLoader.GraphicHotPipeOverlay.Print(layer, parent, 0);
                    break;

                case PipeColor.Blue:
                    GraphicsLoader.GraphicColdPipeOverlay.Print(layer, parent, 0);
                    break;

                case PipeColor.Cyan:
                    GraphicsLoader.GraphicFrozenPipeOverlay.Print(layer, parent, 0);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        ///     Get the Type of Air as String. Hot Cold Frozen etc.
        /// </summary>
        /// <param name="type">Enum for AirFlow Type</param>
        /// <returns>Translated String</returns>
        protected string GetPipeColorString(PipeColor type)
        {
            var res = "";
            switch (type)
            {
                case PipeColor.Blue:
                    res += AirTypeKey.Translate(ColdAirKey.Translate());
                    break;

                case PipeColor.Red:
                    res += AirTypeKey.Translate(HotAirKey.Translate());
                    break;

                case PipeColor.Cyan:
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