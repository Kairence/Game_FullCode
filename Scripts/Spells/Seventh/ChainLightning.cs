using System;
using System.Collections.Generic;
using System.Linq;
using Server.Targeting;
using Server.Mobiles;
using Server.Spells.Fourth;

namespace Server.Spells.Seventh
{
    public class ChainLightningSpell : MagerySpell
    {
        public override DamageType SpellDamageType => DamageType.SpellAOE;

        private static readonly SpellInfo m_Info = new SpellInfo(
            "Chain Lightning", "Vas Ort Grav",
            209, 9022, false,
            Reagent.BlackPearl, Reagent.Bloodmoss, Reagent.MandrakeRoot, Reagent.SulfurousAsh);

        public ChainLightningSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Seventh;

        // [체크] 250 스킬에서도 안된다면 엔진이 막는 것이므로 true로 강제 통과
        public override bool CheckCast()
        {
            return true; 
        }

        public override void OnCast()
        {
            // OnCast에서 타겟을 띄울 때 CheckSequence를 사용하지 않고 바로 띄웁니다.
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237);
                return;
            }

            // 마을 체크 및 마나/시약 소모 (성공률 체크만 뺀 수동 소모)
            if (SpellHelper.CheckTown(p, Caster))
            {
                // 마나가 부족하면 여기서 컷 (GetMana는 7서클 기본 마나)
                if (Caster.Mana < GetMana())
                {
                    Caster.SendLocalizedMessage(500613); // 마나 부족
                    return;
                }
                
                // 시약 소모
                if (!ConsumeReagents())
                {
                    Caster.SendLocalizedMessage(500612); // 시약 부족
                    return;
                }

                Caster.Mana -= GetMana();
                SpellHelper.Turn(Caster, p);
                Point3D loc = new Point3D(p);
                
                // 성공 사운드 강제 출력
                Caster.PlaySound(Caster.Female ? 0x338 : 0x44B);

                // [중요] Spell.cs에 만든 범용 채널링 시스템 호출
                // 간격 3초, 횟수 10번, 움직이면 취소(true), 틱 로직
                StartChanneling(TimeSpan.FromSeconds(3.0), 10, true, (tick) => 
                {
                    IPooledEnumerable eable = Caster.Map.GetMobilesInRange(loc, 2);
                    foreach (Mobile m in eable)
                    {
                        // 시전자 제외 및 적대적 대상 체크
                        if (m != Caster && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
                        {
                            Caster.DoHarmful(m);
                            // 4서클 라이트닝 발사
							double damage = 0;
							int min = 80;
							int max = 120;
							if( Caster is SummonedAirElemental )
							{
								min = 40;
								max = 90;
							}
							if( Caster is Titan )
							{
								min = 15;
								max = 40;
							}
							damage = GetNewAosDamage(0, min, max, m);

							if (m is Mobile)
							{
								Effects.SendBoltEffect(m, true, 0, false);
							}
							else
							{
								Effects.SendBoltEffect(EffectMobile.Create(m.Location, m.Map, EffectMobile.DefaultDuration), true, 0, false);
							}

							if (damage > 0)
							{
								SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
							}
                        }
                    }
                    eable.Free();
                });
            }

            FinishSequence();
        }
        private class InternalTarget : Target
        {
            private readonly ChainLightningSpell m_Owner;
            public InternalTarget(ChainLightningSpell owner) : base(12, true, TargetFlags.None) { m_Owner = owner; }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D) m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetFinish(Mobile from) { m_Owner.FinishSequence(); }
        }
    }
}