using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class Building_CoilVent : Building
    {
        public CompCoolantConsumer compCoolant;

        private const float CoilVentMultiplier = 2.0f;

        private float ThermalWorkMultiplier; // Heat output to both surroundings and and coolant is multiplied by this. 

        public float ThermalWork = 0f;

        /// <summary>
        ///     Building spawned on the map
        /// </summary>
        /// <param name="map">RimWorld Map</param>
        /// <param name="respawningAfterLoad">Unused flag</param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compCoolant = GetComp<CompCoolantConsumer>();
            ThermalWorkMultiplier = compCoolant.Props.ThermalWorkMultiplier;
        }

        /// <summary>
        ///     Get the Gizmos for AirVent
        ///     Here, we generate the Gizmo for Chaning Pipe Priority
        /// </summary>
        /// <returns>List of Gizmos</returns>
        public override System.Collections.Generic.IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
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
        ///     Tick function for Coil Vents
        /// </summary>
        public override void TickRare()
        {
            ThermalWork = 0f;
            if (!compCoolant.IsConnected()) return;
            if (!compCoolant.coolantNet.IsNetActive()) return;
            var outputTile = Position + IntVec3.North.RotatedBy(Rotation);
            if (outputTile.Impassable(Map)) return;

            // Linear, KISS
            ThermalWork = (outputTile.GetTemperature(Map) - compCoolant.coolantNet.GetNetCoolantTemperature()) * CoilVentMultiplier * ThermalWorkMultiplier; // Positive if room is hotter than coolant
            compCoolant.PushThermalLoad(ThermalWork); // Push coolant net (positive ThermalWork)
            GenTemperature.PushHeat(outputTile, base.Map, -ThermalWork); // Push exhaust (negative ThermalWork)
        }
    }
}