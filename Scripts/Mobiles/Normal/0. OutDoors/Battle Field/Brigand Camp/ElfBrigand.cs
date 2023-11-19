using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("an elf corpse")]
    public class ElfBrigand : BaseCreature
    {
        [Constructable]
        public ElfBrigand()
            : base(AIType.AI_Archer, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Race = Race.Elf;

            if (Female = Utility.RandomBool())
            {
                Body = 606;
                Name = NameList.RandomName("Elf female");
            }
            else
            {
                Body = 605;
                Name = NameList.RandomName("Elf male");
            }

            Title = "the brigand";
            Hue = Race.RandomSkinHue();

            SetStr(236, 240);
            SetDex(321, 335);
            SetInt(131, 135);

			SetHits(560, 580);
			SetMana(150);
			SetStam(365, 370);			
			
            SetDamage(15, 44);
            this.VirtualArmor = 5;

			SetAttackSpeed( 3.0 );

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 10, 15);
            SetResistance(ResistanceType.Fire, 10, 15);
            SetResistance(ResistanceType.Cold, 10, 15);
            SetResistance(ResistanceType.Poison, 10, 15);
            SetResistance(ResistanceType.Energy, 10, 15);

            SetSkill(SkillName.MagicResist, 115.0, 117.5);
            SetSkill(SkillName.Archery, 115.0, 117.5);
            SetSkill(SkillName.Tactics, 115.0, 117.5);

            Fame = 6000;
            Karma = -6000;

            // outfit
            AddItem(new Shirt(Utility.RandomNeutralHue()));

            this.AddItem(new Bow());
            //this.PackItem(new Arrow(Utility.RandomMinMax(50, 70)));


            if (Female)
            {
                if (Utility.RandomBool())
                    AddItem(new Skirt(Utility.RandomNeutralHue()));
                else
                    AddItem(new Kilt(Utility.RandomNeutralHue()));
            }
            else
                AddItem(new ShortPants(Utility.RandomNeutralHue()));

            // hair, facial hair			
            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();

            // weapon, shield
            //Item weapon = Loot.RandomWeapon();

            //AddItem(weapon);

            //if (weapon.Layer == Layer.OneHanded && Utility.RandomBool())
             //   AddItem(Loot.RandomShield());

            PackGold(50, 150);
        }

        public ElfBrigand(Serial serial)
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
        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }
        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.75)
                c.DropItem(new SeveredElfEars());
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
