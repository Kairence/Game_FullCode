using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Sixth
{
    public class DispelSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Dispel", "An Ort",
            218,
            9002,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);

        public DispelSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Sixth;

        public override void OnCast()
        {
            this.Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else
            {
                BaseCreature bc = m as BaseCreature;

                // 1. 소환수 여부 확인
                bool isSummoned = (bc != null && bc.Summoned);
                
                // 2. 정령 여부 확인 (ElementalBan 슬레이어 그룹 활용)
                // SlayerGroup.GetEntryByName을 통해 ElementalBan 그룹에 속한 몹인지 체크합니다.
                SlayerEntry entry = SlayerGroup.GetEntryByName(SlayerName.ElementalBan);
                bool isElemental = (entry != null && entry.Slays(m));

                // 소환수도 아니고 정령도 아니면 종료
                if (!isSummoned && !isElemental)
                {
                    return;
                }
                else if (CheckHSequence(m))
                {
                    SpellHelper.Turn(Caster, m);

                    // 데미지 계산 (250 ~ 500 + 보너스)
                    int damage = this.GetNewAosDamage(150, 250, 500, m);

                    // 기존 디스펠 이펙트 연출
                    Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                    Effects.PlaySound(m, m.Map, 0x201);

                    // 화염 속성 데미지 적용 (100% Fire)
                    SpellHelper.Damage(this, m, damage, 0, 100, 0, 0, 0);

                    this.HarmfulSpell(m);
                }
            }

            this.FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly DispelSpell m_Owner;

            public InternalTarget(DispelSpell owner)
                : base(12, false, TargetFlags.Harmful)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    this.m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }
        }
    }
}