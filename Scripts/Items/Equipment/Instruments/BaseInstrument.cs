using System;
using System.Collections;
using Server.Engines.Craft;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Misc; 

namespace Server.Items
{
    public delegate void InstrumentPickedCallback(Mobile from, BaseInstrument instrument);

    // IDurability 인터페이스 상속 유지
    public abstract class BaseInstrument : Item, ISlayer, IQuality, IResource, IEquipOption, IDurability
    {
        public static readonly double MaxBardingDifficulty = 160.0;

        private int m_WellSound, m_BadlySound;
        private SlayerName m_Slayer, m_Slayer2;
        private ItemQuality m_Quality;
        private Mobile m_Crafter;
        private int m_UsesRemaining;
        private CraftResource m_Resource;

        #region IEquipOption & IDurability 구현부
        private int[] m_PrefixOption = new int[100];
        private int[] m_SuffixOption = new int[100];

        private int m_MaxHitPoints;
        private int m_HitPoints;
        private ItemPower m_ItemPower;
        private bool m_Identified;
        private bool m_PlayerConstructed;

        [CommandProperty(AccessLevel.GameMaster)]
        public int[] PrefixOption { get { return m_PrefixOption; } set { m_PrefixOption = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int[] SuffixOption { get { return m_SuffixOption; } set { m_SuffixOption = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxHitPoints
        {
            get { return this.Layer == Layer.Invalid ? 0 : m_MaxHitPoints; }
            set { m_MaxHitPoints = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoints
        {
            get { return this.Layer == Layer.Invalid ? 0 : m_HitPoints; }
            set 
            { 
                if (value != m_HitPoints && m_MaxHitPoints > 0)
                {
                    m_HitPoints = value;
                    if (m_HitPoints < 0) Delete();
                    else if (m_HitPoints > m_MaxHitPoints) m_HitPoints = m_MaxHitPoints;
                    InvalidateProperties();
                }
            }
        }

        public virtual int InitMinHits { get { return 100; } }
        public virtual int InitMaxHits { get { return 100; } }
        
        public override void AddNameProperty(ObjectPropertyList list)
        {
            Server.Misc.NewOptionOPL.AppendName(list, this);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemPower ItemPower
        {
            get { return m_ItemPower; }
            set { m_ItemPower = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Identified
        {
            get { return m_Identified; }
            set { m_Identified = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PlayerConstructed
        {
            get { return m_PlayerConstructed; }
            set { m_PlayerConstructed = value; InvalidateProperties(); }
        }

        public virtual bool CanFortify { get { return true; } }

        public void UnscaleDurability()
        {
            int scale = 100;
            m_HitPoints = ((m_HitPoints * 100) + (scale - 1)) / scale;
            m_MaxHitPoints = ((m_MaxHitPoints * 100) + (scale - 1)) / scale;
            InvalidateProperties();
        }

        public void ScaleDurability()
        {
            int scale = 100;
            m_HitPoints = ((m_HitPoints * scale) + 99) / 100;
            m_MaxHitPoints = ((m_MaxHitPoints * scale) + 99) / 100;
            if (m_MaxHitPoints > 255) m_MaxHitPoints = 255;
            if (m_HitPoints > 255) m_HitPoints = 255;
            InvalidateProperties();
        }
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public int SuccessSound { get { return m_WellSound; } set { m_WellSound = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int FailureSound { get { return m_BadlySound; } set { m_BadlySound = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer { get { return m_Slayer; } set { m_Slayer = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer2 { get { return m_Slayer2; } set { m_Slayer2 = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemQuality Quality
        {
            get { return m_Quality; }
            set { UnscaleUses(); m_Quality = value; InvalidateProperties(); ScaleUses(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter { get { return m_Crafter; } set { m_Crafter = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set { m_Resource = value; Hue = CraftResources.GetHue(m_Resource); InvalidateProperties(); }
        }

        public virtual int InitMinUses { get { return 350; } }
        public virtual int InitMaxUses { get { return 450; } }
        public virtual TimeSpan ChargeReplenishRate { get { return TimeSpan.FromMinutes(5.0); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get { CheckReplenishUses(); return m_UsesRemaining; }
            set { m_UsesRemaining = value; InvalidateProperties(); }
        }

        private DateTime m_LastReplenished;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastReplenished
        {
            get { return m_LastReplenished; }
            set { m_LastReplenished = value; CheckReplenishUses(); }
        }

        private bool m_ReplenishesCharges;
        [CommandProperty(AccessLevel.GameMaster)]
        public bool ReplenishesCharges
        {
            get { return m_ReplenishesCharges; }
            set 
            {
                if (value != m_ReplenishesCharges && value) m_LastReplenished = DateTime.UtcNow;
                m_ReplenishesCharges = value; 
            }
        }

        public void RandomInstrument()
        {
            switch (Utility.Random(3))
            {
                case 0: { ItemID = 0xEB2; SuccessSound = 0x45; FailureSound = 0x46; break; }
                case 1: { ItemID = 0xEB3; SuccessSound = 0x4C; FailureSound = 0x4D; break; }
                default: { ItemID = 0xE9C; SuccessSound = 0x38; FailureSound = 0x39; break; }
            }
        }

        public void CheckReplenishUses() { CheckReplenishUses(true); }

        public void CheckReplenishUses(bool invalidate)
        {
            if (!m_ReplenishesCharges || m_UsesRemaining >= InitMaxUses) return;

            if (m_LastReplenished + ChargeReplenishRate < DateTime.UtcNow)
            {
                TimeSpan timeDifference = DateTime.UtcNow - m_LastReplenished;
                m_UsesRemaining = Math.Min(m_UsesRemaining + (int)(timeDifference.Ticks / ChargeReplenishRate.Ticks), InitMaxUses);
                m_LastReplenished = DateTime.UtcNow;
                if (invalidate) InvalidateProperties();
            }
        }

        public void ScaleUses() { UsesRemaining = (UsesRemaining * GetUsesScalar()) / 100; }
        public void UnscaleUses() { UsesRemaining = (UsesRemaining * 100) / GetUsesScalar(); }

        public int GetUsesScalar()
        {
            if (m_Quality == ItemQuality.Exceptional) return 200;
            return 100;
        }

        // =========================================================
        // [수정] 악기 사용 시 내구도 / 충전 횟수 감소 로직
        // =========================================================
        public void ConsumeUse(Mobile from)
        {
            // [삭제 완료] 200 보너스 마에스트로 (내구도/차징 절대 보존) 기획 폐기

            if (this.Layer != Layer.Invalid && this.Parent == from)
            {
                bool found = false;
                for (int i = 0; i < m_PrefixOption.Length; i++)
                {
                    if (m_PrefixOption[i] == 1) // 접두 1: 아이템 세부 내구도
                    {
                        m_SuffixOption[i] += 20; 

                        if (m_SuffixOption[i] >= 10000)
                        {
                            int drop = m_SuffixOption[i] / 10000;
                            m_SuffixOption[i] %= 10000;

                            this.HitPoints -= drop; 
                            if (from != null && this.HitPoints > 0)
                                from.SendMessage("연주로 인해 악기의 내구도가 감소했습니다.");
                        }
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    for (int i = 0; i < m_PrefixOption.Length; i++)
                    {
                        if (m_PrefixOption[i] == 0)
                        {
                            m_PrefixOption[i] = 1;
                            m_SuffixOption[i] = 20;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (UsesRemaining > 1) { --UsesRemaining; }
                else
                {
                    if (from != null) from.SendLocalizedMessage(502079); 
                    Delete();
                }
            }
        }

        public static BaseInstrument GetInstrument(Mobile from)
        {
            Item item = from.FindItemOnLayer(Layer.TwoHanded);
            if (item is BaseInstrument inst2H) return inst2H;

            item = from.FindItemOnLayer(Layer.OneHanded);
            if (item is BaseInstrument inst1H) return inst1H;

            return null; // 백팩 악기 무시
        }

        public static int GetBardRange(Mobile bard, SkillName skill)
        {
            return 8 + (int)(bard.Skills[skill].Value / 15);
        }

        public static void PickInstrument(Mobile from, InstrumentPickedCallback callback)
        {
            BaseInstrument instrument = GetInstrument(from);

            if (instrument != null)
            {
                if (callback != null) callback(from, instrument);
            }
            else
            {
                from.SendMessage("바드 기술을 사용하려면 먼저 전투용 악기를 손에 장착해야 합니다.");
            }
        }

        public static void OnPickedInstrument(Mobile from, object targeted, object state) { /* 사용 안함 */ }
        public static bool IsMageryCreature(BaseCreature bc) { return (bc != null && bc.AI == AIType.AI_Mage && bc.Skills[SkillName.Magery].Base > 5.0); }
        public static bool IsFireBreathingCreature(BaseCreature bc) { if (bc == null) return false; var profile = bc.AbilityProfile; if (profile != null) return profile.HasAbility(SpecialAbility.DragonBreath); return false; }
        public static bool IsPoisonImmune(BaseCreature bc) { return (bc != null && bc.PoisonImmune != null); }
        public static int GetPoisonLevel(BaseCreature bc) { if (bc == null) return 0; Poison p = bc.HitPoison; if (p == null) return 0; return p.Level + 1; }

        public static double GetBaseDifficulty(Mobile targ)
        {
            double val = (targ.HitsMax * 1.6) + targ.StamMax + targ.ManaMax;
            val += targ.SkillsTotal / 10;
            BaseCreature bc = targ as BaseCreature;

            if (IsMageryCreature(bc)) val += 100;
            if (IsFireBreathingCreature(bc)) val += 100;
            if (IsPoisonImmune(bc)) val += 100;
            if (targ is VampireBat || targ is VampireBatFamiliar) val += 100;

            val += GetPoisonLevel(bc) * 20;
            if (val > 700) val = 700 + (int)((val - 700) * (3.0 / 11));
            val /= 10;
            if (bc != null && bc.IsParagon) val += 40.0;
            if (Core.SE && val > MaxBardingDifficulty) val = MaxBardingDifficulty;

            return val;
        }

        public double GetDifficultyFor(Mobile targ)
        {
            double val = GetBaseDifficulty(targ);
            if (m_Quality == ItemQuality.Exceptional) val -= 5.0;

            if (m_Slayer != SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
                if (entry != null) { if (entry.Slays(targ)) val -= 10.0; else if (entry.Group.OppositionSuperSlays(targ)) val += 10.0; }
            }

            if (m_Slayer2 != SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
                if (entry != null) { if (entry.Slays(targ)) val -= 10.0; else if (entry.Group.OppositionSuperSlays(targ)) val += 10.0; }
            }

            if (m_Slayer == SlayerName.None && m_Slayer2 == SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(SlayerSocket.GetSlayer(this));
                if (entry != null) { if (entry.Slays(targ)) val -= 10.0; else if (entry.Group.OppositionSuperSlays(targ)) val += 10.0; }
            }

            return val;
        }

        public static void SetInstrument(Mobile from, BaseInstrument item) { /* 사용 안함 */ }

        public BaseInstrument()
        {
            RandomInstrument();
            UsesRemaining = Utility.RandomMinMax(InitMinUses, InitMaxUses);
            m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);
            m_Identified = true;
        }

        public BaseInstrument(int itemID, int wellSound, int badlySound) : base(itemID)
        {
            m_WellSound = wellSound;
            m_BadlySound = badlySound;
            UsesRemaining = Utility.RandomMinMax(InitMinUses, InitMaxUses);
            m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);
            m_Identified = true;
        }

        public override void AddCraftedProperties(ObjectPropertyList list)
        {
            if (m_Crafter != null) list.Add(1050043, m_Crafter.TitleName); // crafted by ~1_NAME~
            if (m_Quality == ItemQuality.Exceptional) list.Add(1060636); // exceptional
        }

        public override void AddUsesRemainingProperties(ObjectPropertyList list)
        {
            list.Add(1060584, UsesRemaining.ToString()); // uses remaining: ~1_val~
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            int oldUses = m_UsesRemaining;
            CheckReplenishUses(false);
            base.GetProperties(list);

            if (m_ReplenishesCharges) list.Add(1070928); // Replenish Charges

            if (m_Slayer != SlayerName.None) { SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer); if (entry != null) list.Add(entry.Title); }
            if (m_Slayer2 != SlayerName.None) { SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2); if (entry != null) list.Add(entry.Title); }

            if (!CraftResources.IsStandard(m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(m_Resource);
                if (num > 0) list.Add(num); else list.Add(CraftResources.GetName(m_Resource));
            }

            if (m_UsesRemaining != oldUses) Timer.DelayCall(TimeSpan.Zero, new TimerCallback(InvalidateProperties));
            Server.Misc.NewOptionOPL.Append(list, this);
        }

        public override void OnSingleClick(Mobile from)
        {
            ArrayList attrs = new ArrayList();

            if (DisplayLootType)
            {
                if (LootType == LootType.Blessed) attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                else if (LootType == LootType.Cursed) attrs.Add(new EquipInfoAttribute(1049643)); // cursed
            }

            if (m_Quality == ItemQuality.Exceptional) attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));
            if (m_ReplenishesCharges) attrs.Add(new EquipInfoAttribute(1070928)); // Replenish Charges

            if (m_Slayer != SlayerName.None) { SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer); if (entry != null) attrs.Add(new EquipInfoAttribute(entry.Title)); }
            if (m_Slayer2 != SlayerName.None) { SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2); if (entry != null) attrs.Add(new EquipInfoAttribute(entry.Title)); }

            int number;
            if (Name == null) number = LabelNumber; else { LabelTo(from, Name); number = 1041000; }

            if (attrs.Count == 0 && Crafter == null && Name != null) return;
            EquipmentInfo eqInfo = new EquipmentInfo(number, m_Crafter, false, (EquipInfoAttribute[])attrs.ToArray(typeof(EquipInfoAttribute)));
            from.Send(new DisplayEquipmentInfo(this, eqInfo));
        }

        public BaseInstrument(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)6); // version 5 -> 6 (내구도 속성 추가)

            writer.Write((int)m_MaxHitPoints);
            writer.Write((int)m_HitPoints);
            writer.Write((int)m_ItemPower);
            writer.Write((bool)m_Identified);
            writer.Write((bool)m_PlayerConstructed);

            writer.Write(m_PrefixOption.Length);
            for (int i = 0; i < m_PrefixOption.Length; i++)
            {
                writer.Write(m_PrefixOption[i]);
                writer.Write(m_SuffixOption[i]);
            }

            writer.Write((int)m_Resource);
            writer.Write(m_ReplenishesCharges);
            if (m_ReplenishesCharges) writer.Write(m_LastReplenished);
            writer.Write(m_Crafter);
            writer.WriteEncodedInt((int)m_Quality);
            writer.WriteEncodedInt((int)m_Slayer);
            writer.WriteEncodedInt((int)m_Slayer2);
            writer.WriteEncodedInt((int)UsesRemaining);
            writer.WriteEncodedInt((int)m_WellSound);
            writer.WriteEncodedInt((int)m_BadlySound);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 6:
                    {
                        m_MaxHitPoints = reader.ReadInt();
                        m_HitPoints = reader.ReadInt();
                        m_ItemPower = (ItemPower)reader.ReadInt();
                        m_Identified = reader.ReadBool();
                        m_PlayerConstructed = reader.ReadBool();
                        goto case 5;
                    }
                case 5:
                    {
                        int length = reader.ReadInt();
                        m_PrefixOption = new int[100];
                        m_SuffixOption = new int[100];
                        for (int i = 0; i < length && i < 100; i++)
                        {
                            m_PrefixOption[i] = reader.ReadInt();
                            m_SuffixOption[i] = reader.ReadInt();
                        }
                        goto case 4;
                    }
                case 4:
                    {
                        if(version < 5) { m_PrefixOption = new int[100]; m_SuffixOption = new int[100]; }
                        if(version < 6) { m_Identified = true; m_MaxHitPoints = InitMaxHits; m_HitPoints = InitMaxHits; }
                        m_Resource = (CraftResource)reader.ReadInt();
                        goto case 3;
                    }
                case 3:
                    {
                        m_ReplenishesCharges = reader.ReadBool();
                        if (m_ReplenishesCharges) m_LastReplenished = reader.ReadDateTime();
                        goto case 2;
                    }
                case 2:
                    {
                        m_Crafter = reader.ReadMobile();
                        m_Quality = (ItemQuality)reader.ReadEncodedInt();
                        m_Slayer = (SlayerName)reader.ReadEncodedInt();
                        m_Slayer2 = (SlayerName)reader.ReadEncodedInt();
                        UsesRemaining = reader.ReadEncodedInt();
                        m_WellSound = reader.ReadEncodedInt();
                        m_BadlySound = reader.ReadEncodedInt();
                        break;
                    }
                case 1:
                    {
                        m_Crafter = reader.ReadMobile();
                        m_Quality = (ItemQuality)reader.ReadEncodedInt();
                        m_Slayer = (SlayerName)reader.ReadEncodedInt();
                        UsesRemaining = reader.ReadEncodedInt();
                        m_WellSound = reader.ReadEncodedInt();
                        m_BadlySound = reader.ReadEncodedInt();
                        break;
                    }
                case 0:
                    {
                        m_WellSound = reader.ReadInt();
                        m_BadlySound = reader.ReadInt();
                        UsesRemaining = Utility.RandomMinMax(InitMinUses, InitMaxUses);
                        break;
                    }
            }
            CheckReplenishUses();
        }

        // =========================================================
        // [수정] 악기 더블 클릭 (음악 지식 사용 및 150/200 보너스 처리)
        // =========================================================
        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 1))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return;
            }

            if (this.Layer != Layer.Invalid && this.Parent != from)
            {
                from.SendMessage("이 악기는 전투 및 버프용입니다. 손에 장착해야만 연주할 수 있습니다.");
                return;
            }

            if (this.Layer == Layer.Invalid && !this.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            if (from.BeginAction(typeof(BaseInstrument)))
            {
                Timer.DelayCall(TimeSpan.FromMilliseconds(1000), () => { from.EndAction(typeof(BaseInstrument)); });

                double musicSkill = from.Skills[SkillName.Musicianship].Value;

                // [50 보너스] 악기를 착용(장비)하고 있으면 성공 확률 5% 증가
                double successChance = musicSkill / 100.0;
                if (this.Parent == from && this.Layer != Layer.Invalid)
                {
                    successChance += 0.05;
                }

                if (successChance > Utility.RandomDouble())
				{
					PlayInstrumentWell(from);
					from.CheckSkill(SkillName.Musicianship, 100.0); // 성공 시 100 상승
					// (마을 150/200 보너스 로직 등...)
				}
				else
				{
					PlayInstrumentBadly(from);
					from.CheckSkill(SkillName.Musicianship, 10.0); // 실패 시 10%인 10 상승
					// ...
				}

                if (successChance > Utility.RandomDouble())
                {
                    PlayInstrumentWell(from);

                    // [150 / 200 보너스] 마을 공연 (마을 안전 구역 내에서 발동)
                    if (musicSkill >= 150.0 && from.Region.IsPartOf(typeof(Server.Regions.GuardedRegion)))
                    {
                        // 200 보너스: 마을 공연 시 1% 특수 정보 획득
                        if (musicSkill >= 200.0 && 0.01 > Utility.RandomDouble())
                        {
                            from.SendMessage(0x59, "멋진 공연으로 주변 사람들의 이목을 끌어, 특별한 소문을 엿들었습니다!");
                        }
                        // 150 보너스: 마을 공연 시 소문 조작 기회
                        else if (0.05 > Utility.RandomDouble()) // 임의 발생 확률 5%로 설정
                        {
                            from.SendMessage(0x35, "성공적인 공연 덕분에 마을의 소문을 조작할 수 있는 기회를 얻었습니다.");
                        }
                    }

                    ConsumeUse(from);
                }
                else
                {
                    PlayInstrumentBadly(from);
                    ConsumeUse(from);
                }
            }
            else
            {
                from.SendLocalizedMessage(500119); // You must wait to perform another action
            }
        }

        // =========================================================
        // [수정] 기본 음악 성공 여부 판정 (외부 호출용)
        // =========================================================
        public static bool CheckMusicianship(Mobile m) 
        { 
            m.CheckSkill(SkillName.Musicianship, 0.0, 120.0);
            
            double chance = m.Skills[SkillName.Musicianship].Value / 100.0;

            // [50 보너스] 외부에서 CheckMusicianship 호출 시에도 장착 여부 확인
            Item item = m.FindItemOnLayer(Layer.TwoHanded);
            if (!(item is BaseInstrument)) item = m.FindItemOnLayer(Layer.OneHanded);
            
            if (item is BaseInstrument)
            {
                chance += 0.05;
            }

            return (chance > Utility.RandomDouble());
        }

        public void PlayInstrumentWell(Mobile from) { from.PlaySound(m_WellSound); }
        public void PlayInstrumentBadly(Mobile from) { from.PlaySound(m_BadlySound); }

        #region ICraftable Members
        public int OnCraft(
            int quality,
            bool makersMark,
            Mobile from,
            CraftSystem craftSystem,
            Type typeRes,
            ITool tool,
            CraftItem craftItem,
            int resHue)
        {
            Quality = (ItemQuality)quality;
            PlayerConstructed = true; 

            if (makersMark)
            {
                Crafter = from;
            }

            if (typeRes == null)
            {
                typeRes = craftItem.Resources.GetAt(0).ItemType;
            }

            if (Core.AOS)
            {
                if (!craftItem.ForceNonExceptional)
                {
                    Resource = CraftResources.GetFromType(typeRes);
                }

                if (from is PlayerMobile pm)
                {
                    if (this.Layer == Layer.TwoHanded)
                    {
                        double bonus = from.Skills[craftSystem.MainSkill].Value + from.Skills.ArmsLore.Value * 0.2;
                        
                        if (Quality == ItemQuality.Exceptional)
                        {
                            bonus += 50;
                        }
                        
                        int rank = ItemOptionCreator.ItemCreator(this, bonus, pm);

                        if (Quality == ItemQuality.Exceptional)
                            pm.CheckSkill(SkillName.ArmsLore, 1500 + rank * 250);
                        else
                            pm.CheckSkill(SkillName.ArmsLore, 500 + rank * 250);
                    }
                }
            }
            else if (!craftItem.ForceNonExceptional)
            {
                Resource = CraftResources.GetFromType(typeRes);
            }

            if (craftItem != null && !craftItem.ForceNonExceptional)
            {
                CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

                if (resInfo == null)
                {
                    return quality;
                }
            }

            return quality;
        }
        #endregion
    }
}