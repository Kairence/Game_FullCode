using System;

namespace Server.Mobiles
{
    [CorpseName("an ice snake corpse")]
    [TypeAlias("Server.Mobiles.Icesnake")]
    public class IceSnake : BaseCreature
    {
        [Constructable]
        public IceSnake()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an ice snake";
            this.Body = 52;

			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1152;
			else if(Utility.RandomBool() )
				this.Hue = 1153;
			else
				this.Hue = 1154;
			
            this.BaseSoundID = 0xDB;

            /* Ice Snake - Fame 1,000 / Karma -1,000 */
			/* [HP Calculation]
			   - Target HP: ~2,500
			   - Fame Bonus (1,000): ~1,563
			   - SetHits Required: 937 (Target - Bonus)
			*/
			this.SetStr(50, 80);       
			this.SetDex(150, 200);       
			this.SetInt(20, 50);         

			// [Hits] 최종 약 2,000 ~ 3,000 타겟
			this.SetHits(437, 1437); 
			this.SetStam(150, 200);      

			SetAttackSpeed(2.0);
			SetDamage(12, 18);    

			this.SetDamageType(ResistanceType.Cold, 100);

			this.SetResistance(ResistanceType.Physical, 10, 20);
			this.SetResistance(ResistanceType.Fire, -10, 5);    
			this.SetResistance(ResistanceType.Cold, 75, 75);    // Max 75%
			this.SetResistance(ResistanceType.Poison, 50, 65);

			this.SetSkill(SkillName.Wrestling, 60.0, 75.0);
			this.SetSkill(SkillName.Tactics, 60.0, 75.0);
			this.SetSkill(SkillName.Poisoning, 60.0, 80.0);

			this.VirtualArmor = 2;       
			this.Tamable = true;         
			this.ControlSlots = 1;       
			this.MinTameSkill = 50.0;    

			this.Fame = 1000;           
			this.Karma = -1000;
        }

        public IceSnake(Serial serial)
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
                return 1;
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
