using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a fire steed corpse")]
    public class FireSteed : BaseMount
    {
        [Constructable]
        public FireSteed()
            : this("a fire steed")
        {
        }

        [Constructable]
        public FireSteed(string name)
            : base(name, 0xBE, 0x3E9E, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0xA8;
            Hue = 1161;

            /* Fire Steed - Fame 15,000 / Karma -15,000 */
			/* [HP Calculation]
			   - Target HP: ~55,000
			   - Fame Bonus (15,000): ~37,200
			   - SetHits Required: 17,800 (Target - Bonus)
			*/
			this.SetStr(600, 800);       
			this.SetDex(250, 350);       // 압도적인 기동성
			this.SetInt(300, 450);       

			// [Hits] 최종 약 50,000 ~ 60,000 타겟
			this.SetHits(12800, 22800); 
			this.SetStam(250, 350);      
			this.SetMana(300, 450);      

			this.SetAttackSpeed(1.8);    // 매우 빠른 공격 속도
			this.SetDamage(25, 40);      

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 50, 65);
			this.SetResistance(ResistanceType.Fire, 75, 75);     // Max 75%
			this.SetResistance(ResistanceType.Cold, 5, 20);      
			this.SetResistance(ResistanceType.Poison, 40, 55);

			this.SetSkill(SkillName.Wrestling, 110.0, 125.0);
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.VirtualArmor = 10;      

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 2;       // 기동성을 위해 2슬롯 유지 (테이머의 강력한 동반자)
			this.MinTameSkill = 185.0;   // 200 상한 서버의 최상위 타겟

			this.Fame = 15000;           
			this.Karma = -15000;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public FireSteed(Serial serial)
            : base(serial)
        {
        }

        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Daemon | PackInstinct.Equine;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1)
            {
                for (int i = 0; i < Skills.Length; ++i)
                {
                    Skills[i].Cap = Math.Max(100.0, Skills[i].Cap * 0.9);

                    if (Skills[i].Base > Skills[i].Cap)
                    {
                        Skills[i].Base = Skills[i].Cap;
                    }
                }
            }
        }
    }
}
