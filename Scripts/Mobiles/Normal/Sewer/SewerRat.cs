using System;

namespace Server.Mobiles
{
    [CorpseName("a sewer rat corpse")]
    public class Sewerrat : BaseCreature
    {
        [Constructable]
        public Sewerrat()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a sewer rat";
            this.Body = 238;
            this.BaseSoundID = 0xCC;

			/* [Sewerrat - Fame 500 / Sewer / Weight 1.12]
			   - 테이밍 코드 추가 (초보 테이머용)
			   - 먹이 설정: 고기, 곡물 등 잡식성
			   -------------------------------------------------- */

			// [Attributes] 명성 500 보너스 + 가중치 1.12 반영
			this.SetStr(4, 8); 
			this.SetHits(100, 130); 
			this.SetDex(1, 2);
			this.SetInt(1, 2);

			// [Taming & AI] ★ 초보 테이머용 핵심 코드
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 5.0; // 0~30 구간 유저를 위한 입문 난이도

			// [Combat Options]
			this.SetDamage(1, 5);
			this.SetAttackSpeed(2.0);

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances]
			this.SetResistance(ResistanceType.Physical, 5, 10); 
			this.SetResistance(ResistanceType.Poison, 20, 30); // 하수구 보정

			// [Skills] 유저 스킬 0 ~ 30 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 15.0, 25.0); 
			this.SetSkill(SkillName.Tactics, 15.0, 25.0);
			this.SetSkill(SkillName.MagicResist, 5.0, 10.0);

			this.VirtualArmor = 1;
			this.Fame = 500;
			this.Karma = -500;
        }

        public Sewerrat(Serial serial)
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
                return FoodType.Meat | FoodType.Eggs | FoodType.FruitsAndVegies;
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