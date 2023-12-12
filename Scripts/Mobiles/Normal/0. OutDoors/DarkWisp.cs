using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a wisp corpse")]
    public class DarkWisp : BaseCreature
    {
        [Constructable]
        public DarkWisp()
            : base(AIType.AI_NecroMage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a wisp";
            Body = 165;
            BaseSoundID = 466;

            SetStr(996, 1225);
            SetDex(996, 1225);
            SetInt(996, 1225);

            SetHits(11800, 13500);
            SetStam(7500, 7750);
            SetMana(500, 1000);

			SetAttackSpeed(1.5);
			
            SetDamage(52, 75);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Chaos, 100);

            SetResistance(ResistanceType.Physical, 35, 45);
            SetResistance(ResistanceType.Fire, 20, 40);
            SetResistance(ResistanceType.Cold, 10, 30);
            SetResistance(ResistanceType.Poison, 5, 10);
            SetResistance(ResistanceType.Energy, 45, 50);

            SetSkill(SkillName.EvalInt, 160.0);
            SetSkill(SkillName.Magery, 160.0);
            SetSkill(SkillName.MagicResist, 160.0);
            SetSkill(SkillName.Tactics, 160.0);
            SetSkill(SkillName.Wrestling, 160.0);
            SetSkill(SkillName.Necromancy, 160.0);
            SetSkill(SkillName.SpiritSpeak, 160.0);

            Fame = 10500;
            Karma = -10500;

            VirtualArmor = 10;

            //AddItem(new LightSource());
        }

        public DarkWisp(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Wisp;
            }
        }
        /*public override Ethics.Ethic EthicAllegiance
        {
            get
            {
                return Ethics.Ethic.Evil;
            }
        }*/
        public override TimeSpan ReacquireDelay
        {
            get
            {
                return TimeSpan.FromSeconds(1.0);
            }
        }
        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
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