using System;

namespace Server.Mobiles
{
    [CorpseName("a giant toad corpse")]
    [TypeAlias("Server.Mobiles.Gianttoad")]
    public class GiantToad : BaseCreature
    {
        [Constructable]
        public GiantToad()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a giant toad";
            this.Body = 80;
            this.BaseSoundID = 0x26B;

			/* [Giant Toad - Fame 2,800 / Sewer / Weight 1.18]
			   - 스킬 200 마스터 서버용 '중급 맷집형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (2,800/1000) + 1.2 = 4
			   - 테이밍 난이도: 55.0 ~ 65.0 (중급 테이머의 관문)
			   -------------------------------------------------- */

			// [Attributes] 명성 2,800 보너스 + 가중치 1.18 반영
			this.SetStr(40, 55); 
			this.SetHits(850, 1050); 
			this.SetDex(8, 12);
			this.SetInt(8, 12);

			// [Combat Options]
			this.SetDamage(12, 22);
			this.SetAttackSpeed(2.4);

			// [Damage Types] 100% 물리
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 양서류 특성 (에너지에 취약, 냉기에 강점)
			this.SetResistance(ResistanceType.Physical, 20, 30); 
			this.SetResistance(ResistanceType.Fire, 10, 20);
			this.SetResistance(ResistanceType.Cold, 40, 50);    // 차가운 하수구물 적응
			this.SetResistance(ResistanceType.Poison, 30, 40); 
			this.SetResistance(ResistanceType.Energy, 5, 15);   // 전기에 튀겨지기 쉬움

			// [Skills] 유저 스킬 60 ~ 80 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 65.0, 80.0); 
			this.SetSkill(SkillName.Tactics, 65.0, 80.0);
			this.SetSkill(SkillName.MagicResist, 40.0, 55.0);

			// [Taming & Food] ★ 가상 방어구 상단 배치
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 55.0; // 중급 테이머들이 거느리기 시작하는 든든한 고기방패

			// [Misc]
			this.VirtualArmor = 4;

			this.Fame = 2800;
			this.Karma = -2800;            
            if (Utility.RandomDouble() < 0.2)
			{
				switch (Utility.Random(2))
				{
					case 0:
						{
							Hue = 191;
							break;
						}
					case 1:
						{
							Hue = 1166;
							break;
						}
				}
			}
        }

        public GiantToad(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get
            {
                return 12;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
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
            this.AddLoot(LootPack.Poor);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            if (version < 1)
            {
                this.AI = AIType.AI_Melee;
                this.FightMode = FightMode.Closest;
            }
        }
    }
}
