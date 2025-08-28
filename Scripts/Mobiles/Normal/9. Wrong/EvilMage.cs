using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("an evil mage corpse")] 
    public class EvilMage : BaseCreature 
    { 
        [Constructable] 
        public EvilMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            Name = NameList.RandomName("evil mage");
            Title = "the evil mage";

            this.AddItem(new Robe(1172));
            this.AddItem(new Sandals(1172));

            if (Female = Utility.RandomBool())
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
                AddItem(new Skirt(Utility.RandomNeutralHue()));
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
                AddItem(new ShortPants(Utility.RandomNeutralHue()));
            }

			Hue = 1172;
			
            SetStr(1281, 1305);
            SetDex(991, 1150);
            SetInt(2296, 3200);

            SetHits(2490, 2630);
			SetStam(300, 400);
			SetMana(2100, 2520);

			SetAttackSpeed( 50.0 );

            SetDamage(50, 100);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 15, 20);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Poison, 5, 10);
            SetResistance(ResistanceType.Energy, 5, 10);

            SetSkill(SkillName.EvalInt, 175.1, 200.0);
            SetSkill(SkillName.Magery, 175.1, 200.0);
            SetSkill(SkillName.MagicResist, 175.0, 197.5);
            SetSkill(SkillName.Tactics, 165.0, 187.5);
            SetSkill(SkillName.Wrestling, 20.2, 60.0);

            Fame = 10000;
            Karma = -10000;

            VirtualArmor = 16;
            PackReg(6);
        }

        public override int GetDeathSound()
        {
            return 0x423;
        }

        public override int GetHurtSound()
        {
            return 0x436;
        }

        public EvilMage(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
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
                return Core.AOS ? 1 : 0;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
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
