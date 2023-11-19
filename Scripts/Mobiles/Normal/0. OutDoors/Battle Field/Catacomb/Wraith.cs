using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a ghostly corpse")]
    public class Wraith : BaseCreature
    {
        [Constructable]
        public Wraith()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a wraith";
            this.Body = 26;
            this.Hue = 0x4001;
            this.BaseSoundID = 0x482;

            this.SetStr(126, 130);
            this.SetDex(126, 138);
            this.SetInt(106, 114);

            this.SetHits(280, 320);
            this.SetStam(150, 180);
			this.SetMana(20, 30);
			
            this.SetDamage(2, 11);
			SetAttackSpeed( 60.0 );

            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Cold, 50);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 10, 20);

            this.SetSkill(SkillName.EvalInt, 3.1, 6.0);
            this.SetSkill(SkillName.Magery, 3.1, 6.0);

            this.Fame = 2000;
            this.Karma = -2000;

            this.VirtualArmor = 0;
        }

        public Wraith(Serial serial)
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
