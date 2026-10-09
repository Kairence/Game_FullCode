using System;
using Server.Factions;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a arch daemon corpse")]
    public class ArchDaemon : BaseCreature
    {
        [Constructable]
        public ArchDaemon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an Arch Deamon";
            this.Body = 9;
            this.BaseSoundID = 357;

            /* Arch Daemon - Fame 22,000 / Karma -22,000 */
			/* [HP Calculation]
			   - Target HP: ~95,000
			   - Fame Bonus (22,000): ~64,250
			   - SetHits Required: 30,750 (Target - Bonus)
			*/
			this.SetStr(800, 1000);      
			this.SetDex(200, 300);       
			this.SetInt(900, 1200);      

			// [Hits] 최종 약 90,000 ~ 100,000 타겟
			this.SetHits(25750, 35750); 
			this.SetStam(200, 300);      
			this.SetMana(900, 1200);      

			SetAttackSpeed(2.5);
			SetDamage(75, 110);     

			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 75, 75);     
			this.SetResistance(ResistanceType.Cold, 50, 65);     
			this.SetResistance(ResistanceType.Poison, 65, 75);
			this.SetResistance(ResistanceType.Energy, 65, 75);

			this.SetSkill(SkillName.Magery, 120.0, 135.0);
			this.SetSkill(SkillName.EvalInt, 120.0, 135.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 140.0);
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0);

			this.VirtualArmor = 15;      
			this.Tamable = false;

			this.Fame = 22000;           
			this.Karma = -22000;
        }

        public ArchDaemon(Serial serial)
            : base(serial)
        {
        }

        public override double DispelDifficulty
        {
            get
            {
                return 125.0;
            }
        }
        public override double DispelFocus
        {
            get
            {
                return 45.0;
            }
        }
        public override Faction FactionAllegiance
        {
            get
            {
                return Shadowlords.Instance;
            }
        }
        public override Ethics.Ethic EthicAllegiance
        {
            get
            {
                return Ethics.Ethic.Evil;
            }
        }
        public override bool CanRummageCorpses
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
        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Average, 2);
            this.AddLoot(LootPack.MedScrolls, 2);
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
