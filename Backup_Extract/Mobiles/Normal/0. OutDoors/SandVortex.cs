using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a sand vortex corpse")]
    public class SandVortex : BaseCreature
    {
        private DateTime m_NextAttack;
        [Constructable]
        public SandVortex()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a sand vortex";
            this.Body = 790;
            this.BaseSoundID = 263;

            // [역산] 명성 2,500 보너스 반영
			this.SetStr(1, 50);    // 최종 Str 700~750
			this.SetDex(154, 204); // 최종 Dex ~450 (매우 빠름)

			this.SetHits(3051, 4051); // 최종 Hits 8,000~9,000
			this.SetStam(54, 104);
			this.SetMana(0);

			this.SetAttackSpeed(2.5);
			this.SetDamage(25, 35); // 명성 2500 뱀(4.0s/35-55)과 맞춘 초고속 DPS 밸런스

			// 공격 속성: 살을 찢는 모래와 바람
			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 45, 50); // 물리 면역 수준
			this.SetResistance(ResistanceType.Fire, 30, 40);
			this.SetResistance(ResistanceType.Energy, 5, 15);   // 마법적 에너지에 분산됨

			// 최종 스킬 130.0 목표 (130.0 - 6.8 = 123.2)
			this.SetSkill(SkillName.Wrestling, 123.2, 143.2);

			this.Fame = 2500;
			this.Karma = -2500;
			this.VirtualArmor = 5;

            this.PackItem(new Bone());
        }

        public SandVortex(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager, 2);
        }

        public override void OnActionCombat()
        {
            Mobile combatant = this.Combatant as Mobile;

            if (combatant == null || combatant.Deleted || combatant.Map != this.Map || !this.InRange(combatant, 12) || !this.CanBeHarmful(combatant) || !this.InLOS(combatant))
                return;

            if (DateTime.UtcNow >= this.m_NextAttack)
            {
                this.SandAttack(combatant);
                this.m_NextAttack = DateTime.UtcNow + TimeSpan.FromSeconds(10.0 + (10.0 * Utility.RandomDouble()));
            }
        }

        public void SandAttack(Mobile m)
        {
            this.DoHarmful(m);

            m.FixedParticles(0x36B0, 10, 25, 9540, 2413, 0, EffectLayer.Waist);

            new InternalTimer(m, this).Start();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile m_Mobile;
            private readonly Mobile m_From;
            public InternalTimer(Mobile m, Mobile from)
                : base(TimeSpan.FromSeconds(1.0))
            {
                this.m_Mobile = m;
                this.m_From = from;
                this.Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                this.m_Mobile.PlaySound(0x4CF);
                AOS.Damage(this.m_Mobile, this.m_From, Utility.RandomMinMax(1, 40), 90, 10, 0, 0, 0);
            }
        }
    }
}
