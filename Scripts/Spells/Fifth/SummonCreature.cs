using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Targeting;
using AutoUserConnect; // SummonEntry와 SummonPool이 있는 네임스페이스

namespace Server.Spells.Fifth
{
    public class SummonCreatureSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Summon Creature", "Kal Xen",
            215, 9002, false,
            Reagent.Bloodmoss, Reagent.MandrakeRoot, Reagent.SpidersSilk);

        public SummonCreatureSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Fifth;

        public override bool CheckCast()
        {
            if (!base.CheckCast()) return false;

            if ((Caster.Followers + 1) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers...
                return false;
            }
            return true;
        }

        public override void OnCast()
{		
			if (CheckSequence())
			{
				// 매니저에게 조건에 맞는 랜덤 타입 하나를 받아옵니다.
				Type chosen = SummonPoolManager.GetEligibleAnimal(Caster);
				BaseCreature summon = Activator.CreateInstance(chosen) as BaseCreature;

				if (summon != null)
				{
					summon.ControlSlots = 1;
					SpellHelper.Summon(summon, Caster, 0x215, TimeSpan.FromMinutes(1.0), false, false);
				}
			}
			FinishSequence();
		}

        public override TimeSpan GetCastDelay() => base.GetCastDelay();
    }
}