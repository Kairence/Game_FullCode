using System;

namespace Server.Mobiles
{
    [CorpseName("a chicken corpse")]
    public class Chicken : BaseCreature
    {
        [Constructable]
        public Chicken()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            // [역산] 보너스(Str+511, Hits+450, Stam+52, Skill+0.3) 제외 설정
			this.SetStr(4, 9); 
			this.SetDex(8, 18); 
			this.SetInt(10, 20);

			this.SetHits(10, 20); // 최종 Hits 460~470
			this.SetStam(8, 18);
			this.SetMana(0);

			this.SetAttackSpeed(4.0);  // 새(3.0s)보다 느린 4초. 가끔 툭툭 쪼는 수준.
			this.SetDamage(8, 12);     // 방어 10인 유저에게는 0~2 데미지 (사실상 노 데미지)

			this.SetSkill(SkillName.Wrestling, 0.7, 1.2); 

			this.Fame = 150;
			this.VirtualArmor = 0;

			this.Tamable = true;
			this.MinTameSkill = -18.9;
			this.SetDamageType(ResistanceType.Physical, 100);

        }

        public Chicken(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override MeatType MeatType
        {
            get
            {
                return MeatType.Bird;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.GrainsAndHay;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override int Feathers
        {
            get
            {
                return 25;
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
