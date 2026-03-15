using System;

namespace Server.Mobiles
{
    [CorpseName("a snake corpse")]
    public class Snake : BaseCreature
    {
        [Constructable]
        public Snake()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a snake";
            this.Body = 52;
            this.Hue = Utility.RandomSnakeHue();
            this.BaseSoundID = 0xDB;

            this.SetStr(1, 5);      // 최종 Str 524~528
			this.SetDex(1, 5);      // 최종 Dex ~150 (작고 빠름)

			this.SetHits(1, 9);     // 최종 Hits 712~720 (슬라임과 동급)
			this.SetStam(1, 5);     
			this.SetMana(0);

			SetAttackSpeed(3.0);
			SetDamage(12, 16); // 슬라임(10-16)과 동급 밸런스
			
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Poison, 20); // 초보용이라 독 비중을 낮춤			

			this.SetResistance(ResistanceType.Physical, 0, 5);
			this.SetResistance(ResistanceType.Poison, 5, 10); // 독 저항 최소화

			// 최종 Skill 10.0 미만 목표 (10.0 - 0.7 = 9.3)
			this.SetSkill(SkillName.Wrestling, 4.3, 9.3);
			this.SetSkill(SkillName.Poisoning, 4.3, 9.3);

			this.Fame = 300;
			this.Tamable = true;
			this.MinTameSkill = 15.1;
        }

        public Snake(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lesser;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lesser;
            }
        }
        public override bool DeathAdderCharmable
        {
            get
            {
                return true;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Eggs;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0 && (AbilityProfile == null || AbilityProfile.MagicalAbility == MagicalAbility.None))
            {
                SetMagicalAbility(MagicalAbility.Poisoning);
            }
        }
    }
}