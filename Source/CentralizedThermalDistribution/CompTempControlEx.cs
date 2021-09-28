using Verse;
using RimWorld;

namespace CentralizedThermalDistribution
{
	// CompTempControl, but with ability to change what gets printed during inspection.
	public class CompTempControlEx : CompTempControl
    {
		public string targetTempString = "TargetTemperature";
		public string consumptionModeString = "PowerConsumptionMode";
		public string consumptionHighString = "PowerConsumptionHigh";
		public string consumptionLowString = "PowerConsumptionLow";

		public override string CompInspectStringExtra()
        {
            System.Text.StringBuilder stringBuilder = new();
			if (targetTempString != null)
            {
				stringBuilder.AppendLine(targetTempString.Translate() + ": " + targetTemperature.ToStringTemperature("F0"));
			}
			if (consumptionModeString != null)
			{
				stringBuilder.Append(consumptionModeString.Translate() + ": ");
				if (operatingAtHighPower)
				{
					stringBuilder.AppendLine(consumptionHighString.Translate().CapitalizeFirst());
				}
				else
				{
					stringBuilder.AppendLine(consumptionLowString.Translate().CapitalizeFirst());
				}
			}
			return stringBuilder.ToString().Trim();
		}
    }
}
