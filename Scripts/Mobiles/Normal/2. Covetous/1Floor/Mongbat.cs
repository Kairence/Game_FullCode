using System;

namespace Server.Mobiles
{
    [CorpseName("a mongbat corpse")]
    public class Mongbat : BaseCreature
    {
        [Constructable]
        public Mongbat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a mongbat";
            Body = 39;
            BaseSoundID = 422;

            this.SetStr(6, 16);      // 최종 Str 568~578
			this.SetDex(26, 46);     
			this.SetInt(6, 16);      // 최종 Int 67~77

			this.SetHits(78, 178);   // 최종 Hits 1,700~1,800
			this.SetStam(26, 46);

			SetAttackSpeed(2.5);
			SetDamage(14, 22);

			this.SetDamageType(ResistanceType.Physical, 100);
			this.SetResistance(ResistanceType.Physical, 5, 10);

			this.SetSkill(SkillName.Wrestling, 22.9, 32.9);
			this.SetSkill(SkillName.Tactics, 22.9, 32.9);

			// 테이밍 설정
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 35.1;

			this.Fame = 800;
			this.Karma = -800;
        }

        public Mongbat(Serial serial)
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
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }

        public override bool CanFly
        {
            get
            {
                return true;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Poor);
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