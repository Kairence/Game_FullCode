using System;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a lizardman corpse")]
    public class LizardmanDefender : BaseCreature
    {
        [Constructable]
        public LizardmanDefender()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = NameList.RandomName("lizardman");
            this.Title = "the defender";
            this.Body = 35;//Utility.RandomList(35, 36);
            this.BaseSoundID = 417;

            this.SetStr(51, 101);    // 최종 Str 750~800
			this.SetDex(101, 151);   // 최종 Dex ~450
			this.SetInt(42, 92);     // 최종 Int 150~200

			this.SetHits(51, 551);   // 최종 Hits 5,000~5,500
			this.SetStam(101, 151);

			SetAttackSpeed(2.5);
			SetDamage(22, 32); 

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 갑옷을 입어 일반 리자드맨보다 높지만 30%대 유지
			this.SetResistance(ResistanceType.Physical, 30, 35);
			this.SetResistance(ResistanceType.Cold, 25, 30);

			// 최종 Skill 75.0~85.0 (85.0 - 6.8 = 78.2)
			this.SetSkill(SkillName.Wrestling, 68.2, 78.2);
			this.SetSkill(SkillName.Tactics, 78.2, 88.2);
			this.SetSkill(SkillName.Anatomy, 78.2, 88.2);
			this.SetSkill(SkillName.MagicResist, 68.2, 78.2);

			this.VirtualArmor = 15;

			this.Fame = 2500;
			this.Karma = -2500;		
			this.SpecialType2 = 5;
			this.SpecialChance2 = 0.1;	
        }

        public LizardmanDefender(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType { get { return InhumanSpeech.Lizardman; } }
        public override bool CanRummageCorpses { get { return true; } }
		public override int TreasureMapLevel { get { return 3; } }
        public override int Meat { get { return 1; } }
        public override int Hides { get { return 12; } }
        public override HideType HideType { get { return HideType.Spined; } }

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
