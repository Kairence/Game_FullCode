using System;

namespace Server.Mobiles
{
    [CorpseName("a deer corpse")]
    [TypeAlias("Server.Mobiles.Greathart")]
    public class GreatHart : BaseCreature
    {
        [Constructable]
        public GreatHart()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a great hart";
            Body = 0xEA;

            // [역산] 명성 400 보너스 반영
			this.SetStr(20, 30); // 최종 Str 550~560
			this.SetDex(100, 150); // 최종 Dex ~350 (매우 빠름)

			this.SetHits(176, 200); // 최종 Hits 1,000~1,024
			this.SetStam(94, 144);  // 최종 Stam 150~200
			this.SetMana(0);

			this.SetAttackSpeed(3.5);  // [조정] 2.5초 -> 3.5초. 
									   // 고양이(2.5s)나 쿠거(2.0s)보다는 느리게 설정하여
									   // 초보자가 "대응할 수 있는 빠른 몹"의 기준을 잡았습니다.

			this.SetDamage(16, 24);    // [방어구 가치 존중] 돌고래(16-24)와 동급.

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 10, 20);
			this.SetResistance(ResistanceType.Cold, 15, 25);

			this.Fame = 400;
			this.VirtualArmor = 2;
			this.Tamable = true;
			this.MinTameSkill = 35.1;
        }

        public GreatHart(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 6;
            }
        }
        public override int Hides
        {
            get
            {
                return 15;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override int GetAttackSound() 
        { 
            return 0x82; 
        }

        public override int GetHurtSound() 
        { 
            return 0x83; 
        }

        public override int GetDeathSound() 
        { 
            return 0x84; 
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
