using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a chaos dragoon elite corpse")]
    public class ChaosDragoonElite : BaseCreature
    {
        [Constructable]
        public ChaosDragoonElite()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.15, 0.4)
        {
            Name = "a chaos dragoon elite";
            Body = 0x190;
            Hue = Utility.RandomSkinHue();

            SetStr(4760, 5500);
            SetDex(3666, 4990);
            SetInt(1260, 1500);

            SetHits( 20276, 30350 );
			SetStam( 10000, 15000 );
			SetMana( 10000, 15000 );
			
			SetAttackSpeed( 2.5 );

            SetDamage(129, 239);

            SetDamageType(ResistanceType.Physical, 25);
            SetDamageType(ResistanceType.Fire, 25);
            SetDamageType(ResistanceType.Cold, 25);
            SetDamageType(ResistanceType.Energy, 25);

            SetResistance(ResistanceType.Physical, 85, 95);
            SetResistance(ResistanceType.Fire, 65, 75);
            SetResistance(ResistanceType.Cold, 65, 75);
            SetResistance(ResistanceType.Poison, 65, 75);
            SetResistance(ResistanceType.Energy, 65, 75);

            SetSkill(SkillName.Tactics, 240.1, 250.0);
            SetSkill(SkillName.MagicResist, 250.1, 260.0);
            SetSkill(SkillName.Anatomy, 240.1, 250.0);
            SetSkill(SkillName.Magery, 245.1, 250.0);
            SetSkill(SkillName.EvalInt, 245.1, 250.0);
            SetSkill(SkillName.Swords, 242.5, 245.0);
            SetSkill(SkillName.Fencing, 245.1, 250);
            SetSkill(SkillName.Macing, 245.1, 250);

            Fame = 23000;
            Karma = -23000;
            VirtualArmor = 105;

            CraftResource res = CraftResource.None;

            switch (Utility.Random(6))
            {
                case 0:
                    res = CraftResource.BlackScales;
                    break;
                case 1:
                    res = CraftResource.RedScales;
                    break;
                case 2:
                    res = CraftResource.BlueScales;
                    break;
                case 3:
                    res = CraftResource.YellowScales;
                    break;
                case 4:
                    res = CraftResource.GreenScales;
                    break;
                case 5:
                    res = CraftResource.WhiteScales;
                    break;
            }

            BaseWeapon melee = null;

            switch (Utility.Random(3))
            {
                case 0:
                    melee = new Kryss();
                    break;
                case 1:
                    melee = new Broadsword();
                    break;
                case 2:
                    melee = new Katana();
                    break;
            }

            melee.Movable = false;
            AddItem(melee);

            DragonChest Tunic = new DragonChest();
            Tunic.Resource = res;
            Tunic.Movable = false;
            AddItem(Tunic);

            DragonLegs Legs = new DragonLegs();
            Legs.Resource = res;
            Legs.Movable = false;
            AddItem(Legs);

            DragonArms Arms = new DragonArms();
            Arms.Resource = res;
            Arms.Movable = false;
            AddItem(Arms);

            DragonGloves Gloves = new DragonGloves();
            Gloves.Resource = res;
            Gloves.Movable = false;
            AddItem(Gloves);

            DragonHelm Helm = new DragonHelm();
            Helm.Resource = res;
            Helm.Movable = false;
            AddItem(Helm);

            ChaosShield shield = new ChaosShield();
            shield.Movable = false;
            AddItem(shield);

            AddItem(new Boots(0x455));
            AddItem(new Shirt(Utility.RandomMetalHue()));

            int amount = Utility.RandomMinMax(1, 3);

            switch (res)
            {
                case CraftResource.BlackScales:
                    AddItem(new BlackScales(amount));
                    break;
                case CraftResource.RedScales:
                    AddItem(new RedScales(amount));
                    break;
                case CraftResource.BlueScales:
                    AddItem(new BlueScales(amount));
                    break;
                case CraftResource.YellowScales:
                    AddItem(new YellowScales(amount));
                    break;
                case CraftResource.GreenScales:
                    AddItem(new GreenScales(amount));
                    break;
                case CraftResource.WhiteScales:
                    AddItem(new WhiteScales(amount));
                    break;
            }
			res = CraftResource.Gold;
			
			/*
            switch (Utility.Random(9))
            {
                case 0:
                    res = CraftResource.DullCopper;
                    break;
                case 1:
                    res = CraftResource.ShadowIron;
                    break;
                case 2:
                    res = CraftResource.Copper;
                    break;
                case 3:
                    res = CraftResource.Bronze;
                    break;
                case 4:
                    res = CraftResource.Gold;
                    break;
                case 5:
                    res = CraftResource.Agapite;
                    break;
                case 6:
                    res = CraftResource.Verite;
                    break;
                case 7:
                    res = CraftResource.Valorite;
                    break;
                case 8:
                    res = CraftResource.Iron;
                    break;
            }
			*/
            SwampDragon mt = new SwampDragon();
            mt.HasBarding = true;
            mt.BardingResource = res;
            mt.BardingHP = mt.BardingMaxHP;
            mt.Rider = this;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public ChaosDragoonElite(Serial serial)
            : base(serial)
        {
        }

        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
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
        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }
        public override int GetIdleSound()
        {
            return 0x2CE;
        }

        public override int GetDeathSound()
        {
            return 0x2CC;
        }

        public override int GetHurtSound()
        {
            return 0x2D1;
        }

        public override int GetAttackSound()
        {
            return 0x2C8;
        }

        public override void GenerateLoot()
        {

        }

        public override bool OnBeforeDeath()
        {
            IMount mount = Mount;

            if (mount != null)
            {
                if (mount is SwampDragon)
                    ((SwampDragon)mount).HasBarding = false;

                mount.Rider = null;
            }

            return base.OnBeforeDeath();
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            if (to is Dragon || to is WhiteWyrm || to is SwampDragon || to is Drake || to is Nightmare || to is Hiryu || to is LesserHiryu || to is Daemon)
                damage *= 3;
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
