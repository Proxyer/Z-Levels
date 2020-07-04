﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ZLevels
{
	[HarmonyPatch(typeof(DynamicDrawManager), "DrawDynamicThings")]
	public static class GenerateGraphics
	{
		[HarmonyPostfix]
		public static void DynamicDrawManagerPostfix(DynamicDrawManager __instance, Map ___map, HashSet<Thing> ___drawThings, ref bool ___drawingNow)
		{
			try
			{
				var ZTracker = ZUtils.ZTracker;
				int curLevel = ZTracker.GetZIndexFor(___map);
				foreach (var map2 in ZTracker.GetAllMaps(___map.Tile)
						.OrderBy(x => ZTracker.GetZIndexFor(x)))
				{
					int baseLevel = ZTracker.GetZIndexFor(map2);
					if (curLevel > baseLevel && baseLevel >= 0)
					{
						if (!DebugViewSettings.drawThingsDynamic)
						{
							return;
						}
						___drawingNow = true;
						try
						{
							bool[] fogGrid = map2.fogGrid.fogGrid;
							CellRect cellRect = Find.CameraDriver.CurrentViewRect;
							cellRect.ClipInsideMap(map2);
							cellRect = cellRect.ExpandedBy(1);
							CellIndices cellIndices = map2.cellIndices;
							foreach (Thing thing in ___drawThings)
							{
								IntVec3 position = thing.Position;
								if (position.GetTerrain(___map) == ZLevelsDefOf.ZL_OutsideTerrain)
								{
									if ((cellRect.Contains(position) || thing.def.drawOffscreen)
										//&& (!fogGrid[cellIndices.CellToIndex(position)]
										//|| thing.def.seeThroughFog) 
										&& (thing.def.hideAtSnowDepth >= 1f
										|| map2.snowGrid.GetDepth(position) <= thing.def.hideAtSnowDepth))
									{
										try
										{
											if (thing.Graphic is Graphic_Mote || thing.Graphic is Graphic_Linked)
											{

											}
											else if (thing.Graphic is Graphic_LinkedCornerFiller
												|| thing.Graphic is Graphic_RandomRotated)
											{
												thing.Draw();
											}
											else if (thing is Pawn pawn)
											{
												var newRenderer = new PawnRendererScaled(pawn);
												pawn.Drawer.renderer.graphics.ResolveAllGraphics();
												newRenderer.graphics.nakedGraphic = pawn.Drawer.renderer.graphics.nakedGraphic;
												newRenderer.graphics.headGraphic = pawn.Drawer.renderer.graphics.headGraphic;
												newRenderer.graphics.hairGraphic = pawn.Drawer.renderer.graphics.hairGraphic;
												newRenderer.graphics.rottingGraphic = pawn.Drawer.renderer.graphics.rottingGraphic;
												newRenderer.graphics.dessicatedGraphic = pawn.Drawer.renderer.graphics.dessicatedGraphic;
												newRenderer.graphics.apparelGraphics = pawn.Drawer.renderer.graphics.apparelGraphics;
												newRenderer.graphics.packGraphic = pawn.Drawer.renderer.graphics.packGraphic;
												newRenderer.graphics.flasher = pawn.Drawer.renderer.graphics.flasher;
												newRenderer.RenderPawnAt(thing.DrawPos, curLevel, baseLevel);
											}
											else
											{
												Vector2 drawSize = thing.Graphic.drawSize;
												drawSize.x *= 1f - (((float)(curLevel) - (float)baseLevel) / 5f);
												drawSize.y *= 1f - (((float)(curLevel) - (float)baseLevel) / 5f);
												var newGraphic = thing.Graphic.GetCopy(drawSize);
												newGraphic.Draw(thing.DrawPos, thing.Rotation, thing);
											}
										}
										catch (Exception ex)
										{
											Log.Error(string.Concat(new object[]
											{
											"Exception drawing ",
											thing,
											": ",
											ex.ToString()
											}), false);
										}
									}
								}
							}
						}
						catch (Exception arg)
						{
							Log.Error("Exception drawing dynamic things: " + arg, false);
						}
						___drawingNow = false;
					}
				}
			}
			catch { };
		}
	}
}

