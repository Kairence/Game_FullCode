using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a liche's corpse")]
    public class Lich : BaseCreature
    {
        [Constructable]
        public Lich()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a lich";
            Body = 24;
            BaseSoundID = 0x3E9;

            /* Lich - Fame 14,000 / Undead Mage */
			this.SetStr(550, 650);       
			this.SetDex(250, 350);       
			this.SetInt(900, 1050);      

			// [Hits] 최종 약 49,000 ~ 51,000 타겟
			this.SetHits(15300, 17300); 
			this.SetStam(250, 350);      
			this.SetMana(900, 1050);     

			SetAttackSpeed(5.5);
			SetDamage(40, 55);      

			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Cold, 30);
			this.SetDamageType(ResistanceType.Energy, 30);

			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Fire, 15, 25);
			this.SetResistance(ResistanceType.Cold, 45, 55);
			this.SetResistance(ResistanceType.Energy, 40, 50);
			this.SetResistance(ResistanceType.Poison, 45, 55);

			this.SetSkill(SkillName.Magery, 100.0, 110.0);
			this.SetSkill(SkillName.EvalInt, 100.0, 110.0);
			this.SetSkill(SkillName.Meditation, 90.0, 100.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 110.0);
			this.SetSkill(SkillName.Wrestling, 100.0, 110.0);
			this.SetSkill(SkillName.Tactics, 90.0, 100.0);

			this.VirtualArmor = 25;      
			this.Tamable = false;

			this.Fame = 14000;           
			this.Karma = -14000;
			this.SpecialType2 = 4;
			this.SpecialChance2 = 0.15;	
			
        }

        public Lich(Serial serial)
            : base(serial)
        {
        }

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override bool BleedImmune
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
                return Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 3;
            }
        }
        public override void GenerateLoot()
        {

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
