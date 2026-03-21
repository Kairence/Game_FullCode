using System;

namespace Server.Mobiles
{
    [CorpseName("a walrus corpse")]
    public class Walrus : BaseCreature
    {
        [Constructable]
        public Walrus()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a walrus";
            this.Body = 0xDD;
            this.BaseSoundID = 0xE0;

            this.Fame = 450;
			this.Karma = 0;

			// [역산] 보너스: Str+532, Hits+888, Skill+1.1
			this.SetStr(1, 10);     // 최종 Str 533~543
			this.SetDex(1, 10);     
			this.SetHits(112, 212); // 최종 Hits 1,000~1,100
			this.SetStam(1, 10);

			SetAttackSpeed(4.0);
			SetDamage(16, 24); // 큰뿔사슴(16-24)과 동급이나 공속으로 난이도 조절

			// 공격 속성: 들이받기와 냉기
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Cold, 20);

			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Cold, 45, 50);

			// 최종 Skill 25.0 내외
			this.SetSkill(SkillName.Wrestling, 23.9, 33.9);

			this.Tamable = true;
			this.MinTameSkill = 31.1;

            if (Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;
        }

        public Walrus(Serial serial)
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
                return 12;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish;
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