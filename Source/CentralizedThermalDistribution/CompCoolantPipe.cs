namespace CentralizedThermalDistribution
{
    public class CompCoolantPipe : CompCoolant
    {
        /// <summary>
        ///     Component Inspection for Pipes
        /// </summary>
        /// <returns>String to Display for Pipes</returns>
        public override string CompInspectStringExtra()
        {
            string output = GetAirTypeString(Props.flowType);
            output += "\nConnected: " + IsConnected();
            output += "\nGrid ID: " + GridID;
            return output;
        }
    }
}