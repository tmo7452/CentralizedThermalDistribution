using Verse;

namespace CentralizedThermalDistribution
{
    public enum ThermalInOutDirection
    {
        center,
        north,
        east,
        south,
        west,
    }
    
    public static class Extensions
    {
        public static Rot4 ToRot4(this ThermalInOutDirection dir)
        {
            return new Rot4((int)dir - 1);
        }

        public static IntVec3 ToIntVec3(this ThermalInOutDirection dir)
        {
            return dir switch
            {
                ThermalInOutDirection.center => IntVec3.Zero,
                ThermalInOutDirection.north => IntVec3.North,
                ThermalInOutDirection.south => IntVec3.South,
                ThermalInOutDirection.east => IntVec3.East,
                ThermalInOutDirection.west => IntVec3.West,
                _ => IntVec3.Zero
            };
        }

        public static LinkDirections ToLinkDir(this ThermalInOutDirection dir)
        {
            return dir switch
            {
                ThermalInOutDirection.center => LinkDirections.None,
                ThermalInOutDirection.north => LinkDirections.Up,
                ThermalInOutDirection.south => LinkDirections.Down,
                ThermalInOutDirection.east => LinkDirections.Right,
                ThermalInOutDirection.west => LinkDirections.Left,
                _ => LinkDirections.None
            };
        }
    } 

    public class CompProperties_Coolant : CompProperties
    {

        public CompCoolant.PipeColor pipeColor = CompCoolant.PipeColor.None;
        public bool pipeIsHidden = false;

        public float traderThermalWorkMultiplier = 1.00f;
        public float traderMaxTemperatureDelta;
        public int providerCoolantThermalMass = 20;
        public float providerLowFuelConsumptionFactor = 0.1f;
        //public System.Collections.Generic.List<ThermalInOutDirection> traderInputDirections;
        public System.Collections.Generic.List<ThermalInOutDirection> traderOutputDirections;
    }
}