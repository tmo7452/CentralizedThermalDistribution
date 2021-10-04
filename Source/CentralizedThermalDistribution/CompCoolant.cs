using Verse;

namespace CentralizedThermalDistribution
{
    // Member of a coolant network
    public abstract class CompCoolant : ThingComp
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

        public PipeColor BasePipeColor => Props.pipeColor;
        public PipeColor CurrentPipeColor { get; private set; } = PipeColor.None;

        public bool IsConnected => coolantNet != null;
        public uint NetID { get; private set; } = 0;

        public CoolantNetManager coolantNetManager { get; private set; }
        public CoolantNet coolantNet { get; private set; }

        public CompProperties_Coolant Props => (CompProperties_Coolant)props;

        public virtual void SetNet(CoolantNet newNet)
        {
            coolantNet = newNet;
            NetID = newNet == null ? 0 : newNet.NetID;
        }

        /// <summary>
        ///     Post Spawn for Component
        /// </summary>
        /// <param name="respawningAfterLoad">Unused Flag</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            coolantNetManager = parent.Map.GetComponent<CoolantNetManager>();
        }

        /// <summary>
        ///     Building de-spawned from the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        public override void PostDeSpawn(Map map)
        {
            SetNet(null);
            coolantNetManager = null;
            base.PostDeSpawn(map);
        }

        public virtual void SetPipeColor(PipeColor newColor)
        {
            CurrentPipeColor = newColor;
        }

        /// <summary>
        ///     Inspect Component String
        /// </summary>
        /// <returns>String to be Displayed on the Component window</returns>
        public override string CompInspectStringExtra()
        {
            System.Text.StringBuilder output = new();
            output.AppendLine(base.CompInspectStringExtra());

            if (!IsConnected)
            {
                output.AppendLine(NotConnectedKey.Translate());
            }
            else
            {
                if (coolantNet.IsNetActive())
                    output.AppendLine("DEBUG " + CurrentPipeColor + " net coolant temp: " + coolantNet.GetNetCoolantTemperature());
                else
                    output.AppendLine("DEBUG " + CurrentPipeColor + " net inactive");
            }
            return output.ToString().Trim();
        }

        /// <summary>
        ///     Print the Component for Overlay Grid
        /// </summary>
        /// <param name="layer">Section Layer that is being Printed</param>
        /// <param name="type">AirFlow Type</param>
        public void PrintOverlay(SectionLayer layer)
        {
            switch (BasePipeColor)
            {
                case PipeColor.Red:
                    GraphicsLoader.GraphicRedPipeOverlay.Print(layer, parent, 0);
                    break;

                case PipeColor.Blue:
                    GraphicsLoader.GraphicBluePipeOverlay.Print(layer, parent, 0);
                    break;

                case PipeColor.Cyan:
                    GraphicsLoader.GraphicCyanPipeOverlay.Print(layer, parent, 0);
                    break;

                case PipeColor.Green:
                    GraphicsLoader.GraphicGreenPipeOverlay.Print(layer, parent, 0);
                    break;

                case PipeColor.Trader:
                    GraphicsLoader.GraphicTraderPipeOverlay.Print(layer, parent, 0);
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
            return type switch
            {
                PipeColor.Blue => AirTypeKey.Translate(ColdAirKey.Translate()),
                PipeColor.Red => AirTypeKey.Translate(HotAirKey.Translate()),
                PipeColor.Cyan => AirTypeKey.Translate(FrozenAirKey.Translate()),
                PipeColor.Green => "Green Pipe",
                _ => AirTypeKey.Translate("Unknown"),
            };
        }
    }
}