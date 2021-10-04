using Verse;
using RimWorld;
using System.Collections.Generic;
using static CentralizedThermalDistribution.CompProperties_ThermalWorker;

namespace CentralizedThermalDistribution
{
    class CompThermalWorker : ThingComp
    {
        public enum EfficiencyLimit
        {
            E10 = 10, // 10%
            E25 = 25, // 25%
            E50 = 50, // 50%
            E80 = 80, // 80%
        }
        private const EfficiencyLimit DefaultEfficiencyLimit = EfficiencyLimit.E25;

        public enum ThermalWorkerStatus
        {
            offline,
            idle,
            ready, // Used by Interface
            working,
        }
        private static readonly string[] statusStrings = { "Offline", "Idle", "Ready", "Working" }; // TODO change to translation keys

        private const int ThermalWorkInterval = 50;
        private const float TotalMultiplier = 0.01f;
#if DEBUG
        [TweakValue("CTD", 0.0f, 0.5f)]
        private static float PassiveMultiplier = 0.15f;
        [TweakValue("CTD", 0.0f, 10f)]
        private static float ActiveMultiplier = 4f;
#else
        private const float PassiveMultiplier = 0.15f;
        private const float ActiveMultiplier = 4f;
#endif

        private CompProperties_ThermalWorker Props = null;
        private CompTempControl TempComp = null;
        private CompFlickable FlickComp = null;
        private ThermalWorkerInterface InputInterface;
        private ThermalWorkerInterface OutputInterface;
        private CompPowerTrader PowerComp = null;

        private List<System.Func<Gizmo>> Gizmos = new(); // A list of lambda functions to be called during gizmo checks.

        private int ThermalWorkDirection = 0;
        private float ActiveCurveOffset;
        private float WorkingPowerOutput = 0;
        private float IdlePowerOutput = 0;
        private const float OfflinePowerOutput = 0;

        private ThermalWorkerStatus Status = ThermalWorkerStatus.offline;
        private string StatusReason = null;
#if DEBUG
        private float Teps = 0f;
#endif
        private bool ThermostatReversed = false;
        private EfficiencyLimit EfficiencyLimitSelection = DefaultEfficiencyLimit;
        public float ThermalWorkDone { get; private set; } = 0f;
        private float ThermalTransferMultiplier = 1.0f;
        private float ThermalTransferEfficiency
        { get
            {
                return Props.thermalTransferMode switch
                {
                    TransferMode.direct => 1.0f,
                    TransferMode.passive => ThermalTransferMultiplier / PassiveMultiplier,
                    TransferMode.active => ThermalTransferMultiplier / ActiveMultiplier,
                    _ => throw new System.NotImplementedException(),
                };
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            Props = (props as CompProperties_ThermalWorker);
            TempComp = parent.GetComp<CompTempControl>();
            PowerComp = parent.GetComp<CompPowerTrader>();
            FlickComp = parent.GetComp<CompFlickable>();
            InputInterface = Props.inputMedium switch
            {
                ThermalMedium.none => new ThermalWorkerInterface(this, true),
                ThermalMedium.coolant => new ThermalWorkerInterface_Coolant(this, true, parent.GetComp<CompCoolantConsumer>()),
                ThermalMedium.ambient => new ThermalWorkerInterface_Ambient(this, true, parent, Props.ambientInput_ignoredBlockedCells),
                ThermalMedium.power => new ThermalWorkerInterface_Power(this, true, PowerComp),
                ThermalMedium.fuel => new ThermalWorkerInterface_Fuel(this, true, parent.GetComp<CompRefuelable>(), Props.fuelInput_idleFuelConsumptionFactor),
                _ => throw new System.NotImplementedException(),
            };
            OutputInterface = Props.outputMedium switch
            {
                ThermalMedium.coolant => new ThermalWorkerInterface_Coolant(this, false, parent.GetComp<CompCoolantProvider>()),
                ThermalMedium.ambient => new ThermalWorkerInterface_Ambient(this, false, parent, Props.ambientOutput_ignoredBlockedCells),
                _ => throw new System.NotImplementedException(),
            };

            ThermalWorkDirection = System.Math.Sign(Props.totalWorkMultiplier);
            ActiveCurveOffset = Props.active_maxTemperatureDelta * 2;
            if (PowerComp is not null)
            {
                WorkingPowerOutput = -PowerComp.Props.basePowerConsumption;
                IdlePowerOutput = (TempComp is null) ? WorkingPowerOutput : WorkingPowerOutput * TempComp.Props.lowPowerConsumptionFactor;
            }

            // If we have a TempComp and it's of type CompTempControlEx and it has a lowPowerConsumptionFactor of 1, then we disable printing the consumption mode.
            if (((TempComp as CompTempControlEx)?.Props.lowPowerConsumptionFactor ?? 0f) == 1.0f)
                (TempComp as CompTempControlEx).consumptionModeString = null;

            if (Props.enableEfficiencyLimit)
                AddGizmo(() => GetMinEfficiencyToggle(this));
        }

        /// <summary>
        ///     Method called during Game Save/Load
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ThermostatReversed, "ThermostatReversed", false);
            Scribe_Values.Look(ref EfficiencyLimitSelection, "EfficiencyLimitSelection", DefaultEfficiencyLimit);
        }

        public override string CompInspectStringExtra()
        {
            System.Text.StringBuilder output = new();
            output.AppendLine(base.CompInspectStringExtra());
#if DEBUG
            output.AppendLine("DEBUG Thermal output: " + System.Math.Abs(Teps).ToString("0.000") + " teps " + ((ThermalWorkDone >= 0) ? "heating " : "cooling ") + Props.outputMedium);
            output.AppendLine("DEBUG Status: " + Status + (StatusReason is not null ? " (" + StatusReason + ")" : ""));
            if (Props.enableEfficiencyLimit)
            {
                output.AppendLine("DEBUG Efficiencies "
                    + (InputInterface.Efficiency.HasValue ? ("Input: " + (InputInterface.Efficiency.Value * 100).ToString("0.0") + "%, ") : "")
                    + (OutputInterface.Efficiency.HasValue ? ("Output: " + (OutputInterface.Efficiency.Value * 100).ToString("0.0") + "%, ") : "")
                    + "Transfer: "+ (ThermalTransferEfficiency * 100).ToString("0.0") + "%");
            };
            output.AppendLine("DEBUG Multipliers Input: " + InputInterface.Multiplier + ", Output: " + OutputInterface.Multiplier + ", Transfer: " + ThermalTransferMultiplier);
            output.AppendLine("DEBUG Dir: " + ThermalWorkDirection + ", ThermalWorkDone: " + ThermalWorkDone);
#endif
            return output.ToString().Trim();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(ThermalWorkInterval))
                DoThermalTick(ThermalWorkInterval);
            InputInterface.CompTick();
            OutputInterface.CompTick();
        }
        public override void CompTickRare()
        {
            base.CompTickRare();
            DoThermalTick(250);
            InputInterface.CompTick();
            OutputInterface.CompTick();
        }

        private void DoThermalTick(int tickCount)
        {
            if ((FlickComp is not null) && (!FlickComp.SwitchIsOn))
            {
                GoOffline("FlickedOff");
                return;
            }
            if ((PowerComp is not null) && (!PowerComp.PowerOn))
            {
                GoOffline("PoweredOff");
                return;
            }
            InputInterface.Update();
            OutputInterface.Update();
            if (InputInterface.Status == ThermalWorkerStatus.offline)
            {
                GoOffline("Input" + InputInterface.StatusReason);
                return;
            }
            if (OutputInterface.Status == ThermalWorkerStatus.offline)
            {
                GoOffline("Output" + OutputInterface.StatusReason);
                return;
            }
            if (InputInterface.Status == ThermalWorkerStatus.idle)
            {
                GoIdle("Input" + InputInterface.StatusReason);
                return;
            }
            if (OutputInterface.Status == ThermalWorkerStatus.idle)
            {
                GoIdle("Output" + OutputInterface.StatusReason);
                return;
            }
            if (TempComp is not null)
            {
                var currentTemp = ThermostatReversed ? InputInterface.Temperature : OutputInterface.Temperature;
                if ((ThermalWorkDirection * currentTemp) >= (ThermalWorkDirection * TempComp.targetTemperature))
                {
                    GoIdle("TempReached");
                    return;
                }
            }
            switch (Props.thermalTransferMode)
            {
                case TransferMode.direct:
                    break;
                case TransferMode.passive:
                    // Heat goes in the direction of the temperature difference. Magnitude is a linear scale based on the difference.
                    ThermalTransferMultiplier = PassiveMultiplier * (InputInterface.Temperature.Value - OutputInterface.Temperature.Value);
                    ThermalWorkDirection = System.Math.Sign(ThermalTransferMultiplier);
                    break;
                case TransferMode.active:
                    // Heat goes in a specific direction regardless of the temperature difference. Magnitude is an exponential curve based on how far it is from the active_maxTemperatureDelta.
                    // https://www.desmos.com/calculator/alkcu4cdjt
                    float deltaFromMax = ((InputInterface.Temperature.Value - OutputInterface.Temperature.Value) * ThermalWorkDirection) + ActiveCurveOffset;
                    if (deltaFromMax > 0)
                        ThermalTransferMultiplier = ActiveMultiplier * (deltaFromMax * deltaFromMax) / (ActiveCurveOffset * ActiveCurveOffset);
                    else
                    {
                        ThermalTransferMultiplier = 0f;
                        GoIdle("EfficiencyLow");
                        return;
                    }
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            if (Props.enableEfficiencyLimit)
            {
                float TotalEfficiency = ThermalTransferEfficiency;
                if (InputInterface.Efficiency.HasValue)
                    TotalEfficiency *= InputInterface.Efficiency.Value;
                if (OutputInterface.Efficiency.HasValue)
                    TotalEfficiency *= OutputInterface.Efficiency.Value;
                if ((TotalEfficiency * 100) < (int)EfficiencyLimitSelection)
                {
                    GoIdle("EfficiencyLow");
                    return;
                }
            }

            Status = ThermalWorkerStatus.working;
            StatusReason = null;
            if (PowerComp is not null)
                PowerComp.PowerOutput = WorkingPowerOutput + InputInterface.PowerOutput + OutputInterface.PowerOutput;
            ThermalWorkDone = tickCount * TotalMultiplier * ThermalTransferMultiplier * InputInterface.Multiplier * OutputInterface.Multiplier * Props.totalWorkMultiplier;
#if DEBUG
            Teps = ThermalWorkDone * 60f / tickCount;
#endif
            InputInterface.PushWork(-ThermalWorkDone * Props.inputWorkMultiplier);
            OutputInterface.PushWork(ThermalWorkDone * Props.outputWorkMultiplier);
        }

        private void GoOffline(string reason)
        {
            Status = ThermalWorkerStatus.offline;
            StatusReason = reason;
            if (PowerComp is not null)
                PowerComp.PowerOutput = OfflinePowerOutput;
            ThermalWorkDone = 0f;
            InputInterface.DoOffline();
            OutputInterface.DoOffline();
        }

        private void GoIdle(string reason)
        {
            Status = ThermalWorkerStatus.idle;
            StatusReason = reason;
            if (PowerComp is not null)
                PowerComp.PowerOutput = IdlePowerOutput;
            ThermalWorkDone = 0f;
            InputInterface.DoIdle();
            OutputInterface.DoIdle();
        }

        /// <summary>
        ///     Gizmo for changing condenser minimum efficiency setting.
        /// </summary>
        /// <param name="compCoolant">Component Asking for Gizmo</param>
        /// <returns>Action Button Gizmo</returns>
        public static Command_Action GetMinEfficiencyToggle(CompThermalWorker thermalWorker)
        {
            UnityEngine.Texture2D icon;
            string label;

            switch (thermalWorker.EfficiencyLimitSelection)
            {

                case EfficiencyLimit.E10:
                    //label = SwitchPipeRedKey.Translate();
                    label = "Efficiency Cutoff: 10%";
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/MinEfficiencySelect_E10");
                    break;

                case EfficiencyLimit.E25:
                default:
                    //label = SwitchPipeBlueKey.Translate();
                    label = "Efficiency Cutoff: 25%";
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/MinEfficiencySelect_E25");
                    break;

                case EfficiencyLimit.E50:
                    //label = SwitchPipeCyanKey.Translate();
                    label = "Efficiency Cutoff: 50%";
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/MinEfficiencySelect_E50");
                    break;

                case EfficiencyLimit.E80:
                    //label = SwitchPipeAutoKey.Translate();
                    label = "Efficiency Cutoff: 80%";
                    icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/MinEfficiencySelect_E80");
                    break;
            }

            return new Command_Action
            {
                defaultLabel = label,
                defaultDesc = "CentralizedThermalDistribution.Command.SwitchPipe.Desc".Translate(),
                //hotKey = KeyBindingDefOf.Misc4,
                icon = icon,
                action = delegate
                {
                    thermalWorker.EfficiencyLimitSelection = thermalWorker.EfficiencyLimitSelection switch
                    {
                        EfficiencyLimit.E10 => EfficiencyLimit.E25,
                        EfficiencyLimit.E25 => EfficiencyLimit.E50,
                        EfficiencyLimit.E50 => EfficiencyLimit.E80,
                        EfficiencyLimit.E80 => EfficiencyLimit.E10,
                        _ => DefaultEfficiencyLimit
                    };
                }
            };
        }

        public void AddGizmo(System.Func<Gizmo> gizmo)
        {
            Gizmos.Add(gizmo);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            foreach (var gizmo in Gizmos)
                yield return gizmo();
        }

        public static List<IntVec3> GetAmbientInputLocations(ThingDef def, IntVec3 center, Rot4 rot, bool onlyUniqueRooms = false, Map map = null)
        {
            return GetAmbientLocations(def.GetCompProperties<CompProperties_ThermalWorker>().ambientInput_directions, def.size, center, rot, onlyUniqueRooms, map);
        }

        public static List<IntVec3> GetAmbientOutputLocations(ThingDef def, IntVec3 center, Rot4 rot, bool onlyUniqueRooms = false, Map map = null)
        {
            return GetAmbientLocations(def.GetCompProperties<CompProperties_ThermalWorker>().ambientOutput_directions, def.size, center, rot, onlyUniqueRooms, map);
        }

        private static List<IntVec3> GetAmbientLocations(List<AmbientDirection> dirs, IntVec2 size, IntVec3 center, Rot4 rot, bool onlyUniqueRooms = false, Map map = null)
        {
            List<IntVec3> locs = new();
            foreach (var dir in dirs)
            {
                //Iterating ThermalOutputDirections, as defined in the comp def. 
                foreach (var cell in dir == AmbientDirection.center ? GenAdj.CellsOccupiedBy(center, rot, size) : CellsAdjacentDirection(center, rot, size, new Rot4((int)dir)))
                {
                    //Iterating individual cells, based on building size and rotation.
                    if (onlyUniqueRooms)
                    {
                        var room = cell.GetRoom(map);
                        if (room is null)
                            continue;

                        bool isUnique = true;
                        foreach (var loc in locs)
                        {
                            if (room == loc.GetRoom(map))
                                isUnique = false;
                        }
                        if (isUnique)
                            locs.Add(cell);
                    }
                    else locs.Add(cell);
                }
            }
            return locs;
        }

        // Modified from GenAdj.CellsAdjacent decompile to filter output to a specific side.
        // I'd assume this is what GenAdj.CellsAdjacentAlongEdge is for, but its output is the wrong size.
        private static IEnumerable<IntVec3> CellsAdjacentDirection(IntVec3 center, Rot4 rot, IntVec2 size, Rot4 dir)
        {
            GenAdj.AdjustForRotation(ref center, ref size, rot);
            int minX = center.x - (size.x - 1) / 2 - 1;
            int maxX = minX + size.x + 1;
            int minZ = center.z - (size.z - 1) / 2 - 1;
            int maxZ = minZ + size.z + 1;
            IntVec3 cur;
            switch ((rot.AsInt + dir.AsInt) % 4)
            {
                case 0:
                    cur = new IntVec3(maxX, 0, maxZ);
                    do
                    {
                        cur.x--;
                        yield return cur;
                    }
                    while (cur.x > minX + 1);
                    yield break;
                case 1:
                    cur = new IntVec3(maxX, 0, minZ);
                    do
                    {
                        cur.z++;
                        yield return cur;
                    }
                    while (cur.z < maxZ - 1);
                    yield break;
                case 2:
                    cur = new IntVec3(minX, 0, minZ);
                    do
                    {
                        cur.x++;
                        yield return cur;
                    }
                    while (cur.x < maxX - 1);
                    yield break;
                case 3:
                    cur = new IntVec3(minX, 0, maxZ);
                    do
                    {
                        cur.z--;
                        yield return cur;
                    }
                    while (cur.z > minZ + 1);
                    yield break;
                default:
                    yield break;
            }
        }
    }
}
