﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ZLevels
{
    public class JobDriver_GoToMap : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => ZUtils.ZTracker.jobTracker.ContainsKey(pawn) && ZUtils.ZTracker.jobTracker[pawn].failIfTargetMapIsNotDest 
                && ZUtils.ZTracker.jobTracker[pawn].target.Map != ZUtils.ZTracker.jobTracker[pawn].targetDest.Map);
            foreach (var toil in Toils_ZLevels.FindRouteWithStairs(GetActor(), ZUtils.ZTracker.jobTracker[pawn].targetDest, this))
            {
                yield return toil;
            }
        }
    }
}

