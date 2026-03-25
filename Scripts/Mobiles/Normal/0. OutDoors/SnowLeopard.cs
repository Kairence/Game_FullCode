using System;

namespace Server.Mobiles
{
    [CorpseName("a leopard corpse")]
    [TypeAlias("Server.Mobiles.Snowleopard")]
    public class SnowLeopard : BaseCreature
    {
        [Constructable]
        public SnowLeopard()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a snow leopard";
            this.Body = Utility.RandomList(64, 65);
            this.BaseSoundID = 0x73;

            this.Fame = 800;
			this.Karma = -800;

			// [역산] 보너스: Str+561, Hits+1,566, Skill+2.0
			this.SetStr(10, 20);    // 최종 Str 571~581
			this.SetDex(50, 80);    
			this.SetHits(34, 134);  // 최종 Hits 1,600~1,700
			this.SetStam(37, 87);

			SetAttackSpeed(2.5);
			SetDamage(18, 26); // 팬서(18-26)와 동일한 명성 800급 포식자

			// 공격 속성: 차가운 발톱
			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Cold, 30);

			this.SetResistance(ResistanceType.Physical, 15, 20);
			this.SetResistance(ResistanceType.Cold, 35, 45);

			// 최종 Skill 55.0 내외 (중하급 사냥꾼)
			this.SetSkill(SkillName.Wrestling, 53.0, 63.0);

			this.Tamable = true;
			this.MinTameSkill = 53.1;

        }

        public SnowLeopard(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int Hides
        {
            get
            {
                return 8;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Fish;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Feline;
            }
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
        }
    }
}
