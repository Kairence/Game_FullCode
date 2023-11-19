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

            SetStr(132, 135);
            SetDex(125, 130);
            SetInt(112, 115);

            SetHits(475, 480);
			SetMana(100, 105);
			SetStam(130, 135);
			
			SetAttackSpeed( 5.0 );

            SetDamage(6, 9);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 25, 30);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Cold, 5, 10);
            SetResistance(ResistanceType.Poison, 10, 20);			
			
            SetSkill(SkillName.MagicResist, 44.1, 45.0);
            SetSkill(SkillName.Tactics, 45.1, 50.0);
            SetSkill(SkillName.Wrestling, 45.1, 50.0);

            Fame = 5000;
            Karma = -5000;

            VirtualArmor = 4;
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