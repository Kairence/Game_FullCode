using System;

namespace Server.Mobiles
{
    [CorpseName("a troll corpse")]
    public class Troll : BaseCreature
    {
        [Constructable]
        public Troll()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a troll";
            this.Body = 54;//Utility.RandomList(53, 54);
            this.BaseSoundID = 461;

            this.SetStr(64, 114);    // 최종 Str 850~900
			this.SetDex(64, 94);     // 에틴보다 빠름
			this.SetInt(20, 40);     // 최종 Int 150~170

			this.SetHits(251, 751);  // 최종 Hits 7,000~7,500
			this.SetStam(64, 94);

			SetAttackSpeed(4.5);
			SetDamage(35, 50);

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 질긴 피부 (35% 내외)
			this.SetResistance(ResistanceType.Physical, 30, 35);
			this.SetResistance(ResistanceType.Fire, 5, 10); // 재생 생물 특유의 화염 약점

			// 최종 Skill 80.0~90.0 (90.0 - 10.0 = 80.0)
			this.SetSkill(SkillName.Wrestling, 70.0, 80.0);
			this.SetSkill(SkillName.Tactics, 70.0, 80.0);

			this.VirtualArmor = 10;

			this.Fame = 3500;
			this.Karma = -3500;	
			
			this.SpecialType2 = 4;
			this.SpecialChance2 = 0.1;			
        }

        public Troll(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
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