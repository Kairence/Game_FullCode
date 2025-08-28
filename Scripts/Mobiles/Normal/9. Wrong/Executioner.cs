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

            SetStr(2386, 4400);
            SetDex(3651, 5665);
            SetInt(2361, 3375);

            SetHits(9000, 11263);
			SetStam(9130, 10040);
			SetMana(1000, 1100);

            SetDamage(158, 380);
			SetAttackSpeed( 5.0 );

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 95, 105);
            SetResistance(ResistanceType.Fire, 25, 30);
            SetResistance(ResistanceType.Cold, 25, 30);
            SetResistance(ResistanceType.Poison, 10, 20);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.Anatomy, 225.0);
            SetSkill(SkillName.Fencing, 146.0, 177.5);
            SetSkill(SkillName.Macing, 135.0, 157.5);
            SetSkill(SkillName.Poisoning, 160.0, 182.5);
            SetSkill(SkillName.MagicResist, 183.5, 192.5);
            SetSkill(SkillName.Swords, 225.0);
            SetSkill(SkillName.Tactics, 225.0);
            SetSkill(SkillName.Lumberjacking, 425.0);

            Fame = 19000;
            Karma = -19000;

            VirtualArmor = 40;

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
