using System;

namespace Server.Mobiles
{
    [CorpseName("a pig corpse")]
    public class Boar : BaseCreature
    {
        [Constructable]
        public Boar()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a boar";
            this.Body = 0x122;
            this.BaseSoundID = 0xC4;

            // [역산] 보너스(Str+527, Hits+772, Stam+57, Skill+0.9) 제외 설정
			this.SetStr(23, 33); 
			this.SetDex(33, 43); // 최종 Dex ~143
			this.SetInt(10, 20);

			this.SetHits(228, 250); // 최종 Hits 1,000~1,022
			this.SetStam(43, 53);   // 최종 Stam 100~110
			this.SetMana(0);

			this.SetAttackSpeed(4.0);  // 유저(2.5~3.0)보다 느리게 설정. 맷돼지가 들이받기 위해 거리를 두거나 준비하는 느낌.
			this.SetDamage(18, 26);    // 방어 10인 유저에게 최종 8~16의 데미지 전달.

			this.SetSkill(SkillName.Wrestling, 2.1, 3.1); 
			this.SetSkill(SkillName.Tactics, 2.1, 3.1);

			this.Fame = 350;
			this.VirtualArmor = 2; // 유저 풀플레트(10) 대비 20% 수준

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 29.1;
			
            if (Core.AOS && Utility.Random(1000) == 0) // 0.1% chance to have mad cows
                FightMode = FightMode.Closest;			
        }

        public Boar(Serial serial)
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
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
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