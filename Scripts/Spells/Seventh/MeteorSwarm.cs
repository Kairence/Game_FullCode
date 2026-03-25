using System;
using System.Collections.Generic;
using System.Linq;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;

namespace Server.Spells.Seventh
{
    public class MeteorSwarmSpell : MagerySpell
    {
        public override DamageType SpellDamageType => DamageType.SpellAOE;

        private static readonly SpellInfo m_Info = new SpellInfo(
            "Meteor Swarm", "Flam Kal Des Ylem",
            233, 9042, false,
            Reagent.Bloodmoss, Reagent.MandrakeRoot, Reagent.SulfurousAsh, Reagent.SpidersSilk);

        public MeteorSwarmSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info) { }

        public override SpellCircle Circle => SpellCircle.Seventh;

        // [성공 보장] 매저리 수치와 상관없이 시전 허용
        public override bool CheckCast() { return true; }

        public override void OnCast()
        {
            // 1. 수동 마나 및 시약 소모 체크 (피즐 방지용)
            if (Caster.Mana < GetMana())
            {
                Caster.SendLocalizedMessage(500613); // 마나 부족
                return;
            }

            if (!ConsumeReagents())
            {
                Caster.SendLocalizedMessage(500612); // 시약 부족
                return;
            }

            // 시전 성공 확정
            Caster.Mana -= GetMana();
            Caster.PlaySound(Caster.Female ? 0x338 : 0x44B);

            // 주문 완료 처리 (채널링 시작 전 호출하여 시전 프로세스 종결)
            FinishSequence();

            // 2. Spell.cs의 범용 채널링 시스템 시작
            // 1초 간격으로 20번 발사 (총 20초), 움직이면 취소(true)
            StartChanneling(TimeSpan.FromSeconds(1.0), 20, true, (tick) =>
            {
                // 주변 5타일 내 유효한 적 탐색
                List<Mobile> targets = new List<Mobile>();
                IPooledEnumerable eable = Caster.Map.GetMobilesInRange(Caster.Location, 5);
                
                foreach (Mobile m in eable)
                {
                    if (m != Caster && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
                    {
                        targets.Add(m);
                    }
                }
                eable.Free();

                if (targets.Count > 0)
                {
                    // 무작위 대상 선정
                    Mobile randomTarget = targets[Utility.Random(targets.Count)];

                    // 실제 타격 시 가시거리(LOS) 체크
                    if (Caster.CanSee(randomTarget))
                    {
                        Caster.DoHarmful(randomTarget);
                        Caster.MovingParticles(randomTarget, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
                        Caster.PlaySound(Core.AOS ? 0x15E : 0x44B);

                        // [핵심] GetNewAosDamage 호출로 데미지 처리 및 스펠위빙 연쇄 발동 유도
                        // Spell.cs에서 Timer.DelayCall로 연쇄 발동을 처리하므로 채널링이 끊기지 않습니다.
						int damage = this.GetNewAosDamage(0, 80, 160, randomTarget is PlayerMobile, 1.0, randomTarget);
                        SpellHelper.Damage(this, randomTarget, damage, 0, 100, 0, 0, 0);
					}
                    else
                    {
                        // 적이 리스트에는 있으나 지형에 가려진 경우 주변 랜덤 낙하 연출
                        DropRandomMeteor(Caster.Location);
                    }
                }
                else
                {
                    // 주변에 적이 아예 없을 때 지면 폭발 연출 (유지 체감용)
                    DropRandomMeteor(Caster.Location);
                }
            });
        }

        // 주변 지면에 무작위로 메테오를 떨어뜨리는 헬퍼 메서드
        private void DropRandomMeteor(Point3D center)
        {
            int x = center.X + Utility.RandomMinMax(-4, 4);
            int y = center.Y + Utility.RandomMinMax(-4, 4);
            int z = center.Z;

            IPoint3D p = new Point3D(x, y, z);
            SpellHelper.GetSurfaceTop(ref p);

            Point3D loc = new Point3D(p);
            Effects.SendLocationEffect(loc, Caster.Map, 0x36BD, 20, 10);
            Caster.PlaySound(0x11D);
        }
    }
}
