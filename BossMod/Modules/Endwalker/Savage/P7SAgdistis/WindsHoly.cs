﻿using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class WindsHoly : Components.StackSpread
    {
        public int NumCasts { get; private set; }
        private List<Actor>[] _futureStacks = { new(), new(), new(), new() };
        private List<Actor>[] _futureSpreads = { new(), new(), new(), new() };

        public WindsHoly() : base(6, 7, 4) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.InviolateWinds1:
                case SID.PurgatoryWinds1:
                    SpreadTargets.Add(actor);
                    break;
                case SID.InviolateWinds2:
                case SID.PurgatoryWinds2:
                    _futureSpreads[0].Add(actor);
                    break;
                case SID.PurgatoryWinds3:
                    _futureSpreads[1].Add(actor);
                    break;
                case SID.PurgatoryWinds4:
                    _futureSpreads[2].Add(actor);
                    break;
                case SID.HolyBonds1:
                case SID.HolyPurgation1:
                    StackTargets.Add(actor);
                    break;
                case SID.HolyBonds2:
                case SID.HolyPurgation2:
                    _futureStacks[0].Add(actor);
                    break;
                case SID.HolyPurgation3:
                    _futureStacks[1].Add(actor);
                    break;
                case SID.HolyPurgation4:
                    _futureStacks[2].Add(actor);
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.HemitheosHolyExpire)
            {
                StackTargets = _futureStacks[NumCasts];
                SpreadTargets = _futureSpreads[NumCasts];
                ++NumCasts;
            }
        }
    }
}
