using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a liche's corpse")]
    public class Lich : BaseCreature
    {
        [Constructable]
        public Lich()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a lich";
            Body = 24;
            BaseSoundID = 0x3E9;

            SetStr(2371, 3440);
            SetDex(3236, 3245);
            SetInt(3476, 3505);

            SetHits(5553, 5600);
			SetStam(5100, 5150);
			SetMana(5200, 5250);

			SetAttackSpeed( 10.0 );

            SetDamage(310, 575);

            SetDamageType(ResistanceType.Physical, 10);
            SetDamageType(ResistanceType.Cold, 40);
            SetDamageType(ResistanceType.Energy, 50);

            SetResistance(ResistanceType.Physical, 50, 60);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 85, 95);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.EvalInt, 200.0, 215.0);
            SetSkill(SkillName.Magery, 215.1, 220.0);
            SetSkill(SkillName.Meditation, 215.1, 220.0);
            SetSkill(SkillName.MagicResist, 215.1, 220.0);
            SetSkill(SkillName.Tactics, 195.1, 200.0);
            SetSkill(SkillName.Wrestling, 290.1, 300.0);

            Fame = 14000;
            Karma = -14000;

            VirtualArmor = 50;
        }

        public Lich(Serial serial)
            : base(serial)
        {
        }

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 3;
            }
        }
        public override void GenerateLoot()
        {

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
