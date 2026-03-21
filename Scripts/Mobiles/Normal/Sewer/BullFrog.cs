using System;

namespace Server.Mobiles
{
    [CorpseName("a bull frog corpse")]
    [TypeAlias("Server.Mobiles.Bullfrog")]
    public class BullFrog : BaseCreature
    {
        [Constructable]
        public BullFrog()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a bull frog";
            Body = 81;
            Hue = Utility.RandomList(0x5AC, 0x5A3, 0x59A, 0x591, 0x588, 0x57F);
            BaseSoundID = 0x266;

			/* [Bullfrog - Fame 1,500 / Sewer / Weight 1.16]
			   - 명성 하향 조정: 4,500 -> 1,500 (하수구 중급 몬스터)
			   - 가상 방어력(VirtualArmor): (1,500/1000) + 1.5 = 3
			   - 테이밍 난이도: 35.0 ~ 45.0 (Giant Rat 다음 단계)
			   -------------------------------------------------- */

			// [Attributes] 명성 1,500 보너스 + 가중치 1.16 반영
			this.SetStr(15, 25); 
			this.SetHits(350, 450); 
			this.SetDex(3, 5);
			this.SetInt(3, 5);

			// [Combat Options]
			this.SetDamage(8, 16);
			this.SetAttackSpeed(2.4);

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances]
			this.SetResistance(ResistanceType.Physical, 15, 25); 
			this.SetResistance(ResistanceType.Fire, 10, 15);
			this.SetResistance(ResistanceType.Cold, 35, 45);    
			this.SetResistance(ResistanceType.Poison, 30, 40); 
			this.SetResistance(ResistanceType.Energy, 10, 15);

			// [Skills] 유저 스킬 40 ~ 60 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 45.0, 60.0); 
			this.SetSkill(SkillName.Tactics, 45.0, 60.0);
			this.SetSkill(SkillName.MagicResist, 30.0, 45.0);

			// [Taming & Food] ★ 가상 방어구 상단 배치
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 35.0; // Giant Rat(25.0)을 마스터한 테이머의 다음 타겟

			// [Misc]
			this.VirtualArmor = 3;

			this.Fame = 1500;
			this.Karma = -1500;
        }

        public BullFrog(Serial serial)
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
                return 4;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish | FoodType.Meat;
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