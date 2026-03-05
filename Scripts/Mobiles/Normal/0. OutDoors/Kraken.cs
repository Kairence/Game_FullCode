using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a krakens corpse")]
    public class Kraken : BaseCreature
    {
		private DateTime m_NextWaterBall;
        [Constructable]
        public Kraken()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.m_NextWaterBall = DateTime.Now;

            this.Name = "a kraken";
            this.Body = 77;
            this.BaseSoundID = 353;

            // [역산] 명성 15,000 보너스(Str+2187, Hits+37824, Stam+417, Skill+56.2) 반영
			// 최종 Str 3,500~3,800 목표 (함선을 부수는 힘)
			this.SetStr(1313, 1613);
			this.SetDex(83, 133); // 최종 Dex ~600 도달

			// 최종 Hits 80,000~85,000 목표 (전사 4000 기준 약 20배 맷집)
			this.SetHits(42176, 47176);
			this.SetStam(183, 283); // 최종 Stam 600~700
			this.SetMana(500, 1000);

			SetAttackSpeed(4.0);
			SetDamage(60, 95); // 평균 77.5

			// 공격 속성: 차가운 심해의 타격
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Cold, 40);

			// 저항 설정 (심해 생물 특성)
			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Fire, 10, 20); // 불에 취약
			this.SetResistance(ResistanceType.Cold, 45, 50); // 냉기 면역 수준
			this.SetResistance(ResistanceType.Poison, 30, 40);
			this.SetResistance(ResistanceType.Energy, 40, 50);

			// 스킬 역산 (최종 스킬 150.0 목표)
			this.SetSkill(SkillName.Wrestling, 93.8, 103.8);
			this.SetSkill(SkillName.Tactics, 93.8, 103.8);

			this.Fame = 15000;
			this.Karma = -15000;
			this.VirtualArmor = 15;

			this.Tamable = false; // 길들일 수 없는 공포

            this.CanSwim = true;
            this.CantWalk = true;

            SetSpecialAbility(SpecialAbility.DragonBreath);

            //Rope is supposed to be a rare drop.  ref UO Guide Kraken
        }

        public Kraken(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel { get { return 4; } }

		/*
        public override void OnActionCombat()
        {
            Mobile combatant = this.Combatant as Mobile;

            if (combatant == null || combatant.Deleted || combatant.Map != this.Map || !this.InRange(combatant, 12) || !this.CanBeHarmful(combatant) || !this.InLOS(combatant))
                return;

            if (DateTime.Now >= this.m_NextWaterBall)
            {
                double damage = 40 + combatant.Hits * 0.3;

                this.DoHarmful(combatant);
                this.MovingParticles(combatant, 0x36D4, 5, 0, false, false, 195, 0, 9502, 3006, 0, 0, 0);
                AOS.Damage(combatant, this, (int)damage, 0, 0, 100, 0, 0);

                if (combatant is PlayerMobile && combatant.Mount != null)
                {
                    (combatant as PlayerMobile).SetMountBlock(BlockMountType.DismountRecovery, TimeSpan.FromSeconds(10), true);
                }

                m_NextWaterBall = DateTime.Now + TimeSpan.FromSeconds(20);
            }
        }
		*/
        public override void GenerateLoot()
        {

        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_NextWaterBall = DateTime.UtcNow;
        }
    }
}
