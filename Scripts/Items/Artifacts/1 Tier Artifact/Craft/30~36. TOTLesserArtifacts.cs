using System;

namespace Server.Items
{
    public enum LesserPigmentType
    {
        None,
        PaleOrange,
        FreshRose,
        ChaosBlue,
        Silver,
        NobleGold,
        LightGreen,
        PaleBlue,
        FreshPlum,
        DeepBrown,
        BurntBrown
    }

    public class AncientFarmersKasa : StrawHat
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public AncientFarmersKasa()
            : base()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 72;
        }

        public AncientFarmersKasa(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070922;
            }
        }// Ancient Farmer's Kasa

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

    public class AncientSamuraiDo : PlateChest
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public AncientSamuraiDo()
            : base()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 65;
        }

        public AncientSamuraiDo(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070926;
            }
        }// Ancient Samurai Do

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

    public class ArmsOfTacticalExcellence : RingmailArms
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public ArmsOfTacticalExcellence()
            : base()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 77;
        }

        public ArmsOfTacticalExcellence(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070921;
            }
        }// Arms of Tactical Excellence

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

    public class BlackLotusHood : SkullCap
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public BlackLotusHood()
            : base()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 96;
        }

        public BlackLotusHood(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070919;
            }
        }// Black Lotus Hood

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

    public class DaimyosHelm : Helmet
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public DaimyosHelm()
            : base()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 40;
        }

        public DaimyosHelm(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070920;
            }
        }// Daimyo's Helm

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

    public class DemonForks : Kryss //30번 악마사냥 크리스
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public DemonForks()
            : base()
        {
			//데몬슬 300%, 공속 20%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 61; //옵션 종류
			SuffixOption[11] = 100000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 2000; //옵션 값
        }

        public DemonForks(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070917;
            }
        }// Demon Forks

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

    public class DragonNunchaku : Club //31. 용 조각 몽둥이
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public DragonNunchaku()
            : base()
        {
			//물치확 8%
			SuffixOption[0] = 1; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 42; //옵션 종류
			SuffixOption[11] = 800; //옵션 값
        }

        public DragonNunchaku(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070914;
            }
        }// Dragon Nunchaku

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


    public class PeasantsBokuto : BlackStaff //농민의 검은 지팡이
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public PeasantsBokuto()
            : base()
        {
			//곤충 피해 200%, 무기 피해 60%, 공속 20%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 59; //옵션 종류
			SuffixOption[11] = 20000; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 6000; //옵션 값
			PrefixOption[13] = 40; //옵션 종류
			SuffixOption[13] = 2000; //옵션 값
        }

        public PeasantsBokuto(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070912;
            }
        }// Peasant's Bokuto

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

    public class PilferedDancerFans : ExecutionersAxe //32. 도난당한 사형집행인의 도끼
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public PilferedDancerFans()
            : base()
        {
			//물리치명피해 10%, 공속 20%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 44; //옵션 종류
			SuffixOption[11] = 1000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 2000; //옵션 값
        }

        public PilferedDancerFans(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070916;
            }
        }// Pilfered Dancer Fans

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


    public class TomeOfEnlightenment : Magerybook //33. 깨우침의 고서
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public TomeOfEnlightenment()
            : base()
        {
            LootType = LootType.Regular;

			//지능 1, 마나 200
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 2; //옵션 종류
			SuffixOption[11] = 100; //옵션 값
			PrefixOption[12] = 6; //옵션 종류
			SuffixOption[12] = 20000; //옵션 값
        }

        public TomeOfEnlightenment(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070934;
            }
        }// Tome of Enlightenment
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

    public class LeurociansMempoOfFortune : LeatherMempo
    {
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public LeurociansMempoOfFortune()
            : base()
        {
            LootType = LootType.Regular;
            Hue = 0x501;

            Attributes.Luck = 300;
            Attributes.RegenMana = 1;
        }

        public LeurociansMempoOfFortune(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1071460;
            }
        }// Leurocian's mempo of fortune
        public override int BasePhysicalResistance
        {
            get
            {
                return 15;
            }
        }
        public override int BaseFireResistance
        {
            get
            {
                return 10;
            }
        }
        public override int BaseColdResistance
        {
            get
            {
                return 10;
            }
        }
        public override int BasePoisonResistance
        {
            get
            {
                return 10;
            }
        }
        public override int BaseEnergyResistance
        {
            get
            {
                return 15;
            }
        }
        public override int InitMinHits
        {
            get
            {
                return 255;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 255;
            }
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
	

    public class TheDestroyer : Halberd  //34. 파괴의 할버드
    {
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public TheDestroyer()
            : base()
        {
			//무피 300%, 체력 -300
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 7; //옵션 종류
			SuffixOption[11] = 30000; //옵션 값
			PrefixOption[12] = 4; //옵션 종류
			SuffixOption[12] = -30000; //옵션 값
        }

        public TheDestroyer(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070915;
            }
        }// The Destroyer

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
	public class GlovesOfTheSun : LeatherGloves
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public GlovesOfTheSun()
            : base()
        {
			PrefixOption[80] = 1;
			PrefixOption[81] = 13;
        }

        public GlovesOfTheSun(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070924;
            }
        }// Gloves of the Sun

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

    public class LegsOfStability : PlateLegs
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public LegsOfStability()
            : base()
        {
            Attributes.BonusStam = 5;

            ArmorAttributes.SelfRepair = 3;
            ArmorAttributes.LowerStatReq = 100;
            ArmorAttributes.MageArmor = 1;
        }

        public LegsOfStability(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070925;
            }
        }// Legs of Stability

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

    public class HanzosBow : Yumi //35. 한조의 유미
    {
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public HanzosBow()
            : base()
        {
			//파충류슬 300%, 공속 20%
			SuffixOption[0] = 2; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 60; //옵션 종류
			SuffixOption[11] = 100000; //옵션 값
			PrefixOption[12] = 40; //옵션 종류
			SuffixOption[12] = 2000; //옵션 값
        }

        public HanzosBow(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070918;
            }
        }// Hanzo's Bow

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
    //Non weapon/armor ones:
    public class AncientUrn : Item
    {
		public override bool IsArtifact { get { return true; } }
        private static readonly string[] m_Names = new string[]
        {
            "Akira",
            "Avaniaga",
            "Aya",
            "Chie",
            "Emiko",
            "Fumiyo",
            "Gennai",
            "Gennosuke",
            "Genjo",
            "Hamato",
            "Harumi",
            "Ikuyo",
            "Juri",
            "Kaori",
            "Kaoru",
            "Kiyomori",
            "Mayako",
            "Motoki",
            "Musashi",
            "Nami",
            "Nobukazu",
            "Roku",
            "Romi",
            "Ryo",
            "Sanzo",
            "Sakamae",
            "Satoshi",
            "Takamori",
            "Takuro",
            "Teruyo",
            "Toshiro",
            "Yago",
            "Yeijiro",
            "Yoshi",
            "Zeshin"
        };
        private string m_UrnName;
        [Constructable]
        public AncientUrn(string urnName)
            : base(0x241D)
        {
            m_UrnName = urnName;
            Weight = 1.0;
        }

        [Constructable]
        public AncientUrn()
            : this(m_Names[Utility.Random(m_Names.Length)])
        {
        }

        public AncientUrn(Serial serial)
            : base(serial)
        {
        }

        public static string[] Names
        {
            get
            {
                return m_Names;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public string UrnName
        {
            get
            {
                return m_UrnName;
            }
            set
            {
                m_UrnName = value;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1071014;
            }
        }// Ancient Urn
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
            writer.Write(m_UrnName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            m_UrnName = reader.ReadString();

            Utility.Intern(ref m_UrnName);
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1070935, m_UrnName); // Ancient Urn of ~1_name~
        }

        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, 1070935, m_UrnName); // Ancient Urn of ~1_name~
        }
    }

    public class HonorableSwords : Item
    {
		public override bool IsArtifact { get { return true; } }
        private string m_SwordsName;
        [Constructable]
        public HonorableSwords(string swordsName)
            : base(0x2853)
        {
            m_SwordsName = swordsName;

            Weight = 5.0;
        }

        [Constructable]
        public HonorableSwords()
            : this(AncientUrn.Names[Utility.Random(AncientUrn.Names.Length)])
        {
        }

        public HonorableSwords(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string SwordsName
        {
            get
            {
                return m_SwordsName;
            }
            set
            {
                m_SwordsName = value;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1071015;
            }
        }// Honorable Swords
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
            writer.Write(m_SwordsName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            m_SwordsName = reader.ReadString();

            Utility.Intern(ref m_SwordsName);
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1070936, m_SwordsName); // Honorable Swords of ~1_name~
        }

        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, 1070936, m_SwordsName); // Honorable Swords of ~1_name~
        }
    }

    [Furniture]
    [Flipable(0x2811, 0x2812)]
    public class ChestOfHeirlooms : LockableContainer
    {
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public ChestOfHeirlooms()
            : base(0x2811)
        {
            Locked = true;
            LockLevel = 95;
            MaxLockLevel = 140;
            RequiredSkill = 95;
			
            TrapType = TrapType.ExplosionTrap;
            TrapLevel = 10;
            TrapPower = 100;
			
            GumpID = 0x10B;
			
            for (int i = 0; i < 10; ++i)
            {
                Item item = Loot.ChestOfHeirloomsContains();
				
                int attributeCount = Utility.RandomMinMax(1, 5);
                int min = 20;
                int max = 80;
				
                if (item is BaseWeapon)
                {
                    BaseWeapon weapon = (BaseWeapon)item;

                    if (Core.AOS)
                        BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);
                    else
                    {
                        weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(6);
                        weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(6);
                        weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random(6);
                    }
                }
                else if (item is BaseArmor)
                {
                    BaseArmor armor = (BaseArmor)item;

                    if (Core.AOS)
                        BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);
                    else
                    {
                        armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(6);
                        armor.Durability = (ArmorDurabilityLevel)Utility.Random(6);
                    }
                }
                else if (item is BaseHat && Core.AOS)
                    BaseRunicTool.ApplyAttributesTo((BaseHat)item, attributeCount, min, max);
                else if (item is BaseJewel && Core.AOS)
                    BaseRunicTool.ApplyAttributesTo((BaseJewel)item, attributeCount, min, max);
				
                DropItem(item);
            }
        }

        public ChestOfHeirlooms(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070937;
            }
        }// Chest of heirlooms
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

    public class FluteOfRenewal : BambooFlute
    {
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public FluteOfRenewal()
            : base()
        {
            Slayer = SlayerGroup.RandomSuperSlayerAOS();

            ReplenishesCharges = true;
        }

        public FluteOfRenewal(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070927;
            }
        }// Flute of Renewal
        public override int InitMinUses
        {
            get
            {
                return 300;
            }
        }
        public override int InitMaxUses
        {
            get
            {
                return 300;
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

            if (version == 0 && Slayer == SlayerName.Fey)
                Slayer = SlayerGroup.Groups[Utility.Random(SlayerGroup.Groups.Length - 1)].Super.Name;
        }
    }
    public class Exiler : ElvenCompositeLongbow //37. 은색의 엘프 합성 장궁
    {
		public override bool IsArtifact { get { return true; } }
        public override int ArtifactRarity
        {
            get
            {
                return 1;
            }
        }
        [Constructable]
        public Exiler()
            : base()
        {
			//악마슬 200%, 무피 60%, 공속 20%
			SuffixOption[0] = 3; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 61; //옵션 종류
			SuffixOption[11] = 20000; //옵션 값
			PrefixOption[12] = 7; //옵션 종류
			SuffixOption[12] = 6000; //옵션 값
			PrefixOption[13] = 40; //옵션 종류
			SuffixOption[13] = 2000; //옵션 값
        }

        public Exiler(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                return 1070913;
            }
        }// Exiler

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
    public class LesserPigmentsOfTokuno : BasePigmentsOfTokuno
    {
		public override bool IsArtifact { get { return true; } }
        private static readonly int[][] m_Table = new int[][]
        {
            // Hue, Label
            new int[] { /*PigmentType.None,*/ 0, -1 },
            new int[] { /*PigmentType.PaleOrange,*/ 0x02E, 1071458 },
            new int[] { /*PigmentType.FreshRose,*/ 0x4B9, 1071455 },
            new int[] { /*PigmentType.ChaosBlue,*/ 0x005, 1071459 },
            new int[] { /*PigmentType.Silver,*/ 0x3E9, 1071451 },
            new int[] { /*PigmentType.NobleGold,*/ 0x227, 1071457 },
            new int[] { /*PigmentType.LightGreen,*/ 0x1C8, 1071454 },
            new int[] { /*PigmentType.PaleBlue,*/ 0x24F, 1071456 },
            new int[] { /*PigmentType.FreshPlum,*/ 0x145, 1071450 },
            new int[] { /*PigmentType.DeepBrown,*/ 0x3F0, 1071452 },
            new int[] { /*PigmentType.BurntBrown,*/ 0x41A, 1071453 }
        };
        private LesserPigmentType m_Type;
        [Constructable]
        public LesserPigmentsOfTokuno()
            : this((LesserPigmentType)Utility.Random(0, 11))
        {
        }

        [Constructable]
        public LesserPigmentsOfTokuno(LesserPigmentType type)
            : base(1)
        {
            Weight = 1.0;
            Type = type;
        }

        public LesserPigmentsOfTokuno(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public LesserPigmentType Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
				
                int v = (int)m_Type;

                if (v >= 0 && v < m_Table.Length)
                {
                    Hue = m_Table[v][0];
                    Label = m_Table[v][1];
                }
                else
                {
                    Hue = 0;
                    Label = -1;
                }
            }
        }
        public static int[] GetInfo(LesserPigmentType type)
        {
            int v = (int)type;

            if (v < 0 || v >= m_Table.Length)
                v = 0;
			
            return m_Table[v];
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);

            writer.WriteEncodedInt((int)m_Type);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = (InheritsItem ? 0 : reader.ReadInt()); // Required for BasePigmentsOfTokuno insertion
			
            switch ( version )
            {
                case 1:
                    Type = (LesserPigmentType)reader.ReadEncodedInt();
                    break;
                case 0:
                    break;
            }
        }
    }

    public class MetalPigmentsOfTokuno : BasePigmentsOfTokuno
    {
		public override bool IsArtifact { get { return true; } }
        [Constructable]
        public MetalPigmentsOfTokuno()
            : base(1)
        {
            RandomHue();
            Label = -1;
        }

        public MetalPigmentsOfTokuno(Serial serial)
            : base(serial)
        {
        }

        public void RandomHue()
        {
            int a = Utility.Random(0, 30);
            if (a != 0)
                Hue = a + 0x960;
            else
                Hue = 0;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = (InheritsItem ? 0 : reader.ReadInt()); // Required for BasePigmentsOfTokuno insertion
        }
    }
}