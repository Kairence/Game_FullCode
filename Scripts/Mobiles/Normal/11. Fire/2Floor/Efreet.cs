using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an efreet corpse")]
    public class Efreet : BaseCreature
    {
        [Constructable]
        public Efreet()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an efreet";
            this.Body = 131;
            this.BaseSoundID = 768;

            /* Efreet - Fame 12,000 / Karma -12,000 */
			/* [HP Calculation]
			   - Target HP: ~45,000
			   - Fame Bonus (12,000): ~28,650
			   - SetHits Required: 16,350 (Target - Bonus)
			*/
			this.SetStr(450, 600);       
			this.SetDex(200, 300);       
			this.SetInt(600, 800);       

			// [Hits] 최종 약 42,000 ~ 48,000 타겟
			this.SetHits(13350, 19350); 
			this.SetStam(200, 300);      
			this.SetMana(600, 800);      

			SetAttackSpeed(10.0);
			SetDamage(15, 25);     

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 75, 75);     // 화염 면역 (Max 75%)
			this.SetResistance(ResistanceType.Cold, -10, 5);     // 매우 취약
			this.SetResistance(ResistanceType.Poison, 50, 65);
			this.SetResistance(ResistanceType.Energy, 50, 65);

			this.SetSkill(SkillName.Magery, 110.0, 125.0);
			this.SetSkill(SkillName.EvalInt, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);

			this.VirtualArmor = 10;      
			this.Tamable = false;

			this.Fame = 12000;           
			this.Karma = -12000;
        }

        public Efreet(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get
            {
                return Core.AOS ? 4 : 5;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Gems);

            if (0.02 > Utility.RandomDouble())
            {
                switch ( Utility.Random(5) )
                {
                    case 0:
                        this.PackItem(new DaemonArms());
                        break;
                    case 1:
                        this.PackItem(new DaemonChest());
                        break;
                    case 2:
                        this.PackItem(new DaemonGloves());
                        break;
                    case 3:
                        this.PackItem(new DaemonLegs());
                        break;
                    case 4:
                        this.PackItem(new DaemonHelm());
                        break;
                }
            }
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