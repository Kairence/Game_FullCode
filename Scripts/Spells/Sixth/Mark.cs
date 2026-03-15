using System;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.Sixth
{
    public class MarkSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mark", "Kal Por Ylem",
            218,
            9002,
            Reagent.BlackPearl,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);
        public MarkSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Sixth;
            }
        }
        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            return SpellHelper.CheckTravel(Caster, TravelCheckType.Mark);
        }
		// --- 기획: 시전 속도 보너스 적용 (보너스 * 0.0025초 감소) ---
        public override TimeSpan GetCastDelay()
        {
            // 기본 시전 시간 가져오기
            TimeSpan baseDelay = base.GetCastDelay();
            
            // 보너스 수치에 따른 감소분 계산 (예: 보너스 1000점이면 1.0초 감소)
            double bonusReduction = SpellHelper.GetMagicValue(Caster, 0.0025);
            
            // 최소 시전 시간 보장 (예: 최소 0.5초는 걸리도록 설정, 엔진 상황에 맞춰 조절 가능)
            double minDelay = 0.5;
            double finalSeconds = Math.Max(minDelay, baseDelay.TotalSeconds - bonusReduction);

            return TimeSpan.FromSeconds(finalSeconds);
        }
        public void Target(RecallRune rune)
        {
            BaseBoat boat = BaseBoat.FindBoatAt(Caster.Location, Caster.Map);

            if (!Caster.CanSee(rune))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.Mark))
            {
            }
            else if (boat == null && SpellHelper.CheckMulti(Caster.Location, Caster.Map, !Core.AOS))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (boat != null && !(boat is BaseGalleon))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501800); // You cannot mark an object at that location.
            }
            else if (!rune.IsChildOf(Caster.Backpack))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1062422); // You must have this rune in your backpack in order to mark it.
            }
            else if (CheckSequence())
            {
                rune.Mark(Caster);

                Caster.PlaySound(0x1FA);
                Effects.SendLocationEffect(Caster, Caster.Map, 14201, 16);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly MarkSpell m_Owner;
            public InternalTarget(MarkSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is RecallRune)
                {
                    m_Owner.Target((RecallRune)o);
                }
                else
                {
                    from.Send(new MessageLocalized(from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 501797, from.Name, "")); // I cannot mark that object.
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
