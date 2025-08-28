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

            this.SetStr(3526, 3600);
            this.SetDex(1460, 1550);
            this.SetInt(100, 120);

            this.SetHits(3500, 3750);
            this.SetMana(10, 15);
			SetStam(2500, 3450);

			SetAttackSpeed( 10.0 );

            this.SetDamage(210, 550);

            this.SetDamageType(ResistanceType.Physical, 100);
			
            this.SetResistance(ResistanceType.Physical, 30, 35);
            this.SetResistance(ResistanceType.Fire, 15, 25);
            this.SetResistance(ResistanceType.Cold, 15, 25);
            this.SetResistance(ResistanceType.Poison, 15, 25);
            this.SetResistance(ResistanceType.Energy, 25);
			
            this.SetSkill(SkillName.MagicResist, 100.1, 105.0);
            this.SetSkill(SkillName.Tactics, 110.1, 115.0);
            this.SetSkill(SkillName.Wrestling, 115.1, 120.0);

            this.Fame = 10000;
            this.Karma = -10000;

            this.VirtualArmor = Utility.RandomMinMax(45, 100);

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