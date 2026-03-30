using System;
using Server.Items;
using Server.Mobiles;
using System.Linq;
using System.Collections.Generic;

namespace Server.Engines.Harvest
{
    public class Farming : HarvestSystem
    {
        private static Farming m_System;
        public static Farming System => m_System ??= new Farming();

        private Farming()
        {
            HarvestDefinition farm = new HarvestDefinition();

            farm.Skill = SkillName.Herding; // 🌟 모토 스킬: 목동(Herding)
            farm.MaxRange = 2; // 작물을 캘 수 있는 최대 거리 (제자리에서 2칸 이내)

            // 🌟 채집 애니메이션 및 딜레이 설정
            farm.EffectActions = new int[] { 32 }; // 허리를 숙여 바닥을 긁어내는 애니메이션
            farm.EffectSounds = new int[] { 0x125, 0x126 }; // 흙 파는/사각거리는 소리
            farm.EffectCounts = new int[] { 2 }; // 농사는 가벼우니 2회 루프
            farm.EffectDelay = TimeSpan.FromSeconds(1.0);
            farm.EffectSoundDelay = TimeSpan.FromSeconds(0.5);

            farm.FailMessage = 500446; // That is too far away.
            farm.OutOfRangeMessage = 500446; // That is too far away.
            farm.PackFullMessage = 503176; // 가방이 꽉 찼습니다.
            farm.ToolBrokeMessage = 1044038; // You have worn out your tool!

            Definitions.Add(farm);
        }

        // 🌟 [핵심] 타겟팅 대상을 지형(Tile)이 아닌 작물(BaseFarmItem)로 강제 우회
        public override (bool Success, int TileID, Map Map, Point3D Loc) GetHarvestDetails(Mobile from, Item tool, object toHarvest)
        {
            if (toHarvest is BaseFarmItem crop && !crop.Deleted)
            {
                return (true, crop.ItemID, crop.Map, crop.GetWorldLocation());
            }
            return (false, 0, null, Point3D.Zero);
        }

        public override bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            if (!base.CheckHarvest(from, tool, def, toHarvest)) return false;

            // 1. 도구 검사 (쇠스랑이나 괭이만 허용)
            // (유저님의 서버 환경에 맞춰 Pitchfork 클래스명 확인 요망)
            if (!(tool is Pitchfork) && tool.GetType().Name != "Hoe")
            {
                from.SendMessage("농작물은 쇠스랑(Pitchfork)이나 괭이(Hoe)를 이용해야 수확할 수 있습니다.");
                return false;
            }

            // 2. 작물 상태 검사
            if (toHarvest is BaseFarmItem crop)
            {
                if (crop.Owner != from && from.AccessLevel == AccessLevel.Player)
                {
                    from.SendMessage("당신의 작물이 아닙니다.");
                    return false;
                }
                if (crop.Stage == CropStage.Decaying)
                {
                    from.SendMessage("이미 부패하여 수확할 수 없습니다.");
                    return false;
                }
                if (crop.Stage != CropStage.Harvestable)
                {
                    crop.CheckStatus(from); // 아직 덜 자란 경우 남은 시간 출력
                    return false;
                }
                return true;
            }
            return false;
        }

        // 🌟 [핵심] 수확 보상 및 제자리 Loop 시스템
        public override void FinishHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked)
        {
            from.EndAction(locked);
            if (!CheckHarvest(from, tool, def, toHarvest)) return;

            if (toHarvest is BaseFarmItem crop && !crop.Deleted)
            {
                // 기존 BaseFarmItem.cs의 수확량 계산 로직 (Herding 스킬 및 풍년/흉년) 호출
                int amount = crop.CalculateYield(from);

                if (amount <= 0)
                {
                    from.SendMessage(33, "흉년입니다... 작물을 건질 수 없었습니다.");
                    crop.Delete();
                }
                else
                {
                    // 🌟 기존 crop.Harvest(from) 대신 여기서 아이템을 생성하여 지급합니다.
                    // (crop.Harvest() 안에 있던 Animate 효과가 HarvestSystem과 겹치기 때문)
                    Item harvestItem = Activator.CreateInstance(crop.ResultType, amount) as Item;
                    if (harvestItem != null)
                    {
                        if (from.PlaceInBackpack(harvestItem))
                        {
                            from.SendMessage(68, $"대풍년! 당신은 작물을 {amount}개 수확합니다!");
                        }
                        else
                        {
                            harvestItem.MoveToWorld(from.Location, from.Map);
                            from.SendMessage(33, "가방이 꽉 차서 작물이 바닥에 떨어졌습니다!");
                        }
                    }
                    crop.Delete();
                }

                // 배고픔 깎기
                if (from is PlayerMobile pmHunger)
                {
                    pmHunger.MacroCheck += 10;
                    from.Hunger -= 30; // 농사는 가벼우므로 30만 깎음
                    pmHunger.LastTarget = tool;
                }

                // 도구 내구도 소모
                if (tool is IUsesRemaining toolWithUses)
                {
                    toolWithUses.ShowUsesRemaining = true;
                    if (toolWithUses.UsesRemaining > 0) toolWithUses.UsesRemaining--;
                    if (toolWithUses.UsesRemaining < 1)
                    {
                        tool.Delete();
                        def.SendMessageTo(from, def.ToolBrokeMessage);
                        if (from is PlayerMobile p) p.Loop = false;
                    }
                }

                // 🌟 [제자리 순차 채취 Loop 시스템]
                if (from is PlayerMobile pm && pm.Loop && tool != null && !tool.Deleted)
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(1.0), () =>
                    {
                        if (!pm.Alive || tool == null || tool.Deleted) return;

                        BaseFarmItem nextCrop = null;

                        // 내 주변 2칸 이내에 있는 '수확 가능한 내 작물' 스캔
                        IPooledEnumerable eable = pm.Map.GetItemsInRange(pm.Location, 2);
                        foreach (Item item in eable)
                        {
                            if (item is BaseFarmItem farmItem && farmItem.Owner == pm && farmItem.Stage == CropStage.Harvestable && !farmItem.Deleted)
                            {
                                nextCrop = farmItem;
                                break; // 하나 찾으면 즉시 중단 (순차 채취)
                            }
                        }
                        eable.Free();

                        if (nextCrop != null)
                        {
                            // 이동하지 않고 제자리에서 다음 작물에 낫질 시작
                            StartHarvesting(pm, tool, nextCrop);
                        }
                        else
                        {
                            pm.SendMessage(68, "주변 반경에 더 이상 수확할 작물이 없습니다. 자동 수확을 종료합니다.");
                            pm.Loop = false;
                        }
                    });
                }
            }
        }
    }
}