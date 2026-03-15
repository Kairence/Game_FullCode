using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class SkeletalMage : BaseCreature
    {
        [Constructable]
        public SkeletalMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a skeletal mage";
            Body = 148;
            BaseSoundID = 451;

            /* Skeletal Mage - Fame 10,500 */
			this.Fame = 10500;
			this.Karma = -10500;

			this.SetInt(400, 500);
			this.SetHits(1500, 2000); // 최종 Hits 약 36,000
			this.SetMana(800, 1000);

			SetAttackSpeed(10.0);
			SetDamage(12, 18);

			this.SetSkill(SkillName.Magery, 140.0, 155.0); 
			this.SetSkill(SkillName.EvalInt, 140.0, 155.0);
			this.SetSkill(SkillName.Meditation, 120.0);

			this.SetDamageType(ResistanceType.Energy, 100);
			this.SetResistance(ResistanceType.Physical, 20, 30);
			this.SetResistance(ResistanceType.Energy, 50, 60);
			this.VirtualArmor = 10;
        }

        public SkeletalMage(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune { get { return true; } }
        public override OppositionGroup OppositionGroup { get { return OppositionGroup.FeyAndUndead; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override TribeType Tribe { get { return TribeType.Undead; } }

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
