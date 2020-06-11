﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace ZLevels
{
    public class Building_StairsUp : Building
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                Map mapUpper = ZTracker.GetUpperLevel(this.Map.Tile, this.Map);
                if (mapUpper != null)
                {
                    if (this.Position.GetThingList(mapUpper).Where(x => x.def == ZLevelsDefOf.ZL_StairsDown).Count() == 0)
                    {
                        var stairsToSpawn = ThingMaker.MakeThing(ZLevelsDefOf.ZL_StairsDown, this.Stuff);
                        GenPlace.TryPlaceThing(stairsToSpawn, this.Position, mapUpper, ThingPlaceMode.Direct);
                        stairsToSpawn.SetFaction(this.Faction);
                    }
                }
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                foreach (var dir in GenRadial.RadialCellsAround(this.Position, 20, true))
                {
                    foreach (var t in dir.GetThingList(this.Map))
                    {
                        if (t is Pawn pawn &&
                            pawn.HostileTo(this.Faction) && !pawn.mindState.MeleeThreatStillThreat
                            && GenSight.LineOfSight(this.Position, pawn.Position, this.Map))
                        {
                            var ZTracker = Current.Game.GetComponent<ZLevelsManager>();
                            
                            if (this.visitedPawns == null) this.visitedPawns = new HashSet<string>();
                            if (!this.visitedPawns.Contains(pawn.ThingID))
                            {
                                Job goToStairs = JobMaker.MakeJob(ZLevelsDefOf.ZL_GoToStairs, this);
                                pawn.jobs.jobQueue.EnqueueFirst(goToStairs);
                                this.visitedPawns.Add(pawn.ThingID);
                            }
                            else if (ZTracker.GetLowerLevel(this.Map.Tile, this.Map) != null &&
                                ZTracker.GetLowerLevel(this.Map.Tile, this.Map).mapPawns.AllPawnsSpawned.Where(x => pawn.HostileTo(x)).Any())
                            {
                                Job goToStairs = JobMaker.MakeJob(ZLevelsDefOf.ZL_GoToStairs, this);
                                pawn.jobs.jobQueue.EnqueueFirst(goToStairs);
                            }
                            else if (ZTracker.GetZIndexFor(this.Map) != 0)
                            {
                                Job goToStairs = JobMaker.MakeJob(ZLevelsDefOf.ZL_GoToStairs, this);
                                            
                                pawn.jobs.jobQueue.EnqueueFirst(goToStairs);
                            }
                        }
                    }
                }
            }
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            var text = "GoUP".Translate();
            foreach (var opt in base.GetFloatMenuOptions(selPawn))
            {
                if (opt.Label != text)
                {
                    yield return opt;
                }
            }
            var opt2 = new FloatMenuOption(text, () => {
                    ZLogger.Message("Test");
                    Job job = JobMaker.MakeJob(ZLevelsDefOf.ZL_GoToStairs, this);
                    selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                }, MenuOptionPriority.Default, null, this);
            yield return opt2;

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref shouldSpawnStairsUpper, "shouldSpawnStairsUpper");
            Scribe_Values.Look<string>(ref this.pathToPreset, "pathToPreset");
            Scribe_Collections.Look<string>(ref this.visitedPawns, "visitedPawns");
        }

        public HashSet<String> visitedPawns = new HashSet<string>();

        public string pathToPreset = "";
        public bool shouldSpawnStairsUpper = true;
    }
}
