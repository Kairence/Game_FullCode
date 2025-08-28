using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a liche's corpse")]
    public class LichLord : BaseCreature
    {
        [Constructable]
        public LichLord()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a lich lord";
            this.Body = 78;
            this.BaseSoundID = 412;

            this.SetStr(4160, 5150);
            this.SetDex(2460, 3265);
            this.SetInt(5686, 5705);

            this.SetHits(8000, 8103);
			this.SetStam(6200, 7270);
			SetMana(7318, 7766);
            this.SetDamage(372, 825);

			SetAttackSpeed( 10.0 );

            this.SetDamageType(ResistanceType.Physical, 0);
            this.SetDamageType(ResistanceType.Cold, 60);
            this.SetDamageType(ResistanceType.Energy, 40);

            this.SetResistance(ResistanceType.Physical, 70, 80);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 50, 60);
            this.SetResistance(ResistanceType.Poison, 90, 100);
            this.SetResistance(ResistanceType.Energy, 40, 50);

            this.SetSkill(SkillName.EvalInt, 260.1, 270.0);
            this.SetSkill(SkillName.Magery, 260.1, 270.0);
            this.SetSkill(SkillName.MagicResist, 260.5, 270.0);
            this.SetSkill(SkillName.Tactics, 260.1, 270.0);
            this.SetSkill(SkillName.Wrestling, 265.1, 270.0);

            this.Fame = 20000;
            this.Karma = -20000;

            this.VirtualArmor = 100;
        }

        public LichLord(Serial serial)
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
                return 4;
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
