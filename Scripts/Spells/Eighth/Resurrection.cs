using System;
using Server.Gumps;
using Server.Targeting;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Spells.Eighth
{
    public class ResurrectionSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Resurrection", "An Corp",
            245, 9062,
            Reagent.Bloodmoss, Reagent.Garlic, Reagent.Ginseng);

        public ResurrectionSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Eighth;

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237);
            }
            else if (m == Caster)
            {
                Caster.SendLocalizedMessage(501039);
            }
            else if (!Caster.Alive)
            {
                Caster.SendLocalizedMessage(501040);
            }
            else if (!Caster.InRange(m, 2))
            {
                Caster.SendLocalizedMessage(501042);
            }
            // 기획 반영: 플레이어(코마 체크) 또는 본디드 펫(사망 체크)만 대상 가능
            else if (m.Alive && (!(m is PlayerMobile) || !((PlayerMobile)m).Coma))
            {
                Caster.SendLocalizedMessage(501041);
            }
            else if (m is BaseCreature && !((BaseCreature)m).IsDeadBondedPet)
            {
                Caster.SendLocalizedMessage(501043);
            }
            else if (this.CheckBSequence(m, true))
            {
                SpellHelper.Turn(Caster, m);

                // --- 기획: 회복량 계산 (700 ~ 1300 + 보너스 * 0.2) ---
                double bonus = SpellHelper.GetMagicValue(Caster, 0.2);
                int healAmount = Utility.RandomMinMax(700, 1300) + (int)bonus;

                if (m is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)m;
                    if (pm.Coma)
                    {
                        pm.Coma = false;
                        // pm.SendMessage("코마 상태가 해제되었습니다.");
                    }
                    
                    if (!pm.Alive)
                    {
                        pm.CloseGump(typeof(ResurrectGump));
                        pm.SendGump(new ResurrectGump(pm, Caster));
                    }
                }
                else if (m is BaseCreature)
                {
                    BaseCreature pet = (BaseCreature)m;
                    if (pet.IsDeadBondedPet)
                    {
                        pet.ResurrectPet();
                    }
                }

                // --- [수정] SpellHelper를 통한 힐 처리 ---
                // 부활 후 즉시 체력을 채워주며, 힐 관련 이펙트와 시스템 로직을 통합 관리합니다.
                SpellHelper.Heal(healAmount, m, Caster);

                m.PlaySound(0x214);
                m.FixedEffect(0x376A, 10, 16);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly ResurrectionSpell m_Owner;
            public InternalTarget(ResurrectionSpell owner) : base(2, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
