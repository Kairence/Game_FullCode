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

            SetStr(196, 225);
            SetDex(196, 225);
            SetInt(196, 225);

            SetHits(1180, 1350);
            SetStam(500, 750);
            SetMana(50, 100);

			SetAttackSpeed(1.5);
			
            SetDamage(16, 22);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Chaos, 100);

            SetResistance(ResistanceType.Physical, 35, 45);
            SetResistance(ResistanceType.Fire, 20, 40);
            SetResistance(ResistanceType.Cold, 10, 30);
            SetResistance(ResistanceType.Poison, 5, 10);
            SetResistance(ResistanceType.Energy, 50, 70);

            SetSkill(SkillName.EvalInt, 80.0);
            SetSkill(SkillName.Magery, 80.0);
            SetSkill(SkillName.MagicResist, 80.0);
            SetSkill(SkillName.Tactics, 80.0);
            SetSkill(SkillName.Wrestling, 80.0);
            SetSkill(SkillName.Necromancy, 80.0);
            SetSkill(SkillName.SpiritSpeak, 80.0);

            Fame = 6500;
            Karma = -6500;

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