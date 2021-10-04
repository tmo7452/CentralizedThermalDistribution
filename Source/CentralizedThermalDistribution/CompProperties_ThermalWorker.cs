using System.Collections.Generic;
using Verse;
using RimWorld;

namespace CentralizedThermalDistribution
{
    public class CompProperties_ThermalWorker : CompProperties
    {
        public CompProperties_ThermalWorker() : base()
        {
            compClass = typeof(CompThermalWorker);
        }

        public enum TransferMode
        {
            direct,
            passive,
            active,
        }

        public enum ThermalMedium
        {
            none, // Magic
            coolant,
            ambient,
            power,
            fuel,
        }
        public static readonly float[] ThermalMediumWorkMultiplier = { 0.0f, 1.0f, 0.8f, 0.0008f, 0.08f };

        public enum AmbientDirection
        {
            // Values set for easy conversion to Rot4
            north = 0,
            east = 1,
            south = 2,
            west = 3,
            center = 4,
        }

        // Used by all
        public ThermalMedium inputMedium = ThermalMedium.none;
        public ThermalMedium outputMedium = ThermalMedium.none;
        public TransferMode thermalTransferMode = TransferMode.direct;
        public float totalWorkMultiplier = 1.00f;
        public float inputWorkMultiplier = 1.00f;
        public float outputWorkMultiplier = 1.00f;
        public bool enableThermostatReversal = true;
        public bool enableEfficiencyLimit = false;
        public bool allowBreakingLawsOfThermodynamics = false;

        //Used by specific transfer modes
        public float active_maxTemperatureDelta = 50.0f;

        //Used by specific thermal mediums
        public List<AmbientDirection> ambientInput_directions = new();
        public List<AmbientDirection> ambientOutput_directions = new();
        public int ambientInput_ignoredBlockedCells = 0; // How many adjacent cells can be blocked before it negatively affects performance.
        public int ambientOutput_ignoredBlockedCells = 0;
        public float fuelInput_idleFuelConsumptionFactor = 0.05f;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            // Any errors that start with "illegal" can be bypassed by setting allowBreakingLawsOfThermodynamics to true.

            if (((inputMedium == ThermalMedium.none) || (outputMedium == ThermalMedium.none)) && !allowBreakingLawsOfThermodynamics)
                yield return parentDef.defName + " illegal input/output type \"none\".";

            switch (thermalTransferMode)
            {
                case TransferMode.direct:
                    // Technically breaking laws of entropy not thermodynamics, but no reason to further complicate.
                    if (MediumHasTemperature(inputMedium) && !allowBreakingLawsOfThermodynamics)
                        yield return parentDef.defName + " illegal inputMedium for selected thermalTransferMode.";
                    break;
                case TransferMode.passive:
                case TransferMode.active:
                    // Passive and active modes require both (input and output) mediums to have a measureable temperature.
                    if (!MediumHasTemperature(inputMedium))
                        yield return parentDef.defName + " unsupported inputMedium for selected thermalTransferMode.";
                    if (!MediumHasTemperature(outputMedium))
                        yield return parentDef.defName + " unsupported outputMedium for selected thermalTransferMode.";
                    break;
                default:
                    yield return parentDef.defName + " invalid thermalTransferMode.";
                    break;
            }

            if (totalWorkMultiplier == 0f)
                yield return parentDef.defName + " invalid totalWorkMultiplier, must be nonzero.";

            if ((totalWorkMultiplier < 0f) && (thermalTransferMode != TransferMode.active) && !allowBreakingLawsOfThermodynamics)
                yield return parentDef.defName + " illegal totalWorkMultiplier, must be positive unless using active thermalTransferMode.";

            if (((inputWorkMultiplier != 1.0f) || (outputWorkMultiplier != 1.0f)) && !allowBreakingLawsOfThermodynamics)
                yield return parentDef.defName + " illegal use of inputWorkMultiplier or outputWorkMultiplier.";

            if ((inputWorkMultiplier <= 0f) || (outputWorkMultiplier <= 0f))
                yield return parentDef.defName + " invalid input/output work multiplier, must be positive.";

            // Output medium must have a temperature. Might support otherwise in the future.
            if (!MediumHasTemperature(outputMedium))
                yield return parentDef.defName + " unsupported outputMedium.";

            // Check for missing interface components.
            if ((inputMedium == ThermalMedium.coolant) && (parentDef.GetCompProperties<CompProperties_Coolant>()?.compClass != typeof(CompCoolantConsumer)))
                yield return parentDef.defName + " missing CompProperties_Coolant or CompCoolantConsumer for coolant input.";

            if ((outputMedium == ThermalMedium.coolant) && (parentDef.GetCompProperties<CompProperties_Coolant>()?.compClass != typeof(CompCoolantProvider)))
                yield return parentDef.defName + " missing CompProperties_Coolant or CompCoolantProvider for coolant output.";

            if ((inputMedium == ThermalMedium.power) && (parentDef.GetCompProperties<CompProperties_Power>()?.compClass != typeof(CompPowerTrader)))
                yield return parentDef.defName + " missing CompProperties_Power or CompPowerTrader for power input.";

            if (((inputMedium == ThermalMedium.fuel) || (outputMedium == ThermalMedium.fuel)) && (parentDef.GetCompProperties<CompProperties_Refuelable>() is null))
                yield return parentDef.defName + " missing CompProperties_Refuelable for fuel input.";

            if ((inputMedium == ThermalMedium.ambient) && (ambientInput_directions.Count == 0))
                yield return parentDef.defName + " missing ambient input directions.";

            if ((outputMedium == ThermalMedium.ambient) && (ambientOutput_directions.Count == 0))
                yield return parentDef.defName + " missing ambient output directions.";

            yield break;
        }

        /// <returns>True if thermal medium has a measurable temperature.</returns>
        private bool MediumHasTemperature(ThermalMedium medium)
        {
            return medium switch
            {
                ThermalMedium.none => false,
                ThermalMedium.coolant => true,
                ThermalMedium.ambient => true,
                ThermalMedium.power => false,
                ThermalMedium.fuel => false,
                _ => false,
            };
        }
    }
}