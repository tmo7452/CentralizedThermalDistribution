using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CompCoolantConsumerCoilVent : CompCoolantConsumer
    {
        private const float CoilVentMultiplier = 0.4f;

        /// <summary>
        ///     Update function for Coil Vents
        /// </summary>
        /// <param name="tickMultiplier">Number of 50 tick increments to process.</param>
        protected override void CoolantTick(int tickMultiplier)
        {
            ThermalWork = 0f;
            if (!IsConnected) return;
            if (!coolantNet.IsNetActive()) return;
            var outputTile = parent.Position + IntVec3.North.RotatedBy(parent.Rotation);
            if (outputTile.Impassable(parent.Map)) return;

            // Linear, KISS
            ThermalWork = (outputTile.GetTemperature(parent.Map) - coolantNet.GetNetCoolantTemperature()) * CoilVentMultiplier * ThermalWorkMultiplier * tickMultiplier; // Positive if room is hotter than coolant
            PushThermalLoad(ThermalWork); // Push coolant net (positive ThermalWork)
            GenTemperature.PushHeat(outputTile, parent.Map, -ThermalWork); // Push exhaust (negative ThermalWork)
        }


    }
}