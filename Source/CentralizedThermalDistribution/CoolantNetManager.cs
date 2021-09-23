using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static CentralizedThermalDistribution.CompCoolant;

namespace CentralizedThermalDistribution
{
    public class CoolantNetManager : MapComponent
    {
        private uint NextNetId = 1;
        private List<CompCoolantTrader> Traders = new();
        private List<CompCoolantProvider> Providers = new();
        private List<CompCoolantConsumer> Consumers = new();
        private List<CompCoolantPipe>[] Pipes;
        private List<CoolantNet>[] Nets;
        private byte[] PipeFlagGrid; // To speed up pipe lookups. One byte per map tile. One bit per pipe color.
        private bool[] DirtyPipeColor;
        private bool IsDirty;

        // Given a map tile location and a single pipe color, these get, set, or clear the appropriate bit on the PipeFlagGrid.
        private bool GetPipeFlag(IntVec3 pos, int colorIndex)
        {
            return (PipeFlagGrid[map.cellIndices.CellToIndex(pos)] & (byte)(1 << colorIndex)) > 0;
        }

        private void SetPipeFlag(IntVec3 pos, int colorIndex)
        {
            PipeFlagGrid[map.cellIndices.CellToIndex(pos)] |= (byte)(1 << colorIndex);
        }

        private void ClearPipeFlag(IntVec3 pos, int colorIndex)
        {
            PipeFlagGrid[map.cellIndices.CellToIndex(pos)] &= (byte)~(1 << colorIndex);
        }

        /// <summary>
        ///     Constructor of the Network Manager
        ///     - Init the Pipe Matrix
        ///     - Mark Dirty for 1st reconstruction
        /// </summary>
        /// <param name="map">RimWorld Map Object</param>
        public CoolantNetManager(Map map) : base(map)
        {
            //var x = PipeColorCount;
            Pipes = new List<CompCoolantPipe>[PipeColorCount];
            Nets = new List<CoolantNet>[PipeColorCount];
            PipeFlagGrid = new byte[map.AllCells.Count()];
            DirtyPipeColor = new bool[PipeColorCount]; // One bool for each pipe color

            for (var i = 0; i < PipeColorCount; i++)
            {
                Pipes[i] = new();
                Nets[i] = new();
                SetDirty(i);
            }
        }

        public void SetDirty(int pipeColorIndex)
        {
            DirtyPipeColor[pipeColorIndex] = true;
            IsDirty = true;
        }
        public void SetDirty(PipeColor pipeColor)
        {
            if (pipeColor == PipeColor.None)
                return;

            DirtyPipeColor[PipeColorToIndex(pipeColor)] = true;
            IsDirty = true;
        }

        /// <summary>
        ///     Register a Pipe to the Manager
        /// </summary>
        /// <param name="pipe">A Pipe's AirFlow Component</param>
        public void RegisterPipe(CompCoolantPipe pipe)
        {
            Pipes[PipeColorToIndex(pipe.CurrentPipeColor)].Add(pipe);
            SetPipeFlag(pipe.parent.Position, PipeColorToIndex(pipe.CurrentPipeColor));
            SetDirty(pipe.CurrentPipeColor);
        }

        /// <summary>
        ///     Remove a Pipe from the Manager
        /// </summary>
        /// <param name="pipe">The Pipe's AirFlow Component</param>
        public void DeregisterPipe(CompCoolantPipe pipe)
        {
            ClearPipeFlag(pipe.parent.Position, PipeColorToIndex(pipe.CurrentPipeColor));
            Pipes[PipeColorToIndex(pipe.CurrentPipeColor)].Remove(pipe);
            SetDirty(pipe.CurrentPipeColor);
        }

        /// <summary>
        ///     Register a coolant provider to the network manager.
        /// </summary>
        /// <param name="provider">Provider's Coolant Component</param>
        public void RegisterProvider(CompCoolantProvider provider)
        {
            Traders.Add(provider);
            Providers.Add(provider);
        }

        /// <summary>
        ///     Deregister a provider from the network manager
        /// </summary>
        /// <param name="provider">Provider's's Coolant Component</param>
        public void DeregisterProvider(CompCoolantProvider provider)
        {
            Traders.Remove(provider);
            Providers.Remove(provider);
        }

        /// <summary>
        ///     Register an Air Flow Consumer to the Network Manager
        /// </summary>
        /// <param name="consumer">Consumer's Air Flow Component</param>
        public void RegisterConsumer(CompCoolantConsumer consumer)
        {
            Traders.Add(consumer);
            Consumers.Add(consumer);
        }

        /// <summary>
        ///     Deregister a Consumer from the Network Manager
        /// </summary>
        /// <param name="consumer">Consumer's Air Flow Component</param>
        public void DeregisterConsumer(CompCoolantConsumer consumer)
        {
            Traders.Remove(consumer);
            Consumers.Remove(consumer);
        }

        /// <summary>
        ///     Lookup cooolant network by Net ID.
        /// </summary>
        /// <param name="pos">Position of the cell</param>
        /// <param name="flowType">Airflow type</param>
        /// <param name="id">GridID to check for</param>
        /// <returns>Boolean result if perfect pipe exists at cell or not</returns>
        public CoolantNet GetNet(uint id)
        {
            foreach (var NetColor in Nets)
            {
                foreach (var Net in NetColor)
                {
                    if (Net.NetID == id) return Net;
                }
            }
            return null;
        }

        public bool IsPipeAt(IntVec3 pos, PipeColor pipeColor)
        {
            if ((pipeColor == PipeColor.None) || (pipeColor == PipeColor.Trader))
                return false;
            return GetPipeFlag(pos, PipeColorToIndex(pipeColor));
        }

        public bool IsPipeAt(IntVec3 pos, int pipeColorIndex)
        {
            return GetPipeFlag(pos, pipeColorIndex);
        }

        public CompCoolantPipe GetPipeAt(IntVec3 pos, PipeColor pipeColor)
        {
            return GetPipeAt(pos, PipeColorToIndex(pipeColor));
        }

        public CompCoolantPipe GetPipeAt(IntVec3 pos, int pipeColorIndex)
        {
            foreach (var pipe in Pipes[pipeColorIndex])
            {
                if (pipe.parent.Position == pos)
                    return pipe;
            }
            return null;
        }

        /// <summary>
        ///     Update Map Event
        ///     - Check if Dirty
        ///     - If it is Dirty then Reconstruct Pipe Grids
        /// </summary>
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();

            if (!IsDirty)
            {
                return;
            }

            for (var pipeColorIndex = 0; pipeColorIndex < PipeColorCount; pipeColorIndex++)
            {
                if (DirtyPipeColor[pipeColorIndex])
                {
                    RebuildPipeGrid(pipeColorIndex);
                    DirtyPipeColor[pipeColorIndex] = false;
                }

            }
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

            foreach (var coolantNetColors in Nets)
            {
                foreach (var coolantNet in coolantNetColors)
                {
                    coolantNet.CoolantNetTick();
                }
            }

            base.MapComponentTick();
        }

        /// <summary>
        ///     Main rebuild function. We Rebuild all different pipetypes here.
        /// </summary>
        /// <param name="pipeColorIndex">Type of Pipe (Red, Blue, Cyan) as an integer</param>
        private void RebuildPipeGrid(int pipeColorIndex)
        {

            // Destroy and reset the list of networks for this color
            foreach (var net in Nets[pipeColorIndex])
            {
                net.DestroyNet();
            }
            Nets[pipeColorIndex] = new();

            // Create new nets
            foreach (var pipe in Pipes[pipeColorIndex])
            {
                // Looking for pipes that have not yet been registered to a net.
                if (pipe.NetID == 0)
                {
                    Nets[pipeColorIndex].Add(CreateCoolantNet(pipe, pipeColorIndex));
                }
            }

            // Assign new nets to attached traders
            foreach (var trader in Traders)
                trader.RescanNets();
        }

        // Given a single starting pipe, follow adjacent pipes and create a CoolantNet containing them.
        // Returns the CoolentNet it created.
        private CoolantNet CreateCoolantNet(CompCoolantPipe firstPipe, int pipeColorIndex)
        {
            CoolantNet newNet = new(NextNetId++, PipeColorFromIndex(pipeColorIndex));

            Stack<CompCoolantPipe> pendingPipes = new();
            pendingPipes.Push(firstPipe);

            while (pendingPipes.Count > 0)
            {
                var pipe = pendingPipes.Pop();
                if (pipe.NetID != 0) { continue; } // We already got this one, move on.

                newNet.Pipes.Add(pipe);
                pipe.SetNet(newNet);

                // Check adjacent cells for pipes and add them to the stack.
                foreach (var direction in GenAdj.CardinalDirections)
                {
                    if (IsPipeAt(pipe.parent.Position + direction, pipeColorIndex))
                        pendingPipes.Push(GetPipeAt(pipe.parent.Position + direction, pipeColorIndex));
                }
            }

            return newNet;
        }
    }
}