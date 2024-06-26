using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a chaos dragoon corpse")]
    public class ChaosDragoon : BaseCreature
    {
        [Constructable]
        public ChaosDragoon()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.15, 0.4)
        {
            Name = "a chaos dragoon";
            Body = 0x190;
            Hue = Utility.RandomSkinHue();

            SetStr(176, 225);
            SetDex(81, 95);
            SetInt(61, 85);

            SetHits(9760, 10225);

            SetDamage(24, 28);

			SetAttackSpeed( 2.5 );
			
            SetDamageType(ResistanceType.Physical, 25);
            SetDamageType(ResistanceType.Fire, 25);
            SetDamageType(ResistanceType.Cold, 25);
            SetDamageType(ResistanceType.Energy, 25);

			SetResistance(ResistanceType.Physical, 80, 89);
			SetResistance(ResistanceType.Fire, 50, 59);
			SetResistance(ResistanceType.Cold, 50, 59);
			SetResistance(ResistanceType.Poison, 50, 59);
			SetResistance(ResistanceType.Energy, 50, 59);
			
            SetSkill(SkillName.Fencing, 177.6, 192.5);
            SetSkill(SkillName.Healing, 160.3, 190.0);
            SetSkill(SkillName.Macing, 177.6, 192.5);
            SetSkill(SkillName.Anatomy, 177.6, 187.5);
            SetSkill(SkillName.MagicResist, 177.6, 197.5);
            SetSkill(SkillName.Swords, 177.6, 192.5);
            SetSkill(SkillName.Tactics, 177.6, 187.5);

            Fame = 19000;
            Karma = -19000;
			
            VirtualArmor = 155;

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

            DragonHelm helm = new DragonHelm();
            helm.Resource = res;
            helm.Movable = false;
            AddItem(helm);

            DragonChest chest = new DragonChest();
            chest.Resource = res;
            chest.Movable = false;
            AddItem(chest);

            DragonArms arms = new DragonArms();
            arms.Resource = res;
            arms.Movable = false;
            AddItem(arms);

            DragonGloves gloves = new DragonGloves();
            gloves.Resource = res;
            gloves.Movable = false;
            AddItem(gloves);

            DragonLegs legs = new DragonLegs();
            legs.Resource = res;
            legs.Movable = false;
            AddItem(legs);

            ChaosShield shield = new ChaosShield();
            shield.Movable = false;
            AddItem(shield);

            AddItem(new Shirt());
            AddItem(new Boots());

            int amount = Utility.RandomMinMax(1, 3);

            switch ( res )
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

            new SwampDragon().Rider = this;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public ChaosDragoon(Serial serial)
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
            AddLoot(LootPack.Rich);
        }

        public override bool OnBeforeDeath()
        {
            IMount mount = Mount;

            if (mount != null)
                mount.Rider = null;

            return base.OnBeforeDeath();
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            if (to is Dragon || to is WhiteWyrm || to is SwampDragon || to is Drake || to is Nightmare || to is Hiryu || to is LesserHiryu || to is Daemon)
                damage *= 2;
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
