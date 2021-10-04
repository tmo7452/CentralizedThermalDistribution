using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class PlaceWorker_ThermalWorker : PlaceWorker
    {
        private Color GetWorkColor(float work)
        {
            if (work == 0)
                return Color.white;
            return (work > 0) ? GenTemperature.ColorRoomHot : GenTemperature.ColorRoomCold;
        }

        /// <summary>
        ///     Draw overlay when selected or placing.
        ///     Overlay is dynamic based on the worker's properties.
        /// </summary>
        /// <param name="def">The Thing's Def</param>
        /// <param name="center">Location</param>
        /// <param name="rot">Rotation</param>
        /// <param name="ghostCol">Ghost Color</param>
        /// <param name="thing"></param>
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var props = def.GetCompProperties<CompProperties_ThermalWorker>();
            if (props is null)
                return;
            if ((props.inputMedium != CompProperties_ThermalWorker.ThermalMedium.ambient) && (props.outputMedium != CompProperties_ThermalWorker.ThermalMedium.ambient))
                return;

            var map = Find.CurrentMap;
            Color inputColor = Color.white;
            Color outputColor = Color.white;
            // If we have an existing instance selected, we use the current work done value to calculate colors.
            var comp = thing?.TryGetComp<CompThermalWorker>();
            if (comp is not null)
            {
                inputColor = GetWorkColor(-comp.ThermalWorkDone * props.inputWorkMultiplier);
                outputColor = GetWorkColor(comp.ThermalWorkDone * props.outputWorkMultiplier);
            }
            // Otherwise:
            // If using direct or active transfer, we calculate colors from the def.
            // If using passive transfer, leave them white.
            else if ((props.thermalTransferMode == CompProperties_ThermalWorker.TransferMode.direct) || (props.thermalTransferMode == CompProperties_ThermalWorker.TransferMode.active))
            {
                inputColor = GetWorkColor(-props.totalWorkMultiplier * props.inputWorkMultiplier);
                outputColor = GetWorkColor(props.totalWorkMultiplier * props.outputWorkMultiplier);
            }

            // We  shade the room border, based on the colors we calculated.
            foreach (var cell in CompThermalWorker.GetAmbientInputLocations(def, center, rot, true, map))
            {
                var room = cell.GetRoom(map);
                if ((room is null) || room.UsesOutdoorTemperature)
                    continue; // Don't shade outside
                GenDraw.DrawFieldEdges(room.Cells.ToList(), inputColor);
            }
            foreach (var cell in CompThermalWorker.GetAmbientOutputLocations(def, center, rot, true, map))
            {
                var room = cell.GetRoom(map);
                if ((room is null) || room.UsesOutdoorTemperature)
                    continue; // Don't shade outside
                GenDraw.DrawFieldEdges(room.Cells.ToList(), outputColor);
            }

            // We also draw boxes around each input/output cell.
            // We skip "center" cells that would overlap the structure.
            // If the cell is blocked, we draw it black.
            // Else, if we have both ambient inputs *and* outputs (such as a double sided cooler), we draw them using the same colors as above.
            // Else, we draw them white to keep things looking cleaner.
            if ((props.inputMedium != CompProperties_ThermalWorker.ThermalMedium.ambient) || (props.outputMedium != CompProperties_ThermalWorker.ThermalMedium.ambient))
                inputColor = outputColor = Color.white;
            foreach (var cell in CompThermalWorker.GetAmbientInputLocations(def, center, rot))
            {
                if (GenAdj.IsInside(cell, center, rot, def.size))
                    continue;
                GenDraw.DrawFieldEdges(new List<IntVec3> { cell }, cell.Impassable(map) ? Color.black : inputColor);
            }
            foreach (var cell in CompThermalWorker.GetAmbientOutputLocations(def, center, rot))
            {
                if (GenAdj.IsInside(cell, center, rot, def.size))
                    continue;
                GenDraw.DrawFieldEdges(new List<IntVec3> { cell }, cell.Impassable(map) ? Color.black : outputColor);
            }

        }

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var props = (def as ThingDef)?.GetCompProperties<CompProperties_ThermalWorker>();
            if (props is null)
                return AcceptanceReport.WasAccepted;
            if ((props.inputMedium != CompProperties_ThermalWorker.ThermalMedium.ambient) && (props.outputMedium != CompProperties_ThermalWorker.ThermalMedium.ambient))
                return AcceptanceReport.WasAccepted;

            int blocked = 0;
            foreach (var cell in CompThermalWorker.GetAmbientInputLocations(def as ThingDef, center, rot, false))
            {
                if (cell.Impassable(map))
                    blocked++;
            }
            if (blocked > props.ambientInput_ignoredBlockedCells)
                return AcceptanceReport.WasRejected;

            blocked = 0;
            foreach (var cell in CompThermalWorker.GetAmbientOutputLocations(def as ThingDef, center, rot, false))
            {
                if (cell.Impassable(map))
                    blocked++;
            }
            if (blocked > props.ambientOutput_ignoredBlockedCells)
                return AcceptanceReport.WasRejected;

            return AcceptanceReport.WasAccepted;
        }
    }
}