using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Regions;

namespace Server.Misc
{
    // ==============================================================================
    // [모험가의 무덤 (Corpse to Chest) 시스템]
    // ==============================================================================
    public class AdventurerChestManager
    {
        public static void ProcessAdventurerDeath(Mobile dead, Point3D loc, Map map, int carryGold)
        {
            if (map == null || map == Map.Internal) return;

            RegionCode code = RegionSaver.GetRegionCode(map, loc.X, loc.Y, loc.Z);
            
            // 1. 던전 구역인지 확인
            if (!DungeonManager.Zones.TryGetValue(code, out DungeonZone zone)) return;

            // 🌟 [최적화 적용] 월드 전체 스캔(O(W)) 대신 ChestMonitor의 명부(Registry) 스캔(O(1)) 사용
            int maxAllowed = Math.Max(2, zone.MaxPopulation / 5); 
            
            var activeChests = DungeonChestMonitor.GetValidChests(code);
            int currentChestCount = activeChests.Count;
            LockableContainer nearestChest = null;
            double nearestDist = 9999.0;

            foreach (var chest in activeChests)
            {
                if (chest.Map == map)
                {
                    double dist = Utility.GetDistanceToSqrt(loc, chest.Location);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestChest = chest;
                    }
                }
            }

            // 3. 분기 A: 아직 상자 여유가 있고, 근처(10타일 내)에 다른 상자가 없을 때 신규 생성
            if (currentChestCount < maxAllowed && nearestDist > 10.0)
            {
                WoodenChest chest = new WoodenChest();
                chest.Movable = false;
                chest.Locked = true;
                chest.RequiredSkill = 30; // 기본 자물쇠 난이도
                chest.LockLevel = 30;
                
                chest.DropItem(new Gold(carryGold));
                
                // 모험가가 들고 있던 물약/붕대 등 일부 유품 추가 (LINQ 배제 최적화)
                if (dead != null && dead.Backpack != null)
                {
                    Pouch supplyBag = new Pouch { Name = "Supplies" };
                    int added = 0;
                    for (int i = 0; i < dead.Backpack.Items.Count; i++)
                    {
                        Item it = dead.Backpack.Items[i];
                        if (it is BasePotion || it is Bandage || it is BaseReagent)
                        {
                            supplyBag.DropItem(it);
                            added++;
                            if (added >= 5) break;
                        }
                    }
                    if (supplyBag.Items.Count > 0) chest.DropItem(supplyBag);
                    else supplyBag.Delete();
                }

                chest.MoveToWorld(loc, map);
                
                // 🌟 명부에 방금 생성한 신규 상자 등록 (모니터링 연동)
                DungeonChestMonitor.RegisterChest(code, chest);
            }
            // 4. 분기 B: 포화 상태 (기존 상자 잭팟 업그레이드)
            else if (nearestChest != null)
            {
                nearestChest.RequiredSkill = Math.Min(120, nearestChest.RequiredSkill + 15);
                nearestChest.LockLevel = nearestChest.RequiredSkill;
                
                // 함정 레벨업
                if (nearestChest.TrapType == TrapType.None) nearestChest.TrapType = TrapType.ExplosionTrap;
                else nearestChest.TrapPower = Math.Min(100, nearestChest.TrapPower + 20);

                // 유품(골드) 누적
                nearestChest.DropItem(new Gold(carryGold));
                
                // 모험가 유품 일부 추가 (LINQ 배제 최적화)
                if (dead != null && dead.Backpack != null)
                {
                    int added = 0;
                    for (int i = 0; i < dead.Backpack.Items.Count; i++)
                    {
                        Item it = dead.Backpack.Items[i];
                        if (it is BasePotion || it is Bandage || it is BaseReagent)
                        {
                            nearestChest.DropItem(it);
                            added++;
                            if (added >= 3) break;
                        }
                    }
                }
            }
        }
    }
}