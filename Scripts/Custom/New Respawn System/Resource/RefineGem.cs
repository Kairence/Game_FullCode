using System;
using System.Collections.Generic;
using Server.Targeting;
using Server.Engines.Craft;
using Server.Misc;

namespace Server.Items
{
    public class RefineGem : Item, ICraftable
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int GemIndex { get; set; }
        
        [CommandProperty(AccessLevel.GameMaster)]
        public int TierValue { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string CrafterName { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxBans { get; set; }

        // ItemOptionCreator.cs 컴파일 에러 방지용 (데이터는 비워둠)
        public List<int> ExcludedIDs { get; set; } = new List<int>();

        [Constructable]
        public RefineGem() : this(0, 40) { }

        [Constructable]
        public RefineGem(int gemIndex, int tierValue) : base(0x1EA7)
        {
            GemIndex = gemIndex;
            TierValue = tierValue;
            Weight = 1.0;
            Stackable = true;
            UpdateProperties();
        }

        public void UpdateProperties()
        {
            // [유저 원본 유지] 희귀, 영웅, 서사, 전설, 신화
            string tierName = TierValue switch 
            { 
                <= 40 => "희귀", 
                50 => "영웅", 
                60 => "서사", 
                80 => "전설", 
                _ => "신화" 
            };
            
            Name = $"{tierName} {Server.SkillHandlers.Imbuing.GemNames[GemIndex]}";
            Hue = 1150 + GemIndex;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!string.IsNullOrEmpty(CrafterName))
            {
                list.Add(1050043, CrafterName); // 명장 각인
            }

            if (ExcludedIDs.Count > 0)
            {
                string banList = "";
                for (int i = 0; i < ExcludedIDs.Count; i++)
                {
                    string optName = Server.Misc.ClilocData.GetString(Misc.ItemOptionCreator.GetCliloc(ExcludedIDs[i])).Replace("~1_val~", "").Replace("~1_VAL~", "").Trim();
                    banList += optName;
                    if (i < ExcludedIDs.Count - 1) banList += ", ";
                }
                list.Add(1070722, $"<BASEFONT COLOR=#FF0000>[제외 옵션: {banList}]</BASEFONT>");
            }
            else if (MaxBans > 0)
            {
                list.Add(1070722, $"<BASEFONT COLOR=#777777>[제외 슬롯 {MaxBans}개 - 미설정]</BASEFONT>");
            }
            else
            {
                list.Add(1070722, $"<BASEFONT COLOR=#777777>[필터링 불가]</BASEFONT>");
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack)) return;
            
            from.SendMessage("보석을 박을 장비를 선택하세요.");
            from.Target = new InternalTarget(this); 
        }

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            if (makersMark)
            {
                CrafterName = from.Name;
            }

            double minSkill = craftItem.Skills.Count > 0 ? craftItem.Skills.GetAt(0).MinSkill : 0.0;
            
            TierValue = minSkill switch
            {
                >= 160.0 => 100,
                >= 120.0 => 80,
                >= 80.0  => 60,
                >= 40.0  => 50,
                _        => 40
            };

            // GemIndex 결정 (재료 타입 기반)
            Type actualResType = craftItem.Resources.Count > 0 ? craftItem.Resources.GetAt(0).ItemType : null;
            if (actualResType == typeof(StarSapphire)) GemIndex = 0;
            else if (actualResType == typeof(Emerald)) GemIndex = 1;
            else if (actualResType == typeof(Sapphire)) GemIndex = 2;
            else if (actualResType == typeof(Ruby)) GemIndex = 3;
            else if (actualResType == typeof(Citrine)) GemIndex = 4;
            else if (actualResType == typeof(Amethyst)) GemIndex = 5;
            else if (actualResType == typeof(Tourmaline)) GemIndex = 6;
            else if (actualResType == typeof(Amber)) GemIndex = 7;
            else if (actualResType == typeof(Diamond)) GemIndex = 8;
            else GemIndex = 0;

            // [핵심] 스킬에 따라 필터링 "가능한 슬롯" 개수 할당
            MaxBans = (int)(from.Skills[SkillName.Imbuing].Value / 50.0);
            if (MaxBans > 4) MaxBans = 4;
            
            // 밴 로직 복구: 유저의 필터 세팅 불러와서 보석에 자동 주입
            var profile = Server.Misc.RefineFilterSystem.GetProfile(from);
            if (profile.TryGetValue(GemIndex, out var banned))
            {
                for (int i = 0; i < Math.Min(banned.Count, MaxBans); i++)
                {
                    ExcludedIDs.Add(banned[i]);
                }
            }

            UpdateProperties();
            return 1;
        }

        private class InternalTarget : Target
        {
            private RefineGem m_Gem;
            public InternalTarget(RefineGem gem) : base(2, false, TargetFlags.None) { m_Gem = gem; }
            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Item item) 
                    ItemOptionCreator.ApplyGemRefinement(from, item, m_Gem);
                else if (targeted is Server.Mobiles.BaseCreature pet)
                    ItemOptionCreator.ApplyPetGemRefinement(from, pet, m_Gem);
                else 
                    from.SendMessage("대상이 장비나 펫이 아닙니다.");
            }
        }

        public RefineGem(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) 
        {
            base.Serialize(writer);
            writer.Write(6); 
            writer.Write(CrafterName == null ? "" : CrafterName);
            writer.Write(GemIndex);
            writer.Write(TierValue);
            writer.Write(MaxBans);
            writer.Write(ExcludedIDs.Count);
            foreach (int id in ExcludedIDs) writer.Write(id);
        }

        public override void Deserialize(GenericReader reader) 
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (version >= 5)
            {
                CrafterName = reader.ReadString();
                GemIndex = reader.ReadInt();
                TierValue = reader.ReadInt();
                MaxBans = reader.ReadInt();
            }
            ExcludedIDs = new List<int>();
            if (version >= 6)
            {
                int count = reader.ReadInt();
                for (int i = 0; i < count; i++) ExcludedIDs.Add(reader.ReadInt());
            }
            UpdateProperties();
        }
    }
}