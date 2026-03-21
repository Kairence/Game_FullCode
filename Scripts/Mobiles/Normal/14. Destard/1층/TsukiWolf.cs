using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a tsuki wolf corpse")]
    public class TsukiWolf : BaseCreature
    {
        private static readonly Hashtable m_Table = new Hashtable();
        [Constructable]
        public TsukiWolf()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a tsuki wolf";
            Body = 250;

            switch( Utility.Random(3) )
            {
                case 0:
                    Hue = Utility.RandomNeutralHue();
                    break; //No, this really isn't accurate ;->
            }

            /* Tsuki Wolf - Fame 14,000 / Karma -14,000 */
			/* [HP Calculation]
			   - Target HP: ~45,000
			   - Fame Bonus (14,000): ~33,680
			   - SetHits Required: 11,320 (Target - Bonus)
			*/
			this.SetStr(600, 800);       
			this.SetDex(220, 320);       
			this.SetInt(300, 500);       

			// [Hits] 최종 약 42,000 ~ 48,000 타겟
			this.SetHits(8320, 14320); 
			this.SetStam(220, 320);      
			this.SetMana(300, 500);      

			SetAttackSpeed(2.0);
			SetDamage(65, 95);     

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			this.SetResistance(ResistanceType.Physical, 60, 75); 
			this.SetResistance(ResistanceType.Fire, 40, 55);     
			this.SetResistance(ResistanceType.Cold, 40, 55);     
			this.SetResistance(ResistanceType.Energy, 65, 75);   // 에너지 저항 특화

			this.SetSkill(SkillName.Wrestling, 110.0, 125.0);
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 125.0);
			this.SetSkill(SkillName.Magery, 90.0, 110.0);

			this.VirtualArmor = 10;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 3;       
			this.MinTameSkill = 165.0;   // 200 상한 대비 상급 숙련도

			this.Fame = 14000;           
			this.Karma = -14000;

            
            if (Core.ML && Utility.RandomDouble() < .33)
                PackItem(Engines.Plants.Seed.RandomPeculiarSeed(1));

            PackBodyPartOrBones();

            SetSpecialAbility(SpecialAbility.Rage);
        }

        public TsukiWolf(Serial serial)
            : base(serial)
        {
        }

        public override bool CanAngerOnTame { get { return true; } }

		public override int TreasureMapLevel { get { return 3; } }
        public override int Meat { get { return 4; } }
        public override int Hides { get { return 25; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }
		
		public override int GetAngerSound()
        {
            return 0x52D;
        }

        public override int GetIdleSound()
        {
            return 0x52C;
        }

        public override int GetAttackSound()
        {
            return 0x52B;
        }

        public override int GetHurtSound()
        {
            return 0x52E;
        }

        public override int GetDeathSound()
        {
            return 0x52A;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
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
