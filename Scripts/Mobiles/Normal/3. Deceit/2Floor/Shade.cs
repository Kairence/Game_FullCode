using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a ghostly corpse")]
    public class Shade : BaseCreature
    {
        [Constructable]
        public Shade()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a shade";
            this.Body = 740;
            this.Hue = 0x4001;
            this.BaseSoundID = 0x482;

            this.SetStr(276, 300);
            this.SetDex(276, 295);
            this.SetInt(3060, 3600);

            this.SetHits(2846, 2860);
			this.SetStam(400, 500);
			this.SetMana(2000, 2300);
			
            this.SetDamage(49, 130);

			SetAttackSpeed( 120 );

            this.SetDamageType(ResistanceType.Cold, 100);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 10, 20);

            this.SetSkill(SkillName.EvalInt, 145.1, 150.0);
            this.SetSkill(SkillName.Magery, 145.1, 150.0);
            this.SetSkill(SkillName.MagicResist, 145.1, 150.0);
            this.SetSkill(SkillName.Tactics, 145.1, 150.0);
            this.SetSkill(SkillName.Wrestling, 215.1, 226.0);

            this.Fame = 11000;
            this.Karma = -11000;

            this.VirtualArmor = 22;

            //this.PackReg(10);
        }

        public Shade(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
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
