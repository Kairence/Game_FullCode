using System;
using Server.Items;
using Server.Network;
using System.Linq;
using Server.Regions;
using Server.Mobiles;

namespace Server.Engines.Harvest
{
    public class Lumberjacking : HarvestSystem
    {
        private static Lumberjacking m_System;

        public static Lumberjacking System
        {
            get
            {
                if (m_System == null)
                    m_System = new Lumberjacking();

                return m_System;
            }
        }

        private readonly HarvestDefinition m_Definition;

        public HarvestDefinition Definition
        {
            get
            {
                return this.m_Definition;
            }
        }

        private Lumberjacking()
        {
            HarvestResource[] res;
            HarvestVein[] veins;

            #region Lumberjacking
            HarvestDefinition lumber = new HarvestDefinition();

            // Resource banks are every 4x3 tiles
            lumber.BankWidth = 32;
            lumber.BankHeight = 32;

            // Every bank holds from 20 to 45 logs
            lumber.MinTotal = 240;
            lumber.MaxTotal = 360;

            // A resource bank will respawn its content every 20 to 30 minutes
            lumber.MinRespawn = TimeSpan.FromMinutes(300.0);
            lumber.MaxRespawn = TimeSpan.FromMinutes(600.0);

            // Skill checking is done on the Lumberjacking skill
            lumber.Skill = SkillName.Lumberjacking;

            // Set the list of harvestable tiles
            lumber.Tiles = m_TreeTiles;

            // Players must be within 2 tiles to harvest
            lumber.MaxRange = 1;

            // Ten logs per harvest action
            lumber.ConsumedPerHarvest = 5;
            lumber.ConsumedPerFeluccaHarvest = 1;

            // The chopping effect
            lumber.EffectActions = new int[] { Core.SA ? 7 : 13 };
            lumber.EffectSounds = new int[] { 0x13E };
            lumber.EffectCounts = (Core.AOS ? new int[] { 3, 4, 5, 6, 7 } : new int[] { 1, 2, 2, 2, 3 });
            lumber.EffectDelay = TimeSpan.FromSeconds(0.9);
            lumber.EffectSoundDelay = TimeSpan.FromSeconds(1.6);

            lumber.NoResourcesMessage = 500493; // There's not enough wood here to harvest.
            lumber.FailMessage = 500495; // You hack at the tree for a while, but fail to produce any useable wood.
            lumber.OutOfRangeMessage = 500446; // That is too far away.
            lumber.PackFullMessage = 500497; // You can't place any wood into your backpack!
            lumber.ToolBrokeMessage = 500499; // You broke your axe.

            if (Core.ML)
            {
                res = new HarvestResource[]
                {
                    new HarvestResource(00.0, 00.0, 50.0, 1072540, typeof(Log))
					/*
                    new HarvestResource(65.0, 65.0, 105.0, 1072541, typeof(OakLog)),
                    new HarvestResource(80.0, 80.0, 120.0, 1072542, typeof(AshLog)),
                    new HarvestResource(95.0, 95.0, 135.0, 1072543, typeof(YewLog)),
                    new HarvestResource(100.0, 100.0, 140.0, 1072544, typeof(HeartwoodLog)),
                    new HarvestResource(100.0, 100.0, 140.0, 1072545, typeof(BloodwoodLog)),
                    new HarvestResource(100.0, 100.0, 140.0, 1072546, typeof(FrostwoodLog))
					*/
                };

                veins = new HarvestVein[]
                {
                    new HarvestVein(100.0, 0.0, res[0], null) // Ordinary Logs
					/*
                    new HarvestVein(30.0, 0.5, res[1], res[0]), // Oak
                    new HarvestVein(10.0, 0.5, res[2], res[0]), // Ash
                    new HarvestVein(05.0, 0.5, res[3], res[0]), // Yew
                    new HarvestVein(03.0, 0.5, res[4], res[0]), // Heartwood
                    new HarvestVein(02.0, 0.5, res[5], res[0]), // Bloodwood
                    new HarvestVein(01.0, 0.5, res[6], res[0]), // Frostwood
					*/
                };

                lumber.BonusResources = new BonusHarvestResource[]
                {
                    new BonusHarvestResource(0, 100.0, null, null) //Nothing
					/*
                    new BonusHarvestResource(100, 10.0, 1072548, typeof(BarkFragment)),
                    new BonusHarvestResource(100, 03.0, 1072550, typeof(LuminescentFungi)),
                    new BonusHarvestResource(100, 02.0, 1072547, typeof(SwitchItem)),
                    new BonusHarvestResource(100, 01.0, 1072549, typeof(ParasiticPlant)),
                    new BonusHarvestResource(100, 01.0, 1072551, typeof(BrilliantAmber)),
                    new BonusHarvestResource(100, 01.0, 1113756, typeof(CrystalShards), Map.TerMur),
					*/
                };
            }
            else
            {
                res = new HarvestResource[]
                {
                    new HarvestResource(00.0, 00.0, 100.0, 500498, typeof(Log))
                };

                veins = new HarvestVein[]
                {
                    new HarvestVein(100.0, 0.0, res[0], null)
                };
            }

            lumber.Resources = res;
            lumber.Veins = veins;

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
            public Type[] m_Types;

            public MutateEntry(double minSkill, double maxSkill, bool deepForest, params Type[] types)
            {
                //m_ReqSkill = reqSkill;
                m_MinSkill = minSkill;
                m_MaxSkill = maxSkill;
                m_DeepForest = deepForest;
                m_Types = types;
            }
        }

        private static MutateEntry[] m_MutateTable = new MutateEntry[]
		{
			new MutateEntry( 0.0, 50.0,  false, typeof( Log ) ),
			new MutateEntry( 20.0, 70.0, false, typeof( OakLog ) ),
			new MutateEntry( 40.0, 90.0, false, typeof( AshLog ) ),
			new MutateEntry( 60.0,  110.0, false, typeof( YewLog ) ),
			new MutateEntry( 80.0,  130.0, false, typeof( HeartwoodLog ) ),
			new MutateEntry( 100.0,  150.0,  false, typeof( BloodwoodLog ) ),
			new MutateEntry( 120.0,  170.0,  false, typeof( FrostwoodLog ) )
		};

            //new MutateEntry( 80.0,  80.0, 2500.0, false, typeof( MudPuppy ), typeof( RedHerring) ),
			//new MutateEntry( 0.0, 200.0,  -200.0, false, new Type[1]{ null } )

        public override Type MutateType(Type type, Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource, out double chance, out double point, out bool failcheck, out Type basic, out Type upgrade )
        {
			double skillBase = from.Skills[SkillName.Lumberjacking].Base;
			double skillValue = from.Skills[SkillName.Lumberjacking].Value;
				
			var table = m_MutateTable;

			MutateEntry entry = m_MutateTable[0];

			bool selectOre = false;
			failcheck = false;
			basic = entry.m_Types[0];
			upgrade = entry.m_Types[0];

			int count = 0;
			for (int i = table.Length -1; i >= 1; --i)
			{
				entry = m_MutateTable[i];
				int maxchance = Misc.Util.upgradechance[i];
				if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					maxchance = Misc.Util.ExpHarvestBonus( pm, maxchance );
				}
				if (skillValue >= entry.m_MinSkill && Utility.RandomMinMax(1, 10000) <= maxchance )
				{
					count = i;
					break;
				}
			}

			entry = m_MutateTable[count];
			upgrade = entry.m_Types[0];
			
			//if( type != upgrade )
			//	type = upgrade;
			
			if( count > 0 && Utility.RandomMinMax(0, count * 2) != 0 )
				failcheck = true;

			point = entry.m_MaxSkill + entry.m_MinSkill;
			chance = 1 + ( skillValue - entry.m_MaxSkill ) * 0.02;
			return type;

			
			/*
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				for ( int i = 0; i < buffpoint.Length; i++ )
				{
					entry = m_MutateTable[i + 1];
					if( pm.BuffCheck[i] && skillValue >= entry.m_MinSkill )
					{
						if( pm.GoldPoint[0] >= buffpoint[i] )
						{
							pm.GoldPoint[0] -= buffpoint[i];

							basic = entry.m_Types[0];
							upgrade = entry.m_Types[0];
							selectOre = true;
							failcheck = true;
							break;
						}
						else
						{
							pm.BuffCheck[i] = false;
							pm.SendMessage("당신은 해당 버프를 적용받기에는 포인트가 낮습니다.");
						}
					}
					else if( pm.BuffCheck[i] )
					{
						pm.BuffCheck[i] = false;
						pm.SendMessage("당신은 해당 버프를 적용받기에는 스킬이 낮습니다.");
					}
				}
			}

			if( !selectOre )
			{
				int count = 0;
				for (int i = table.Length -1; i >= 1; --i)
				{
					entry = m_MutateTable[i];
					if (skillValue >= entry.m_MinSkill && Utility.RandomMinMax(0, i * i) == 0 )
					{
						count = i;
						break;
					}
				}

				entry = m_MutateTable[count];
				upgrade = entry.m_Types[0];
				
				//if( type != upgrade )
				//	type = upgrade;
				
				if( count > 0 && Utility.RandomMinMax(0, count * 2) != 0 )
					failcheck = true;
			}
			point = entry.m_MaxSkill;
			chance = 1 + ( skillValue - entry.m_MaxSkill ) * 0.02;
			return type;
			*/
		}
        public override void SendSuccessTo(Mobile from, Item item, HarvestResource resource)
        {
            if (item != null)
            {
                if (item != null && item.GetType().IsSubclassOf(typeof(BaseWoodBoard)))
                {
                    from.SendLocalizedMessage(1158776); // The axe magically creates boards from your logs.
                    return;
                }
                else
                {
                    foreach (var res in m_Definition.Resources.Where(r => r.Types != null))
                    {
                        foreach (var type in res.Types)
                        {
                            if (item.GetType() == type)
                            {
                                res.SendSuccessTo(from);
                                return;
                            }
                        }
                    }
                }
            }

            base.SendSuccessTo(from, item, resource);
        }

        public override bool CheckHarvest(Mobile from, Item tool)
        {
            if (!base.CheckHarvest(from, tool))
                return false;

            return true;
        }
        private bool IsDungeonRegion(Mobile from)
        {
            if (from == null)
                return false;

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
                from.SendMessage("배 안에서는 나무를 벌목할 수 없습니다"); // You can't mine while polymorphed.
                return false;
			}
			else if( dungeon )
			{
                from.SendMessage("던전 안에서는 나무를 벌목할 수 없습니다"); // You can't mine while polymorphed.
                return false;
			}

			return true;
        }

		/*
        public override Type GetResourceType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
        {
            #region Void Pool Items
            HarvestMap hmap = HarvestMap.CheckMapOnHarvest(from, loc, def);

            if (hmap != null && hmap.Resource >= CraftResource.RegularWood && hmap.Resource <= CraftResource.Frostwood)
            {
                hmap.UsesRemaining--;
                hmap.InvalidateProperties();

                CraftResourceInfo info = CraftResources.GetInfo(hmap.Resource);

                if (info != null)
                    return info.ResourceTypes[0];
            }
            #endregion

            return base.GetResourceType(from, tool, def, map, loc, resource);
        }
		*/
        public override bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            if (HarvestMap.CheckMapOnHarvest(from, loc, def) == null)
                return base.CheckResources(from, tool, def, map, loc, timed);

            return true;
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