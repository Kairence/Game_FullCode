using Server.Ethics;
using Server.Factions;
using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a daemon corpse")]
    public class Daemon : BaseCreature
    {
        [Constructable]
        public Daemon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("daemon");
            Body = 10;
            BaseSoundID = 357;

            /* Daemon - Fame 15,000 / Karma -15,000 */
			/* [HP Calculation]
			   - Target HP: ~45,000
			   - Fame Bonus (15,000): ~37,200
			   - SetHits Required: 7,800 (Target - Bonus)
			*/
			this.SetStr(500, 700);       
			this.SetDex(150, 250);       
			this.SetInt(500, 700);       

			// [Hits] 최종 약 42,000 ~ 48,000 타겟
			this.SetHits(4800, 10800); 
			this.SetStam(150, 250);      
			this.SetMana(500, 700);      

			SetAttackSpeed(2.5);
			SetDamage(60, 85);   

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 50, 65);
			this.SetResistance(ResistanceType.Fire, 65, 75);     
			this.SetResistance(ResistanceType.Cold, 35, 50);     
			this.SetResistance(ResistanceType.Poison, 50, 65);

			this.SetSkill(SkillName.Magery, 105.0, 120.0);
			this.SetSkill(SkillName.EvalInt, 105.0, 120.0);
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);

			this.VirtualArmor = 10;      
			this.Tamable = false;

			this.Fame = 15000;           
			this.Karma = -15000;

            switch (Utility.Random(20))
            {
                case 0:
                    PackItem(new LichFormScroll());
                    break;
                case 1:
                    PackItem(new PoisonStrikeScroll());
                    break;
                case 2:
                    PackItem(new StrangleScroll());
                    break;
                case 3:
                    PackItem(new VengefulSpiritScroll());
                    break;
                case 4:
                    PackItem(new WitherScroll());
                    break;
            }


            ControlSlots = Core.SE ? 4 : 5;
        }

        public Daemon(Serial serial)
            : base(serial)
        {
        }

        public override double DispelDifficulty
        {
            get { return 125.0; }
        }

        public override double DispelFocus
        {
            get { return 45.0; }
        }

        public override Faction FactionAllegiance
        {
            get { return Shadowlords.Instance; }
        }

        public override Ethic EthicAllegiance
        {
            get { return Ethic.Evil; }
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Regular; }
        }

        public override int TreasureMapLevel
        {
            get { return 4; }
        }

        public override int Meat
        {
            get { return 1; }
        }

        public override bool CanFly
        {
            get { return true; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average, 2);
            AddLoot(LootPack.MedScrolls, 2);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}
