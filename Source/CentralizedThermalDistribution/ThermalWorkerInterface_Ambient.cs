using System.Collections.Generic;
using Verse;
using static CentralizedThermalDistribution.CompThermalWorker;

namespace CentralizedThermalDistribution
{
    class ThermalWorkerInterface_Ambient : ThermalWorkerInterface
    {
        public ThermalWorkerInterface_Ambient(CompThermalWorker thermalWorker, bool isInput, ThingWithComps building, int ignoredBlockedCells) : base(thermalWorker, isInput)
        {
            ParentBuilding = building;
            IgnoredBlockedCells = -ignoredBlockedCells;
        }

        private ThingWithComps ParentBuilding;
        private int IgnoredBlockedCells;
        private List<TemperatureCell> TempCells = new();

        public override void Update()
        {
            base.Update();
            List<IntVec3> allCells;
            List<IntVec3> uniqueCells;
            if (IsInput)
            {
                allCells = GetAmbientInputLocations(ParentBuilding.def, ParentBuilding.Position, ParentBuilding.Rotation, false);
                uniqueCells = GetAmbientInputLocations(ParentBuilding.def, ParentBuilding.Position, ParentBuilding.Rotation, true, ParentBuilding.Map);
            }
            else
            {
                allCells = GetAmbientOutputLocations(ParentBuilding.def, ParentBuilding.Position, ParentBuilding.Rotation, false);
                uniqueCells = GetAmbientOutputLocations(ParentBuilding.def, ParentBuilding.Position, ParentBuilding.Rotation, true, ParentBuilding.Map);
            }

            Temperature = 0f;
            int totalCells = allCells.Count;
            int blockedCells = 0;
            foreach (var cell in allCells)
            {
                if (cell.Impassable(ParentBuilding.Map))
                    blockedCells++;
                else
                    Temperature += cell.GetTemperature(ParentBuilding.Map);
            }
            int openCells = totalCells - blockedCells;
            if (openCells == 0)
            {
                Status = ThermalWorkerStatus.offline;
                StatusReason = "AllCellsBlocked";
                Temperature = null;
                return;
            }
            Temperature /= openCells;

            // IgnoredBlockedCells gets factored into efficiency calculations
            if (blockedCells <= IgnoredBlockedCells)
                blockedCells = 0;
            Efficiency = (allCells.Count - (float)blockedCells) / allCells.Count;

            TempCells.Clear();
            foreach (var unique in uniqueCells)
            {
                int count = 0;
                foreach (var cell in allCells)
                    if (unique.GetRoom(ParentBuilding.Map) == cell.GetRoom(ParentBuilding.Map))
                        count++;
                TempCells.Add(new TemperatureCell { pos = unique, factor = count / (allCells.Count - blockedCells) });
            }

            Multiplier = Efficiency.Value;
            
        }

        public override void PushWork(float work)
        {
            base.PushWork(work);
            foreach (var cell in TempCells)
            {
                GenTemperature.PushHeat(cell.pos, ParentBuilding.Map, work * cell.factor);
            }
        }

        private struct TemperatureCell
        {
            public IntVec3 pos;
            public float factor;
        }
    }
}
