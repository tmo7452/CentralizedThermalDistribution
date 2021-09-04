using System.Linq;
using RimWorld;
using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_ResistiveHeater : Building_TempControl
    {
        public enum Status
        {
            Offline,
            TemperatureReached,
            Working,
        }

        public CompCoolantProvider compCoolant;

        private const float ResistiveHeaterMultiplier = 0.2f;
        private float ThermalWorkMultiplier;
        private int WattageSetting;

        public Status status = Status.Offline;
        public bool IsTemperatureReached = false;

        public float ThermalWork = 0f;

        /// <summary>
        ///     Building spawned on the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = GetComp<CompPowerTrader>();
            compCoolant = GetComp<CompCoolantProvider>();
            compCoolant.Props.flowType = CoolantPipeColor.Any;

            ThermalWorkMultiplier = compCoolant.Props.ThermalWorkMultiplier;

            compCoolant.CoolantTemperature = Position.GetTemperature(Map);

            compCoolant.CoolantThermalMass = 100; //TEMP
            WattageSetting = 800; //TEMP
        }

        /// <summary>
        ///     Get the Gizmos for AirVent
        ///     Here, we generate the Gizmo for Chaning Pipe Priority
        /// </summary>
        /// <returns>List of Gizmos</returns>
        public override System.Collections.Generic.IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (compCoolant != null)
            {
                yield return CentralizedThermalDistributionUtility.GetPipeSwitchToggle(compCoolant);
            }
        }

        public override string GetInspectString()
        {
            string output = base.GetInspectString();
            output += "\nThermalWork: " + ThermalWork;
            return output;
        }

        /// <summary>
        ///     Tick heater unit.
        /// </summary>
        public override void TickRare()
        {
            if (compPowerTrader.PowerOn)
            {

                // === Temperature setting check ===
                IsTemperatureReached = compCoolant.CoolantTemperature >= compTempControl.targetTemperature; // True if coolant temp is at or beyond desired temp.

                compCoolant.ActiveOnNetwork = true;

                if (IsTemperatureReached)
                {
                    // === Heater is online and idle ===
                    compTempControl.operatingAtHighPower = false;
                    compPowerTrader.PowerOutput = -WattageSetting * compTempControl.Props.lowPowerConsumptionFactor; // TODO: fix
                    status = Status.TemperatureReached;
                    ThermalWork = 0;
                }
                else
                {
                    // === Heater is online and working ===
                    status = Status.Working;
                    compTempControl.operatingAtHighPower = true;
                    compPowerTrader.PowerOutput = -WattageSetting;
                    ThermalWork = WattageSetting * ResistiveHeaterMultiplier * ThermalWorkMultiplier;
                    compCoolant.PushThermalLoad(ThermalWork); // Push to coolant
                }
            }
            else
            {
                // === Heater is offline ===
                status = Status.Offline;
                compCoolant.ActiveOnNetwork = false;
                //compTempControl.operatingAtHighPower = false; // Vanilla coolers and heaters simply leave these values as-is.
                //compPowerTrader.PowerOutput = 0;
                ThermalWork = 0;
            }
        }
    }
}