using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class Executioner : BaseCreature 
    { 
        [Constructable] 
        public Executioner()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            SpeechHue = Utility.RandomDyedHue(); 
            Title = "the executioner"; 
            Hue = Utility.RandomSkinHue(); 

            if (Female = Utility.RandomBool()) 
            { 
                Body = 0x191; 
                Name = NameList.RandomName("female"); 
                AddItem(new Skirt(Utility.RandomRedHue())); 
            }
            else 
            { 
                Body = 0x190; 
                Name = NameList.RandomName("male"); 
                AddItem(new ShortPants(Utility.RandomRedHue())); 
            }

            /* Executioner - Fame 18,000 / Karma -18,000 */
			/* [HP Calculation]
			   - Target HP: ~85,000
			   - Fame Bonus (18,000): ~47,400
			   - SetHits Required: 37,600 (Target - Bonus)
			*/
			this.SetStr(800, 1000);      
			this.SetDex(250, 350);       // 집행자다운 민첩한 움직임
			this.SetInt(100, 200);       

			// [Hits] 최종 약 80,000 ~ 90,000 타겟
			this.SetHits(32600, 42600); 
			this.SetStam(250, 350);      
			this.SetMana(100, 200);      

			SetAttackSpeed(4.5);
			SetDamage(70, 100);     // '집행'급 물리 데미지

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 60, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 35, 45);
			this.SetResistance(ResistanceType.Cold, 35, 45);
			this.SetResistance(ResistanceType.Poison, 35, 45);
			this.SetResistance(ResistanceType.Energy, 35, 45);

			this.SetSkill(SkillName.Swords, 125.0, 145.0); // 도끼 숙련도 최상급
			this.SetSkill(SkillName.Tactics, 120.0, 140.0);
			this.SetSkill(SkillName.Anatomy, 120.0, 140.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.VirtualArmor = 12;      // 노출된 가슴과 가죽 보호구 (낮은 수치 유지)
			this.Tamable = false;

			this.Fame = 18000;           
			this.Karma = -18000;

            AddItem(new ThighBoots(Utility.RandomRedHue())); 
            AddItem(new Surcoat(Utility.RandomRedHue()));    
            AddItem(new ExecutionersAxe());

            Utility.AssignRandomHair(this);
        }

        public Executioner(Serial serial)
            : base(serial)
        { 
        }

        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }

        public bool BlockReflect { get; set; }
        
        public override int Damage(int amount, Mobile from, bool informMount, bool checkDisrupt)
        {
            int dam = base.Damage(amount, from, informMount, checkDisrupt);

            if (!BlockReflect && from != null && dam > 0)
            {
                BlockReflect = true;
                AOS.Damage(from, this, dam, 0, 0, 0, 0, 0, 0, 100);
                BlockReflect = false;
                
                from.PlaySound(0x1F1);
            }

            return dam;
        }

        public override void GenerateLoot()
        {

        }

        public override void Serialize(GenericWriter writer) 
        { 
            base.Serialize(writer); 

            writer.Write((int)0); // version 
        }

        public override void Deserialize(GenericReader reader) 
        { 
            base.Deserialize(reader); 

            int version = reader.ReadInt(); 
        }
    }
}
