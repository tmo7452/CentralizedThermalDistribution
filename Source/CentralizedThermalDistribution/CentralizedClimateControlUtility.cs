using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public static class CentralizedThermalDistributionUtility
    {
        private const string SwitchPipeAutoKey = "CentralizedThermalDistribution.Command.SwitchPipe.Auto";
        private const string SwitchPipeRedKey = "CentralizedThermalDistribution.Command.SwitchPipe.Red";
        private const string SwitchPipeBlueKey = "CentralizedThermalDistribution.Command.SwitchPipe.Blue";
        private const string SwitchPipeCyanKey = "CentralizedThermalDistribution.Command.SwitchPipe.Cyan";

        /// <summary>
        ///     Get the Network Manager of the Map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <returns>AirFlow Net Manager</returns>
        public static CoolantNetManager GetNetManager(Map map)
        {
            return map.GetComponent<CoolantNetManager>();
        }

        /// <summary>
        ///     Gizmo for Changing Pipes
        /// </summary>
        /// <param name="compAirFlowConsumer">Component Asking for Gizmo</param>
        /// <returns>Action Button Gizmo</returns>
        public static Command_Action GetPipeSwitchToggle(CompCoolantConsumer compAirFlowConsumer)
        {
            var currentPriority = compAirFlowConsumer.AirTypePriority;
            Texture2D icon;
            string label;

            switch (currentPriority)
            {
                
                case CoolantPipeColorPriority.Red:
                    label = SwitchPipeRedKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Red");
                    break;

                case CoolantPipeColorPriority.Blue:
                    label = SwitchPipeBlueKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Blue");
                    break;

                case CoolantPipeColorPriority.Cyan:
                    label = SwitchPipeCyanKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Cyan");
                    break;

                case CoolantPipeColorPriority.Auto:
                default:
                    label = SwitchPipeAutoKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Auto");
                    break;
            }

            return new Command_Action
            {
                defaultLabel = label,
                defaultDesc = "CentralizedThermalDistribution.Command.SwitchPipe.Desc".Translate(),
                hotKey = KeyBindingDefOf.Misc4,
                icon = icon,
                action = delegate
                {
                    switch (currentPriority)
                    {
                        case CoolantPipeColorPriority.Auto:
                            compAirFlowConsumer.SetPriority(CoolantPipeColorPriority.Red);
                            break;

                        case CoolantPipeColorPriority.Red:
                            compAirFlowConsumer.SetPriority(CoolantPipeColorPriority.Blue);
                            break;

                        case CoolantPipeColorPriority.Blue:
                            compAirFlowConsumer.SetPriority(CoolantPipeColorPriority.Cyan);
                            break;
                        case CoolantPipeColorPriority.Cyan:
                            compAirFlowConsumer.SetPriority(CoolantPipeColorPriority.Auto);
                            break;

                        default:
                            compAirFlowConsumer.SetPriority(CoolantPipeColorPriority.Auto);
                            break;
                    }
                }
            };
        }
    }
}