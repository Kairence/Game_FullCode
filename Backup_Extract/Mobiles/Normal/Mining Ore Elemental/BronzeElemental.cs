using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ore elemental corpse")]
    public class BronzeElemental : BaseCreature, IOreElemental
    {
        [Constructable]
        public BronzeElemental()
            : this(25)
        {
        }

        [Constructable]
        public BronzeElemental(int oreAmount)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            // TODO: Gas attack
            Name = "a bronze elemental";
            Body = 108;
            BaseSoundID = 268;

            /* Earth Elemental - Fame 4,500 / Common Spirit */
			this.SetStr(250, 350);       
			this.SetDex(60, 100);       
			this.SetInt(60, 100);       

			// [Hits] 명성 보석(9,800) 포함 최종 1.1만 내외
			this.SetHits(2000, 2400); 
			this.SetStam(60, 100);      
			this.SetMana(60, 100);      

			SetAttackSpeed(4.5);
			SetDamage(45, 65);    

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 5, 10);
			this.SetResistance(ResistanceType.Cold, 5, 10);
			this.SetResistance(ResistanceType.Poison, 15, 25);
			this.SetResistance(ResistanceType.Energy, 10, 20);

			this.SetSkill(SkillName.Wrestling, 70.0, 80.0);
			this.SetSkill(SkillName.Tactics, 70.0, 80.0);
			this.SetSkill(SkillName.MagicResist, 40.0, 60.0);

			this.VirtualArmor = 15;      
			this.Tamable = false;

			this.Fame = 0;           
			this.Karma = 0;
		}

        public BronzeElemental(Serial serial)
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
        public override bool AutoDispel
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

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Gems, 2);
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
