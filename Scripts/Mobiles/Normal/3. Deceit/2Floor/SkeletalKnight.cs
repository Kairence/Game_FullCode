using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class SkeletalKnight : BaseCreature
    {
        [Constructable]
        public SkeletalKnight()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a skeletal knight";
            Body = 147;
            BaseSoundID = 451;

            /* Skeletal Knight - Fame 11,000 */
			this.Fame = 11000;
			this.Karma = -11000;

			// [역산] 보너스: Str +1,571 / Hits +36,180 / Skill +41.1
			this.SetStr(400, 500);    // 최종 Str 약 2,000
			this.SetHits(3000, 4000);  // 최종 Hits 약 40,000

			this.SetAttackSpeed(2.0);
			SetDamage(40, 55);

			this.SetSkill(SkillName.Wrestling, 130.0, 145.0); 
			this.SetSkill(SkillName.Tactics, 130.0, 145.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 145.0);

			this.SetDamageType(ResistanceType.Physical, 100);
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 10, 20);
			this.VirtualArmor = 30;
        }

        public SkeletalKnight(Serial serial)
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
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Meager);
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
