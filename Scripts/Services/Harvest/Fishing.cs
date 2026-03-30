using System;
using System.Collections.Generic;
using Server.Engines.Quests;
using Server.Engines.Quests.Collector;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System.Linq;

namespace Server.Engines.Harvest
{
    public class Fishing : HarvestSystem
    {
        private static Fishing m_System;

        public static Fishing System => m_System ??= new Fishing();

        private readonly HarvestDefinition m_Definition;

        public HarvestDefinition Definition => this.m_Definition;

        private Fishing()
        {
            #region Fishing
            HarvestDefinition fish = new HarvestDefinition();

            // Resource banks are every 8x8 tiles
            fish.BankWidth = 64;
            fish.BankHeight = 64;

            // Every bank holds from 5 to 15 fish
            fish.MinTotal = 150;
            fish.MaxTotal = 250;

            // A resource bank will respawn its content every 10 to 20 minutes
            fish.MinRespawn = TimeSpan.FromMinutes(600.0);
            fish.MaxRespawn = TimeSpan.FromMinutes(1200.0);

            // Skill checking is done on the Fishing skill
            fish.Skill = SkillName.Fishing;

            // Set the list of harvestable tiles
            fish.Tiles = m_WaterTiles;
            fish.RangedTiles = true;

            // Players must be within 4 tiles to harvest
            fish.MaxRange = 4;

            // One fish per harvest action
            fish.ConsumedPerHarvest = 5;

            // 🌟 [공통 1] 기본 애니메이션 루프를 3회로 고정
            fish.EffectActions = new int[] { Core.SA ? 12 : 12 };
            fish.EffectSounds = new int[0];
            fish.EffectCounts = new int[] { 3 }; // 기본 3회 루프
            
            fish.EffectDelay = TimeSpan.FromSeconds(8.0);
            fish.EffectSoundDelay = TimeSpan.FromSeconds(8.0);

            fish.NoResourcesMessage = 503172; // The fish don't seem to be biting here.
            fish.FailMessage = 503171; // You fish a while, but fail to catch anything.
            fish.TimedOutOfRangeMessage = 500976; // You need to be closer to the water to fish!
            fish.OutOfRangeMessage = 500976; // You need to be closer to the water to fish!
            fish.PackFullMessage = 503176; // You do not have room in your backpack for a fish.
            fish.ToolBrokeMessage = 503174; // You broke your fishing pole.

            this.m_Definition = fish;
            this.Definitions.Add(fish);
            #endregion
        }

        public override void OnConcurrentHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            from.SendLocalizedMessage(500972); // You are already fishing.
        }

        private class MutateEntry
        {
            public double m_MinSkill, m_MaxSkill;
            public bool m_DeepWater;
            public Type m_Type;

            public MutateEntry(double minSkill, double maxSkill, bool deepWater, Type type)
            {
                m_MinSkill = minSkill;
                m_MaxSkill = maxSkill;
                m_DeepWater = deepWater;
                m_Type = type;
            }
        }

        private static readonly MutateEntry[] m_MutateTable = new MutateEntry[]
        {
            new MutateEntry( 0.0, 50.0,  false, typeof( Trout ) ),
            new MutateEntry( 20.0, 70.0, false, typeof( Bass ) ),
            new MutateEntry( 40.0, 90.0, false, typeof( Shiner ) ),
            new MutateEntry( 60.0,  110.0, false, typeof( CrucianCarp ) ),
            new MutateEntry( 80.0,  130.0, false, typeof( CatFish ) ),
            new MutateEntry( 100.0,  150.0,  true, typeof( CodFish ) ),
            new MutateEntry( 120.0,  170.0,  true, typeof( PerchFish ) )            
        };

        // 🌟 튜플을 반환하는 코어 방식 적용 (Bank 삭제)
        public override (Type Type, double Chance, double SkillMax, bool Fail) MutateType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, object toHarvest)
        {
            double skillBase = from.Skills[SkillName.Fishing].Base;
            double skillValue = from.Skills[SkillName.Fishing].Value;

            bool deepWater = IsDeepWater(loc, map);
            
            int count = 0;
            for (int i = m_MutateTable.Length - 1; i >= 1; --i)
            {
                int maxchance = Misc.Util.upgradechance[i];
                if( from is PlayerMobile pm )
                {
                    maxchance = Misc.Util.ExpHarvestBonus( pm, maxchance );
                }
                
                // 심해 고기가 배열 뒤쪽에 있으므로, 얕은 물이면 해당 고기들을 스킵
                if (!deepWater && m_MutateTable[i].m_DeepWater)
                    continue;

                if (skillValue >= m_MutateTable[i].m_MinSkill && Utility.RandomMinMax(1, 10000) <= maxchance)
                {
                    count = i;
                    break;
                }
            }

            MutateEntry entry = m_MutateTable[count];
            Type upgrade = entry.m_Type;
            
            bool failcheck = (count > 0 && Utility.RandomMinMax(0, count * 2) != 0);
            double point = entry.m_MaxSkill + entry.m_MinSkill;
            double chance = 1 + (skillValue - entry.m_MaxSkill) * 0.02;

            // 🌟 낚시 대회용 Big Fish 드랍 체크 (스킬 80 이상, 심해)
            if (deepWater && skillValue >= 80.0 && Utility.RandomDouble() < 0.02) // 2% 확률
            {
                upgrade = typeof(BigFish);
            }

            return (upgrade, chance, point, failcheck);
        }

        private bool IsDeepWater(Point3D p, Map map)
        {
            return Items.SpecialFishingNet.ValidateDeepWater(map, p.X, p.Y) && (map == Map.Trammel || map == Map.Felucca || map == Map.Tokuno);
        }

        public override bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            Container pack = from.Backpack;

            if (pack != null)
            {
                List<SOS> messages = pack.FindItemsByType<SOS>();

                for (int i = 0; i < messages.Count; ++i)
                {
                    SOS sos = messages[i];
                    if ((from.Map == Map.Felucca || from.Map == Map.Trammel) && from.InRange(sos.TargetLocation, 60))
                        return true; // SOS 병이 있으면 자원이 부족해도 무조건 낚시 가능
                }
            }

            return base.CheckResources(from, tool, def, map, loc, timed);
        }

        public override Item Construct(Type type, Mobile from, Item tool)
        {
            if (type == typeof(BaseWeapon))
                return null;

            if (type == typeof(TreasureMap))
            {
                int level = (from is PlayerMobile pm && pm.Young && from.Map == Map.Trammel && TreasureMap.IsInHavenIsland(from)) ? 0 : 1;
                return new TreasureMap(level, from.Map == Map.Felucca ? Map.Felucca : Map.Trammel);
            }
            else if (type == typeof(BlackPearl))
            {
                return new BlackPearl(Utility.RandomMinMax(10, 20));
            }
            else if (type == typeof(MessageInABottle))
            {
                return new MessageInABottle(from.Map == Map.Felucca ? Map.Felucca : Map.Trammel);
            }
            else if (type == typeof(WhitePearl))
            {
                return new WhitePearl();
            }

            // 🌟 SOS 보물 인양 체크
            Container pack = from.Backpack;
            if (pack != null)
            {
                List<SOS> messages = pack.FindItemsByType<SOS>();
                for (int i = 0; i < messages.Count; ++i)
                {
                    SOS sos = messages[i];
                    if ((from.Map == Map.Felucca || from.Map == Map.Trammel) && from.InRange(sos.TargetLocation, 60))
                    {
                        // 보물 상자 인양 (내용물 구성은 TreasureMapChest.Fill 등 별도 클래스에서 처리 권장)
                        LockableContainer chest = (0.01 > Utility.RandomDouble()) ? new ShipsStrongbox(sos.Level) : 
                                                  (Utility.RandomBool() ? new MetalGoldenChest() : new WoodenChest());

                        if (sos.IsAncient) chest.Hue = 0x481;

                        // TODO: 보물(트레져박스) 코드와 결합 시 이곳의 Fill 로직을 해당 시스템으로 이관하세요.
                        TreasureMapChest.Fill(chest, from is PlayerMobile pm ? pm.RealLuck : from.Luck, Math.Max(1, Math.Min(4, sos.Level)), true, from.Map);
                        sos.OnSOSComplete(chest);

                        chest.DropItem(sos.IsAncient ? new FabledFishingNet() : new SpecialFishingNet());

                        chest.Movable = true;
                        chest.Locked = false;
                        chest.TrapType = TrapType.None;
                        chest.TrapPower = 0;
                        chest.TrapLevel = 0;
                        chest.IsShipwreckedItem = true;

                        sos.Delete();
                        return chest;
                    }
                }
            }

            return base.Construct(type, from, tool);
        }

        public override bool Give(Mobile m, Item item, bool placeAtFeet)
        {
            if (item is TreasureMap || item is MessageInABottle || item is SpecialFishingNet)
            {
                BaseCreature serp = (0.25 > Utility.RandomDouble()) ? new DeepSeaSerpent() : new SeaSerpent();
                int x = m.X, y = m.Y;
                Map map = m.Map;

                for (int i = 0; map != null && i < 20; ++i)
                {
                    int tx = m.X - 10 + Utility.Random(21);
                    int ty = m.Y - 10 + Utility.Random(21);

                    LandTile t = map.Tiles.GetLandTile(tx, ty);

                    if (t.Z == -5 && ((t.ID >= 0xA8 && t.ID <= 0xAB) || (t.ID >= 0x136 && t.ID <= 0x137)) && !Spells.SpellHelper.CheckMulti(new Point3D(tx, ty, -5), map))
                    {
                        x = tx;
                        y = ty;
                        break;
                    }
                }

                serp.MoveToWorld(new Point3D(x, y, -5), map);
                serp.Home = serp.Location;
                serp.RangeHome = 10;
                serp.PackItem(item);

                m.SendLocalizedMessage(503170); // Uh oh! That doesn't look like a fish!
                return true; // we don't want to give the item to the player, it's on the serpent
            }

            if (item is BigFish || item is WoodenChest || item is MetalGoldenChest)
                placeAtFeet = true;

            return base.Give(m, item, placeAtFeet);
        }

        public override void SendSuccessTo(Mobile from, Item item, Type resourceType)
        {
            if (item is BigFish bigFish)
            {
                from.SendLocalizedMessage(1042635); // Your fishing pole bends as you pull a big fish from the depths!

                // 🌟 낚시 대회용 데이터 자동 세팅
                bigFish.Fisher = from;
                bigFish.DateCaught = DateTime.Now;
                bigFish.Weight = Math.Max(1, 200 - (int)Math.Sqrt(Utility.RandomMinMax(0, 40000)));
            }
            else if (item is WoodenChest || item is MetalGoldenChest)
            {
                from.SendLocalizedMessage(503175); // You pull up a heavy chest from the depths of the ocean!
            }
            else if (item != null)
            {
                int number = 1043297;
                string name;

                if ((item.ItemData.Flags & TileFlag.ArticleA) != 0)
                    name = "a " + item.ItemData.Name;
                else if ((item.ItemData.Flags & TileFlag.ArticleAn) != 0)
                    name = "an " + item.ItemData.Name;
                else
                    name = item.ItemData.Name;

                NetState ns = from.NetState;
                if (ns == null) return;

                if (number == 1043297 || ns.HighSeas)
                    from.SendLocalizedMessage(number, name);
                else
                    from.SendLocalizedMessage(number, true, name);
            }
        }

        public override bool BeginHarvesting(Mobile from, Item tool)
        {
            if (!base.BeginHarvesting(from, tool))
                return false;

            from.SendLocalizedMessage(500974); // What water do you want to fish in?
            return true;
        }

        public override bool CheckHarvest(Mobile from, Item tool)
        {
            if (!base.CheckHarvest(from, tool))
                return false;

            // 🌟 낚시대(착용 필수) vs 투망/통발(미착용 가능) 구분
            if (tool is FishingPole && tool.Parent != from)
            {
                from.SendMessage("낚시대는 착용해야만 사용할 수 있습니다.");
                return false;
            }
            
            // TODO: 투망이나 통발 아이템을 추가할 경우 아래 주석을 해제하여 사용
            /*
            if ((tool is CastingNet || tool is CrabTrap) && tool.Parent != from.Backpack)
            {
                from.SendMessage("해당 도구는 가방 안에 있어야만 사용할 수 있습니다.");
                return false;
            }
            */

            if (from.Mounted || from.Flying)
            {
                from.SendLocalizedMessage(500971); // You can't fish while riding!
                return false;
            }
            return true;
        }

        public override bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            if (!base.CheckHarvest(from, tool, def, toHarvest))
                return false;

            if (from.Mounted || from.Flying)
            {
                from.SendLocalizedMessage(500971); // You can't fish while riding!
                return false;
            }

            return true;
        }

        private static readonly int[] m_WaterTiles = new int[]
        {
            0x00A8, 0x00AB,
            0x0136, 0x0137,
            0x5797, 0x579C,
            0x746E, 0x7485,
            0x7490, 0x74AB,
            0x74B5, 0x75D5
        };
    }
}