using System;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a lizardman corpse")]
    public class Lizardman : BaseCreature
    {
        [Constructable]
        public Lizardman()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("lizardman");
            Body = 36; //Utility.RandomList(35, 36);
            BaseSoundID = 417;

            this.SetStr(32, 82);     // 최종 Str 650~700
			this.SetDex(66, 116);    // 최종 Dex ~400 (빠름)
			this.SetInt(16, 26);     // 최종 Int 100~110

			this.SetHits(134, 334);  // 최종 Hits 3,000~3,200
			this.SetStam(66, 116);

			SetAttackSpeed(2.2);
			SetDamage(16, 24); 

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 비늘 피부 (20~25%)
			this.SetResistance(ResistanceType.Physical, 20, 25);
			this.SetResistance(ResistanceType.Cold, 25, 30);

			// 최종 Skill 55.0~65.0 (65.0 - 3.9 = 61.1)
			this.SetSkill(SkillName.Wrestling, 51.1, 61.1);
			this.SetSkill(SkillName.Tactics, 51.1, 61.1);
			this.SetSkill(SkillName.Anatomy, 51.1, 61.1);

			this.VirtualArmor = 5;

			this.Fame = 1500;
			this.Karma = -1500;
			this.SpecialType2 = 5;
			this.SpecialChance2 = 0.05;			
        }

        public Lizardman(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Lizardman;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
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
                return 12;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Derned;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
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