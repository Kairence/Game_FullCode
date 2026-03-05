using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an air elemental corpse")]
    public class AirElemental : BaseCreature
    {
        [Constructable]
        public AirElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an air elemental";
            Body = 13;
            Hue = 0x4001;
            BaseSoundID = 655;

            /* Air Elemental - Fame 6,000 / Fast Spirit */
			this.SetStr(200, 300);       
			this.SetDex(350, 450);       // 민첩 가중치
			this.SetInt(300, 450);       

			// [Hits] 최종 약 14,000 ~ 16,000 타겟
			this.SetHits(150, 2150); 
			this.SetStam(350, 450);      
			this.SetMana(300, 450);      

			this.SetAttackSpeed(1.8);    // 매우 빠른 공속
			this.SetDamage(12, 18);      

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 15, 25);
			this.SetResistance(ResistanceType.Cold, 15, 25);
			this.SetResistance(ResistanceType.Poison, 20, 30);
			this.SetResistance(ResistanceType.Energy, 60, 75);

			this.SetSkill(SkillName.Wrestling, 80.0, 95.0);
			this.SetSkill(SkillName.Tactics, 80.0, 95.0);
			this.SetSkill(SkillName.Magery, 80.0, 90.0);

			this.VirtualArmor = 15;      
			this.Tamable = false;

			this.Fame = 6000;           
			this.Karma = -6000;

            //this.PackItem(new SulfurousAsh(4));

			/*
			switch (Utility.Random(24))
            {
                case 0: PackItem(new PainSpikeScroll()); break;
                case 1: PackItem(new PoisonStrikeScroll()); break;
                case 2: PackItem(new StrangleScroll()); break;
                case 3: PackItem(new VengefulSpiritScroll()); break;
			}
			*/

            ControlSlots = 2;
        }

        public AirElemental(Serial serial)
            : base(serial)
        {
        }

        public override double DispelDifficulty
        {
            get
            {
                return 117.5;
            }
        }
        public override double DispelFocus
        {
            get
            {
                return 45.0;
            }
        }
        public override bool BleedImmune
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
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.MedScrolls);
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