using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ogre corpse")]
    public class Ogre : BaseCreature
    {
        [Constructable]
        public Ogre()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an ogre";
            this.Body = 1;
            this.BaseSoundID = 427;

            this.SetStr(182, 282);   // 최종 Str 1,600~1,700
			this.SetDex(66, 86);     // 둔중함
			this.SetInt(7, 27);      // 최종 Int 250~270

			this.SetHits(551, 1551); // 최종 Hits 20,000~21,000
			this.SetStam(66, 86);

			SetAttackSpeed(4.0);     
			SetDamage(45, 70); 

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 거대한 살덩이 (40% 미만)
			this.SetResistance(ResistanceType.Physical, 35, 40);
			this.SetResistance(ResistanceType.Fire, 15, 25);
			this.SetResistance(ResistanceType.Poison, 30, 40);

			// 최종 Skill 120.0~130.0 (130.0 - 32.0 = 98.0)
			this.SetSkill(SkillName.Wrestling, 88.0, 98.0);
			this.SetSkill(SkillName.Tactics, 98.0, 108.0);
			this.SetSkill(SkillName.MagicResist, 88.0, 98.0);

			this.VirtualArmor = 20;

			this.Fame = 10000;
			this.Karma = -10000;

            this.PackItem(new Club());
        }

        public Ogre(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override int Meat
        {
            get
            {
                return 2;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Potions);
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