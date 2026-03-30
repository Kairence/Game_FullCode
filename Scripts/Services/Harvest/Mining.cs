using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System.Linq;
using Server.Misc; // ResourceManager 사용을 위해 추가

namespace Server.Engines.Harvest
{
    public class Mining : HarvestSystem
    {
        private static Mining m_System;

        public static Mining System => m_System ??= new Mining();

        private readonly HarvestDefinition m_OreAndStone;
        private readonly HarvestDefinition m_Sand;

        public HarvestDefinition OreAndStone => this.m_OreAndStone;
        public HarvestDefinition Sand => this.m_Sand;
        
        private Mining()
        {
            #region Mining for ore and stone
            HarvestDefinition oreAndStone = this.m_OreAndStone = new HarvestDefinition();

            oreAndStone.BankWidth = 8;
            oreAndStone.BankHeight = 8;

            oreAndStone.MinTotal = 200;
            oreAndStone.MaxTotal = 350;

            oreAndStone.MinRespawn = TimeSpan.FromMinutes(60.0);
            oreAndStone.MaxRespawn = TimeSpan.FromMinutes(180.0);

            oreAndStone.Skill = SkillName.Mining;
            oreAndStone.Tiles = m_MountainAndCaveTiles;
            oreAndStone.MaxRange = 1;
            oreAndStone.ConsumedPerHarvest = 5;

            // 🌟 [공통 1] 기본 애니메이션 루프를 3회로 고정
            // (자원 등급 및 스킬에 따른 동적 증감은 HarvestTimer에서 EffectCounts 길이를 조절하도록 연동해야 완벽합니다.)
            oreAndStone.EffectActions = new int[] { Core.SA ? 3 : 11 };
            oreAndStone.EffectSounds = new int[] { 0x125, 0x126 };
            oreAndStone.EffectCounts = new int[] { 3 }; 
            oreAndStone.EffectDelay = TimeSpan.FromSeconds(0.9);
            oreAndStone.EffectSoundDelay = TimeSpan.FromSeconds(1.6);

            oreAndStone.NoResourcesMessage = 503040; // There is no metal here to mine.
            oreAndStone.DoubleHarvestMessage = 503042; // Someone has gotten to the metal before you.
            oreAndStone.TimedOutOfRangeMessage = 503041; // You have moved too far away to continue mining.
            oreAndStone.OutOfRangeMessage = 500446; // That is too far away.
            oreAndStone.FailMessage = 503043; // You loosen some rocks but fail to find any useable ore.
            oreAndStone.PackFullMessage = 1010481; // Your backpack is full, so the ore you mined is lost.
            oreAndStone.ToolBrokeMessage = 1044038; // You have worn out your tool!

            oreAndStone.RaceBonus = Core.ML;
            oreAndStone.RandomizeVeins = Core.ML;

            this.Definitions.Add(oreAndStone);
            #endregion

            #region Mining for sand
            HarvestDefinition sand = this.m_Sand = new HarvestDefinition();

            sand.BankWidth = 8;
            sand.BankHeight = 8;
            sand.MinTotal = 6;
            sand.MaxTotal = 13;

            sand.MinRespawn = TimeSpan.FromMinutes(10.0);
            sand.MaxRespawn = TimeSpan.FromMinutes(20.0);

            sand.Skill = SkillName.Mining;
            sand.Tiles = m_SandTiles;
            sand.MaxRange = 1;
            sand.ConsumedPerHarvest = 5;

            sand.EffectActions = new int[] { Core.SA ? 3 : 11 };
            sand.EffectSounds = new int[] { 0x125, 0x126 };
            sand.EffectCounts = new int[] { 3 }; // 기본 3회
            sand.EffectDelay = TimeSpan.FromSeconds(0.9);
            sand.EffectSoundDelay = TimeSpan.FromSeconds(1.6);

            sand.NoResourcesMessage = 1044629; // There is no sand here to mine.
            sand.DoubleHarvestMessage = 1044629; // There is no sand here to mine.
            sand.TimedOutOfRangeMessage = 503041; // You have moved too far away to continue mining.
            sand.OutOfRangeMessage = 500446; // That is too far away.
            sand.FailMessage = 1044630; // You dig for a while but fail to find any of sufficient quality for glassblowing.
            sand.PackFullMessage = 1044632; // Your backpack can't hold the sand, and it is lost!
            sand.ToolBrokeMessage = 1044038; // You have worn out your tool!

            this.Definitions.Add(sand);
            #endregion
        }

        public override void SendSuccessTo(Mobile from, Item item, Type resourceType)
        {
            if (item is BaseGranite)
                from.SendLocalizedMessage(1044606); // You carefully extract some workable stone from the ore vein!
            else if (item is IGem)
                from.SendLocalizedMessage(1112233); // You carefully extract a glistening gem from the vein!
            else if (item != null)
            {
                // 광물 성공 메시지 전송
                if (resourceType == typeof(IronOre)) from.SendLocalizedMessage(1044530); // You loosen some rocks and put the ore in your backpack.
                else from.SendLocalizedMessage(1044530); // 기본 성공 메시지
            }
        }

        public override bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            if (HarvestMap.CheckMapOnHarvest(from, loc, def) == null)
                return base.CheckResources(from, tool, def, map, loc, timed);

            return true;
        }

        public override bool CheckHarvest(Mobile from, Item tool)
        {
            if (!base.CheckHarvest(from, tool))
                return false;

            if (from.IsBodyMod && !from.Body.IsHuman)
            {
                from.SendLocalizedMessage(501865); // You can't mine while polymorphed.
                return false;
            }

            return true;
        }

        public override bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            if (!base.CheckHarvest(from, tool, def, toHarvest))
                return false;

            bool boat = Server.Multis.BaseBoat.FindBoatAt(from, from.Map) != null;
            bool dungeon = IsDungeonRegion(from);
            
            // 🌟 [수정 2] Sand 마이닝 제한 해제 (스킬 100이나 책 읽은 여부 체크 삭제)
            
            if (from.Mounted)
            {
                from.SendLocalizedMessage(501864); // You can't mine while riding.
                return false;
            }
            else if (from.IsBodyMod && !from.Body.IsHuman)
            {
                from.SendLocalizedMessage(501865); // You can't mine while polymorphed.
                return false;
            }
            else if( boat )
            {
                from.SendMessage("배 안에서는 광물을 채취할 수 없습니다"); 
                return false;
            }
            else if( dungeon )
            {
                from.SendMessage("던전 안에서는 광물을 채취할 수 없습니다"); 
                return false;
            }
            return true;
        }

        private class MutateEntry
        {
            public double m_MinSkill, m_MaxSkill;
            public bool m_DeepForest;
            public Type m_Type;

            public MutateEntry(double minSkill, double maxSkill, bool deepForest, Type type)
            {
                m_MinSkill = minSkill;
                m_MaxSkill = maxSkill;
                m_DeepForest = deepForest;
                m_Type = type;
            }
        }

        private static readonly MutateEntry[] m_MutateTable = new MutateEntry[]
        {
            new MutateEntry( 0.0, 50.0,  false, typeof( IronOre ) ),
            new MutateEntry( 20.0, 70.0, false, typeof( CopperOre ) ),
            new MutateEntry( 40.0, 90.0, false, typeof( BronzeOre ) ),
            new MutateEntry( 60.0,  110.0, false, typeof( GoldOre ) ),
            new MutateEntry( 80.0,  130.0, false, typeof( AgapiteOre ) ),
            new MutateEntry( 100.0,  150.0,  false, typeof( VeriteOre ) ),
            new MutateEntry( 120.0,  170.0,  false, typeof( ValoriteOre ) )
        };

        // 🌟 [핵심] 리뉴얼된 MutateType (튜플 반환, Bank 사용 안함)
        public override (Type Type, double Chance, double SkillMax, bool Fail) MutateType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, object toHarvest)
        {
            double skillBase = from.Skills[SkillName.Mining].Base;
            double skillValue = from.Skills[SkillName.Mining].Value;
        
            if( def == m_Sand )
            {
                double chance = 1 + ( skillValue - 150 ) * 0.02;
                return (typeof(Sand), chance, 250, true);
            }
            else
            {
                int count = 0;
                for (int i = m_MutateTable.Length - 1; i >= 1; --i)
                {
                    int maxchance = Misc.Util.upgradechance[i];
                    if( from is PlayerMobile pm )
                    {
                        maxchance = Misc.Util.ExpHarvestBonus( pm, maxchance );
                    }
                    if (skillValue >= m_MutateTable[i].m_MinSkill && Utility.RandomMinMax(1, 10000) <= maxchance )
                    {
                        count = i;
                        break;
                    }
                }

                MutateEntry entry = m_MutateTable[count];
                Type upgrade = entry.m_Type;
                double point = entry.m_MaxSkill + entry.m_MinSkill;
                bool failcheck = (count > 0 && Utility.RandomMinMax(0, count * 2) != 0);
                double chance = 1 + (skillValue - entry.m_MaxSkill) * 0.02;

                // 🌟 [공통 1] 에니메이션 루프 수치 계산용 참고 로직 (타이머 연동을 위해 변수화 가능)
                // int animLoop = 3; 
                // animLoop += count; // 자원 등급 올라가면 +1씩
                // if (tool is GargoylesPickaxe) animLoop -= 1; // 고급 자원 툴이면 -1
                // if (skillBase >= 100.0) animLoop -= 1; // 스킬 100 이상이면 -1
                // animLoop = Math.Max(1, animLoop); // 1회 이하는 불가능

                return (upgrade, chance, point, failcheck);
            }
        }

        private static readonly int[] m_Offsets = new int[]
        {
            -1, -1, -1, 0, -1, 1, 0, -1,
            0, 1, 1, -1, 1, 0, 1, 1
        };

        // 🌟 [핵심] 튜플화 된 리소스 타입(resourceType)을 직접 받아 처리하는 OnHarvestFinished
        public override void OnHarvestFinished(Mobile from, Item tool, HarvestDefinition def, Type resourceType, object harvested)
        {
            if (def == this.m_OreAndStone)
            {
                // 🌟 [수정 3] 가고일 곡괭이로 광을 캘 때 1% 확률로 "동일 색상"의 대리석(Granite) 드랍
                if (tool is GargoylesPickaxe && Utility.RandomDouble() < 0.01)
                {
                    Type graniteType = GetGraniteType(resourceType);
                    if (graniteType != null)
                    {
                        Item granite = Construct(graniteType, from, tool);
                        if (granite != null)
                        {
                            from.SendLocalizedMessage(1044606); // You carefully extract some workable stone from the ore vein!
                            Give(from, granite, true);
                        }
                    }
                }
                // 🌟 [수정 4] 인테리어 물품(바위 등) 랜덤 드랍 예약 공간 (현재 비워둠)
                if (Utility.RandomDouble() < 0.05) // 확률 설정 예시 (5%)
                {
                    // TODO: 인테리어 물품(바위, 원석 등) 추가
                    // Item deco = new DecorativeRock();
                    // Give(from, deco, true);
                    // from.SendMessage("쓸만한 장식용 돌을 발견했습니다.");
                }
            }
        }

        // 광물(Ore) 타입에 맞는 대리석(Granite) 매칭 함수
        private Type GetGraniteType(Type oreType)
        {
            if (oreType == typeof(IronOre)) return typeof(Granite);
            if (oreType == typeof(CopperOre)) return typeof(CopperGranite);
            if (oreType == typeof(BronzeOre)) return typeof(BronzeGranite);
            if (oreType == typeof(GoldOre)) return typeof(GoldGranite);
            if (oreType == typeof(AgapiteOre)) return typeof(AgapiteGranite);
            if (oreType == typeof(VeriteOre)) return typeof(VeriteGranite);
            if (oreType == typeof(ValoriteOre)) return typeof(ValoriteGranite);
            return null;
        }

        #region High Seas
        public override bool SpecialHarvest(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc)
        {
            if (!Core.HS)
                return base.SpecialHarvest(from, tool, def, map, loc);

            bool boat = Server.Multis.BaseBoat.FindBoatAt(from, from.Map) != null;
            bool dungeon = IsDungeonRegion(from);

            if (!boat && !dungeon)
                return false;

            // 🌟 Bank를 사용하지 않으므로 ResourceManager에서 Niter 가능 여부 확인
            var poolKey = new ResourceKey(map.Name, NewSpawnManager.GetGoGumpZoneName(loc, map), ResourceType.Mining);
            
            // 기존 NiterDeposit.HasBeenChecked를 우회하거나 풀 상태로 체크
            if (boat || (ResourceManager.Pools.TryGetValue(poolKey, out var pool) && pool.CurrentCapacity > 0))
            {
                int luck = from is PlayerMobile pm ? pm.RealLuck : from.Luck;
                double bonus = (from.Skills[SkillName.Mining].Value / 9999) + ((double)luck / 150000);

                if (boat) bonus -= (bonus * .33);

                if (Utility.RandomDouble() < bonus)
                {
                    int size = Utility.RandomMinMax(1, 5);
                    if (luck / 2500.0 > Utility.RandomDouble()) size++;

                    NiterDeposit niter = new NiterDeposit(size);

                    if (!dungeon)
                    {
                        niter.MoveToWorld(new Point3D(loc.X, loc.Y, from.Z + 3), from.Map);
                        from.SendLocalizedMessage(1149918, niter.Size.ToString()); //You have uncovered a ~1_SIZE~ deposit of niter! Mine it to obtain saltpeter.
                        return true;
                    }
                    else
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            int x = Utility.RandomMinMax(loc.X - 2, loc.X + 2);
                            int y = Utility.RandomMinMax(loc.Y - 2, loc.Y + 2);
                            int z = from.Z;

                            if (from.Map.CanSpawnMobile(x, y, z))
                            {
                                niter.MoveToWorld(new Point3D(x, y, z), from.Map);
                                from.SendLocalizedMessage(1149918, niter.Size.ToString()); //You have uncovered a ~1_SIZE~ deposit of niter! Mine it to obtain saltpeter.
                                return true;
                            }
                        }
                    }
                    niter.Delete();
                }
            }
            return false;
        }

        private bool IsDungeonRegion(Mobile from)
        {
            if (from == null) return false;

            Map map = from.Map;
            Region reg = from.Region;
            Rectangle2D bounds = new Rectangle2D(0, 0, 5114, 4100);

            if ((map == Map.Felucca || map == Map.Trammel) && bounds.Contains(new Point2D(from.X, from.Y)))
                return false;

            return reg != null && (reg.IsPartOf<Server.Regions.DungeonRegion>() || map == Map.Ilshenar);
        }
        #endregion

        public override bool BeginHarvesting(Mobile from, Item tool)
        {
            if (!base.BeginHarvesting(from, tool))
                return false;

            from.SendLocalizedMessage(503033); // Where do you wish to dig?
            return true;
        }

        public override void OnHarvestStarted(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            base.OnHarvestStarted(from, tool, def, toHarvest);

            if (Core.ML)
                from.RevealingAction();
        }

        public override void OnBadHarvestTarget(Mobile from, Item tool, object toHarvest)
        {
            if (toHarvest is LandTarget)
            {
                from.SendLocalizedMessage(501862); // You can't mine there.
            }            
            else if (!(toHarvest is LandTarget))
            {
                from.SendLocalizedMessage(501863); // You can't mine that.
            }
            else if (from.Mounted || from.Flying)
            {
                from.SendLocalizedMessage(501864); // You can't dig while riding or flying.
            }
        }

        #region Tile lists
        private static readonly int[] m_MountainAndCaveTiles = new int[]
        {
            220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
            230, 231, 236, 237, 238, 239, 244, 245, 246, 247, 
            252, 253, 254, 255, 256, 257, 258, 259, 260, 261, 
            262, 263, 268, 269, 270, 271, 272, 273, 274, 275, 
            276, 277, 278, 279, 286, 287, 288, 289, 290, 291, 
            292, 293, 294, 296, 296, 297, 321, 322, 323, 324, 
            467, 468, 469, 470, 471, 472, 473, 474, 476, 477, 
            478, 479, 480, 481, 482, 483, 484, 485, 486, 487, 
            492, 493, 494, 495, 543, 544, 545, 546, 547, 548, 
            549, 550, 551, 552, 553, 554, 555, 556, 557, 558, 
            559, 560, 561, 562, 563, 564, 565, 566, 567, 568, 
            569, 570, 571, 572, 573, 574, 575, 576, 577, 578, 
            579, 581, 582, 583, 584, 585, 586, 587, 588, 589, 
            590, 591, 592, 593, 594, 595, 596, 597, 598, 599, 
            600, 601, 610, 611, 612, 613,
            1010, 1741, 1742, 1743, 1744, 1745, 1746, 1747, 1748, 1749,
            1750, 1751, 1752, 1753, 1754, 1755, 1756, 1757, 1771, 1772,
            1773, 1774, 1775, 1776, 1777, 1778, 1779, 1780, 1781, 1782,
            1783, 1784, 1785, 1786, 1787, 1788, 1789, 1790, 1801, 1802,
            1803, 1804, 1805, 1806, 1807, 1808, 1809, 1811, 1812, 1813,
            1814, 1815, 1816, 1817, 1818, 1819, 1820, 1821, 1822, 1823,
            1824, 1831, 1832, 1833, 1834, 1835, 1836, 1837, 1838, 1839,
            1840, 1841, 1842, 1843, 1844, 1845, 1846, 1847, 1848, 1849,
            1850, 1851, 1852, 1853, 1854, 1861, 1862, 1863, 1864, 1865,
            1866, 1867, 1868, 1869, 1870, 1871, 1872, 1873, 1874, 1875,
            1876, 1877, 1878, 1879, 1880, 1881, 1882, 1883, 1884, 1981,
            1982, 1983, 1984, 1985, 1986, 1987, 1988, 1989, 1990, 1991,
            1992, 1993, 1994, 1995, 1996, 1997, 1998, 1999, 2000, 2001,
            2002, 2003, 2004, 2028, 2029, 2030, 2031, 2032, 2033, 2100,
            2101, 2102, 2103, 2104, 2105,
            0x453B, 0x453C, 0x453D, 0x453E, 0x453F, 0x4540, 0x4541,
            0x4542, 0x4543, 0x4544, 0x4545, 0x4546, 0x4547, 0x4548,
            0x4549, 0x454A, 0x454B, 0x454C, 0x454D, 0x454E, 0x454F
        };

        private static readonly int[] m_SandTiles = new int[]
        {
            22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39, 40, 41,
            42, 43, 44, 45, 46, 47, 48, 49, 50, 51,
            52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
            62, 68, 69, 70, 71, 72, 73, 74, 75,
            286, 287, 288, 289, 290, 291, 292, 293, 294, 295,
            296, 297, 298, 299, 300, 301, 402, 424, 425, 426,
            427, 441, 442, 443, 444, 445, 446, 447, 448, 449,
            450, 451, 452, 453, 454, 455, 456, 457, 458, 459,
            460, 461, 462, 463, 464, 465, 642, 643, 644, 645,
            650, 651, 652, 653, 654, 655, 656, 657, 821, 822,
            823, 824, 825, 826, 827, 828, 833, 834, 835, 836,
            845, 846, 847, 848, 849, 850, 851, 852, 857, 858,
            859, 860, 951, 952, 953, 954, 955, 956, 957, 958,
            967, 968, 969, 970,
            1447, 1448, 1449, 1450, 1451, 1452, 1453, 1454, 1455,
            1456, 1457, 1458, 1611, 1612, 1613, 1614, 1615, 1616,
            1617, 1618, 1623, 1624, 1625, 1626, 1635, 1636, 1637,
            1638, 1639, 1640, 1641, 1642, 1647, 1648, 1649, 1650
        };
        #endregion
    }
}