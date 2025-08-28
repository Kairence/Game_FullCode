using System;

namespace Server.Mobiles
{
    [CorpseName("a ghostly corpse")]
    public class Ghoul : BaseCreature
    {
        [Constructable]
        public Ghoul()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a ghoul";
            this.Body = 740;
            this.BaseSoundID = 0x482;

            this.SetStr(2356, 2400);
            this.SetDex(2226, 3235);
            this.SetInt(1116, 1120);

            this.SetHits(3446, 4550);
			this.SetStam(3400, 3500);
            this.SetMana(100, 110);

			SetAttackSpeed( 15.0 );

            this.SetDamage(120, 342);

            this.SetDamageType(ResistanceType.Energy, 100);

            this.SetResistance(ResistanceType.Physical, 65, 70);
            this.SetResistance(ResistanceType.Cold, 60, 70);
            this.SetResistance(ResistanceType.Poison, 85, 90);
            this.SetResistance(ResistanceType.Energy, 60, 70);

            this.SetSkill(SkillName.MagicResist, 130.1, 141.0);
            this.SetSkill(SkillName.Tactics, 130.1, 131.0);
            this.SetSkill(SkillName.Wrestling, 145.1, 146.0);

            this.Fame = 10000;
            this.Karma = -10000;

            this.VirtualArmor = 77;
        }

        public Ghoul(Serial serial)
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
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
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
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
            this.PackItem(Loot.RandomWeapon());
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
