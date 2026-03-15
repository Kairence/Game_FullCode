using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class BoneMagi : BaseCreature
    {
        [Constructable]
        public BoneMagi()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a bone mage";
            Body = 148;
            BaseSoundID = 451;

            /* Bone Magi - Fame 4,000 */
			this.Fame = 4000;
			this.Karma = -4000;

			this.SetStr(100, 120);
			this.SetInt(200, 250);     // 최종 Int 약 1,200
			this.SetHits(100, 150);    // 최종 Hits 약 11,000
			this.SetMana(500, 600);    

			SetAttackSpeed(10.0);
			SetDamage(12, 18);       // 물리 데미지는 낮으나 마법과 병행

			this.SetSkill(SkillName.Magery, 100.0, 115.0); 
			this.SetSkill(SkillName.EvalInt, 100.0, 110.0);
			this.SetSkill(SkillName.Meditation, 120.0);

			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Energy, 80);

			this.SetResistance(ResistanceType.Physical, 10, 20);
			this.SetResistance(ResistanceType.Energy, 30, 40);
			this.VirtualArmor = 5;
        }

        public BoneMagi(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune { get { return true; } }
        public override OppositionGroup OppositionGroup { get { return OppositionGroup.FeyAndUndead; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.Potions);
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
