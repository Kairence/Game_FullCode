using System;

namespace Server.Mobiles
{
    [CorpseName("a cougar corpse")]
    public class Cougar : BaseCreature
    {
        [Constructable]
        public Cougar()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a cougar";
            this.Body = 0x3F;
            this.BaseSoundID = 0x73;

            this.SetStr(55, 80);
            this.SetDex(85, 105);
            this.SetInt(25, 50);

            this.SetHits(90, 120);
            this.SetStam(85, 105);

			this.SetAttackSpeed(2.0);  // 서버 최상위권 공속 유지. 고양이(2.5s)보다 더 빠르고 위협적임.
			this.SetDamage(16, 24);    // 방어 10인 유저에게 최종 6~14 데미지 전달.

            this.SetSkill(SkillName.Wrestling, 12.0, 15.0); // 기술도 일반 동물보다 높음
            this.SetSkill(SkillName.Tactics, 12.0, 15.0);
            this.SetSkill(SkillName.MagicResist, 8.0, 12.0);

            this.Fame = 800; // 멧돼지보다 높은 명성
            this.Tamable = true;
            this.ControlSlots = 1;
            this.MinTameSkill = 41.1;
        }

        public Cougar(Serial serial)
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
                return 10;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish | FoodType.Meat;
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