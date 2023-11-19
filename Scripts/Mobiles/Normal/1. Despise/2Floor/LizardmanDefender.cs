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

            SetStr(150, 160);
            SetDex(155, 165);
            SetInt(112, 120);

            SetHits(650, 660);
			SetMana(110, 115);
			SetStam(250, 255);
			
			SetAttackSpeed( 5.0 );

            SetDamage(15, 17);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 10, 20);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 10, 20);			
			
            SetSkill(SkillName.MagicResist, 56.1, 60.0);
            SetSkill(SkillName.Tactics, 56.1, 60.0);
            SetSkill(SkillName.Wrestling, 56.1, 60.0);

            Fame = 6500;
            Karma = -6500;

            VirtualArmor = 25;
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