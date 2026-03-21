using System;

namespace Server.Mobiles
{
    [CorpseName("a white wolf corpse")]
    [TypeAlias("Server.Mobiles.Whitewolf")]
    public class WhiteWolf : BaseCreature
    {
        [Constructable]
        public WhiteWolf()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a white wolf";
            this.Body = Utility.RandomList(34, 37);
            this.BaseSoundID = 0xE5;

            this.Fame = 800;
			this.Karma = -800;

			// [역산] 보너스: Str+561, Hits+1,566, Skill+2.0
			this.SetStr(10, 20);
			this.SetDex(50, 80);
			this.SetHits(34, 134);  // 최종 Hits 1,600~1,700

			SetAttackSpeed(2.5);
			SetDamage(10, 16);

			// 공격 속성: 혹한의 이빨
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Cold, 40);

			this.SetResistance(ResistanceType.Physical, 10, 15);
			this.SetResistance(ResistanceType.Cold, 45, 50);

			// 최종 Skill 55.0 내외
			this.SetSkill(SkillName.Wrestling, 53.0, 63.0);

			this.Tamable = true;
			this.MinTameSkill = 65.1;
        }

        public WhiteWolf(Serial serial)
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
                return 6;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Canine;
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