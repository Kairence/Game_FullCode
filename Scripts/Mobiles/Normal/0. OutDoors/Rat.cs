using System;

namespace Server.Mobiles
{
    [CorpseName("a rat corpse")]
    public class Rat : BaseCreature
    {
        [Constructable]
        public Rat()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a rat";
            this.Body = 238;
            this.BaseSoundID = 0xCC;

            this.SetStr(1, 5);
			this.SetDex(28, 48); // 최종 Dex ~130

			this.SetHits(50, 80); // 최종 Hits 500~530
			this.SetStam(28, 48);
			this.SetMana(0);

			this.SetAttackSpeed(2.5); // 쥐의 잽싼 특징
			this.SetDamage(8, 12); // 빠른 공속 대신 닭과 같은 데미지 풀 공유

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 0, 5);
			this.SetResistance(ResistanceType.Poison, 10, 20);

			this.Fame = 150;
			this.Tamable = true;
			this.MinTameSkill = -18.9;
        }

        public Rat(Serial serial)
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
                return FoodType.Meat | FoodType.Fish | FoodType.Eggs | FoodType.GrainsAndHay;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Poor);
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