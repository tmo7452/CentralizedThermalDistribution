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

            // Iterate over all output cells
            foreach (var outputCell in UniqueOutputLocations)
            {
                // Skip cells without heatable space
                if (outputCell.GetRoom(parent.Map) is null)
                    continue;

                // Calculate desired work for that cell based on temperature difference.
                // Linear, KISS
                float desiredWork = (outputCell.GetTemperature(parent.Map) - coolantNet.GetNetCoolantTemperature()) * CoilVentMultiplier * ThermalWorkMultiplier * tickMultiplier; // Positive if room is hotter than coolant

                // Push exhaust (negative ThermalWork) to this cell
                GenTemperature.PushHeat(outputCell, parent.Map, -desiredWork);
                ThermalWork += desiredWork;
            }
            // Push work done to coolant net (positive ThermalWork)
            PushThermalLoad(ThermalWork);
        }


    }
}