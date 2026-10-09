using System;
using Server.Items;
using Server.Network;
using System.Linq;
using Server.Regions;
using Server.Mobiles;
using Server.Misc; // ResourceManager 등 연동

namespace Server.Engines.Harvest
{
    public class Lumberjacking : HarvestSystem
    {
        private static Lumberjacking m_System;

        public static Lumberjacking System => m_System ??= new Lumberjacking();

        private readonly HarvestDefinition m_Definition;

        public HarvestDefinition Definition => this.m_Definition;

        private Lumberjacking()
        {
            #region Lumberjacking
            HarvestDefinition lumber = new HarvestDefinition();

            lumber.BankWidth = 32;
            lumber.BankHeight = 32;

            lumber.MinTotal = 240;
            lumber.MaxTotal = 360;

            lumber.MinRespawn = TimeSpan.FromMinutes(300.0);
            lumber.MaxRespawn = TimeSpan.FromMinutes(600.0);

            lumber.Skill = SkillName.Lumberjacking;
            lumber.Tiles = m_TreeTiles;
            lumber.MaxRange = 1;

            lumber.ConsumedPerHarvest = 5;

            // 🌟 [공통 1] 기본 애니메이션 루프를 3회로 고정
            lumber.EffectActions = new int[] { Core.SA ? 7 : 13 };
            lumber.EffectSounds = new int[] { 0x13E };
            lumber.EffectCounts = new int[] { 3 }; 
            lumber.EffectDelay = TimeSpan.FromSeconds(0.9);
            lumber.EffectSoundDelay = TimeSpan.FromSeconds(1.6);

            lumber.NoResourcesMessage = 500493; // There's not enough wood here to harvest.
            lumber.FailMessage = 500495; // You hack at the tree for a while, but fail to produce any useable wood.
            lumber.OutOfRangeMessage = 500446; // That is too far away.
            lumber.PackFullMessage = 500497; // You can't place any wood into your backpack!
            lumber.ToolBrokeMessage = 500499; // You broke your axe.

            lumber.RaceBonus = Core.ML;
            lumber.RandomizeVeins = Core.ML;

            this.m_Definition = lumber;
            this.Definitions.Add(lumber);
            #endregion
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
            new MutateEntry( 0.0, 50.0,  false, typeof( Log ) ),
            new MutateEntry( 20.0, 70.0, false, typeof( OakLog ) ),
            new MutateEntry( 40.0, 90.0, false, typeof( AshLog ) ),
            new MutateEntry( 60.0,  110.0, false, typeof( YewLog ) ),
            new MutateEntry( 80.0,  130.0, false, typeof( HeartwoodLog ) ),
            new MutateEntry( 100.0,  150.0,  false, typeof( BloodwoodLog ) ),
            new MutateEntry( 120.0,  170.0,  false, typeof( FrostwoodLog ) ),
            new MutateEntry( 140.0,  190.0,  false, typeof( EbonyLog ) ),
            new MutateEntry( 160.0,  210.0,  false, typeof( EthrnalLog ) )
        };

        // 🌟 [핵심] 리뉴얼된 MutateType (튜플 반환, Bank 사용 안함)
        public override (Type Type, double Chance, double SkillMax, bool Fail) MutateType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, object toHarvest)
        {
            double skillBase = from.Skills[SkillName.Lumberjacking].Base;
            double skillValue = from.Skills[SkillName.Lumberjacking].Value;
                
            int count = 0;
            for (int i = m_MutateTable.Length - 1; i >= 1; --i)
            {
                int maxchance = Misc.Util.upgradechance[i];
                if( from is PlayerMobile pm )
                {
                    maxchance = Misc.Util.ExpHarvestBonus( pm, maxchance );
                }
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

            // 🌟 [공통 1] 에니메이션 루프 수치 계산용 참고 로직 (타이머 연동)
            // int animLoop = 3; 
            // animLoop += count; // 자원 등급 올라가면 +1씩
            // if (skillBase >= 100.0) animLoop -= 1; // 스킬 100 이상이면 -1
            // animLoop = Math.Max(1, animLoop); // 1회 이하는 불가능

            return (upgrade, chance, point, failcheck);
        }

        public override void SendSuccessTo(Mobile from, Item item, Type resourceType)
        {
            if (item != null)
            {
                if (item.GetType().IsSubclassOf(typeof(BaseWoodBoard)))
                {
                    from.SendLocalizedMessage(1158776); // The axe magically creates boards from your logs.
                }
                else
                {
                    from.SendLocalizedMessage(500498); // You put some wood into your backpack.
                }
            }
        }

        public override bool CheckHarvest(Mobile from, Item tool)
        {
            if (!base.CheckHarvest(from, tool))
                return false;

            return true;
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

        public override bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            if (!base.CheckHarvest(from, tool, def, toHarvest))
                return false;

            // 🌟 [수정 1] 벌목 도구 제한 (오직 톱과 손도끼만 허용, 일반 Axe 금지)
            if (!(tool is Hatchet) && !(tool is Saw))
            {
                from.SendMessage("벌목은 오직 톱(장비 해제)이나 손도끼(장비 착용)로만 가능합니다.");
                return false;
            }

            bool boat = Server.Multis.BaseBoat.FindBoatAt(from, from.Map) != null;
            bool dungeon = IsDungeonRegion(from);

            if (tool.Parent != from && from.Backpack != null && !tool.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1080058); // This must be in your backpack to use it.
                return false;
            }
            else if (from.Mounted)
            {
                from.SendMessage("말을 탄 상태에서는 나무를 벌목할 수 없습니다.");
                return false;
            }
            else if (from.IsBodyMod && !from.Body.IsHuman)
            {
                from.SendMessage("폴리모프 상태에서는 나무를 벌목할 수 없습니다.");
                return false;
            }
            else if( boat )
            {
                from.SendMessage("배 안에서는 나무를 벌목할 수 없습니다.");
                return false;
            }
            else if( dungeon )
            {
                from.SendMessage("던전 안에서는 나무를 벌목할 수 없습니다.");
                return false;
            }

            return true;
        }

        public override bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            if (HarvestMap.CheckMapOnHarvest(from, loc, def) == null)
                return base.CheckResources(from, tool, def, map, loc, timed);

            return true;
        }

        // 🌟 [핵심] 리뉴얼된 OnHarvestFinished (튜플 적용, 찌꺼기 제거)
        public override void OnHarvestFinished(Mobile from, Item tool, HarvestDefinition def, Type resourceType, object harvested)
        {
            // 🌟 [수정 2] 공통 인테리어 물품(특이한 나무뿌리 등) 랜덤 드랍 예약 공간
            if (Utility.RandomDouble() < 0.05) // 확률 설정 예시 (5%)
            {
                // TODO: 인테리어 물품(특이한 가지, 원목 등) 추가
                // Item deco = new DecorativeWood();
                // Give(from, deco, true);
                // from.SendMessage("쓸만한 장식용 나뭇가지를 발견했습니다.");
            }
        }

        public override void OnBadHarvestTarget(Mobile from, Item tool, object toHarvest)
        {
            if (toHarvest is Mobile)
                ((Mobile)toHarvest).PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500450, from.NetState); // You can only skin dead creatures.
            else if (toHarvest is Item)
                ((Item)toHarvest).LabelTo(from, 500464); // Use this on corpses to carve away meat and hide
            else if (toHarvest is Targeting.StaticTarget || toHarvest is Targeting.LandTarget)
                from.SendLocalizedMessage(500489); // You can't use an axe on that.
            else
                from.SendLocalizedMessage(1005213); // You can't do that
        }

        public override void OnHarvestStarted(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            base.OnHarvestStarted(from, tool, def, toHarvest);

            if (Core.ML)
                from.RevealingAction();
        }

        public static void Initialize()
        {
            Array.Sort(m_TreeTiles);
        }

        #region Tile lists
        private static readonly int[] m_TreeTiles = new int[]
        {
            0x4CCA, 0x4CCB, 0x4CCC, 0x4CCD, 0x4CD0, 0x4CD3, 0x4CD6, 0x4CD8,
            0x4CDA, 0x4CDD, 0x4CE0, 0x4CE3, 0x4CE6, 0x4CF8, 0x4CFB, 0x4CFE,
            0x4D01, 0x4D41, 0x4D42, 0x4D43, 0x4D44, 0x4D57, 0x4D58, 0x4D59,
            0x4D5A, 0x4D5B, 0x4D6E, 0x4D6F, 0x4D70, 0x4D71, 0x4D72, 0x4D84,
            0x4D85, 0x4D86, 0x52B5, 0x52B6, 0x52B7, 0x52B8, 0x52B9, 0x52BA,
            0x52BB, 0x52BC, 0x52BD,
            0x4CCE, 0x4CCF, 0x4CD1, 0x4CD2, 0x4CD4, 0x4CD5, 0x4CD7, 0x4CD9,
            0x4CDB, 0x4CDC, 0x4CDE, 0x4CDF, 0x4CE1, 0x4CE2, 0x4CE4, 0x4CE5,
            0x4CE7, 0x4CE8, 0x4CF9, 0x4CFA, 0x4CFC, 0x4CFD, 0x4CFF, 0x4D00,
            0x4D02, 0x4D03, 0x4D45, 0x4D46, 0x4D47, 0x4D48, 0x4D49, 0x4D4A,
            0x4D4B, 0x4D4C, 0x4D4D, 0x4D4E, 0x4D4F, 0x4D50, 0x4D51, 0x4D52,
            0x4D53, 0x4D5C, 0x4D5D, 0x4D5E, 0x4D5F, 0x4D60, 0x4D61, 0x4D62,
            0x4D63, 0x4D64, 0x4D65, 0x4D66, 0x4D67, 0x4D68, 0x4D69, 0x4D73,
            0x4D74, 0x4D75, 0x4D76, 0x4D77, 0x4D78, 0x4D79, 0x4D7A, 0x4D7B,
            0x4D7C, 0x4D7D, 0x4D7E, 0x4D7F, 0x4D87, 0x4D88, 0x4D89, 0x4D8A,
            0x4D8B, 0x4D8C, 0x4D8D, 0x4D8E, 0x4D8F, 0x4D90, 0x4D95, 0x4D96,
            0x4D97, 0x4D99, 0x4D9A, 0x4D9B, 0x4D9D, 0x4D9E, 0x4D9F, 0x4DA1,
            0x4DA2, 0x4DA3, 0x4DA5, 0x4DA6, 0x4DA7, 0x4DA9, 0x4DAA, 0x4DAB,
            0x52BE, 0x52BF, 0x52C0, 0x52C1, 0x52C2, 0x52C3, 0x52C4, 0x52C5,
            0x52C6, 0x52C7
        };
        #endregion
    }
}