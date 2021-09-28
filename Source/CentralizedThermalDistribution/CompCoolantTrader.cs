using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public abstract class CompCoolantTrader : CompCoolant
    {
        private const string SwitchPipeAutoKey = "CentralizedThermalDistribution.Command.SwitchPipe.Auto";
        private const string SwitchPipeRedKey = "CentralizedThermalDistribution.Command.SwitchPipe.Red";
        private const string SwitchPipeBlueKey = "CentralizedThermalDistribution.Command.SwitchPipe.Blue";
        private const string SwitchPipeCyanKey = "CentralizedThermalDistribution.Command.SwitchPipe.Cyan";
        private const string SwitchPipeGreenKey = "CentralizedThermalDistribution.Command.SwitchPipe.Green";
        public enum PipeColorSelection
        {
            Red = 0,
            Blue = 1,
            Cyan = 2,
            Green = 4,
            Auto = 3,
        }
        public const PipeColorSelection DefaultPipeColorSelection = PipeColorSelection.Auto;

        public static bool PipeColorMatchesSelection(PipeColor color, PipeColorSelection selection)
        {
            return color switch
            {
                PipeColor.Red => (selection == PipeColorSelection.Red) || (selection == PipeColorSelection.Auto),
                PipeColor.Blue => (selection == PipeColorSelection.Blue) || (selection == PipeColorSelection.Auto),
                PipeColor.Cyan => (selection == PipeColorSelection.Cyan) || (selection == PipeColorSelection.Auto),
                PipeColor.Green => (selection == PipeColorSelection.Green) || (selection == PipeColorSelection.Auto),
                _ => false
            };
        }

        private PipeColorSelection _pipeColorSelection = DefaultPipeColorSelection;
        public PipeColorSelection pipeColorSelection
        {
            get => _pipeColorSelection;
            set
            {
                _pipeColorSelection = value;
                UpdateAttachedNet();
            }
        }

        //public List<IntVec3> InputLocations => GetInputLocations(parent.def, parent.Position, parent.Rotation, false);
        //public List<IntVec3> UniqueInputLocations => GetInputLocations(parent.def, parent.Position, parent.Rotation, true, parent.Map);
        public List<IntVec3> OutputLocations => GetOutputLocations(parent.def, parent.Position, parent.Rotation, false);
        public List<IntVec3> UniqueOutputLocations => GetOutputLocations(parent.def, parent.Position, parent.Rotation, true, parent.Map);

        private List<CoolantNet> AvailableNets = new();
        private List<System.Func<Gizmo>> Gizmos = new(); // A list of lambda functions to be called during gizmo checks.

        public float ThermalWork { get; protected set; } = 0f;
        protected float ThermalWorkMultiplier; // Multiplier unique to the building type, set by the Def. Positive if heating coolant, negative if cooling.

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

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Props.pipeColor = PipeColor.Trader;
            ThermalWorkMultiplier = Props.traderThermalWorkMultiplier;
            AddGizmo(() => GetPipeSwitchToggle(this));
            RescanNets();
            pipeColorSelection = DefaultPipeColorSelection;
        }

        /// <summary>
        ///     Method called during Game Save/Load
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            var ExposedPipeColorSelection = pipeColorSelection;
            Scribe_Values.Look(ref ExposedPipeColorSelection, "pipeColorSelection", DefaultPipeColorSelection);
            if (ExposedPipeColorSelection != pipeColorSelection) pipeColorSelection = ExposedPipeColorSelection;
        }

        public override string CompInspectStringExtra()
        {
            System.Text.StringBuilder output = new();
            output.AppendLine(base.CompInspectStringExtra());
            output.AppendLine("DEBUG ThermalWork: " + ThermalWork);
            return output.ToString().Trim();
        }

        public override void SetNet(CoolantNet newNet)
        {
            if (coolantNet != null)
                coolantNet.Traders.Remove(this);
            base.SetNet(newNet);
            SetPipeColor(newNet == null ? PipeColor.None : newNet.pipeColor);
            if (coolantNet != null)
                coolantNet.Traders.Add(this);
        }

        public abstract void PushThermalLoad(float ThermalLoad);

        public void RescanNets()
        {

            AvailableNets = new();

            // For each cell we occupy, scan for any pipes and add their networks.
            foreach (var pos in parent.OccupiedRect().Cells)
            {
                for (int pipeColorIndex = 0; pipeColorIndex < PipeColorCount; pipeColorIndex++)
                {
                    if (coolantNetManager.IsPipeAt(pos, pipeColorIndex))
                    {
                        CoolantNet net = coolantNetManager.GetPipeAt(pos, pipeColorIndex).coolantNet;
                        if (AvailableNets.Contains(net))
                            continue;
                        AvailableNets.Add(net);
                    }
                }
            }
            UpdateAttachedNet();
        }

        private void UpdateAttachedNet()
        {
            SetNet(null);
            foreach (var net in AvailableNets)
            {
                if (PipeColorMatchesSelection(net.pipeColor, pipeColorSelection))
                {
                    SetNet(net);
                    break;
                }
            }
        }

        public void RemoveNet(CoolantNet net)
        {
            if (AvailableNets.Contains(net))
                AvailableNets.Remove(net);
        }

        //public static List<IntVec3> GetInputLocations(ThingDef def, IntVec3 center, Rot4 rot, bool onlyUniqueRooms = false, Map map = null)
        //{
        //    return GetInOutLocations(def.GetCompProperties<CompProperties_Coolant>().traderInputDirections, def.size, center, rot, onlyUniqueRooms, map);
        //}

        public static List<IntVec3> GetOutputLocations(ThingDef def, IntVec3 center, Rot4 rot, bool onlyUniqueRooms = false, Map map = null)
        {
            return GetInOutLocations(def.GetCompProperties<CompProperties_Coolant>().traderOutputDirections, def.size, center, rot, onlyUniqueRooms, map);
        }

        private static List<IntVec3> GetInOutLocations(List<ThermalInOutDirection> dirs, IntVec2 size, IntVec3 center, Rot4 rot, bool onlyUniqueRooms = false, Map map = null)
        {
            List<IntVec3> locs = new();
            foreach (var dir in dirs)
            {
                //Iterating ThermalOutputDirections, as defined in the comp def. 
                foreach (var cell in dir == ThermalInOutDirection.center ? GenAdj.CellsOccupiedBy(center, rot, size) : CellsAdjacentDirection(center, rot, size, dir.ToRot4()))
                {
                    //Iterating individual cells, based on building size and rotation.
                    if (onlyUniqueRooms)
                    {
                        bool isUnique = true;
                        foreach (var loc in locs)
                        {
                            if (cell.GetRoom(map) == loc.GetRoom(map))
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

        /// <summary>
        ///     Gizmo for Changing Pipes
        /// </summary>
        /// <param name="compCoolant">Component Asking for Gizmo</param>
        /// <returns>Action Button Gizmo</returns>
        public static Command_Action GetPipeSwitchToggle(CompCoolantTrader compCoolant)
        {
            var currentSelection = compCoolant.pipeColorSelection;
            Texture2D icon;
            string label;

            switch (currentSelection)
            {

                case PipeColorSelection.Red:
                    label = SwitchPipeRedKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Red");
                    break;

                case PipeColorSelection.Blue:
                    label = SwitchPipeBlueKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Blue");
                    break;

                case PipeColorSelection.Cyan:
                    label = SwitchPipeCyanKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Cyan");
                    break;

                case PipeColorSelection.Green:
                    label = SwitchPipeGreenKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Green");
                    break;

                case PipeColorSelection.Auto:
                default:
                    label = SwitchPipeAutoKey.Translate();
                    icon = ContentFinder<Texture2D>.Get("UI/CoolantPipeSelect_Auto");
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
                    compCoolant.pipeColorSelection = currentSelection switch
                    {
                        PipeColorSelection.Auto => PipeColorSelection.Red,
                        PipeColorSelection.Red => PipeColorSelection.Blue,
                        PipeColorSelection.Blue => PipeColorSelection.Cyan,
                        PipeColorSelection.Cyan => PipeColorSelection.Green,
                        PipeColorSelection.Green => PipeColorSelection.Auto,
                        _ => DefaultPipeColorSelection
                    };
                }
            };
        }
    }
}