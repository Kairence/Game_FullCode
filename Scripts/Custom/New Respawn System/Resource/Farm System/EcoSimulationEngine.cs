using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Multis;
using Server.Mobiles; // VirtualCitizen
using Server.Misc;    // If VirtualCitizen is here

namespace Server.Misc
{
    public enum SimulationSystemType
    {
        None = 0,
        Farming,
        Fishing,
        Mining,
        Retail,
        Lumberjacking,
        Husbandry
    }

    public class EnvironmentAuraState
    {
        public bool HasWater;
        public bool HasHeat;
        public bool IsProtected;
        public bool IsPestControlled;
        public bool IsIndoor;
    }

    public static class EcoSimulationEngine
    {
        public static List<BaseAddon> ActiveSimulations = new List<BaseAddon>();

        public static void Register(BaseAddon addon)
        {
            if (addon != null && !ActiveSimulations.Contains(addon))
            {
                ActiveSimulations.Add(addon);
            }
        }

        public static void Unregister(BaseAddon addon)
        {
            if (addon != null && ActiveSimulations.Contains(addon))
            {
                ActiveSimulations.Remove(addon);
            }
        }

        public static void ProcessGlobalTick()
        {
            foreach (var sim in new List<BaseAddon>(ActiveSimulations))
            {
                if (sim == null || sim.Deleted) { ActiveSimulations.Remove(sim); continue; }

                if (sim is PrivateFarmAddon farm)
                {
                    FarmWaterBarrelComponent water = null;
                    FarmPestControlKegComponent keg = null;
                    bool hasScarecrow = false;
                    List<FarmGrowingCropComponent> crops = new List<FarmGrowingCropComponent>();

                    foreach (AddonComponent c in farm.Components)
                    {
                        if (c is FarmWaterBarrelComponent w) water = w;
                        else if (c is FarmPestControlKegComponent k) keg = k;
                        else if (c.GetType().Name == "FarmScarecrowComponent") hasScarecrow = true;
                        else if (c is FarmGrowingCropComponent crop) crops.Add(crop);
                    }

                    foreach (var crop in crops)
                    {
                        if (crop.GrowthStage >= 2) continue; // 이미 다 자람
                        if (DateTime.Now >= crop.NextGrowthTime)
                        {
                            if (crop.GrowthStage == 0) // 새싹 -> 성장기
                            {
                                crop.GrowthStage = 1;
                                crop.NextGrowthTime = DateTime.Now + TimeSpan.FromHours(10.0);
                                crop.Name = "성장 중인 작물";
                                crop.ItemID = 0x0C56;

                                if (water != null && water.Charges > 0) {
                                    water.Charges--;
                                    crop.NextGrowthTime = DateTime.Now + TimeSpan.FromHours(7.5);
                                }
                            }
                            else if (crop.GrowthStage == 1) // 성장기 -> 수확 가능
                            {
                                crop.GrowthStage = 2;
                                crop.Name = "다 자란 작물 (수확 가능)";
                                crop.ItemID = crop.SeedType != null && crop.SeedType.Contains("Carrot") ? 0x0C76 : (crop.SeedType != null && crop.SeedType.Contains("Cotton") ? 0x0C53 : 0x0C5A);
                                
                                if (keg != null && keg.Charges > 0) {
                                    keg.Charges--;
                                    if (keg.PotionType == 2) // 독
                                    {
                                        crop.PoisonBonus = keg.PotionTier; // 수확 시 보너스
                                    }
                                }
                            }
                        }
                        else if (crop.GrowthStage == 1 && keg != null && keg.Charges > 0 && keg.PotionType == 1) // 활력 포션 남은 시간 단축
                        {
                            keg.Charges--;
                            crop.NextGrowthTime -= TimeSpan.FromHours(0.5 * keg.PotionTier);
                        }
                    }
                }
            }
        }

        public static void ExecuteAIWork(Server.Misc.VirtualCitizen agent, SimulationSystemType jobType)
        {
            if (agent == null || agent.House == null) return;

            if (jobType == SimulationSystemType.Farming)
            {
                foreach (var sim in ActiveSimulations)
                {
                    if (sim is PrivateFarmAddon farm && farm.MobileOwner != null && farm.MobileOwner != null)
                    {
                        farm.JobBoardLaborerAssigned(agent);
                        return;
                    }
                }
            }
        }
    }
}
