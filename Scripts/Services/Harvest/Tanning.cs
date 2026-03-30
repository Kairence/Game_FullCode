using System;
using Server.Items;
using Server.Mobiles;
using Server.Misc;
using Server.Engines.Quests;

namespace Server.Engines.Harvest
{
    public class Tanning : HarvestSystem
    {
        private static Tanning m_System;
        public static Tanning System => m_System ??= new Tanning();

        public HarvestDefinition Definition { get; }

        private Tanning()
        {
            HarvestDefinition def = new HarvestDefinition
            {
                BankWidth = 1,
                BankHeight = 1,
                MinTotal = 1,
                MaxTotal = 1,
                MinRespawn = TimeSpan.Zero,
                MaxRespawn = TimeSpan.Zero,
                
                Skill = SkillName.TasteID, // 🌟 모토 스킬: 맛감정
                
                MaxRange = 3, // 시체 타겟 거리
                ConsumedPerHarvest = 1,
                ConsumedPerFeluccaHarvest = 1,

                // 채집 애니메이션 및 딜레이 설정 (광산과 유사한 딜레이)
                EffectActions = new int[] { Core.SA ? 12 : 32 }, // 칼질 애니메이션
                EffectSounds = new int[] { 0x248 }, // 사각사각 고기/가죽 자르는 소리
                EffectCounts = new int[] { 1 },
                EffectDelay = TimeSpan.FromSeconds(1.5),
                EffectSoundDelay = TimeSpan.FromSeconds(0.5),

                NoResourcesMessage = 500485, // You see nothing useful to carve from the corpse.
                FailMessage = 500485,
                TimedOutOfRangeMessage = 500446, // That is too far away.
                OutOfRangeMessage = 500446,
                PackFullMessage = 500720, // You don't have room for this.
                ToolBrokeMessage = 1044038 // You have worn out your tool!
            };

            Definitions.Add(def);
            Definition = def;
        }

        // [핵심] out 키워드를 삭제하고 Tuple 반환 형태로 변경
		public override (bool Success, int TileID, Map Map, Point3D Loc) GetHarvestDetails(Mobile from, Item tool, object toHarvest)
		{
			if (toHarvest is Corpse corpse && !corpse.Deleted && !corpse.Carved && corpse.Owner is BaseCreature)
			{
				Map map = corpse.Map;
				return (map != null && map != Map.Internal, 1, map, corpse.GetWorldLocation());
			}

			// 바뀐 부모 함수 호출 (인수 3개)
			return base.GetHarvestDetails(from, tool, toHarvest);
		}

        public override bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            if (toHarvest is Corpse corpse && corpse.Carved)
            {
                from.SendMessage("이미 갈무리된 시체입니다.");
                return false;
            }
            return base.CheckHarvest(from, tool, def, toHarvest);
        }

        // 🌟 [핵심] 채집 타이머가 끝난 후 아이템이 들어오는 최종 로직 (기존 OnCarve 완전 이식)
        public override void FinishHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked)
        {
            from.EndAction(locked);

            if (!CheckHarvest(from, tool)) return;

            if (toHarvest is Corpse corpse && !corpse.Deleted && !corpse.Carved && corpse.Owner is BaseCreature bc)
            {
                if (bc.Summoned || bc.IsBonded || corpse.Animated)
                {
                    from.SendMessage(bc.Summoned ? "소환한 몬스터는 얻을 아이템이 없습니다." : "갈무리할 수 없는 시체입니다.");
                    return;
                }

                int feathers = bc.Feathers;
                int meat = bc.Meat;
                int hides = bc.Hides;
                int scales = bc.Scales;
                int fame = bc.Fame;

                if (feathers == 0 && meat == 0 && hides == 0 && scales == 0)
                {
                    from.SendLocalizedMessage(500485); // You see nothing useful to carve from the corpse.
                    return;
                }

                bool skinning = (tool is SkinningKnife || tool is ButcherKnife);
                double tasteID = 0;

                // 🌟 [생태계 연동] 해당 구역의 Tanning 자원이 고갈되었는지 확인
                bool ecoDepleted = false;
                string zoneName = NewSpawnManager.GetGoGumpZoneName(corpse.Location, corpse.Map);
                ResourceKey ecoKey = new ResourceKey(corpse.Map.Name, zoneName, ResourceType.Tanning);
                
                if (ResourceManager.Pools.TryGetValue(ecoKey, out ResourcePool pool))
                {
                    if (pool.CurrentCapacity <= 0 || DateTime.Now < pool.DepletionCooldown)
                    {
                        ecoDepleted = true;
                        from.SendMessage(33, "이 구역의 생태계가 훼손되어 질 좋은 자원을 얻기 힘듭니다.");
                    }
                    else
                    {
                        pool.ConsumeResource(typeof(Hides)); // 자원 1 소모
                    }
                }

                new Blood(0x122D).MoveToWorld(corpse.Location, corpse.Map);

                // --- 1. 고기(Meat) 로직 ---
                if (meat != 0)
                {
                    Item m = null;
                    if (skinning)
                    {
                        if (bc.MeatType == MeatType.Bird) { m = new RawBird(meat); tasteID += meat * 200; }
                        else if (bc.MeatType == MeatType.LambLeg) { m = new RawLambLeg(meat); tasteID += meat * 100; }
                        else if (bc.MeatType == MeatType.Ribs) { m = new RawRibs(meat); tasteID += meat * 10; }

                        if (m != null)
                        {
                            if (!from.AddToBackpack(m)) corpse.AddCarvedItem(m, from);
                            else from.SendLocalizedMessage(1114101); 
                        }
                    }
                    else if (bc.MeatType == MeatType.Ribs || bc.MeatType == MeatType.Rotworm)
                    {
                        if (bc.MeatType == MeatType.Ribs) m = new RawRibs(Math.Max(1, meat / 2));
                        else if (bc.MeatType == MeatType.Rotworm) m = new RawRotwormMeat(meat);

                        if (!from.AddToBackpack(m)) corpse.AddCarvedItem(m, from);
                        else from.SendLocalizedMessage(1114101);
                    }
                    else from.SendMessage("양고기와 새고기는 푸줏간 칼 혹은 피복칼로 다듬어야 할 것 같습니다.");
                }

                // --- 2. 깃털(Feathers) 로직 ---
                if (feathers != 0)
                {
                    if (skinning) tasteID += feathers;
                    else feathers = Math.Max(1, feathers / 2);

                    Item feather = new Feather(feathers);
                    if (!from.AddToBackpack(feather)) corpse.AddCarvedItem(feather, from);
                    else from.SendLocalizedMessage(1114097);
                }

                // --- 3. 가죽(Hides) 로직 ---
                if (hides != 0)
                {
                    corpse.ItemID = 6928;
                    Item leather = null;

                    int temp_Fame = bc.Tamable ? (int)bc.MinTameSkill * 200 : bc.Fame;
                    int loot_grade = temp_Fame < 2000 ? 0 : temp_Fame < 5000 ? 1 : temp_Fame < 8000 ? 2 : temp_Fame < 11000 ? 3 : temp_Fame < 15000 ? 4 : temp_Fame < 19000 ? 5 : 6;
                    
                    int lootPlus = 0; // 유저님 서버의 m_Grade/m_Boss 접근 가능 시 치환
                    // if (bc.m_Boss) { lootPlus = 5; loot_grade++; } else lootPlus = CreatureBalancer.MonsterGrade(bc.m_Grade) - 1;
                    
                    if (loot_grade > 6) { loot_grade = 6; lootPlus++; }
                    
                    // 🌟 생태계 고갈 페널티: 고갈 지역에서는 등급 강등 및 획득량 반토막
                    if (ecoDepleted) 
                    {
                        loot_grade = Math.Max(0, loot_grade - 2);
                        hides /= 2;
                    }

                    hides = (hides + lootPlus + 5) * 2;
                    if (Core.ML && from.Race == Race.Human) hides = (int)Math.Ceiling(hides * 1.1);

                    int point = loot_grade * 20 + 80;
                    if (from.Skills.TasteID.Value >= 100 && ((from.Skills.TasteID.Value + 50 - point) * 0.01 > Utility.RandomDouble())) hides++;

                    if (from is PlayerMobile pm)
                    {
                        int maxchance = Misc.Util.ExpHarvestBonus(pm, Misc.Util.upgradechance[loot_grade]);

                        if (skinning)
                        {
                            double chance = 1 + (from.Skills.TasteID.Value - loot_grade) * 0.02;
                            if (chance >= Utility.RandomDouble())
                            {
                                if (Utility.RandomMinMax(1, 10000) > maxchance)
                                {
                                    from.SendMessage("스킬이 부족하여 좋은 가죽을 얻는데 실패합니다.");
                                    loot_grade = 0;
                                }
                                else from.SendMessage("가죽을 얻는데 성공합니다.");

                                tasteID += hides * 30; 
                                leather = loot_grade switch
                                {
                                    1 => new DernedHides(hides),
                                    2 => new RatnedHides(hides),
                                    3 => new SernedHides(hides),
                                    4 => new SpinedHides(hides),
                                    5 => new HornedHides(hides),
                                    6 => new BarbedHides(hides),
                                    _ => new Hides(hides)
                                };

                                double skillpoint = (loot_grade * 20) * 2 + 50;
                                Server.Misc.Util.SavingAccountPoint(pm, 21, 1);
                                Server.Misc.Util.SavingAccountPoint(pm, 22 + loot_grade, 1);
                                tasteID = skillpoint * hides;
                                QuestHelper.CheckHarvest(pm, leather);
                            }
                            else { from.SendMessage("무두질 스킬이 부족합니다."); hides = 0; }
                        }
                        else
                        {
                            hides = Math.Max(1, hides / 2);
                            leather = new Hides(hides);
                        }

                        if ((int)tasteID > 0) pm.Getgoldpoint((int)tasteID);
                        from.CheckSkill(SkillName.TasteID, tasteID);
                    }

                    if (hides > 0 && leather != null)
                    {
                        if (!from.AddToBackpack(leather)) corpse.AddCarvedItem(leather, from);
                        else from.SendLocalizedMessage(1073555);
                    }
                }

                // --- 4. 비늘(Scales) 로직 ---
                if (scales != 0)
                {
                    Item scaleItem = bc.ScaleType switch
                    {
                        ScaleType.Red => new RedScales(scales),
                        ScaleType.Yellow => new YellowScales(scales),
                        ScaleType.Black => new BlackScales(scales),
                        ScaleType.Green => new GreenScales(scales),
                        ScaleType.White => new WhiteScales(scales),
                        ScaleType.Blue => new BlueScales(scales),
                        _ => new RedScales(scales)
                    };
                    corpse.AddCarvedItem(scaleItem, from);
                    from.SendLocalizedMessage(1079284);
                }

                corpse.Carved = true;
                if (corpse.IsCriminalAction(from)) from.CriminalAction(true);
                if (corpse.TotalItems == 0) corpse.Delete();

                // 도구 내구도 감소
                if (tool is IUsesRemaining usesTool)
                {
                    usesTool.ShowUsesRemaining = true;
                    if (usesTool.UsesRemaining > 0) usesTool.UsesRemaining--;
                    if (usesTool.UsesRemaining < 1)
                    {
                        tool.Delete();
                        from.SendLocalizedMessage(1044038); // You have worn out your tool!
                    }
                }
            }
        }
    }
}