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

            this.SetStr(24, 74);     
			this.SetDex(100, 150);   
			this.SetInt(154, 254);   

			this.SetHits(345, 1345); // 최종 Hits 9,000~10,000
			this.SetMana(1000, 2000);

			SetAttackSpeed(12.0); // 10초 이상의 긴 빈틈
			SetDamage(10, 15);    // 근접전 무력화

			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 40);

			this.SetResistance(ResistanceType.Physical, 20, 25);
			this.SetResistance(ResistanceType.Cold, 20, 25);
			this.SetResistance(ResistanceType.Energy, 15, 20);
			this.SetResistance(ResistanceType.Poison, 45, 50);

			// 최종 Skill 85.0~95.0 목표 (95.0 - 12.7 = 82.3)
			this.SetSkill(SkillName.Magery, 72.3, 82.3);
			this.SetSkill(SkillName.EvalInt, 72.3, 82.3);
			this.SetSkill(SkillName.MagicResist, 72.3, 82.3);

			this.VirtualArmor = 0;

			this.Fame = 4500;
			this.Karma = -4500;
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
