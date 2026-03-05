using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a shadow dragon corpse")]
    public class SwampDragon : BaseMount
    {
        private bool m_BardingExceptional;
        private Mobile m_BardingCrafter;
        private int m_BardingHP;
        private bool m_HasBarding;
        private CraftResource m_BardingResource;

        [Constructable]
        public SwampDragon()
            : this("a shadow dragon")
        {
        }

        [Constructable]
        public SwampDragon(string name)
            : base(name, 0x31F, 0x3EBE, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0x16A;

			this.Fame = 2000;
			this.Karma = -2000;

			// [역산] 보너스: Str+656, Hits+3,898, Skill+5.4
			this.SetStr(144, 194);  // 최종 Str 800~850
			this.SetDex(66, 116);   
			this.SetHits(1102, 1602); // 최종 Hits 5,000~5,500
			this.SetStam(66, 116);

			SetAttackSpeed(3.0);
			SetDamage(15, 25);

			// 공격 속성: 묵직한 타격과 부식성 독
			this.SetDamageType(ResistanceType.Physical, 75);
			this.SetDamageType(ResistanceType.Poison, 25);

			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Poison, 40, 50);

			// 최종 Skill 105.0 내외 (숙련된 야수)
			this.SetSkill(SkillName.Wrestling, 99.6, 109.6);

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 93.1;

            Hue = 0x966;
        }

        public SwampDragon(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile BardingCrafter
        {
            get
            {
                return m_BardingCrafter;
            }
            set
            {
                m_BardingCrafter = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public bool BardingExceptional
        {
            get
            {
                return m_BardingExceptional;
            }
            set
            {
                m_BardingExceptional = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int BardingHP
        {
            get
            {
                return m_BardingHP;
            }
            set
            {
                m_BardingHP = value;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasBarding
        {
            get
            {
                return m_HasBarding;
            }
            set
            {
                m_HasBarding = value;

                if (m_HasBarding)
                {
                    Hue = CraftResources.GetHue(m_BardingResource);
                    BodyValue = 0x31F;
                    ItemID = 0x3EBE;
                }
                else
                {
                    Hue = 0x966;
                    BodyValue = 0x31F;
                    ItemID = 0x3EBE;
                }

                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource BardingResource
        {
            get
            {
                return m_BardingResource;
            }
            set
            {
                m_BardingResource = value;

                if (m_HasBarding)
                    Hue = CraftResources.GetHue(value);

                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int BardingMaxHP
        {
            get
            {
                switch (m_BardingResource)
                {
                    default:
                        return BardingExceptional ? 12000 : 10000;
                    case CraftResource.DullCopper:
                    case CraftResource.Valorite:
                        return BardingExceptional ? 14500 : 12500;
                    case CraftResource.ShadowIron:
                        return BardingExceptional ? 17000 : 15000;
                }
            }
        }

        private int CalculateBardingResistance(ResistanceType type)
        {
            if (m_BardingResource == CraftResource.None || !m_HasBarding)
                return 0;

            CraftResourceInfo resInfo = CraftResources.GetInfo(m_BardingResource);

            if (resInfo == null)
                return 0;

            CraftAttributeInfo attrs = resInfo.AttributeInfo;

            if (attrs == null)
                return 0;

            int expBonus = BardingExceptional ? 1 : 0;
            int resBonus = 0;
            
            switch (type)
            {
                default:
                case ResistanceType.Physical: resBonus = Math.Max(5, attrs.ArmorPhysicalResist); break;
                case ResistanceType.Fire: resBonus = Math.Max(3, attrs.ArmorFireResist); break;
                case ResistanceType.Cold: resBonus = Math.Max(2, attrs.ArmorColdResist); break;
                case ResistanceType.Poison: resBonus = Math.Max(3, attrs.ArmorPoisonResist); break;
                case ResistanceType.Energy: resBonus = Math.Max(2, attrs.ArmorEnergyResist); break;
            }

            return (resBonus + expBonus) * 5;
        }

        public override int GetResistance(ResistanceType type)
        {
            return base.GetResistance(type) + CalculateBardingResistance(type);
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return !Controlled;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int Hides
        {
            get
            {
                return 20;
            }
        }
        public override int Scales
        {
            get
            {
                return 5;
            }
        }
        public override ScaleType ScaleType
        {
            get
            {
                return ScaleType.Green;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }
        public override bool OverrideBondingReqs()
        {
            return true;
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

        public override double GetControlChance(Mobile m, bool useBaseSkill)
        {
            if (PetTrainingHelper.Enabled)
            {
                var profile = PetTrainingHelper.GetAbilityProfile(this);

                if (profile != null && profile.HasCustomized())
                {
                    return base.GetControlChance(m, useBaseSkill);
                }
            }

            return 1.0;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_HasBarding && m_BardingExceptional && m_BardingCrafter != null)
            {
                list.Add(1060853, m_BardingCrafter.Name); // armor exceptionally crafted by ~1_val~
            }

            if (m_HasBarding)
            {
                list.Add(1115719, m_BardingHP.ToString()); // armor points: ~1_val~
            }
        }

        public override void OnRiderDamaged(Mobile from, ref int amount, bool willKill)
        {
            base.OnRiderDamaged(from, ref amount, willKill);

            if (Rider == null)
                return;

            if ((from == null || !from.Player) && Rider.Player && Rider.Mount == this)
            {
                if (HasBarding)
                {
                    int percent = (BardingExceptional ? 20 : 10);
                    int absorbed = AOS.Scale(amount, percent);

                    amount -= absorbed;

                    // Mondain's Legacy mod
                    if (!(this is ParoxysmusSwampDragon))
                        BardingHP -= absorbed;

                    if (BardingHP < 0)
                    {
                        HasBarding = false;
                        BardingHP = 0;

                        Rider.SendLocalizedMessage(1053031); // Your dragon's barding has been destroyed!
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((bool)m_BardingExceptional);
            writer.Write((Mobile)m_BardingCrafter);
            writer.Write((bool)m_HasBarding);
            writer.Write((int)m_BardingHP);
            writer.Write((int)m_BardingResource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_BardingExceptional = reader.ReadBool();
                        m_BardingCrafter = reader.ReadMobile();
                        m_HasBarding = reader.ReadBool();
                        m_BardingHP = reader.ReadInt();
                        m_BardingResource = (CraftResource)reader.ReadInt();
                        break;
                    }
            }

            if (Hue == 0 && !m_HasBarding)
                Hue = 0x851;

            if (BaseSoundID == -1)
                BaseSoundID = 0x16A;
        }
    }
}
