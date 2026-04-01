using System;
using System.Collections.Generic;
using Server.Items;
using Server.Targeting;
using Server.Engines.Quests;
using Server.Engines.Quests.Hag;
using Server.Mobiles;
using System.Linq;

namespace Server.Engines.Harvest
{
    public abstract class HarvestSystem
    {
        public static void Configure()
        {
            EventSink.TargetByResourceMacro += TargetByResource;
        }

        private readonly List<HarvestDefinition> m_Definitions;
		public Dictionary<Mobile, (Type Type, double Chance, double SkillMax, bool Fail)> PreRolledHarvest = new();
        public HarvestSystem()
        {
            m_Definitions = new List<HarvestDefinition>();
        }

        public List<HarvestDefinition> Definitions => m_Definitions;

        public virtual bool CheckTool(Mobile from, Item tool)
        {
            bool wornOut = (tool == null || tool.Deleted || (tool is IUsesRemaining remaining && remaining.UsesRemaining <= 0));

            if (wornOut)
                from.SendLocalizedMessage(1044038); // You have worn out your tool!

            return !wornOut;
        }

        public virtual bool CheckHarvest(Mobile from, Item tool)
        {
            return CheckTool(from, tool);
        }

        public virtual bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            // 배고픔 체크
            int hunger = def.Skill == SkillName.Fishing ? 100 : 50;
            
            if (from.Hunger < hunger)
            {
                from.SendMessage("배고픈 상태에서는 자원 채취를 할 수 없습니다!");
                if (from is PlayerMobile pm)
                    pm.Loop = false;
                return false;
            }
            return CheckTool(from, tool);
        }

        public virtual bool CheckRange(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            bool inRange = (from.Map == map && from.InRange(loc, def.MaxRange));

            if (!inRange)
                def.SendMessageTo(from, timed ? def.TimedOutOfRangeMessage : def.OutOfRangeMessage);

            return inRange;
        }

        // 🌟 [수정] Bank 시스템 삭제. 이제 자식 클래스(Mining 등)에서 ResourceManager를 체크하도록 가상 함수로 둡니다.
        public virtual bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            return true; // 기본적으로 true 반환, 하위 클래스에서 고갈 여부 판단
        }

        public virtual void OnBadHarvestTarget(Mobile from, Item tool, object toHarvest) { }

        public virtual object GetLock(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            return tool; // 다중 채집 방지용 (도구 기준)
        }

        public virtual void OnConcurrentHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest) { }

        public virtual void OnHarvestStarted(Mobile from, Item tool, HarvestDefinition def, object toHarvest) { }

        public virtual bool BeginHarvesting(Mobile from, Item tool)
        {
            if (!CheckHarvest(from, tool))
                return false;

            EventSink.InvokeResourceHarvestAttempt(new ResourceHarvestAttemptEventArgs(from, tool, this));

            if (from is PlayerMobile pm && tool != null && pm.Loop && pm.LastObject != null)
            {
                StartHarvesting(from, tool, pm.LastObject);
                return true;
            }

            from.Target = new HarvestTarget(tool, this);
            return true;
        }

        // 🌟 [핵심] Bank/Vein을 제거하고 Loop 시스템을 장착한 새로운 피니시 로직
        public virtual void FinishHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked)
        {
            from.EndAction(locked);

            if (!CheckHarvest(from, tool)) return;

            // 🌟 튜플(Tuple) 사용
            var details = GetHarvestDetails(from, tool, toHarvest);
            
            if (!details.Success)
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }
            if (!def.Validate(details.TileID) && !def.ValidateSpecial(details.TileID))
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }
            if (!CheckRange(from, tool, def, details.Map, details.Loc, true)) return;
            if (!CheckResources(from, tool, def, details.Map, details.Loc, true)) return;
            if (!CheckHarvest(from, tool, def, toHarvest)) return;

            if (SpecialHarvest(from, tool, def, details.Map, details.Loc)) return;

            var mutate = (Type: (Type)null, Chance: 0.0, SkillMax: 0.0, Fail: false);
    
			if (PreRolledHarvest.ContainsKey(from))
			{
				mutate = PreRolledHarvest[from];
				PreRolledHarvest.Remove(from); // 사용 후 즉시 삭제 (메모리 관리)
			}
			else
			{
				// 혹시 모를 누락 방지용 안전장치
				mutate = MutateType(from, tool, def, details.Map, details.Loc, toHarvest);
			}
			
			int amount = 0;
            
            int skillpoint = def.ConsumedPerHarvest;
            double point = mutate.SkillMax;
            if (def.Skill == SkillName.Fishing) point *= 2;

            if (mutate.Chance > Utility.RandomDouble())
            {
                if (mutate.Type != null)
                {
                    Item item = Construct(mutate.Type, from, tool);
                    if (item != null)
                    {
                        amount = skillpoint;
                        if (from.Skills[def.Skill].Base >= 100) amount++;

                        if (from is PlayerMobile pm)
                        {
                            QuestHelper.CheckHarvest(pm, item);
                            if (pm.GoldPoint[4] > 0)
                                amount += (int)Math.Sqrt(Utility.RandomMinMax(0, pm.GoldPoint[4]));
                        }

                        Caddellite.OnHarvest(from, tool, this, item);

                        if (item.Stackable) item.Amount = amount;

                        if (amount <= 0)
                        {
                            def.SendMessageTo(from, def.FailMessage);
                            item.Delete();
                        }
                        else if (Give(from, item, def.PlaceAtFeetIfFull))
                        {
                            SendSuccessTo(from, item, mutate.Type);
                        }
                        else
                        {
                            SendPackFullTo(from, item, def, mutate.Type);
                            item.Delete();
                        }
                    }
                }
            }
            else
            {
                def.SendMessageTo(from, def.FailMessage);
            }

            from.CheckSkill(def.Skill, point * skillpoint);

            // 보상 및 포인트 처리
            if (from is PlayerMobile pmReward && amount > 0)
            {
                int getgoldpoint = (int)(point * amount);
                if (getgoldpoint > 0)
                {
                    pmReward.Getgoldpoint(getgoldpoint);

                    if (def.Skill == SkillName.Mining)
                    {
                        Server.Misc.Util.SavingAccountPoint(pmReward, 1, 1);
                        if (mutate.Type == typeof(Sand)) Server.Misc.Util.SavingAccountPoint(pmReward, 90, 1);
                        else Server.Misc.Util.SavingAccountPoint(pmReward, (int)(2 + (mutate.SkillMax - 50) / 40), 1);
                    }
                    else if (def.Skill == SkillName.Lumberjacking)
                    {
                        Server.Misc.Util.SavingAccountPoint(pmReward, 11, 1);
                        Server.Misc.Util.SavingAccountPoint(pmReward, (int)(12 + (mutate.SkillMax - 50) / 40), 1);
                    }
                    else if (def.Skill == SkillName.Fishing)
                    {
                        Server.Misc.Util.SavingAccountPoint(pmReward, 81, 1);
                        Server.Misc.Util.SavingAccountPoint(pmReward, (int)(82 + (mutate.SkillMax - 50) / 40), 1);
                    }
                }
            }

            // 도구 손상
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

            // 배고픔 깎기
            if (from is PlayerMobile pmHunger)
            {
                int hunger = def.Skill == SkillName.Fishing ? 200 : 100;
                pmHunger.MacroCheck += 10;
                from.Hunger -= hunger;
                pmHunger.LastTarget = tool;
            }

            OnHarvestFinished(from, tool, def, mutate.Type, toHarvest);

            // 🌟 [Loop 자동 채집 시스템] (자원이 고갈되거나 루프를 끄기 전까지 계속 채집)
            if (from is PlayerMobile loopPm && loopPm.Loop && tool != null && !tool.Deleted)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1.0), () =>
                {
                    if (loopPm.Alive && tool != null && !tool.Deleted)
                    {
                        StartHarvesting(loopPm, tool, toHarvest);
                    }
                });
            }
        }

        public virtual bool CheckHarvestSkill(Map map, Point3D loc, Mobile from, Type resourceType, HarvestDefinition def)
        {
            return true; // 하위 호환 및 자식 클래스에서 재정의
        }
		public virtual int GetHarvestAttemptCount(Mobile from, Item tool, Type resourceType)
        {
            // 기본 시도 횟수를 반환합니다. 필요 시 Mining 등 자식 클래스에서 오버라이드 하세요.
            return 1;
        }
        public virtual void OnToolUsed(Mobile from, Item tool, bool caughtSomething) { }

        public virtual void OnHarvestFinished(Mobile from, Item tool, HarvestDefinition def, Type resourceType, object harvested) { }

        public virtual bool SpecialHarvest(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc) { return false; }

        public virtual Item Construct(Type type, Mobile from, Item tool)
        {
            try { return Activator.CreateInstance(type) as Item; } catch { return null; }
        }

        public virtual void SendSuccessTo(Mobile from, Item item, Type resourceType) { }

        public virtual void SendPackFullTo(Mobile from, Item item, HarvestDefinition def, Type resourceType)
        {
            def.SendMessageTo(from, def.PackFullMessage);
        }

        public virtual bool Give(Mobile m, Item item, bool placeAtFeet)
        {
            if (m.PlaceInBackpack(item)) return true;
            if (!placeAtFeet) return false;

            Map map = m.Map;
            if (map == null || map == Map.Internal) return false;

            List<Item> atFeet = new List<Item>();
            IPooledEnumerable eable = m.GetItemsInRange(0);
            foreach (Item obj in eable) atFeet.Add(obj);
            eable.Free();

            for (int i = 0; i < atFeet.Count; ++i)
            {
                if (atFeet[i].StackWith(m, item, false)) return true;
            }

            ColUtility.Free(atFeet);
            item.MoveToWorld(m.Location, map);
            return true;
        }

        // 🌟 [수정] Bank/Vein이 없으므로 자식 클래스에서 이 튜플을 반환하여 드랍 확률 및 종류를 결정합니다.
        public virtual (Type Type, double Chance, double SkillMax, bool Fail) MutateType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, object toHarvest)
        {
            return (null, 0, 0, false);
        }

        public virtual bool OnHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked, bool last)
        {
            if (!CheckHarvest(from, tool))
            {
                from.EndAction(locked);
                return false;
            }

            var details = GetHarvestDetails(from, tool, toHarvest);

            if (!details.Success)
            {
                from.EndAction(locked);
                OnBadHarvestTarget(from, tool, toHarvest);
                return false;
            }
            else if (!def.Validate(details.TileID) && !def.ValidateSpecial(details.TileID))
            {
                from.EndAction(locked);
                OnBadHarvestTarget(from, tool, toHarvest);
                return false;
            }
            else if (!CheckRange(from, tool, def, details.Map, details.Loc, true))
            {
                from.EndAction(locked);
                return false;
            }
            else if (!CheckResources(from, tool, def, details.Map, details.Loc, true))
            {
                from.EndAction(locked);
                return false;
            }
            else if (!CheckHarvest(from, tool, def, toHarvest))
            {
                from.EndAction(locked);
                return false;
            }

            DoHarvestingEffect(from, tool, def, details.Map, details.Loc);
            new HarvestSoundTimer(from, tool, this, def, toHarvest, locked, last).Start();

            return !last;
        }

        public virtual void DoHarvestingSound(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            if (def.EffectSounds.Length > 0)
                from.PlaySound(Utility.RandomList(def.EffectSounds));
        }

        public virtual void DoHarvestingEffect(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc)
        {
            from.Direction = from.GetDirectionTo(loc);

            if (!from.Mounted)
            {
                if (Core.SA) from.Animate(AnimationType.Attack, Utility.RandomList(def.EffectActions));
                else from.Animate(Utility.RandomList(def.EffectActions), 5, 1, true, false, 0);
            }
        }

        public virtual HarvestDefinition GetDefinition(int tileID)
        {
            return GetDefinition(tileID, null);
        }

        public virtual HarvestDefinition GetDefinition(int tileID, Item tool)
        {
            HarvestDefinition def = null;

            for (int i = 0; def == null && i < m_Definitions.Count; ++i)
            {
                HarvestDefinition check = m_Definitions[i];
                if (check.Validate(tileID)) def = check;
            }
            return def;
        }

        #region High Seas
        public virtual HarvestDefinition GetDefinitionFromSpecialTile(int tileID)
        {
            HarvestDefinition def = null;

            for (int i = 0; def == null && i < m_Definitions.Count; ++i)
            {
                HarvestDefinition check = m_Definitions[i];
                if (check.ValidateSpecial(tileID)) def = check;
            }
            return def;
        }
        #endregion

        public virtual void StartHarvesting(Mobile from, Item tool, object toHarvest)
        {
            if (!CheckHarvest(from, tool)) return;

            var details = GetHarvestDetails(from, tool, toHarvest);

            if (!details.Success)
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }

            HarvestDefinition def = GetDefinition(details.TileID, tool);

            if (def == null)
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }

            if (!CheckRange(from, tool, def, details.Map, details.Loc, false)) return;
            if (!CheckResources(from, tool, def, details.Map, details.Loc, false)) return;
            if (!CheckHarvest(from, tool, def, toHarvest)) return;

            object toLock = GetLock(from, tool, def, toHarvest);

            if (!from.BeginAction(toLock))
            {
                OnConcurrentHarvest(from, tool, def, toHarvest);
                return;
            }

            // 🌟 [추가] 타이머 시작 전에 결과값을 미리 굴려서 딕셔너리에 저장
			var mutate = MutateType(from, tool, def, details.Map, details.Loc, toHarvest);
            PreRolledHarvest[from] = mutate;

			new HarvestTimer(from, tool, this, def, toHarvest, toLock).Start();
        }

        // 🌟 [핵심] out 키워드 삭제 및 Tuple 도입
        public virtual (bool Success, int TileID, Map Map, Point3D Loc) GetHarvestDetails(Mobile from, Item tool, object toHarvest)
        {
            int tileID = 0;
            Map map = null;
            Point3D loc = Point3D.Zero;

            if (toHarvest is Static objStatic && !objStatic.Movable)
            {
                tileID = (objStatic.ItemID & 0x3FFF) | 0x4000;
                map = objStatic.Map;
                loc = objStatic.GetWorldLocation();
            }
            else if (toHarvest is StaticTarget objStaticTarget)
            {
                tileID = (objStaticTarget.ItemID & 0x3FFF) | 0x4000;
                map = from.Map;
                loc = objStaticTarget.Location;
            }
            else if (toHarvest is LandTarget objLandTarget)
            {
                tileID = objLandTarget.TileID;
                map = from.Map;
                loc = objLandTarget.Location;
            }
            else
            {
                return (false, 0, null, Point3D.Zero);
            }

            return (map != null && map != Map.Internal, tileID, map, loc);
        }

        #region Enhanced Client
        public static void TargetByResource(TargetByResourceMacroEventArgs e)
        {
            Mobile m = e.Mobile;
            Item tool = e.Tool;

            HarvestSystem system = null;
            HarvestDefinition def = null;

            if (tool is IHarvestTool harvestTool)
            {
                system = harvestTool.HarvestSystem;
            }

            if (system != null)
            {
                switch (e.ResourceType)
                {
                    case 0: if (system is Mining mSystem) def = mSystem.OreAndStone; break;
                    case 1: if (system is Mining sSystem) def = sSystem.Sand; break;
                    case 2: if (system is Lumberjacking lSystem) def = lSystem.Definition; break;
                    case 3: if (TryHarvestGrave(m)) return; break;
                    case 4: if (TryHarvestShrooms(m)) return; break;
                }

                var tileSearch = FindValidTile(m, def);
                if (def != null && tileSearch.Success)
                {
                    system.StartHarvesting(m, tool, tileSearch.ToHarvest);
                    return;
                }

                system.OnBadHarvestTarget(m, tool, new LandTarget(new Point3D(0, 0, 0), Map.Felucca));
            }
        }

        // 🌟 [수정] out 삭제 및 Tuple 반환
        private static (bool Success, object ToHarvest) FindValidTile(Mobile m, HarvestDefinition definition)
        {
            Map map = m.Map;
            if (m == null || map == null || map == Map.Internal || definition == null)
                return (false, null);
            
            for (int x = m.X - 1; x <= m.X + 1; x++)
            {
                for (int y = m.Y - 1; y <= m.Y + 1; y++)
                {
                    StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, false);

                    if (tiles.Length > 0)
                    {
                        foreach (var tile in tiles)
                        {
                            int id = (tile.ID & 0x3FFF) | 0x4000;
                            if (definition.Validate(id))
                            {
                                return (true, new StaticTarget(new Point3D(x, y, tile.Z), tile.ID));
                            }
                        }
                    }

                    LandTile lt = map.Tiles.GetLandTile(x, y);
                    if (definition.Validate(lt.ID))
                    {
                        return (true, new LandTarget(new Point3D(x, y, lt.Z), map));
                    }
                }
            }
            return (false, null);
        }

        public static bool TryHarvestGrave(Mobile m)
        {
            Map map = m.Map;
            if (map == null) return false;

            for (int x = m.X - 1; x <= m.X + 1; x++)
            {
                for (int y = m.Y - 1; y <= m.Y + 1; y++)
                {
                    StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, false);
                    foreach (var tile in tiles)
                    {
                        int itemID = tile.ID;
                        if (itemID == 0xED3 || itemID == 0xEDF || itemID == 0xEE0 || itemID == 0xEE1 || itemID == 0xEE2 || itemID == 0xEE8)
                        {
                            if (m is PlayerMobile player && player.Quest is WitchApprenticeQuest qs)
                            {
                                if (qs.FindObjective(typeof(FindIngredientObjective)) is FindIngredientObjective obj && !obj.Completed && obj.Ingredient == Ingredient.Bones)
                                {
                                    player.SendLocalizedMessage(1055037); 
                                    obj.Complete();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool TryHarvestShrooms(Mobile m)
        {
            Map map = m.Map;
            if (map == null) return false;

            for (int x = m.X - 1; x <= m.X + 1; x++)
            {
                for (int y = m.Y - 1; y <= m.Y + 1; y++)
                {
                    StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, false);
                    foreach (var tile in tiles)
                    {
                        int itemID = tile.ID;
                        if (itemID == 0xD15 || itemID == 0xD16)
                        {
                            if (m is PlayerMobile player && player.Quest is WitchApprenticeQuest qs)
                            {
                                if (qs.FindObjective(typeof(FindIngredientObjective)) is FindIngredientObjective obj && !obj.Completed && obj.Ingredient == Ingredient.RedMushrooms)
                                {
                                    player.SendLocalizedMessage(1055036); 
                                    obj.Complete();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        #endregion
    }
}

namespace Server
{
    public interface IChopable
    {
        void OnChop(Mobile from);
    }

    public interface IHarvestTool : IEntity
    {
        Engines.Harvest.HarvestSystem HarvestSystem { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FurnitureAttribute : Attribute
    {
        public FurnitureAttribute() { }        

        private static bool IsNotChoppables(Item item)
        {
            return _NotChoppables.Any(t => t == item.GetType());
        }

        private static Type[] _NotChoppables = new Type[]
        {
            typeof(CommodityDeedBox), typeof(ChinaCabinet), typeof(PieSafe), typeof(AcademicBookCase), typeof(JewelryBox),
            typeof(WoodenBookcase), typeof(Countertop), typeof(Mailbox)
        };

        public static bool Check(Item item)
        {
            if (item == null || IsNotChoppables(item)) return false;

            if (item.GetType().IsDefined(typeof(FurnitureAttribute), false)) return true;

            if (item is AddonComponent component && component.Addon != null && component.Addon.GetType().IsDefined(typeof(FurnitureAttribute), false))
                return true;

            return false;
        }
    }
}