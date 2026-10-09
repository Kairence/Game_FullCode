using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;
using System.Collections.Generic;

namespace Server.Misc
{
    public class FarmBuilderGump : Gump
    {
        private PrivateFarmAddon m_Addon;

        public FarmBuilderGump(PrivateFarmAddon addon) : base(50, 50)
        {
            m_Addon = addon;
            AddPage(0);
            
            AddBackground(0, 0, 380, 420, 9270);
            AddAlphaRegion(10, 10, 360, 400);

            AddLabel(110, 20, 1152, "개인 농장 제어기 (" + addon.FarmSize + "x" + addon.FarmSize + ")");
            AddHtml(20, 50, 340, 60, "<color=white>농장 부지(말뚝 내부)를 클릭하여 원하는 시설을 설치하세요. 타일 위에는 스킬에 맞는 씨앗을 심을 수 있습니다.</color>", false, false);

            int y = 110;
            
            AddButton(30, y, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 906, "밭 타일 (Dirt) 설치");

            AddButton(200, y, 4005, 4007, 4, GumpButtonType.Reply, 0);
            AddLabel(235, y + 2, 906, "허수아비 (Scarecrow) [7x7]");

            y += 30;
            AddButton(30, y, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 906, "양봉통 (Beehive) [1x1]");

            AddButton(200, y, 4005, 4007, 5, GumpButtonType.Reply, 0);
            AddLabel(235, y + 2, 906, "물통 (Water Barrel) [5x5]");

            y += 30;
            AddButton(30, y, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 906, "과수원 (Orchard) [3x3]");

            AddButton(200, y, 4005, 4007, 6, GumpButtonType.Reply, 0);
            AddLabel(235, y + 2, 906, "화로 (Heating Stand) [5x5]");

            y += 30;
            AddButton(30, y, 4005, 4007, 7, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 906, "방역통 (Potion Keg) [5x5]");

            y += 50;
            int storageCount = addon.StorageData != null ? addon.StorageData.Count : 0;
            AddButton(30, y, 4029, 4031, 10, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 167, "보관함 열기 (보관된 시설: " + storageCount + "개)");

            y += 40;
            AddButton(30, y, 4020, 4022, 11, GumpButtonType.Reply, 0);
            AddLabel(65, y + 2, 33, "농장 철거 (Pack Up)");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (m_Addon == null || m_Addon.Deleted) return;
            if (from != m_Addon.MobileOwner && from.AccessLevel < AccessLevel.GameMaster) return;

            if (info.ButtonID >= 1 && info.ButtonID <= 7)
            {
                string msg = info.ButtonID == 1 ? "밭 타일" : info.ButtonID == 2 ? "양봉통" : info.ButtonID == 3 ? "과수원" : info.ButtonID == 4 ? "허수아비" : info.ButtonID == 5 ? "물통" : info.ButtonID == 6 ? "화로" : "방역통";
                from.SendMessage(68, msg + "을(를) 설치할 바닥을 선택하세요.");
                from.Target = new FarmDeployTarget(m_Addon, info.ButtonID);
            }
            else if (info.ButtonID == 10) // 보관함
            {
                from.SendGump(new FarmStorageGump(m_Addon));
            }
            else if (info.ButtonID == 11) // 철거
            {
                if (m_Addon.StorageData == null) m_Addon.StorageData = new List<FarmStorageData>();
                m_Addon.StorageData.Clear();
                
                foreach (AddonComponent comp in m_Addon.Components)
                {
                    if (comp is FarmOrchardComponent orchard)
                        m_Addon.StorageData.Add(new FarmStorageData("FarmOrchardComponent", orchard.NextHarvest));
                    else if (comp is FarmBeehiveComponent bee)
                        m_Addon.StorageData.Add(new FarmStorageData("FarmBeehiveComponent", bee.NextHarvest));
                    else if (comp is FarmScarecrowComponent)
                        m_Addon.StorageData.Add(new FarmStorageData("FarmScarecrowComponent", DateTime.MinValue));
                    else if (comp is FarmWaterBarrelComponent)
                        m_Addon.StorageData.Add(new FarmStorageData("FarmWaterBarrelComponent", DateTime.MinValue));
                    else if (comp is FarmHeatingStandComponent)
                        m_Addon.StorageData.Add(new FarmStorageData("FarmHeatingStandComponent", DateTime.MinValue));
                    else if (comp is FarmPestControlKegComponent keg)
                    {
                        var kData = new FarmStorageData("FarmPestControlKegComponent", DateTime.MinValue);
                        kData.CustomData1 = keg.Charges;
                        m_Addon.StorageData.Add(kData);
                    }
                    else if (comp is FarmGrowingCropComponent crop)
                    {
                        var cData = new FarmStorageData("FarmGrowingCropComponent", crop.NextGrowthTime);
                        cData.CustomData1 = crop.GrowthStage;
                        cData.CustomString1 = crop.SeedType;
                        m_Addon.StorageData.Add(cData);
                    }
                }

                from.SendMessage(33, "농장이 안전하게 철거되었습니다. 보관함에 " + m_Addon.StorageData.Count + "개의 시설이 저장되었습니다.");
                
                int size = m_Addon.FarmSize;
                PrivateFarmDeed deed;
                if (size <= 6) deed = new SmallFarmDeed();
                else if (size <= 12) deed = new MediumFarmDeed();
                else deed = new LargeFarmDeed();
                
                deed.StorageData = new List<FarmStorageData>(m_Addon.StorageData);
                from.AddToBackpack(deed);
                m_Addon.Delete();
            }
        }
    }

    public class FarmStorageGump : Gump
    {
        private PrivateFarmAddon m_Addon;

        public FarmStorageGump(PrivateFarmAddon addon) : base(150, 150)
        {
            m_Addon = addon;
            AddPage(0);
            AddBackground(0, 0, 400, 400, 9270);
            AddAlphaRegion(10, 10, 380, 380);

            AddLabel(130, 20, 1152, "농장 보관함 (Storage)");
            AddLabel(20, 50, 167, "철거 시 안전하게 보존된 시설들입니다.");
            
            if (addon.StorageData == null || addon.StorageData.Count == 0)
            {
                AddLabel(20, 90, 33, "보관된 시설물이 없습니다.");
            }
            else
            {
                int y = 90;
                for (int i = 0; i < addon.StorageData.Count; i++)
                {
                    var data = addon.StorageData[i];
                    string name = data.ComponentType == "FarmOrchardComponent" ? "과수원 (3x3)" : 
                                  data.ComponentType == "FarmBeehiveComponent" ? "양봉통 (1x1)" : 
                                  data.ComponentType == "FarmScarecrowComponent" ? "허수아비 (7x7)" : 
                                  data.ComponentType == "FarmWaterBarrelComponent" ? "물통 (5x5)" : 
                                  data.ComponentType == "FarmHeatingStandComponent" ? "화로 (5x5)" : data.ComponentType == "FarmPestControlKegComponent" ? "방역통 (5x5)" : data.ComponentType == "FarmGrowingCropComponent" ? "심어진 작물 (1x1)" : data.ComponentType;
                    
                    AddLabel(20, y, 906, $"- {name}");
                    AddButton(200, y, 4005, 4007, 100 + i, GumpButtonType.Reply, 0);
                    AddLabel(235, y + 2, 68, "꺼내기 (배치)");
                    
                    y += 30;
                    if (y > 340) { AddLabel(20, y, 1152, "... 그 외 다수"); break; }
                }
            }
            
            AddButton(20, 360, 4014, 4016, 1, GumpButtonType.Reply, 0);
            AddLabel(55, 362, 1152, "뒤로 가기");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (m_Addon == null || m_Addon.Deleted) return;

            if (info.ButtonID == 1)
            {
                from.SendGump(new FarmBuilderGump(m_Addon));
            }
            else if (info.ButtonID >= 100)
            {
                int index = info.ButtonID - 100;
                if (m_Addon.StorageData != null && index >= 0 && index < m_Addon.StorageData.Count)
                {
                    from.SendMessage(68, "해당 시설을 다시 배치할 바닥을 선택하세요.");
                    from.Target = new FarmDeployTarget(m_Addon, 1000 + index);
                }
            }
        }
    }

    public class FarmDeployTarget : Target
    {
        private PrivateFarmAddon m_Addon;
        private int m_Type;

        public FarmDeployTarget(PrivateFarmAddon addon, int type) : base(12, true, TargetFlags.None)
        {
            m_Addon = addon;
            m_Type = type;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (m_Addon == null || m_Addon.Deleted) return;

            IPoint3D p = targeted as IPoint3D;
            if (p == null) return;

            Point3D loc = new Point3D(p);

            int minX = m_Addon.X;
            int minY = m_Addon.Y;
            int maxX = m_Addon.X + m_Addon.FarmSize - 1;
            int maxY = m_Addon.Y + m_Addon.FarmSize - 1;

            if (loc.X < minX || loc.X > maxX || loc.Y < minY || loc.Y > maxY)
            {
                from.SendMessage(33, "농장 허가 구역(제어기 반경 " + m_Addon.FarmSize + "칸 이내)을 벗어났습니다. 경계선 안쪽에 설치해주세요.");
                return;
            }

            foreach (AddonComponent comp in m_Addon.Components)
            {
                if (comp is FarmSurveyorPeg || comp is FarmControlComponent) continue;
                if (comp.X == (loc.X - m_Addon.X) && comp.Y == (loc.Y - m_Addon.Y))
                {
                    from.SendMessage(33, "해당 위치에 이미 농장 시설물이 존재합니다.");
                    return;
                }
            }

            int spawnType = m_Type;
            FarmStorageData restoreData = null;

            if (m_Type >= 1000) // Storage Redeploy
            {
                int index = m_Type - 1000;
                if (m_Addon.StorageData == null || index < 0 || index >= m_Addon.StorageData.Count) return;

                restoreData = m_Addon.StorageData[index];
                
                if (restoreData.ComponentType == "FarmOrchardComponent") spawnType = 3;
                else if (restoreData.ComponentType == "FarmBeehiveComponent") spawnType = 2;
                else if (restoreData.ComponentType == "FarmScarecrowComponent") spawnType = 4;
                else if (restoreData.ComponentType == "FarmWaterBarrelComponent") spawnType = 5;
                else if (restoreData.ComponentType == "FarmHeatingStandComponent") spawnType = 6;
                else if (restoreData.ComponentType == "FarmPestControlKegComponent") spawnType = 7;
                else if (restoreData.ComponentType == "FarmGrowingCropComponent") spawnType = 8;
                else spawnType = 1;
            }

            if (spawnType == 3 && (loc.X - 1 < minX || loc.X + 1 > maxX || loc.Y - 1 < minY || loc.Y + 1 > maxY))
            {
                from.SendMessage(33, "과수원은 3x3 공간을 차지합니다. 농장 경계선을 벗어날 수 없습니다.");
                return;
            }

            AddonComponent newComp = null;

            if (spawnType == 1) newComp = new FarmPloughedComponent();
            else if (spawnType == 2) 
            {
                newComp = new FarmBeehiveComponent();
                if (restoreData != null) ((FarmBeehiveComponent)newComp).NextHarvest = restoreData.NextHarvestOrFeed;
            }
            else if (spawnType == 3)
            {
                newComp = new FarmOrchardComponent();
                if (restoreData != null) ((FarmOrchardComponent)newComp).NextHarvest = restoreData.NextHarvestOrFeed;
            }
            else if (spawnType == 4) newComp = new FarmScarecrowComponent();
            else if (spawnType == 5) newComp = new FarmWaterBarrelComponent();
            else if (spawnType == 6) newComp = new FarmHeatingStandComponent();
            else if (spawnType == 7)
            {
                newComp = new FarmPestControlKegComponent();
                if (restoreData != null) ((FarmPestControlKegComponent)newComp).Charges = restoreData.CustomData1;
            }
            else if (spawnType == 8)
            {
                newComp = new FarmGrowingCropComponent(restoreData != null && !string.IsNullOrEmpty(restoreData.CustomString1) ? restoreData.CustomString1 : "Wheat");
                if (restoreData != null)
                {
                    ((FarmGrowingCropComponent)newComp).NextGrowthTime = restoreData.NextHarvestOrFeed;
                    ((FarmGrowingCropComponent)newComp).GrowthStage = restoreData.CustomData1 - 1;
                    ((FarmGrowingCropComponent)newComp).AdvanceStage();
                }
            }

            if (newComp != null)
            {
                m_Addon.AddComponent(newComp, loc.X - m_Addon.X, loc.Y - m_Addon.Y, 0);
                
                if (restoreData != null)
                {
                    m_Addon.StorageData.Remove(restoreData);
                    from.SendMessage(68, "보관함에서 시설을 성공적으로 꺼냈습니다. (기존 데이터 복원 완료)");
                    from.SendGump(new FarmStorageGump(m_Addon));
                }
                else
                {
                    from.SendMessage(68, "성공적으로 설치되었습니다.");
                    from.SendGump(new FarmBuilderGump(m_Addon));
                }
            }
        }
    }
}
