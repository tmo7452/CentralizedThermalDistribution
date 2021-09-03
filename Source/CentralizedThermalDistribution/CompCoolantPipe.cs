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
            output += "\nNet ID: " + NetID;
            var net = GetNet();
            if (net != null)
            {
                output += "\nNet Coolant Temp: " + net.GetNetCoolantTemperature();
            }
            return output;
        }

        /// <summary>
        ///     Look up the coolant network for this pipe.
        /// </summary>
        /// <returns>CoolantNet its connected to</returns>
        public CoolantNet GetNet()
        {
            return CentralizedThermalDistributionUtility.GetNetManager(parent.Map).GetNet(NetID);
        }
    }
}