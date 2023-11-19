using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a ghostly corpse")]
    public class Spectre : BaseCreature
    {
        [Constructable]
        public Spectre()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a spectre";
            this.Body = 26;
            this.BaseSoundID = 0x482;

            this.SetStr(100, 107);
            this.SetDex(106, 120);
            this.SetInt(58, 70);

            this.SetHits(220, 225);
			SetStam(100, 120);
			this.SetMana(40, 60);
			
            this.SetDamage(3, 7);
			SetAttackSpeed( 10.0 );
            this.SetDamageType(ResistanceType.Physical, 50);
            this.SetDamageType(ResistanceType.Cold, 50);

            this.SetResistance(ResistanceType.Physical, 25, 30);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 10, 20);

            this.SetSkill(SkillName.EvalInt, 1.1, 2.0);
            this.SetSkill(SkillName.Magery, 1.1, 2.0);

            this.Fame = 1000;
            this.Karma = -1000;

            this.VirtualArmor = 0;
        }

        public Spectre(Serial serial)
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
