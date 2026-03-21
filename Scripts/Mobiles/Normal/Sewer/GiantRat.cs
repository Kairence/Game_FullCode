using System;

namespace Server.Mobiles
{
    [CorpseName("a giant rat corpse")]
    [TypeAlias("Server.Mobiles.Giantrat")]
    public class GiantRat : BaseCreature
    {
        [Constructable]
        public GiantRat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a giant rat";
            this.Body = 0xD7;
            this.BaseSoundID = 0x188;

			/* [Giant Rat - Fame 1,200 / Sewer / Weight 1.15]
			   - 스킬 200 마스터 서버용 '초급 정예' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (1,200/1000) + 0.8 = 2
			   - 테이밍 난이도: 25.0 ~ 35.0 (Sewerrat 다음 단계)
			   -------------------------------------------------- */

			// [Attributes] 명성 1,200 보너스 + 가중치 1.15 반영
			this.SetStr(12, 18); 
			this.SetHits(250, 350); 
			this.SetDex(2, 4);
			this.SetInt(2, 4);

			// [Combat Options]
			this.SetDamage(5, 12);
			this.SetAttackSpeed(2.2);

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 하수구 생물 특성 (독 저항 우세)
			this.SetResistance(ResistanceType.Physical, 10, 15); 
			this.SetResistance(ResistanceType.Fire, 5, 10);
			this.SetResistance(ResistanceType.Cold, 5, 10);
			this.SetResistance(ResistanceType.Poison, 30, 45); 
			this.SetResistance(ResistanceType.Energy, 5, 10);

			// [Skills] 유저 스킬 30 ~ 50 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 35.0, 50.0); 
			this.SetSkill(SkillName.Tactics, 35.0, 50.0);
			this.SetSkill(SkillName.MagicResist, 20.0, 30.0);

			// [Taming & Food] ★ 가상 방어구 상단 배치
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 25.0; // 테이밍 숙련자용 다음 관문

			// [Misc]
			this.VirtualArmor = 2;

			this.Fame = 1200;
			this.Karma = -1200;
        }

        public GiantRat(Serial serial)
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
                return FoodType.Fish | FoodType.Meat | FoodType.FruitsAndVegies | FoodType.Eggs;
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