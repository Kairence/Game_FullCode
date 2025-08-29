using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("an evil mage lord corpse")] 
    public class GolemLord : BaseCreature 
    { 
        [Constructable] 
        public GolemLord()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            //Name = NameList.RandomName("golem lord");
            Name = "Boss a golem lord";
            Body = Utility.RandomList(125, 126);

            //PackItem(new Robe(Utility.RandomMetalHue())); 
            //PackItem(new WizardsHat(Utility.RandomMetalHue())); 

			Boss = true;

            SetStr(6810, 7050);
            SetDex(10910, 12150);
            SetInt(526, 550);

            SetHits(240900, 262630);
			SetStam(213000, 241400);
			SetMana(100000, 110000);

			SetAttackSpeed( 10.0 );

            SetDamage(1258, 4000);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 35, 40);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.EvalInt, 230.2, 250.0);
            SetSkill(SkillName.Magery, 245.1, 250.0);
            SetSkill(SkillName.Meditation, 227.5, 300.0);
            SetSkill(SkillName.MagicResist, 227.5, 350.0);
            SetSkill(SkillName.Tactics, 235.0, 327.5);
            SetSkill(SkillName.Wrestling, 320.3, 330.0);

            Fame = 25000;
            Karma = -25000;

            SetSpecialAbility(SpecialAbility.ColossalBlow);

            VirtualArmor = 55;
        }

        public override int GetAngerSound()
        {
            return 541;
        }

        public override int GetIdleSound()
        {
            if (!Controlled)
                return 542;

            return base.GetIdleSound();
        }

        public override int GetDeathSound()
        {
            if (!Controlled)
                return 545;

            return base.GetDeathSound();
        }

        public override int GetAttackSound()
        {
            return 562;
        }

        public override int GetHurtSound()
        {
            if (Controlled)
                return 320;

            return base.GetHurtSound();
        }
        public GolemLord(Serial serial)
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
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return Core.AOS ? 2 : 0;
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