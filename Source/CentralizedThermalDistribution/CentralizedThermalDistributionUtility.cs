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
        private const string SwitchPipeGreenKey = "CentralizedThermalDistribution.Command.SwitchPipe.Green";

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
        /// <param name="compCoolant">Component Asking for Gizmo</param>
        /// <returns>Action Button Gizmo</returns>
        public static Command_Action GetPipeSwitchToggle(CompCoolantTrader compCoolant)
        {
            var currentSelection = compCoolant.pipeColorSelection;
            Texture2D icon;
            string label;

            switch (currentSelection)
            {
                
                case CompCoolantTrader.PipeColorSelection.Red:
                    label = SwitchPipeRedKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Red");
                    break;

                case CompCoolantTrader.PipeColorSelection.Blue:
                    label = SwitchPipeBlueKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Blue");
                    break;

                case CompCoolantTrader.PipeColorSelection.Cyan:
                    label = SwitchPipeCyanKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Cyan");
                    break;

                case CompCoolantTrader.PipeColorSelection.Green:
                    label = SwitchPipeGreenKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Green");
                    break;

                case CompCoolantTrader.PipeColorSelection.Auto:
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
                    switch (currentSelection)
                    {
                        case CompCoolantTrader.PipeColorSelection.Auto:
                            compCoolant.SetSelection(CompCoolantTrader.PipeColorSelection.Red);
                            break;

                        case CompCoolantTrader.PipeColorSelection.Red:
                            compCoolant.SetSelection(CompCoolantTrader.PipeColorSelection.Blue);
                            break;

                        case CompCoolantTrader.PipeColorSelection.Blue:
                            compCoolant.SetSelection(CompCoolantTrader.PipeColorSelection.Cyan);
                            break;

                        case CompCoolantTrader.PipeColorSelection.Cyan:
                            compCoolant.SetSelection(CompCoolantTrader.PipeColorSelection.Green);
                            break;

                        case CompCoolantTrader.PipeColorSelection.Green:
                        default:
                            compCoolant.SetSelection(CompCoolantTrader.PipeColorSelection.Auto);
                            break;
                    }
                }
            };
        }

        /// <summary>
        ///     Gizmo for changing condenser minimum efficiency setting.
        /// </summary>
        /// <param name="compCoolant">Component Asking for Gizmo</param>
        /// <returns>Action Button Gizmo</returns>
        public static Command_Action GetMinEfficiencyToggle(CompCoolantProviderAirSourceCondenser compCondenser)
        {
            var currentSelection = compCondenser.minEfficiencySelection;
            Texture2D icon;
            string label;

            switch (currentSelection)
            {

                case CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E10:
                    //label = SwitchPipeRedKey.Translate();
                    label = "Efficiency Cutoff: 10%";
                    icon = ContentFinder<Texture2D>.Get("UI/MinEfficiencySelect_E10");
                    break;

                case CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E25:
                default:
                    //label = SwitchPipeBlueKey.Translate();
                    label = "Efficiency Cutoff: 25%";
                    icon = ContentFinder<Texture2D>.Get("UI/MinEfficiencySelect_E25");
                    break;

                case CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E50:
                    //label = SwitchPipeCyanKey.Translate();
                    label = "Efficiency Cutoff: 50%";
                    icon = ContentFinder<Texture2D>.Get("UI/MinEfficiencySelect_E50");
                    break;

                case CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E80:
                    //label = SwitchPipeAutoKey.Translate();
                    label = "Efficiency Cutoff: 80%";
                    icon = ContentFinder<Texture2D>.Get("UI/MinEfficiencySelect_E80");
                    break;
            }

            return new Command_Action
            {
                defaultLabel = label,
                defaultDesc = "CentralizedThermalDistribution.Command.SwitchPipe.Desc".Translate(),
                //hotKey = KeyBindingDefOf.Misc4,
                icon = icon,
                action = delegate
                {
                    switch (currentSelection)
                    {
                        case CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E10:
                            compCondenser.minEfficiencySelection = CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E25;
                            break;

                        case CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E25:
                        default:
                            compCondenser.minEfficiencySelection = CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E50;
                            break;

                        case CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E50:
                            compCondenser.minEfficiencySelection = CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E80;
                            break;

                        case CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E80:
                            compCondenser.minEfficiencySelection = CompCoolantProviderAirSourceCondenser.MinEfficiencySelection.E10;
                            break;
                    }
                }
            };
        }

    }
}