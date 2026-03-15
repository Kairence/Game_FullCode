using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a lava serpent corpse")]
    [TypeAlias("Server.Mobiles.Lavaserpant")]
    public class LavaSerpent : BaseCreature
    {
        [Constructable]
        public LavaSerpent()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a lava serpent";
            Body = 90;
            BaseSoundID = 219;

			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1174;

            /* Lava Serpent - Fame 10,000 / Karma -10,000 */
				/* [HP Calculation]
			   - Target HP: ~28,000
			   - Fame Bonus (10,000): ~24,150
			   - SetHits Required: 3,850 (Target - Bonus)
			*/
			this.SetStr(400, 600);       
			this.SetDex(150, 250);       

			// [Hits] 최종 약 26,000 ~ 30,000 타겟
			this.SetHits(1850, 5850); 
			this.SetStam(150, 250);      

			SetAttackSpeed(2.5);
			SetDamage(50, 75);    

			this.SetDamageType(ResistanceType.Physical, 30);
			this.SetDamageType(ResistanceType.Fire, 70);

			this.SetResistance(ResistanceType.Physical, 45, 60);
			this.SetResistance(ResistanceType.Fire, 75, 75);     // Max 75%
			this.SetResistance(ResistanceType.Cold, -5, 10);     
			this.SetResistance(ResistanceType.Poison, 40, 55);

			this.SetSkill(SkillName.Wrestling, 100.0, 115.0);
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);

			this.VirtualArmor = 10;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       
			this.MinTameSkill = 135.0;   // 200 상한 대비 중상급 숙련도 요구

			this.Fame = 10000;           
			this.Karma = -10000;
        }

        public LavaSerpent(Serial serial)
            : base(serial)
        {
        }

        public override bool DeathAdderCharmable
        {
            get
            {
                return true;
            }
        }
        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override int Hides
        {
            get
            {
                return 15;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }

        public void AuraEffect(Mobile m)
        {
            m.SendMessage("The radiating heat scorches your skin!");
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
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
