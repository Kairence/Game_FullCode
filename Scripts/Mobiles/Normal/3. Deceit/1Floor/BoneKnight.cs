using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class BoneKnight : BaseCreature
    {
        [Constructable]
        public BoneKnight()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a bone knight";
            this.Body = 57;
            this.BaseSoundID = 451;

            this.SetStr(260, 280);
            this.SetDex(260, 280);
            this.SetInt(260, 280);

            this.SetHits(1380, 1400);
			SetStam(100, 150);
			SetMana(10, 20);
            this.SetDamage(5, 15);

 			SetAttackSpeed( 2.5 );

			this.SetDamageType(ResistanceType.Physical, 40);
            this.SetDamageType(ResistanceType.Cold, 60);

            this.SetResistance(ResistanceType.Physical, 35, 55);
            this.SetResistance(ResistanceType.Fire, 30, 40);
            this.SetResistance(ResistanceType.Cold, 0, 20);
            this.SetResistance(ResistanceType.Poison, 30, 50);
            this.SetResistance(ResistanceType.Energy, 30, 40);

            this.SetSkill(SkillName.MagicResist, 75.1, 77.5);
            this.SetSkill(SkillName.Tactics, 80.1, 85.0);
            this.SetSkill(SkillName.Wrestling, 75.1, 80.0);

            this.Fame = 8000;
            this.Karma = -8000;

            this.VirtualArmor = 15;
        }

        public BoneKnight(Serial serial)
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
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
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
