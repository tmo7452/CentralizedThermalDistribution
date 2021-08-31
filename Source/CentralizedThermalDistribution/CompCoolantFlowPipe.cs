namespace CentralizedThermalDistribution
{
    public class CompCoolantFlowPipe : CompCoolantFlow
    {
        /// <summary>
        ///     Component Inspection for Pipes
        /// </summary>
        /// <returns>String to Display for Pipes</returns>
        public override string CompInspectStringExtra()
        {
            return GetAirTypeString(Props.flowType);
        }
    }
}