using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gargoyle corpse")]
    public class GargoyleEnforcer : BaseCreature
    {
        [Constructable]
        public GargoyleEnforcer()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Gargoyle Enforcer";
            Body = 0x2F2;
            BaseSoundID = 0x174;

            /* Gargoyle Enforcer - Fame 14,000 / Karma -14,000 */
			/* [HP Calculation]
			   - Target HP: ~55,000
			   - Fame Bonus (14,000): ~33,680
			   - SetHits Required: 21,320 (Target - Bonus)
			*/
			this.SetStr(700, 900);       
			this.SetDex(150, 200);       
			this.SetInt(150, 250);       

			// [Hits] 최종 약 52,000 ~ 58,000 타겟
			this.SetHits(18320, 24320); 
			this.SetStam(150, 200);      

			SetAttackSpeed(10.0);
			SetDamage(18, 28);     

			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Fire, 30);

			this.SetResistance(ResistanceType.Physical, 60, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 75, 75);     
			this.SetResistance(ResistanceType.Cold, 20, 35);     
			this.SetResistance(ResistanceType.Poison, 45, 60);

			this.SetSkill(SkillName.Macing, 115.0, 130.0);       // 둔기 숙련
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 125.0);

			this.VirtualArmor = 15;      
			this.Tamable = false;

			this.Fame = 14000;           
			this.Karma = -14000;
        }

        public GargoyleEnforcer(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
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