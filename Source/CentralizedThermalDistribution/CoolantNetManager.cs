using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class CoolantNetManager : MapComponent
    {
        private const int RebuildValue = -2;

        private readonly List<CoolantNet> _backupNets = new List<CoolantNet>();
        private readonly int _pipeColors;
        private int _masterId;
        public List<CompCoolantConditioner> CachedConditioners = new();
        public List<CompCoolantConsumer> CachedConsumers = new();
        public List<CoolantNet> CachedNets = new();
        public List<CompCoolant> CachedPipes = new();
        public List<CompCoolantProducer> CachedProducers = new List<CompCoolantProducer>();
        //public List<CompCoolantFlowCCU> CachedTempControls = new List<CompCoolantFlowCCU>();
        public bool[] DirtyPipeFlag;
        public bool IsDirty;

        public int[,] PipeGrid;

        /// <summary>
        ///     Constructor of the Network Manager
        ///     - Init the Pipe Matrix
        ///     - Mark Dirty for 1st reconstruction
        /// </summary>
        /// <param name="map">RimWorld Map Object</param>
        public CoolantNetManager(Map map) : base(map)
        {
            _pipeColors = Enum.GetValues(typeof(CoolantPipeColor)).Length;
            //var num = map.AllCells.Count();
            //PipeGrid = new int[_pipeColors, num];
            PipeGrid = new int[_pipeColors, map.AllCells.Count()];

            DirtyPipeFlag = new bool[_pipeColors];
            for (var i = 0; i < _pipeColors; i++)
            {
                DirtyPipeFlag[i] = true;

                for (var j = 0; j < PipeGrid.GetLength(1); j++)
                {
                    PipeGrid[i, j] = RebuildValue;
                }
            }

            IsDirty = true;
        }

        /// <summary>
        ///     Register a Pipe to the Manager
        /// </summary>
        /// <param name="pipe">A Pipe's AirFlow Component</param>
        public void RegisterPipe(CompCoolant pipe)
        {
            if (!CachedPipes.Contains(pipe))
            {
                CachedPipes.Add(pipe);
                CachedPipes.Shuffle(); // ! Why Shuffle?  --Brain
            }

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        /// <summary>
        ///     Remove a Pipe from the Manager
        /// </summary>
        /// <param name="pipe">The Pipe's AirFlow Component</param>
        public void DeregisterPipe(CompCoolant pipe)
        {
            if (CachedPipes.Contains(pipe))
            {
                CachedPipes.Remove(pipe);
                CachedPipes.Shuffle(); // ! Why Shuffle?  --Brain
            }

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        
        /// <summary>
        ///     Register a Climate Control Device
        /// </summary>
        /// <param name="device">Climate Control Component</param>
        public void RegisterTempControl(CompCoolantCCU device)
        {
            /*
            if (!CachedTempControls.Contains(device))
            {
                CachedTempControls.Add(device);
                CachedTempControls.Shuffle(); // ! Why Shuffle?  --Brain
            }*/

            // Useless function call --Brain
            // DirtyPipeGrid();
            IsDirty = true;
            
        }

        
        /// <summary>
        ///     Deregister a Climate Control Object from the Manager
        /// </summary>
        /// <param name="device">Climate Control Component</param>
        public void DeregisterTempControl(CompCoolantCCU device)
        {
            /*
            if (CachedTempControls.Contains(device))
            {
                CachedTempControls.Remove(device);
                CachedTempControls.Shuffle(); // ! Why Shuffle?  --Brain
            }*/

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        /// <summary>
        ///     Register a Air Flow Producer
        /// </summary>
        /// <param name="pipe">Producer's Air Flow Component</param>
        public void RegisterConditioner(CompCoolantConditioner pipe)
        {
            if (!CachedConditioners.Contains(pipe))
            {
                CachedConditioners.Add(pipe);
                CachedConditioners.Shuffle(); // ! Why Shuffle?  --Brain
            }

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        /// <summary>
        ///     Deregister a Producer from the Network Manager
        /// </summary>
        /// <param name="pipe">Producer's Component</param>
        public void DeregisterConditioner(CompCoolantConditioner pipe)
        {
            if (CachedConditioners.Contains(pipe))
            {
                CachedConditioners.Remove(pipe);
                CachedConditioners.Shuffle(); // ! Why Shuffle?  --Brain
            }

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        /// <summary>
        ///     Register a Air Flow Producer
        /// </summary>
        /// <param name="pipe">Producer's Air Flow Component</param>
        public void RegisterProducer(CompCoolantProducer pipe)
        {
            if (!CachedProducers.Contains(pipe))
            {
                CachedProducers.Add(pipe);
                CachedProducers.Shuffle(); // ! Why Shuffle?  --Brain
            }

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        /// <summary>
        ///     Deregister a Producer from the Network Manager
        /// </summary>
        /// <param name="pipe">Producer's Component</param>
        public void DeregisterProducer(CompCoolantProducer pipe)
        {
            if (CachedProducers.Contains(pipe))
            {
                CachedProducers.Remove(pipe);
                CachedProducers.Shuffle(); // ! Why Shuffle?  --Brain
            }

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        /// <summary>
        ///     Register an Air Flow Consumer to the Network Manager
        /// </summary>
        /// <param name="device">Consumer's Air Flow Component</param>
        public void RegisterConsumer(CompCoolantConsumer device)
        {
            if (!CachedConsumers.Contains(device))
            {
                CachedConsumers.Add(device);
                CachedConsumers.Shuffle(); // ! Why Shuffle?  --Brain
            }

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        /// <summary>
        ///     Deregister a Consumer from the Network Manager
        /// </summary>
        /// <param name="device">Consumer's Air Flow Component</param>
        public void DeregisterConsumer(CompCoolantConsumer device)
        {
            if (CachedConsumers.Contains(device))
            {
                CachedConsumers.Remove(device);
                CachedConsumers.Shuffle(); // ! Why Shuffle?  --Brain
            }

            // Useless function call  --Brain
            // DirtyPipeGrid();
            IsDirty = true;
        }

        // ? Why are those two methods even here? IsDirty is public, so why raise further calls? --Brain

        // <summary>
        // Dirty the flag for reconstruction
        // </summary>
        /* public void DirtyPipeGrid()
        {
            IsDirty = true;
        } */

        // <summary>
        // Dirty the flag for reconstruction
        // </summary>
        /* public void DirtyPipeWholeGrid()
        {
            IsDirty = true;
        } */

        /// <summary>
        ///     Check if that Zone in the Pipe Matrix has a Pipe of some sort or not.
        /// </summary>
        /// <param name="pos">Position of the cell</param>
        /// <param name="flowType">Airflow type</param>
        /// <returns>Boolean result if pipe exists at cell or not</returns>
        public bool ZoneAt(IntVec3 pos, CoolantPipeColor pipeColor)
        {
            return PipeGrid[(int)pipeColor, map.cellIndices.CellToIndex(pos)] != RebuildValue;
        }

        /// <summary>
        ///     Same as ZoneAt but also checks for the GridID in the Pipe Matrix
        /// </summary>
        /// <param name="pos">Position of the cell</param>
        /// <param name="flowType">Airflow type</param>
        /// <param name="id">GridID to check for</param>
        /// <returns>Boolean result if perfect pipe exists at cell or not</returns>
        public bool PerfectMatch(IntVec3 pos, CoolantPipeColor pipeColor, int id)
        {
            return PipeGrid[(int)pipeColor, map.cellIndices.CellToIndex(pos)] == id;
        }

        /// <summary>
        ///     Update Map Event
        ///     - Check if Dirty
        ///     - If it is Dirty then Reconstruct Pipe Grids
        ///     - Reset Dirty Flags and Update the Cached Variables storing info on the Networks
        /// </summary>
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();

            if (!IsDirty)
            {
                return;
            }

            foreach (var compCoolant in CachedPipes)
            {
                compCoolant.GridID = RebuildValue;
            }

            _backupNets.Clear();

            for (var i = 0; i < _pipeColors; i++)
            {
                if ((CoolantPipeColor)i == CoolantPipeColor.Any)
                {
                    continue;
                }

                RebuildPipeGrid(i);
            }

            CachedNets = _backupNets;

//             TODO: Not Optimized
            map.mapDrawer.WholeMapChanged(MapMeshFlag.Buildings);
            map.mapDrawer.WholeMapChanged(MapMeshFlag.Things);

            IsDirty = false;
        }

        /// <summary>
        ///     Tick of Map Component. Here we tick all the Air Networks that are built.
        /// </summary>
        public override void MapComponentTick()
        {
            if (IsDirty)
            {
                return;
            }

            foreach (var coolantNet in CachedNets)
            {
                coolantNet.CoolantNetTick();
            }

            base.MapComponentTick();
        }

        /// <summary>
        ///     Iterate on all the Occupied cells of a Cell. Here we can each Parent Occupied Rect cell.
        /// </summary>
        /// <param name="compCoolant">The Object under scan</param>
        /// <param name="gridId">Grid ID of the current Network</param>
        /// <param name="pipeColor">Type of Air Flow</param>
        /// <param name="network">The Air Flow Network Object</param>
        private void ParseParentCell(CompCoolant compCoolant, int gridId, int pipeColor, CoolantNet network)
        {
            foreach (var current in compCoolant.parent.OccupiedRect().EdgeCells)
            {
                ScanCell(current, gridId, pipeColor, network);
            }
        }

        /// <summary>
        ///     Here we check for neighbouring Buildings and Pipes at `pos` param.
        ///     If we find the same Flow Type pipe or a Building (which hasnt been selected yet), then we add them to the list and
        ///     assign the same GridID.
        /// </summary>
        /// <param name="pos">Position of Cell to scan</param>
        /// <param name="gridId">Grid ID of the current Network</param>
        /// <param name="flowIndex">Type of Air Flow</param>
        /// <param name="network">The Air Flow Network Object</param>
        public void ScanCell(IntVec3 pos, int gridId, int pipeColor, CoolantNet network)
        {
            for (var i = 0; i < 4; i++)
            {
                //var thingList = (pos + GenAdj.CardinalDirections[i]).GetThingList(map);
                //var buildingList = thingList.OfType<Building>();
                //var buildingList = (pos + GenAdj.CardinalDirections[i]).GetThingList(map).OfType<Building>();

                var compList = new List<CompCoolant>();

                //foreach (var current in buildingList)
                foreach (var current in (pos + GenAdj.CardinalDirections[i]).GetThingList(map).OfType<Building>())
                {
                    //var buildingAirComps = current.GetComps<CompAirFlow>().Where(item => item.FlowType == (AirFlowType)flowIndex || (item.FlowType == AirFlowType.Any && item.GridID == RebuildValue));

                    //foreach (var buildingAirComp in buildingAirComps)
                    foreach (var buildingAirComp in current.GetComps<CompCoolant>()
                        .Where(item =>
                            item.pipeColor == (CoolantPipeColor)pipeColor ||
                            item.pipeColor == CoolantPipeColor.Any && item.GridID == RebuildValue))
                    {
                        // var result = ValidateBuildingPriority(buildingAirComp, network);
                        // if(!result)
                        if (!ValidateBuildingPriority(buildingAirComp, network))
                        {
                            continue;
                        }

                        ValidateBuilding(buildingAirComp, network);
                        compList.Add(buildingAirComp);
                    }
                }

                if (!compList.Any())
                {
                    continue;
                }

                foreach (var compCoolant in compList)
                {
                    if (compCoolant.GridID != -2)
                    {
                        continue;
                    }

                    //var iterator = compAirFlow.parent.OccupiedRect().GetIterator();
                    //while (!iterator.Done())
                    foreach (var item in compCoolant.parent.OccupiedRect())
                    {
                        PipeGrid[pipeColor, map.cellIndices.CellToIndex(item)] = gridId;
                    }

                    compCoolant.GridID = gridId;
                    ParseParentCell(compCoolant, gridId, pipeColor, network);
                }
            }
        }

        /// <summary>
        ///     Main rebuild function. We Rebuild all different pipetypes here.
        /// </summary>
        /// <param name="pipeColorIndex">Type of Pipe (Red, Blue, Cyan) as an integer</param>
        private void RebuildPipeGrid(int pipeColorIndex)
        {
            var pipeColor = (CoolantPipeColor)pipeColorIndex;

            var runtimeNets = new List<CoolantNet>();

            for (var i = 0; i < PipeGrid.GetLength(1); i++)
            {
                PipeGrid[pipeColorIndex, i] = RebuildValue;
            }

            var cachedPipes = CachedPipes.Where(item => item.pipeColor == pipeColor).ToList();

#if DEBUG
            Debug.Log("--- Start Rebuilding --- For Index: " + flowType);
            PrintPipes(cachedPipes);
#endif

            var listCopy = new List<CompCoolant>(cachedPipes);

            for (var compCoolant = listCopy.FirstOrDefault();
                compCoolant != null;
                compCoolant = listCopy.FirstOrDefault())
            {
                compCoolant.GridID = _masterId;

                var network = new CoolantNet
                {
                    GridID = compCoolant.GridID,
                    FlowType = pipeColor
                };
                //network.GridID = compAirFlow.GridID;
                //network.FlowType = flowType;
                _masterId++;

                /* -------------------------------------------
                 *
                 * Scan the Position - Get all Buildings - And Assign to Network if Priority Allows
                 *
                 * -------------------------------------------
                 */
                //var thingList = compAirFlow.parent.Position.GetThingList(map);
                //var buildingList = thingList.OfType<Building>();
                //var buildingList = compAirFlow.parent.Position.GetThingList(map).OfType<Building>();
                //foreach (var current in buildingList)
                foreach (var building in compCoolant.parent.Position.GetThingList(map).OfType<Building>())
                {
                    //var buildingAirComps = current.GetComps<CompAirFlow>().Where(item => item.FlowType == AirFlowType.Any && item.GridID == RebuildValue);

                    //foreach (var buildingAirComp in buildingAirComps)
                    foreach (var buildingCoolantComp in building.GetComps<CompCoolant>()
                        .Where(item => item.pipeColor == CoolantPipeColor.Any && item.GridID == RebuildValue))
                    {
                        //var result = ValidateBuildingPriority(buildingAirComp, network);
                        //if (!result)
                        if (!ValidateBuildingPriority(buildingCoolantComp, network))
                        {
                            continue;
                        }

                        ValidateBuilding(buildingCoolantComp, network);
                        //var itr = buildingAirComp.parent.OccupiedRect().GetIterator();
                        //while (!itr.Done())
                        foreach (var item in buildingCoolantComp.parent.OccupiedRect())
                        {
                            PipeGrid[pipeColorIndex, map.cellIndices.CellToIndex(item)] = compCoolant.GridID;
                        }

                        buildingCoolantComp.GridID = compCoolant.GridID;
                    }
                }

                /* -------------------------------------------
                 *
                 * Iterate the OccupiedRect of the Original compCoolant (This is the Pipe)
                 * So, We add the Pipe to the Grid.
                 *
                 * -------------------------------------------
                 */
                //var iterator = compAirFlow.parent.OccupiedRect().GetIterator();
                //while (!iterator.Done())
                foreach (var item in compCoolant.parent.OccupiedRect())
                {
                    PipeGrid[pipeColorIndex, map.cellIndices.CellToIndex(item)] = compCoolant.GridID;
                }

                ParseParentCell(compCoolant, compCoolant.GridID, pipeColorIndex, network);
                listCopy.RemoveAll(item => item.GridID != RebuildValue);

                network.CoolantNetTick();
#if DEBUG
                Debug.Log(network.DebugString());
#endif
                runtimeNets.Add(network);
            }

            DirtyPipeFlag[pipeColorIndex] = false;
#if DEBUG
            Debug.Log("--- Done Rebuilding ---");
#endif
            _backupNets.AddRange(runtimeNets);
        }

        /// <summary>
        ///     Validate a Building. Check if it is a Consumer, Producer or Climate Control. If so, Add it to the network.
        /// </summary>
        /// <param name="compAirFlow">Building Component</param>
        /// <param name="network">Current Network</param>
        private static void ValidateBuilding(CompCoolant compCoolant, CoolantNet network)
        {
            ValidateAsConditioner(compCoolant, network);
            ValidateAsProducer(compCoolant, network);
            //ValidateAsTempControl(compAirFlow, network);
            ValidateAsConsumer(compCoolant, network);
        }

        /// <summary>
        ///     Validate as a Air Flow Consumer
        /// </summary>
        /// <param name="compAirFlow">Building Component</param>
        /// <param name="network">Current Network</param>
        private static void ValidateAsConsumer(CompCoolant compAirFlow, CoolantNet network)
        {
            if (!(compAirFlow is CompCoolantConsumer consumer))
            {
                return;
            }

            if (!network.Consumers.Contains(consumer))
            {
                network.Consumers.Add(consumer);
            }

            consumer.coolantNet = network;
        }

        /// <summary>
        ///     Check Building Priority. If the Building is a Consumer, we can check for Priority.
        ///     If the Priority is Auto, then we skip the priority check
        ///     else we check if the Network air type matches the Priority. If it does match we add it to the network. Else we skip
        ///     it.
        /// </summary>
        /// <param name="compAirFlow">Building Component</param>
        /// <param name="network">Current Network</param>
        /// <returns>Result if we can add the Building to existing Network</returns>
        private static bool ValidateBuildingPriority(CompCoolant compAirFlow, CoolantNet network)
        {
            if (compAirFlow == null)
            {
                return false;
            }

            if (!(compAirFlow is CompCoolantConsumer consumer))
            {
                return true;
            }

            var priority = consumer.AirTypePriority;

            if (priority == CoolantPipeColorPriority.Auto)
            {
                return true;
            }

            return (int)priority == (int)network.FlowType;
        }

        /// <summary>
        ///     Validate Building as coolant conditioner
        /// </summary>
        /// <param name="compCoolant">Building Component</param>
        /// <param name="network">Current Network</param>
        private static void ValidateAsConditioner(CompCoolant compCoolant, CoolantNet network)
        {
            if (!(compCoolant is CompCoolantConditioner conditioner))
            {
                return;
            }

            if (!network.Conditioners.Contains(conditioner))
            {
                network.Conditioners.Add(conditioner);
            }

            conditioner.coolantNet = network;
        }

        /// <summary>
        ///     Validate Building as Air Flow Producer
        /// </summary>
        /// <param name="compAirFlow">Building Component</param>
        /// <param name="network">Current Network</param>
        private static void ValidateAsProducer(CompCoolant compAirFlow, CoolantNet network)
        {
            if (!(compAirFlow is CompCoolantProducer producer))
            {
                return;
            }

            if (!network.Producers.Contains(producer))
            {
                network.Producers.Add(producer);
            }

            producer.coolantNet = network;
        }

        /*
        /// <summary>
        ///     Validate Building as Climate Control Building
        /// </summary>
        /// <param name="compAirFlow">Building Component</param>
        /// <param name="network">Current Network</param>
        private static void ValidateAsTempControl(CompCoolantFlow compAirFlow, CoolantNet network)
        {
            if (!(compAirFlow is CompCoolantFlowCCU tempControl))
            {
                return;
            }

            if (!network.TempControls.Contains(tempControl))
            {
                network.TempControls.Add(tempControl);
            }

            tempControl.AirFlowNet = network;
        }*/

        /// <summary>
        ///     Print the Pipes for Debug
        /// </summary>
        /// <param name="comps">Pipe List</param>
        private void PrintPipes(IEnumerable<CompCoolant> comps)
        {
            var str = "\nPrinting CompAirFlows -";

            foreach (var compCoolant in comps)
            {
                str += "\n  - " + compCoolant.parent + " (GRID ID: " + compCoolant.GridID + ") ";
            }

            Debug.Log(str);
        }
    }
}