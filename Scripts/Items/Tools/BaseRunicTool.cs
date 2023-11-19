using System;
using System.Collections;

namespace Server.Items
{
    public abstract class BaseRunicTool : BaseTool
    {
        private static readonly SkillName[] m_PossibleBonusSkills = new SkillName[]
        {
            SkillName.Swords,
            SkillName.Fencing,
            SkillName.Macing,
            SkillName.Archery,
            SkillName.Wrestling,
            SkillName.Parry,
            SkillName.Tactics,
            //SkillName.Anatomy,
            SkillName.Healing,
            SkillName.Magery,
            SkillName.Meditation,
            SkillName.EvalInt,
            SkillName.MagicResist,
            SkillName.AnimalTaming,
            SkillName.AnimalLore,
            SkillName.Veterinary,
            SkillName.Musicianship,
            SkillName.Provocation,
            SkillName.Discordance,
            SkillName.Peacemaking,
            //SkillName.Chivalry,
            SkillName.Focus
            //SkillName.Necromancy,
            //SkillName.Stealing,
            //SkillName.Stealth,
            //SkillName.SpiritSpeak,
            //SkillName.Bushido,
            //SkillName.Ninjitsu
        };

        private static readonly SkillName[] m_PossibleSpellbookSkills = new SkillName[]
        {
            SkillName.Magery,
            SkillName.Meditation,
            SkillName.EvalInt,
            SkillName.MagicResist
        };

        private static readonly BitArray m_Props = new BitArray(MaxProperties);
        private static readonly int[] m_Possible = new int[MaxProperties];

        private static bool m_PlayerMade;
        private static int m_LuckChance;

        private const int MaxProperties = 32;

        public BaseRunicTool(CraftResource resource, int itemID)
            : base(itemID)
        {
            Resource = resource;
        }

        public BaseRunicTool(CraftResource resource, int uses, int itemID)
            : base(uses, itemID)
        {
            Resource = resource;
        }

        public BaseRunicTool(Serial serial)
            : base(serial)
        {
        }

        #region Runic Reforging
        public override void OnDoubleClick(Mobile from)
        {

            bool hasSkill = from.Skills[SkillName.Imbuing].Value >= 65;

            IPooledEnumerable eable = from.Map.GetItemsInRange(from.Location, 2);

            foreach (Item item in eable)
            {
                if ((item.ItemID >= 0x4263 && item.ItemID <= 0x4272) || (item.ItemID >= 0x4277 && item.ItemID <= 0x4286) || (item.ItemID >= 17607 && item.ItemID <= 17610))
                {
                    if (!hasSkill)
                    {
                        from.SendLocalizedMessage(1152333); // You do not have enough Imbuing skill to re-forge items. Using standard Runic Crafting instead.
                        break;
                    }

                    from.Target = new RunicReforgingTarget(this);
                    from.SendLocalizedMessage(1152112); // Target the item to reforge.
                    eable.Free();
                    return;
                }
            }

            if (hasSkill)
                from.SendLocalizedMessage(1152334); // You must be near a Soul Forge to re-forge items. Using standard Runic Crafting instead.

            eable.Free();
            base.OnDoubleClick(from);
        }
        #endregion

        public static int GetUniqueRandom(int count)
        {
            int avail = 0;

            for (int i = 0; i < count; ++i)
            {
                if (!m_Props[i])
                    m_Possible[avail++] = i;
            }

            if (avail == 0)
                return -1;

            int v = m_Possible[Utility.Random(avail)];

            m_Props.Set(v, true);

            return v;
        }

        public static void ApplyAttributesTo(Item item, int attributeCount, int min, int max)
        {
            if (item is FishingPole)
            {
                ApplyAttributesTo((FishingPole)item, attributeCount, min, max);
            }
            else if (item is BaseWeapon)
            {
                ApplyAttributesTo((BaseWeapon)item, attributeCount, min, max);
            }
            else if (item is BaseArmor)
            {
                ApplyAttributesTo((BaseArmor)item, attributeCount, min, max);
            }
            else if (item is BaseClothing)
            {
                ApplyAttributesTo((BaseClothing)item, attributeCount, min, max);
            }
            else if (item is BaseJewel)
            {
                ApplyAttributesTo((BaseJewel)item, attributeCount, min, max);
            }
            else if (item is Spellbook)
            {
                ApplyAttributesTo((Spellbook)item, attributeCount, min, max);
            }
        }

		static Item m_Item;
        public static void ApplyAttributesTo(
            Item item,
            bool isRunicTool,
            int luckChance,
            int attributeCount,
            int min,
            int max)
        {
			m_Item = item;
            if (item is FishingPole)
            {
                ApplyAttributesTo((FishingPole)item, isRunicTool, luckChance, attributeCount, min, max);
            }
            else if (item is BaseWeapon)
            {
                ApplyAttributesTo((BaseWeapon)item, isRunicTool, luckChance, attributeCount, min, max);
            }
            else if (item is BaseArmor)
            {
                ApplyAttributesTo((BaseArmor)item, isRunicTool, luckChance, attributeCount, min, max);
            }
            else if (item is BaseClothing)
            {
                ApplyAttributesTo((BaseClothing)item, isRunicTool, luckChance, attributeCount, min, max);
            }
            else if (item is BaseJewel)
            {
                ApplyAttributesTo((BaseJewel)item, isRunicTool, luckChance, attributeCount, min, max);
            }
            else if (item is Spellbook)
            {
                ApplyAttributesTo((Spellbook)item, isRunicTool, luckChance, attributeCount, min, max);
            }
        }

        #region High Seas
        public void ApplyAttributesTo(FishingPole pole)
        {
            CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);

            if (resInfo == null)
                return;

            CraftAttributeInfo attrs = resInfo.AttributeInfo;

            int attributeCount = Utility.RandomMinMax(attrs.RunicMinAttributes, attrs.RunicMaxAttributes);
            int min = attrs.RunicMinIntensity;
            int max = attrs.RunicMaxIntensity;

            ApplyAttributesTo(pole, true, 0, attributeCount, min, max);
        }

        public static void ApplyAttributesTo(FishingPole pole, bool playerMade, int luckChance, int attributeCount, int min, int max)
        {
			return;
            int delta;

            if (min > max)
            {
                delta = min;
                min = max;
                max = delta;
            }

            m_PlayerMade = playerMade;
            m_LuckChance = luckChance;

            AosAttributes primary = pole.Attributes;
            AosSkillBonuses skills = pole.SkillBonuses;

            m_Props.SetAll(false);

            for (int i = 0; i < attributeCount; ++i)
            {
                int random = GetUniqueRandom(21);

                switch (random)
                {
                    case 0: ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 15); break;
                    case 1: ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 1); break;
                    case 2: ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 1); break;
                    case 3: ApplyAttribute(primary, min, max, AosAttribute.AttackChance, 1, 15); break;
                    case 4: ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 100); break;
                    case 5: ApplyAttribute(primary, min, max, AosAttribute.SpellChanneling, 1, 1); break;
                    case 6: ApplyAttribute(primary, min, max, AosAttribute.RegenHits, 1, 2); break;
                    case 7: ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 2); break;
                    case 8: ApplyAttribute(primary, min, max, AosAttribute.RegenStam, 1, 3); break;
                    case 9: ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 8); break;
                    case 10: ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 8); break;
                    case 11: ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 8); break;
                    case 12: ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 8); break;
                    case 13: ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 8); break;
                    case 14: ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 8); break;
                    case 15: ApplyAttribute(primary, min, max, AosAttribute.SpellDamage, 1, 12); break;
                    case 16: ApplySkillBonus(skills, min, max, 0, 1, 15); break;
                    case 17: ApplySkillBonus(skills, min, max, 1, 1, 15); break;
                    case 18: ApplySkillBonus(skills, min, max, 2, 1, 15); break;
                    case 19: ApplySkillBonus(skills, min, max, 3, 1, 15); break;
                    case 20: ApplySkillBonus(skills, min, max, 4, 1, 15); break;
                }
            }
        }
        #endregion

        public static void ApplyAttributesTo(BaseWeapon weapon, int attributeCount, int min, int max)
        {
            ApplyAttributesTo(weapon, false, 0, attributeCount, min, max);
        }

        public static void ApplyAttributesTo(BaseWeapon weapon, bool playerMade, int luckChance, int attributeCount, int min, int max, int firstpick = -1)
        {
            int delta;

            if (min > max)
            {
                delta = min;
                min = max;
                max = delta;
            }

            if (!playerMade && RandomItemGenerator.Enabled)
            {
                RandomItemGenerator.GenerateRandomItem(weapon, luckChance, attributeCount, min, max);
                return;
            }

            m_PlayerMade = playerMade;
            m_LuckChance = luckChance;

            AosAttributes primary = weapon.Attributes;
            AosWeaponAttributes secondary = weapon.WeaponAttributes;

            m_Props.SetAll(false);

			int dice = Utility.RandomMinMax(1, 12);
			
            for (int i = 0; i < attributeCount; ++i)
            {
				
                int random = GetUniqueRandom(20);

                if (random == -1)
                    break;
				
				if( firstpick != -1 )
				{
					random = firstpick;
					m_Props.Set( firstpick, true );
					firstpick = -1;
					switch ( random )
					{
						case 0: 
							ApplyAttribute(primary, min, max, AosAttribute.ReflectPhysical, 1, 30);
							break;
						case 1:
							ApplyElementalDamage(weapon, min, max);
							break;
						case 2: 
							ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 30);
							break;
						case 3:
							ApplyAttribute(primary, min, max, AosAttribute.WeaponSpeed, 1, 30);
							break;
						case 4:
							ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistPhysicalBonus, 1, 20);
							break;
						case 5:
							ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistFireBonus, 1, 20);
							break;
						case 6:
							ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistColdBonus, 1, 20);
							break;
						case 7:
							ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistPoisonBonus, 1, 20);
							break;
						case 8:
							ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistEnergyBonus, 1, 20);
							break;
						case 9:
							ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 60);
							break;
						case 10:
							ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 60);
							break;
						case 11:
							ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 60);
							break;
						case 12:
							ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 60);
							break;
						case 13:
							ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 60);
							break;
						case 14:
							ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 60);
							break;
						case 15:
							ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 60);
							break;
						case 16:
							ApplyAttribute(primary, min, max, AosAttribute.RegenHits, 1, 30);
							break;
						case 17:
							ApplyAttribute(primary, min, max, AosAttribute.RegenStam, 1, 30);
							break;
						case 18:
							ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 30);
							break;
						case 19:
							{
								if( weapon is BaseRanged )
										ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitMagicArrow, 1, 100);
								else if( weapon.Skill is SkillName.Macing )
										ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitHarm, 1, 100);
								else if( weapon.Skill is SkillName.Swords )
										ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitFireball, 1, 100);
								else if( weapon.Skill is SkillName.Fencing )
										ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitLightning, 1, 100);
								break;
							}
						/*

						case 1:
							ApplyAttribute(primary, AosAttribute.AttackChance, 1, 30, percent, 1);
							break;
						case 3:
							ApplyAttribute(secondary, AosWeaponAttribute.LowerStatReq, 5, 50, percent, 5);
							break;
						case 21:
							ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 30);
							break;
						case 22:
							ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 30);
							break;
						*/
					}					
				}

				
				//if ( Utility.Random(100) < 5 )
				//	weapon.Slayer = GetRandomSlayer();
				
				//percent = Dice(min, max);
				//rank += percent;

            }

		}
		private static string GetNameString(BaseWeapon item)
		{
			string name = item.Name;

			if (name == null)
			{
				name = String.Format("#{0}", item.LabelNumber);
			}

			return name;
		}
		private static string GetNameString(BaseArmor item)
		{
			string name = item.Name;

			if (name == null)
			{
				name = String.Format("#{0}", item.LabelNumber);
			}

			return name;
		}
		private static string GetNameString(BaseClothing item)
		{
			string name = item.Name;

			if (name == null)
			{
				name = String.Format("#{0}", item.LabelNumber);
			}

			return name;
		}
		private static string GetNameString(BaseJewel item)
		{
			string name = item.Name;

			if (name == null)
			{
				name = String.Format("#{0}", item.LabelNumber);
			}

			return name;
		}

        public static SlayerName GetRandomSlayer()
        {
            // TODO: Check random algorithm on OSI
            SlayerGroup[] groups = SlayerGroup.Groups;

            if (groups.Length == 0)
                return SlayerName.None;

            SlayerGroup group = groups[Utility.Random(6)]; //-1 To Exclude the Fey Slayer which appears ONLY on a certain artifact.
            SlayerEntry entry;

            if (group.Entries.Length == 0 || 10 > Utility.Random(100)) // 10% chance to do super slayer
            {
                entry = group.Super;
            }
            else
            {
                SlayerEntry[] entries = group.Entries;
                entry = entries[Utility.Random(entries.Length)];
            }

            return entry.Name;
        }

        public static void ApplyAttributesTo(BaseArmor armor, int attributeCount, int min, int max)
        {
            ApplyAttributesTo(armor, false, 0, attributeCount, min, max);
        }

        public static void ApplyAttributesTo(BaseArmor armor, bool playerMade, int luckChance, int attributeCount, int min, int max, int firstpick = -1)
        {
			//bool isShield = (armor is BaseShield);
			//if(isShield)
			//	return;
            int delta;

            if (min > max)
            {
                delta = min;
                min = max;
                max = delta;
            }

            if (!playerMade && RandomItemGenerator.Enabled)
            {
                RandomItemGenerator.GenerateRandomItem(armor, luckChance, attributeCount, min, max);
                return;
            }

            m_PlayerMade = playerMade;
            m_LuckChance = luckChance;

            AosAttributes primary = armor.Attributes;
            AosArmorAttributes secondary = armor.ArmorAttributes;

            m_Props.SetAll(false);
			
			//int rank = 0;
			//int percent = 0;
			
			/*
			아머타입 0 : 방패
			아머타입 1 : 천옷
			아머타입 2 : 가죽
			아머타입 3 : 스텃
			아머타입 4 : 뼈
			아머타입 5 : 링, 투구, 스톤
			아머타입 6 : 체인, 스톤
			아머타입 7 : 플레이트, 산림
			*/
			
			int armortype = 1 + (int)armor.MaterialType;
			if( armor is BaseShield )
				armortype = 0;
			else if( armortype >= 5 && armortype <= 7 )
				armortype = 2;
			else if( armor is Helmet || armor is Bascinet || armor is CloseHelm || armor is NorseHelm )
				armortype = 5;
			else if( armortype == 8 )
				armortype = 5;
			else if( armortype == 9 || armortype == 13 )
				armortype = 6;
			else if( armortype == 10 || armortype == 12 )
				armortype = 7;

			switch ( armortype )
			{
				case 0: //방패
				{
					if( (int)armor.ItemPower > 0 )
					{
						int rank = (int)armor.ItemPower;

						//weapon.Identified = false;
						//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
						armor.PhysicalBonus = 4 + rank * 2;
						armor.BaseArmorRating += 4 + rank * 2;
						armor.MaxHitPoints *= 100 + rank * 20;
						armor.MaxHitPoints /= 100;
						armor.HitPoints = armor.MaxHitPoints;
					}					
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(10);

						if (random == -1)
							break;
						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								ApplyAttribute(primary, min, max, AosAttribute.ReflectPhysical, 1, 15);
								break;
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 5);
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 10);
								break;							
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 10);
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 10);
								break;
							case 5:
								ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 10);
								break;
							case 6:
								ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 10);
								break;
							case 7:
								ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 10);
								break;
							case 8:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 10);
								break;
							case 9:
								ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 10);
								break;	
						}
					}
					break;
				}
				case 1: //천옷, 나뭇잎 갑옷, 가고일 천 갑옷
				{
					if( (int)armor.ItemPower > 0 )
					{
						int rank = (int)armor.ItemPower;

						//weapon.Identified = false;
						//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
						primary.SpellDamage = 2 + rank;
						primary.BonusInt = 4 + rank * 2;
						armor.MaxHitPoints *= 100 + rank * 20;
						armor.MaxHitPoints /= 100;
						armor.HitPoints = armor.MaxHitPoints;
					}					
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(10);

						if (random == -1)
							break;
						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								armor.AbsorptionAttributes.CastingFocus = Utility.RandomMinMax(min, max) / 10;
								break;
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 5);
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 5);
								break;							
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 5);
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 10);
								break;
							case 5:
								ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 10);
								break;
							case 6:
								ApplyResistance(armor, min, max, ResistanceType.Fire, 1, 20);
								break;
							case 7:
								ApplyResistance(armor, min, max, ResistanceType.Cold, 1, 20);
								break;
							case 8:
								ApplyResistance(armor, min, max, ResistanceType.Poison, 1, 20);
								break;
							case 9:
								ApplyResistance(armor, min, max, ResistanceType.Energy, 1, 20);
								break;	
						}
					}
					
					break;
				}
				case 2: //가죽 갑옷
				{
					if( (int)armor.ItemPower > 0 )
					{
						int rank = (int)armor.ItemPower;

						//weapon.Identified = false;
						//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
						primary.BonusMana = 4 + rank * 2;
						primary.RegenMana = 4 + rank * 2;
						armor.MaxHitPoints *= 100 + rank * 20;
						armor.MaxHitPoints /= 100;
						armor.HitPoints = armor.MaxHitPoints;
					}					
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(10);

						if (random == -1)
							break;					
						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								armor.AbsorptionAttributes.CastingFocus = Utility.RandomMinMax(min, max) / 10;
								break;
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 5);
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 5);
								break;							
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 5);
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 10);
								break;
							case 5:
								ApplyAttribute(primary, min, max, AosAttribute.SpellDamage, 1, 10);
								break;
							case 6:
								ApplyResistance(armor, min, max, ResistanceType.Fire, 1, 20);
								break;
							case 7:
								ApplyResistance(armor, min, max, ResistanceType.Cold, 1, 20);
								break;
							case 8:
								ApplyResistance(armor, min, max, ResistanceType.Poison, 1, 20);
								break;
							case 9:
								ApplyResistance(armor, min, max, ResistanceType.Energy, 1, 20);
								break;	
						}
					}
					break;
				}
				case 3: //스텃 갑옷
				{
					if( (int)armor.ItemPower > 0 )
					{
						int rank = (int)armor.ItemPower;

						//weapon.Identified = false;
						//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
						primary.WeaponDamage = 2 + rank;
						primary.BonusDex = 4 + rank * 2;
						armor.MaxHitPoints *= 100 + rank * 20;
						armor.MaxHitPoints /= 100;
						armor.HitPoints = armor.MaxHitPoints;
					}					
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(10);

						if (random == -1)
							break;					
						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								armor.BaseArmorRating += Utility.RandomMinMax(min, max) / 10;
								break;
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 10);
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 10);
								break;							
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 10);
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponSpeed, 1, 5);
								break;
							case 5:
								ApplyResistance(armor, min, max, ResistanceType.Physical, 1, 10);
								break;
							case 6:
								ApplyResistance(armor, min, max, ResistanceType.Fire, 1, 10);
								break;
							case 7:
								ApplyResistance(armor, min, max, ResistanceType.Cold, 1, 10);
								break;
							case 8:
								ApplyResistance(armor, min, max, ResistanceType.Poison, 1, 10);
								break;
							case 9:
								ApplyResistance(armor, min, max, ResistanceType.Energy, 1, 10);
								break;	
						}
					}
					break;
				}
				case 4: //뼈 갑옷
				{
					if( (int)armor.ItemPower > 0 )
					{
						int rank = (int)armor.ItemPower;

						//weapon.Identified = false;
						//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
						primary.WeaponSpeed = 2 + rank;
						primary.Luck = 4 + rank * 2;
						armor.MaxHitPoints *= 100 + rank * 20;
						armor.MaxHitPoints /= 100;
						armor.HitPoints = armor.MaxHitPoints;
					}					
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(10);

						if (random == -1)
							break;					
						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								armor.BaseArmorRating += Utility.RandomMinMax(min, max) / 10;
								break;
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 10);
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 10);
								break;							
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 10);
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponDamage, 1, 5);
								break;
							case 5:
								ApplyResistance(armor, min, max, ResistanceType.Physical, 1, 10);
								break;
							case 6:
								ApplyResistance(armor, min, max, ResistanceType.Fire, 1, 10);
								break;
							case 7:
								ApplyResistance(armor, min, max, ResistanceType.Cold, 1, 10);
								break;
							case 8:
								ApplyResistance(armor, min, max, ResistanceType.Poison, 1, 10);
								break;
							case 9:
								ApplyResistance(armor, min, max, ResistanceType.Energy, 1, 10);
								break;	
						}
					}
					break;
				}
				case 5: //링 갑옷
				{
					if( (int)armor.ItemPower > 0 )
					{
						int rank = (int)armor.ItemPower;

						//weapon.Identified = false;
						//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
						armor.PhysicalBonus = 2 + rank;
						primary.BonusStam = 4 + rank * 2;
						armor.MaxHitPoints *= 100 + rank * 20;
						armor.MaxHitPoints /= 100;
						armor.HitPoints = armor.MaxHitPoints;
					}
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(10);

						if (random == -1)
							break;					
						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								armor.BaseArmorRating += Utility.RandomMinMax(min, max) / 10;
								break;
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponSpeed, 1, 5);
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.AttackChance, 1, 5);
								break;
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 10);
								break;	
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponDamage, 1, 5);
								break;
							case 5:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 10);
								break;
							case 6:
								ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 10);
								break;							
							case 7:
								ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 10);
								break;
							case 8:
								ApplyAttribute(primary, min, max, AosAttribute.RegenHits, 1, 10);
								break;
							case 9:
								ApplyAttribute(primary, min, max, AosAttribute.RegenStam, 1, 10);
								break;
						}
					}
					break;
				}
				case 6: //체인 갑옷
				{
					if( (int)armor.ItemPower > 0 )
					{
						int rank = (int)armor.ItemPower;

						//weapon.Identified = false;
						//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
						armor.PhysicalBonus = 2 + rank;
						primary.BonusStr = 4 + rank * 2;
						armor.MaxHitPoints *= 100 + rank * 20;
						armor.MaxHitPoints /= 100;
						armor.HitPoints = armor.MaxHitPoints;
					}
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(10);

						if (random == -1)
							break;					
						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 10);
								break;
							case 1:
								armor.BaseArmorRating += Utility.RandomMinMax(min, max) / 10;
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 10);
								break;							
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 10);
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 10);
								break;
							case 5:
								ApplyAttribute(primary, min, max, AosAttribute.RegenHits, 1, 10);
								break;
							case 6:
								ApplyAttribute(primary, min, max, AosAttribute.RegenStam, 1, 10);
								break;
							case 7:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponDamage, 1, 5);
								break;
							case 8:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponSpeed, 1, 5);
								break;
							case 9:
								ApplyAttribute(primary, min, max, AosAttribute.ReflectPhysical, 1, 15);
								break;	
						}
					}
					break;
				}
				case 7: //플레이트 갑옷
				{
					if( (int)armor.ItemPower > 0 )
					{
						int rank = (int)armor.ItemPower;

						//weapon.Identified = false;
						//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
						armor.PhysicalBonus = 2 + rank;
						primary.BonusHits = 4 + rank * 2;
						armor.MaxHitPoints *= 100 + rank * 20;
						armor.MaxHitPoints /= 100;
						armor.HitPoints = armor.MaxHitPoints;
					}
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(10);

						if (random == -1)
							break;					
						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								ApplyAttribute(primary, min, max, AosAttribute.ReflectPhysical, 1, 15);
								break;	
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 10);
								break;
							case 2:
								armor.BaseArmorRating += Utility.RandomMinMax(min, max) / 10;
								break;
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 10);
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 10);
								break;							
							case 5:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 10);
								break;
							case 6:
								ApplyAttribute(primary, min, max, AosAttribute.RegenHits, 1, 10);
								break;
							case 7:
								ApplyAttribute(primary, min, max, AosAttribute.RegenStam, 1, 10);
								break;
							case 8:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponDamage, 1, 5);
								break;
							case 9:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponSpeed, 1, 5);
								break;
						}
					}
					break;
				}				
			}
		}

        public static void ApplyAttributesTo(BaseClothing cloth, int attributeCount, int min, int max)
        {
            ApplyAttributesTo(cloth, false, 0, attributeCount, min, max);
        }

        public static void ApplyAttributesTo(BaseClothing cloth, bool playerMade, int luckChance, int attributeCount, int min, int max, int firstpick = -1)
        {
            int delta;

            if (min > max)
            {
                delta = min;
                min = max;
                max = delta;
            }
			bool magiccheck = false;
			/*
			if( cloth.Layer == Layer.Pants || cloth.Layer == Layer.InnerTorso || cloth.Layer == Layer.Arms || cloth.Layer == Layer.Neck || cloth.Layer == Layer.Gloves || cloth.Layer == Layer.Helm)
				magiccheck = true;
			*/
			if( cloth.Layer == Layer.Pants || cloth.Layer == Layer.Helm)
				magiccheck = true;
			
			if( !magiccheck )
			{
				/*
				min = 0;
				max = 0;
				attributeCount = 0;
				*/
				cloth.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), 0);
				return;
			}

            if (!playerMade && RandomItemGenerator.Enabled)
            {
                RandomItemGenerator.GenerateRandomItem(cloth, luckChance, attributeCount, min, max);
                return;
            }

            m_PlayerMade = playerMade;
            m_LuckChance = luckChance;

            AosAttributes primary = cloth.Attributes;
            AosArmorAttributes secondary = cloth.ArmorAttributes;
            AosElementAttributes resists = cloth.Resistances;

			//if ( Utility.Random(100) < 5 )
			//	ApplyAttribute(primary, 0, 100, AosAttribute.SpellChanneling, 1, 1);	
			
            m_Props.SetAll(false);

			if( (int)cloth.ItemPower > 0 )
			{
				int rank = (int)cloth.ItemPower;

				//weapon.Identified = false;
				//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
				primary.SpellDamage = 2 + rank;
				primary.BonusInt = 4 + rank * 2;
				cloth.MaxHitPoints *= 100 + rank * 20;
				cloth.MaxHitPoints /= 100;
				cloth.HitPoints = cloth.MaxHitPoints;
			}					
			for (int i = 0; i < attributeCount; ++i)
			{
				int random = GetUniqueRandom(10);

				if (random == -1)
					break;					
				if( firstpick != -1 )
				{
					random = firstpick;
					m_Props.Set( firstpick, true );
					firstpick = -1;
				}
				switch ( random )
				{
					case 0:
						cloth.AbsorptionAttributes.CastingFocus = Utility.RandomMinMax(min, max) / 10;
						break;
					case 1:
						ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 5);
						break;
					case 2:
						ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 5);
						break;							
					case 3:
						ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 5);
						break;
					case 4:
						ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 10);
						break;
					case 5:
						ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 10);
						break;
					case 6:
						ApplyAttribute(resists, min, max, AosElementAttribute.Fire, 1, 20);
						break;
					case 7:
						ApplyAttribute(resists, min, max, AosElementAttribute.Cold, 1, 20);
						break;
					case 8:
						ApplyAttribute(resists, min, max, AosElementAttribute.Poison, 1, 20);
						break;
					case 9:
						ApplyAttribute(resists, min, max, AosElementAttribute.Energy, 1, 20);
						break;	
				}
			}
		}

        public static void ApplyAttributesTo(BaseJewel jewelry, int attributeCount, int min, int max)
        {
            ApplyAttributesTo(jewelry, false, 0, attributeCount, min, max);
        }

        public static void ApplyAttributesTo(BaseJewel jewelry, bool playerMade, int luckChance, int attributeCount, int min, int max, int firstpick = -1)
        {
            int delta;

            if (min > max)
            {
                delta = min;
                min = max;
                max = delta;
            }

            if (!playerMade && RandomItemGenerator.Enabled)
            {
                RandomItemGenerator.GenerateRandomItem(jewelry, luckChance, attributeCount, min, max);
                return;
            }

            m_PlayerMade = playerMade;
            m_LuckChance = luckChance;

            AosAttributes primary = jewelry.Attributes;
            AosElementAttributes resists = jewelry.Resistances;
            AosSkillBonuses skills = jewelry.SkillBonuses;
			//if ( Utility.Random(100) < 5 )
			//	ApplyAttribute(primary, 0, 100, AosAttribute.EnhancePotions, 10, 10);
			
            m_Props.SetAll(false);

			int jeweltype = 0;
			if( jewelry.Layer == Layer.Earrings || jewelry.Layer == Layer.Neck )
				jeweltype = 1;

			if( (int)jewelry.ItemPower > 0 )
			{
				int rank = (int)jewelry.ItemPower;

				//weapon.Identified = false;
				//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
				jewelry.BaseJewelRating += 2 + rank;
				resists.Fire = 2 + rank;
				resists.Cold = 2 + rank;
				resists.Poison = 2 + rank;
				resists.Energy = 2 + rank;
				jewelry.MaxHitPoints *= 100 + rank * 20;
				jewelry.MaxHitPoints /= 100;
				jewelry.HitPoints = jewelry.MaxHitPoints;
			}
			
			switch( jeweltype )
			{
				case 0:
				{
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(20);

						if (random == -1)
							break;

						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponDamage, 1, 20);
								break;
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 10);
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.AttackChance, 1, 20);
								break;							
							case 3:
								ApplyAttribute(primary, min, max, AosAttribute.WeaponSpeed, 1, 20);
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.SpellDamage, 1, 20);
								break;
							case 5:
								ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 20);
								break;
							case 6:
								ApplyAttribute(primary, min, max, AosAttribute.LowerRegCost, 1, 10);
								break;
							case 7:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 20);
								break;
							case 8:
								ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 20);
								break;
							case 9:
								ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 20);
								break;
							case 10:
								ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 20);
								break;
							case 11:
								ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 20);
								break;
							case 12:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 20);
								break;
							case 13:
								ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 20);
								break;
							case 14:
								ApplyAttribute(primary, min, max, AosAttribute.EnhancePotions, 1, 25);
								break;
							case 15:
								ApplySkillBonus(skills, min, max, 0, 0.1, 2.5);
								break;
							case 16:
								ApplySkillBonus(skills, min, max, 1, 0.1, 2.5);
								break;
							case 17:
								ApplySkillBonus(skills, min, max, 2, 0.1, 2.5);
								break;
							case 18:
								ApplySkillBonus(skills, min, max, 3, 0.1, 2.5);
								break;
							case 19:
								ApplySkillBonus(skills, min, max, 4, 0.1, 2.5);
								break;
						}
					}
					break;
				}					
				case 1:
				{
					for (int i = 0; i < attributeCount; ++i)
					{
						int random = GetUniqueRandom(20);

						if (random == -1)
							break;

						if( firstpick != -1 )
						{
							random = firstpick;
							m_Props.Set( firstpick, true );
							firstpick = -1;
						}
						switch ( random )
						{
							case 0:
								ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 20);
								break;							
							case 1:
								ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 20);
								break;
							case 2:
								ApplyAttribute(primary, min, max, AosAttribute.ReflectPhysical, 1, 30);
								break;
							case 3:
								jewelry.AbsorptionAttributes.CastingFocus = Utility.RandomMinMax(min, max) / 10;
								break;
							case 4:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 20);
								break;
							case 5:
								ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 20);
								break;
							case 6:
								ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 20);
								break;
							case 7:
								ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 20);
								break;
							case 8:
								ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 20);
								break;
							case 9:
								ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 20);
								break;
							case 10:
								ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 20);
								break;
							case 11:
								ApplyAttribute(primary, min, max, AosAttribute.RegenHits, 1, 20);
								break;
							case 12:
								ApplyAttribute(primary, min, max, AosAttribute.RegenStam, 1, 20);
								break;
							case 13:
								ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 20);
								break;
							case 14:
								ApplyAttribute(primary, min, max, AosAttribute.EnhancePotions, 1, 25);
								break;
							case 15:
								ApplySkillBonus(skills, min, max, 0, 0.1, 2.5);
								break;
							case 16:
								ApplySkillBonus(skills, min, max, 1, 0.1, 2.5);
								break;
							case 17:
								ApplySkillBonus(skills, min, max, 2, 0.1, 2.5);
								break;
							case 18:
								ApplySkillBonus(skills, min, max, 3, 0.1, 2.5);
								break;
							case 19:
								ApplySkillBonus(skills, min, max, 4, 0.1, 2.5);
								break;
						}
					}
					break;
				}
			}

        }

        public static void ApplyAttributesTo(Spellbook spellbook, int attributeCount, int min, int max)
        {
            ApplyAttributesTo(spellbook, false, 0, attributeCount, min, max);
        }

        public static void ApplyAttributesTo(Spellbook spellbook, bool playerMade, int luckChance, int attributeCount, int min, int max, int firstpick = -0)
        {
            int delta;

            if (min > max)
            {
                delta = min;
                min = max;
                max = delta;
            }

            m_PlayerMade = playerMade;
            m_LuckChance = luckChance;

            AosAttributes primary = spellbook.Attributes;
            AosElementAttributes resists = spellbook.Resistances;
			SAAbsorptionAttributes secondary = spellbook.AbsorptionAttributes;
            AosSkillBonuses skills = spellbook.SkillBonuses;

			//if ( Utility.Random(100) < 5 )
			//	spellbook.Slayer = GetRandomSlayer();

            m_Props.SetAll(false);

            for (int i = 0; i < attributeCount; ++i)
            {
                int random = GetUniqueRandom(20);

				if( firstpick != -1 )
				{
					random = firstpick;
					m_Props.Set( firstpick, true );
					firstpick = -1;
				}
                if (random == -1)
                    break;

				if( (int)spellbook.ItemPower > 0 )
				{
					int rank = (int)spellbook.ItemPower;

					//weapon.Identified = false;
					//weapon.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), rank);
					primary.SpellDamage = 20 + rank * 10;
					int dice = 6 + rank * 3 + Utility.RandomMinMax( -1, 1 );
					if( dice < 10 )
						dice = 10;
					if ( dice > 30 )
						dice = 30;
					primary.LowerRegCost = dice;
					//ApplyAttribute(primary, 0, 100, AosAttribute.WeaponDamage, rank * 10 - 9, rank * 10);
					//weapon.Name = String.Format( "<basefont color='#FFB400'>{0}</basefont>", GetNameString(weapon) );
					spellbook.MaxHitPoints *= 100 + rank * 20;
					spellbook.MaxHitPoints /= 100;
					spellbook.HitPoints = spellbook.MaxHitPoints;
				}

				
				//if ( Utility.Random(100) < 5 )
				//	spellbook.Slayer = GetRandomSlayer();
                switch ( random )
                {
                    case 0:
                        ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 30);
                        break;
                    case 1:
                        ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 30);
                        break;
                    case 2:
                        ApplyAttribute(secondary, min, max, SAAbsorptionAttribute.CastingFocus, 1, 20);
                        break;
                    case 3:
                        ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 10);
                        break;
                    case 4:
                        ApplyAttribute(resists, min, max, AosElementAttribute.Fire, 1, 20);
                        break;
                    case 5:
                        ApplyAttribute(resists, min, max, AosElementAttribute.Cold, 1, 20);
                        break;
                    case 6:
                        ApplyAttribute(resists, min, max, AosElementAttribute.Poison, 1, 20);
                        break;
                    case 7:
                        ApplyAttribute(resists, min, max, AosElementAttribute.Energy, 1, 20);
                        break;
                    case 8:
                        ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 60);
                        break;
                    case 9:
                        ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 60);
                        break;
                    case 10:
                        ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 60);
                        break;
                    case 11:
                        ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 60);
                        break;
                    case 12:
                        ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 60);
                        break;
                    case 13:
                        ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 60);
                        break;
                    case 14:
                        ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 60);
                        break;
                    case 15:
                        ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 30);
                        break;
                    case 16:
                        ApplyAttribute(secondary, min, max, SAAbsorptionAttribute.ResonanceFire, 1, 20);
                        break;
                    case 17:
                        ApplyAttribute(secondary, min, max, SAAbsorptionAttribute.ResonanceCold, 1, 20);
                        break;
                    case 18:
                        ApplyAttribute(secondary, min, max, SAAbsorptionAttribute.ResonancePoison, 1, 20);
                        break;
                    case 19:
                        ApplyAttribute(secondary, min, max, SAAbsorptionAttribute.ResonanceEnergy, 1, 20);
                        break;
                }
            }
		}

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
            writer.Write((int)Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 0:
                    {
                        Resource = (CraftResource)reader.ReadInt();
                        break;
                    }
            }
        }

        public void ApplyAttributesTo(BaseWeapon weapon)
        {
            CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);

            if (resInfo == null)
                return;

            CraftAttributeInfo attrs = resInfo.AttributeInfo;

            if (attrs == null)
                return;

            int attributeCount = Utility.RandomMinMax(attrs.RunicMinAttributes, attrs.RunicMaxAttributes);
            int min = attrs.RunicMinIntensity;
            int max = attrs.RunicMaxIntensity;

            ApplyAttributesTo(weapon, true, 0, attributeCount, min, max);
        }

        public void ApplyAttributesTo(BaseArmor armor)
        {
            CraftResourceInfo resInfo = CraftResources.GetInfo(Resource);

            if (resInfo == null)
                return;

            CraftAttributeInfo attrs = resInfo.AttributeInfo;

            if (attrs == null)
                return;

            int attributeCount = Utility.RandomMinMax(attrs.RunicMinAttributes, attrs.RunicMaxAttributes);
            int min = attrs.RunicMinIntensity;
            int max = attrs.RunicMaxIntensity;

            ApplyAttributesTo(armor, true, 0, attributeCount, min, max);
        }
	
        public static int Dice(int min, int max)
        {
            int percent;

            if (m_PlayerMade)
            {
                percent = Utility.RandomMinMax(min, max);
            }
            else
            {
                int v = Utility.RandomMinMax(0, 10000);

                v = (int)Math.Sqrt(v);
                v = 100 - v;

				percent = Math.Min(max, min + AOS.Scale((max - min), v));
            }
			return percent;
        }		

        private static int Scale(int low, int high, int percent)
        {
            int scaledBy = Math.Abs(high - low) + 1;

            if (scaledBy != 0)
                scaledBy = 10000 / scaledBy;

            percent *= (10000 + scaledBy);
			
            return low + (((high - low) * percent) / 1000001);
        }

        private static void ApplyAttribute(AosAttributes attrs, AosAttribute attr, int low, int high, int percent, int scale)
        {
            if (attr == AosAttribute.CastSpeed)
                attrs[attr] += Scale(low / scale, high / scale, percent ) * scale;
            else
                attrs[attr] = Scale(low / scale, high / scale, percent) * scale;

            if (attr == AosAttribute.SpellChanneling)
                attrs[AosAttribute.CastSpeed] -= 1;
        }			

        private static void ApplySkillBonus(AosSkillBonuses attrs, int min, int max, int index, double low, double high)
        {
            SkillName[] possibleSkills = (attrs.Owner is Spellbook ? m_PossibleSpellbookSkills : m_PossibleBonusSkills);
            int count = possibleSkills.Length;

            SkillName sk, check;
            double bonus;
            bool found;

            do
            {
                found = false;
                sk = possibleSkills[Utility.Random(count)];

                for (int i = 0; !found && i < 5; ++i)
                    found = (attrs.GetValues(i, out check, out bonus) && check == sk);
            }
            while (found);

            attrs.SetValues(index, sk, Scale(min, max, low, high));
        }
		
		
        private static void ApplySkillBonus(AosSkillBonuses attrs, int index, double low, double high, int percent)
        {
            SkillName[] possibleSkills = (attrs.Owner is Spellbook ? m_PossibleSpellbookSkills : m_PossibleBonusSkills);
            int count = possibleSkills.Length;

            SkillName sk, check;
            double bonus;
            bool found;

            do
            {
                found = false;
                sk = possibleSkills[Utility.Random(count)];

                for (int i = 0; !found && i < 5; ++i)
                    found = (attrs.GetValues(i, out check, out bonus) && check == sk);
            }
            while (found);

            attrs.SetValues(index, sk, Scale((int)( low * 10 ), (int)( high * 10 ), percent)* 0.1);
        }
		
        private static void ApplyAttribute(AosWeaponAttributes attrs, AosWeaponAttribute attr, int low, int high, int percent, int scale)
        {
            attrs[attr] = Scale(low / scale, high / scale, percent) * scale;
        }		

        private static void ApplyAttribute(AosArmorAttributes attrs, AosArmorAttribute attr, int low, int high, int percent, int scale)
        {
            attrs[attr] = Scale(low / scale, high / scale, percent) * scale;
        }	

        private static void ApplyAttribute(AosElementAttributes attrs, AosElementAttribute attr, int low, int high, int percent, int scale)
        {
            attrs[attr] = Scale(low / scale, high / scale, percent) * scale;
        }
		
        private static void ApplyAttribute(SAAbsorptionAttributes attrs, SAAbsorptionAttribute attr, int low, int high, int percent, int scale)
        {
            attrs[attr] = Scale(low / scale, high / scale, percent) * scale;
        }		
		
        private static void ApplyResistance(BaseArmor ar, ResistanceType res, int low, int high, int percent, int scale)
        {
            switch ( res )
            {
                case ResistanceType.Physical:
                    ar.PhysicalBonus += Scale(low / scale, high / scale, percent) * scale;
                    break;
                case ResistanceType.Fire:
                    ar.FireBonus += Scale(low / scale, high / scale, percent) * scale;
                    break;
                case ResistanceType.Cold:
                    ar.ColdBonus += Scale(low / scale, high / scale, percent) * scale;
                    break;
                case ResistanceType.Poison:
                    ar.PoisonBonus += Scale(low / scale, high / scale, percent) * scale;
                    break;
                case ResistanceType.Energy:
                    ar.EnergyBonus += Scale(low / scale, high / scale, percent) * scale;
                    break;
            }
        }		
        private static int Scale(int min, int max, int low, int high)
        {
            int percent;

            if (m_PlayerMade)
            {
                percent = Utility.RandomMinMax(min, max);
            }
            else
            {
                int v = Utility.RandomMinMax(0, 10000);

                v = (int)Math.Sqrt(v);
                v = 100 - v;

				percent = Math.Min(max, min + AOS.Scale((max - min), v));
            }

            int scaledBy = Math.Abs(high - low) + 1;

            if (scaledBy != 0)
                scaledBy = 10000 / scaledBy;

            percent *= (10000 + scaledBy);
			
            return low + (((high - low) * percent) / 1000001);
        }
        private static int Scale(int min, int max, double low, double high)
        {
            int percent;

            if (m_PlayerMade)
            {
                percent = Utility.RandomMinMax(min, max);
            }
            else
            {
                int v = Utility.RandomMinMax(0, 10000);

                v = (int)Math.Sqrt(v);
                v = 100 - v;

				percent = Math.Min(max, min + AOS.Scale((max - min), v));
            }

            int scaledBy = (int)( Math.Abs(high - low) ) + 1;

            if (scaledBy != 0)
                scaledBy = 10000 / scaledBy;

            percent *= (10000 + scaledBy);
			
            return (int)( low + (((high - low) * percent) / 1000001));
        }

        private static void ApplyAttribute(AosAttributes attrs, int min, int max, AosAttribute attr, int low, int high)
        {
            ApplyAttribute(attrs, min, max, attr, low, high, 1);
        }

        private static void ApplyAttribute(AosAttributes attrs, int min, int max, AosAttribute attr, int low, int high, int scale)
        {
            if (attr == AosAttribute.CastSpeed)
                attrs[attr] += Scale(min, max, low / scale, high / scale) * scale;
            else
                attrs[attr] = Scale(min, max, low / scale, high / scale) * scale;

            if (attr == AosAttribute.SpellChanneling)
                attrs[AosAttribute.CastSpeed] -= 1;
        }

        private static void ApplyAttribute(AosArmorAttributes attrs, int min, int max, AosArmorAttribute attr, int low, int high)
        {
            attrs[attr] = Scale(min, max, low, high);
        }

        private static void ApplyAttribute(AosArmorAttributes attrs, int min, int max, AosArmorAttribute attr, int low, int high, int scale)
        {
            attrs[attr] = Scale(min, max, low / scale, high / scale) * scale;
        }

        private static void ApplyAttribute(AosWeaponAttributes attrs, int min, int max, AosWeaponAttribute attr, int low, int high)
        {
            attrs[attr] = Scale(min, max, low, high);
        }

		private static void ApplyAttribute(SAAbsorptionAttributes attrs, int min, int max,SAAbsorptionAttribute attr, int low, int high)
        {
            attrs[attr] = Scale(min, max, low, high);
        }		
	
        private static void ApplyAttribute(AosWeaponAttributes attrs, int min, int max, AosWeaponAttribute attr, int low, int high, int scale)
        {
            attrs[attr] = Scale(min, max, low / scale, high / scale) * scale;
        }

        private static void ApplyAttribute(AosElementAttributes attrs, int min, int max, AosElementAttribute attr, int low, int high)
        {
            attrs[attr] = Scale(min, max, low, high);
        }

        private static void ApplyAttribute(AosElementAttributes attrs, int min, int max, AosElementAttribute attr, int low, int high, int scale)
        {
            attrs[attr] = Scale(min, max, low / scale, high / scale) * scale;
        }

        private static void ApplyVelocityAttribute(BaseRanged ranged, int min, int max, int low, int high, int scale)
        {
            ranged.Velocity = Scale(min, max, low / scale, high / scale) * scale;
        }

        public static void ApplyElementalDamage(BaseWeapon weapon, int min, int max)
        {
            int fire, phys, cold, nrgy, pois, chaos, direct;

            weapon.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

            int intensity = Math.Min(phys, Scale(min, max, 10 / 10, 100 / 10) * 10);

            weapon.AosElementDamages[_DamageTypes[Utility.Random(_DamageTypes.Length)]] = intensity;

            //weapon.Hue = weapon.GetElementalDamageHue();
        }
		
        public static void ApplyElementalDamage(BaseWeapon weapon, int rank)
        {
            int fire, phys, cold, nrgy, pois, chaos, direct;

            weapon.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

            int intensity = Math.Min(phys, rank);

            weapon.AosElementDamages[_DamageTypes[Utility.Random(_DamageTypes.Length)]] = intensity;

            //weapon.Hue = weapon.GetElementalDamageHue();
        }

		
		
        private static AosElementAttribute[] _DamageTypes =
        {
            AosElementAttribute.Cold,
            AosElementAttribute.Energy,
            AosElementAttribute.Fire,
            AosElementAttribute.Poison
        };


        private static void ApplyResistance(BaseArmor ar, int min, int max, ResistanceType res, int low, int high)
        {
            switch ( res )
            {
                case ResistanceType.Physical:
                    ar.PhysicalBonus += Scale(min, max, low, high);
                    break;
                case ResistanceType.Fire:
                    ar.FireBonus += Scale(min, max, low, high);
                    break;
                case ResistanceType.Cold:
                    ar.ColdBonus += Scale(min, max, low, high);
                    break;
                case ResistanceType.Poison:
                    ar.PoisonBonus += Scale(min, max, low, high);
                    break;
                case ResistanceType.Energy:
                    ar.EnergyBonus += Scale(min, max, low, high);
                    break;
            }
        }

        private static int AssignElementalDamage(BaseWeapon weapon, AosElementAttribute attr, int totalDamage)
        {
            if (totalDamage <= 0)
                return 0;

            int random = Utility.Random((int)(totalDamage / 10) + 1) * 10;
            weapon.AosElementDamages[attr] = random;

            return (totalDamage - random);
        }
    }
}
