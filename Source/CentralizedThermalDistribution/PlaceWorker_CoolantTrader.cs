using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    public class PlaceWorker_CoolantTrader : PlaceWorker
    {
        private struct ColoredCell
        {
            public IntVec3 cell;
            public Color color;
        }

        private Color GetWorkColor(ThingDef def, bool invert = false)
        {
            float? work = def?.GetCompProperties<CompProperties_Coolant>()?.traderThermalWorkMultiplier;
            if (work is not null)
                return GetWorkColor(work.Value, invert);
            return Color.white;
        }
        private Color GetWorkColor(ThingWithComps thing, bool invert = false)
        {
            float? work = thing?.GetComp<CompCoolantTrader>()?.ThermalWork;
            if (work is not null)
                return GetWorkColor(work.Value, invert);
            return Color.white;
        }

        private Color GetWorkColor(float work, bool invert = false)
        {
            if (work != 0)
            {
                if (invert)
                    return (work > 0) ? GenTemperature.ColorRoomHot : GenTemperature.ColorRoomCold;
                return (work > 0) ? GenTemperature.ColorRoomCold : GenTemperature.ColorRoomHot;
            }
            return Color.white;
        }

        /// <summary>
        ///     Draw overlay when selected or placing.
        ///     Overlay is dynamic based on the trader's properties.
        /// </summary>
        /// <param name="def">The Thing's Def</param>
        /// <param name="center">Location</param>
        /// <param name="rot">Rotation</param>
        /// <param name="ghostCol">Ghost Color</param>
        /// <param name="thing"></param>
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var thingType = def.GetCompProperties<CompProperties_Coolant>()?.compClass;
            if (thingType is null) return;

            var map = Find.CurrentMap;

            List<ColoredCell> boxes = new();
            List<ColoredCell> areas = new();
            
            if (thingType == typeof(CompCoolantConsumerCoilVent))
            {
                // White boxes for output cells
                foreach (var loc in CompCoolantTrader.GetOutputLocations(def, center, rot, false))
                {
                    boxes.Add(new ColoredCell { cell = loc, color = Color.white });
                }

                // Room border based on the current operating direction (GetWorkColor handles null things)
                foreach (var loc in CompCoolantTrader.GetOutputLocations(def, center, rot, true, map))
                {
                    areas.Add(new ColoredCell { cell = loc, color = GetWorkColor(thing as ThingWithComps) });
                }
            }
            // For most providers, we highlight the room border, with color based on the work direction from the def
            else if (thingType == typeof(CompCoolantProviderAirSourceCondenser))
            {
                foreach (var loc in CompCoolantTrader.GetOutputLocations(def, center, rot, true, map))
                {
                    areas.Add(new ColoredCell { cell = loc, color = GetWorkColor(def) });
                }

                // Condensers also need to show boxes for adjacent cells, which should not be blocked.
                foreach (var cell in GenAdj.CellsAdjacentCardinal(center, rot, def.size))
                {
                    foreach (var loc in CompCoolantTrader.GetOutputLocations(def, center, rot, false))
                    {
                        boxes.Add(new ColoredCell { cell = loc, color = cell.Impassable(map) ? Color.red : Color.white });
                    }
                }
            }
            else if (thingType == typeof(CompCoolantProviderElectricFurnace))
            {
                foreach (var loc in CompCoolantTrader.GetOutputLocations(def, center, rot, true, map))
                {
                    areas.Add(new ColoredCell { cell = loc, color = GetWorkColor(def) });
                }
            }
            else if (thingType == typeof(CompCoolantProviderFueledFurnace))
            {
                foreach (var loc in CompCoolantTrader.GetOutputLocations(def, center, rot, true, map))
                {
                    areas.Add(new ColoredCell { cell = loc, color = GetWorkColor(def) });
                }
            }

            foreach (var box in boxes)
            {
                GenDraw.DrawFieldEdges(new List<IntVec3> { box.cell }, box.color);
            }

            foreach (var area in areas)
            {
                var room = area.cell.GetRoom(map);
                if ((room is null) || room.UsesOutdoorTemperature) continue;
                GenDraw.DrawFieldEdges(room.Cells.ToList(), area.color);
            }
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (var cell in CompCoolantTrader.GetOutputLocations(def as ThingDef, center, rot, false))
            {
                if (cell.Impassable(map))
                    return AcceptanceReport.WasRejected;
            }
            return AcceptanceReport.WasAccepted;
        }
    }
}