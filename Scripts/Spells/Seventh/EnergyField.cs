using System;
using System.Collections.Generic;
using System.Linq;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Spells.Seventh
{
    public class EnergyFieldSpell : MagerySpell
    {
        public override DamageType SpellDamageType => DamageType.SpellAOE;

        private static readonly SpellInfo m_Info = new SpellInfo(
            "Energy Field", "In Sanct Grav",
            221, 9022, false,
            Reagent.BlackPearl, Reagent.MandrakeRoot, Reagent.SpidersSilk, Reagent.SulfurousAsh);

        public EnergyFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Seventh;

        // [성공 보장] 250 스킬에서 피즐 사운드 방지
        public override bool CheckCast() { return true; }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237);
                return;
            }

            if (SpellHelper.CheckTown(p, Caster))
            {
                // 수동 자원 소모 로직 (피즐 원천 차단)
                if (Caster.Mana < GetMana())
                {
                    Caster.SendLocalizedMessage(500613);
                    return;
                }

                if (!ConsumeReagents())
                {
                    Caster.SendLocalizedMessage(500612);
                    return;
                }

                Caster.Mana -= GetMana();
                SpellHelper.Turn(Caster, p);
                
                if (p is Item) p = ((Item)p).GetWorldLocation();
                Point3D loc = new Point3D(p);

                // 시각 효과 및 사운드
                Effects.PlaySound(loc, Caster.Map, 0x20B);
                Caster.PlaySound(Caster.Female ? 0x338 : 0x44B);

                // [핵심] 3타일 내 적들에게 일괄 공격 수행
                List<Mobile> targets = AcquireIndirectTargets(loc, 3).OfType<Mobile>().ToList();

                foreach (Mobile m in targets)
                {
                    if (Caster.CanBeHarmful(m, false))
                    {
                        Caster.DoHarmful(m);
                        
                        // 데미지 및 연쇄 발동 처리 실행
                        DoEnergyDamage(m);
                    }
                }
            }

            FinishSequence();
        }

        // 데미지 및 연쇄 발동 처리 공용 메서드 (Energy Bolt급 위력)
        public void DoEnergyDamage(Mobile target)
        {
            if (target == null || !target.Alive) return;

            // 에너지 볼트급 데미지 (6서클 기준 약 35~55)
            int damage = this.GetNewAosDamage(0, 35, 55, target is PlayerMobile, 1.0, target);
            
            // 에너지 100% 데미지 적용
            SpellHelper.Damage(this, target, damage, 0, 0, 0, 0, 100);

            // 개별 타격 연출
            target.FixedParticles(0x374A, 1, 15, 5038, EffectLayer.Waist);
            target.PlaySound(0x20B);
        }

        // [통합 규격] Spell.cs의 연쇄 발동(Extra Cast)에서 호출할 메서드
        public void OnExtraCast(Mobile target)
        {
            DoEnergyDamage(target);
        }

        public class InternalTarget : Target
        {
            private readonly EnergyFieldSpell m_Owner;
            public InternalTarget(EnergyFieldSpell owner) : base(12, true, TargetFlags.None) { m_Owner = owner; }
            protected override void OnTarget(Mobile from, object o) { if (o is IPoint3D) m_Owner.Target((IPoint3D)o); }
            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}