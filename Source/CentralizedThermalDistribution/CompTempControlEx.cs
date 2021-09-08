using Verse;
using RimWorld;

namespace CentralizedThermalDistribution
{
    public class CompTempControlEx : CompTempControl
    {
		// Need to be able to control what gets printed during inspection.

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
					stringBuilder.AppendLine(consumptionHighString.Translate());
				}
				else
				{
					stringBuilder.AppendLine(consumptionLowString.Translate());
				}
			}
			return stringBuilder.ToString().Trim();
		}
    }
}
