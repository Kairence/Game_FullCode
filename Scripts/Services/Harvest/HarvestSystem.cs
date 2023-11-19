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

        public HarvestSystem()
        {
            m_Definitions = new List<HarvestDefinition>();
        }

        public List<HarvestDefinition> Definitions
        {
            get
            {
                return m_Definitions;
            }
        }

        public virtual bool CheckTool(Mobile from, Item tool)
        {
            bool wornOut = (tool == null || tool.Deleted || (tool is IUsesRemaining && ((IUsesRemaining)tool).UsesRemaining <= 0));

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
			//배고픔 체크
			int hunger = 50;
			if( def.Skill is SkillName.Fishing )
				hunger *= 2;
			/*
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				if( pm.TimerList[71] > 0 )
				{
					pm.LastObject = null;
					return false;
				}
			}
			*/
			if( from.Hunger < hunger )
			{
				from.SendMessage( String.Format("배고픈 상태에서는 자원 채취를 할 수 없습니다!" ) );
				if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					pm.Loop = false;
				}
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

        public virtual bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
        {
            HarvestBank bank = def.GetBank(map, loc.X, loc.Y);
            bool available = (bank != null && bank.Current >= def.ConsumedPerHarvest);

            if (!available)
                def.SendMessageTo(from, timed ? def.DoubleHarvestMessage : def.NoResourcesMessage);

            return available;
        }

        public virtual void OnBadHarvestTarget(Mobile from, Item tool, object toHarvest)
        {
        }

        public virtual object GetLock(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
            /* Here we prevent multiple harvesting.
            * 
            * Some options:
            *  - 'return tool;' : This will allow the player to harvest more than once concurrently, but only if they use multiple tools. This seems to be as OSI.
            *  - 'return GetType();' : This will disallow multiple harvesting of the same type. That is, we couldn't mine more than once concurrently, but we could be both mining and lumberjacking.
            *  - 'return typeof( HarvestSystem );' : This will completely restrict concurrent harvesting.
            */
            return tool;
        }

        public virtual void OnConcurrentHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
        }

        public virtual void OnHarvestStarted(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
        {
        }

        public virtual bool BeginHarvesting(Mobile from, Item tool)
        {
            if (!CheckHarvest(from, tool))
                return false;

			EventSink.InvokeResourceHarvestAttempt(new ResourceHarvestAttemptEventArgs(from, tool, this));

			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				if( tool != null && pm.Loop && pm.LastObject != null )
				{
					StartHarvesting(from, tool, pm.LastObject);
					return true;
				}
			}
            from.Target = new HarvestTarget(tool, this);
            return true;
        }

        public virtual void FinishHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked)
        {
            from.EndAction(locked);

            if (!CheckHarvest(from, tool))
                return;
			
            int tileID;
            Map map;
            Point3D loc;

            if (!GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }
            else if (!def.Validate(tileID) && !def.ValidateSpecial(tileID))
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }
			
            if (!CheckRange(from, tool, def, map, loc, true))
                return;
            else if (!CheckResources(from, tool, def, map, loc, true))
                return;
            else if (!CheckHarvest(from, tool, def, toHarvest))
                return;

            if (SpecialHarvest(from, tool, def, map, loc))
                return;

            HarvestBank bank = def.GetBank(map, loc.X, loc.Y);

            if (bank == null)
                return;

			
            HarvestVein vein = bank.Vein;

            if (vein != null)
                vein = MutateVein(from, tool, def, bank, toHarvest, vein);

            if (vein == null)
                return;
            HarvestResource primary = vein.PrimaryResource;
            HarvestResource fallback = vein.FallbackResource;
            HarvestResource resource = MutateResource(from, tool, def, map, loc, vein, primary, fallback);
			
			//HarvestVein vein = null;
			//HarvestResource resource = null;
			
            double skillBase = from.Skills[def.Skill].Base;

            Type type = bank.BankType;
		
			//Type type = null;
		
			double chance = 0;
			double point = 0;
			double skillmax = 0;
			double dice, MaxValue;
			bool failcheck;
			Type basic, upgrade;
			bool fail = false;
			Type basictype = null;
			Type upgradetype = null;
			if( bank.BankType == null )
			{
				type = MutateType(type, from, tool, def, map, loc, resource, out dice, out MaxValue, out failcheck, out basic, out upgrade);
				chance = dice;
				skillmax = MaxValue;
				fail = failcheck;
				point = MaxValue;
				basictype = basic;
				upgradetype = upgrade;
				if( bank.BankType == null )
					bank.BankType = upgradetype;
				if( fail )
				{
					type = basictype;
					skillmax = point = 50;
				}
				else
					type = bank.BankType;
			}
			else
			{
				type = MutateType(type, from, tool, def, map, loc, resource, out dice, out MaxValue, out failcheck, out basic, out upgrade);
				chance = dice;
				skillmax = MaxValue;
				fail = failcheck;
				point = MaxValue;
				basictype = basic;
				upgradetype = upgrade;

				bank.BankType = upgradetype;
				
				if( fail )
				{
					type = basictype;
					skillmax = point = 50;
				}
				else
					type = bank.BankType;
			}
			int amount = 0;
			int skillpoint = def.ConsumedPerHarvest;
			if( def.Skill is SkillName.Fishing )
				point *= 2;
			//if(CheckHarvestSkill(map, loc, from, resource, def))
			if( chance > Utility.RandomDouble())
			{
				//point *= 2;

                if (type != null)
                {
                    Item item = Construct(type, from, tool);

                    if (item == null)
                    {
                        type = null;
                    }
                    else
                    {
						amount = skillpoint;
						/*
						if( item is IronOre || item is Granite || item is Fish || item is Log )
							amount += Utility.RandomMinMax( (int)( skillBase / 10 ), (int)( skillBase / 5 ));
						*/
						if( skillBase >= 100 )
							amount++;
						if( from is PlayerMobile )
						{
							PlayerMobile pm = from as PlayerMobile;
							QuestHelper.CheckHarvest(pm, item);

							if( pm.GoldPoint[4] > 0 )
							{
								int harvestbonus = Utility.RandomMinMax( 0, pm.GoldPoint[4] );
								harvestbonus = (int)Math.Sqrt(harvestbonus);
								amount += harvestbonus;
							}
							//if( pm.Tired >= 5000 )
							//	amount -= (int)( pm.Tired - 5000 ) / 2000;
						}

                        Caddellite.OnHarvest(from, tool, this, item);

                        //The whole harvest system is kludgy and I'm sure this is just adding to it.
                        if (item.Stackable)
                        {
							/*
                            int racialAmount = (int)Math.Ceiling(amount * 1.1);
                            //int feluccaRacialAmount = (int)Math.Ceiling(feluccaAmount * 1.1);

                            bool eligableForRacialBonus = (def.RaceBonus && from.Race == Race.Human);
                            //bool inFelucca = map == Map.Felucca && !Siege.SiegeShard;
							*/
							
							if ( def.Skill is SkillName.Fishing && from.Map == Map.Trammel && Items.SpecialFishingNet.ValidateDeepWater(from.Map, from.Location.X, from.Location.Y) )
								HarvestLoop = true;

							item.Amount = amount;
                        }
						HarvestLoop = false;
						
						if( amount <= 0 )
						{
							amount = 0;
							def.SendMessageTo(from, def.FailMessage);
							item.Delete();
						}
                        else if (Give(from, item, def.PlaceAtFeetIfFull))
                        {
                            SendSuccessTo(from, item, resource);
                        }
                        else
                        {
                            SendPackFullTo(from, item, def, resource);
                            item.Delete();
                        }
					}
				}
			}
			else
			{
				def.SendMessageTo(from, def.FailMessage);
				
			}
			from.CheckSkill( def.Skill, point * skillpoint );

			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				int getgoldpoint = (int)(point * amount );
				if( getgoldpoint > 0 )
				{
					pm.Getgoldpoint(getgoldpoint);
					
					if( def.Skill is SkillName.Mining )
					{
						Server.Misc.Util.SavingAccountPoint(pm, 1, 1);
						if( type == typeof( Sand ) )
						{
							Server.Misc.Util.SavingAccountPoint(pm, 90, 1 );
						}
						else
						{
							Server.Misc.Util.SavingAccountPoint(pm, (int)( 2 + ( skillmax - 50 ) / 40 ), 1 );
						}
					}
					else if( def.Skill is SkillName.Lumberjacking )
					{
						Server.Misc.Util.SavingAccountPoint(pm, 11, 1);
						Server.Misc.Util.SavingAccountPoint(pm, (int)( 12 + ( skillmax - 50 ) / 40 ), 1 );
					}
					else if( def.Skill is SkillName.Fishing )
					{
						Server.Misc.Util.SavingAccountPoint(pm, 81, 1);
						Server.Misc.Util.SavingAccountPoint(pm, (int)( 82 + ( skillmax - 50 ) / 40 ), 1 );
					}
				}
			}
			if (tool is IUsesRemaining && (tool is BaseHarvestTool || tool is Pickaxe || tool is SturdyPickaxe || tool is GargoylesPickaxe || Siege.SiegeShard))
			{
				IUsesRemaining toolWithUses = (IUsesRemaining)tool;

				toolWithUses.ShowUsesRemaining = true;

				if (toolWithUses.UsesRemaining > 0)
					--toolWithUses.UsesRemaining;

				if (toolWithUses.UsesRemaining < 1)
				{
					Item NextTool = null;
					if( from is PlayerMobile )
					{
						PlayerMobile pm = from as PlayerMobile;
						if( pm.Loop )
						{
							Container pack = from.Backpack;
							if( pack != null )
							{
								List<Item> tools = pack.FindItemsByType<Item>();
								for ( int i = tools.Count -1; i >= 0; i--)
								{
									Item item = tools[i];
									if( item is BaseHarvestTool )
									{
										BaseHarvestTool bht = item as BaseHarvestTool;
										if( bht is IUsesRemaining )
										{
											if( bht.ItemID == tool.ItemID && tool != bht )
											{
												NextTool = bht;
												break;
											}
										}
									}
								}
							}
						}
					}
					tool.Delete();
					if( NextTool != null )
						tool = NextTool;
					else
					{
						if( from is PlayerMobile )
						{
							PlayerMobile pm = from as PlayerMobile;
							pm.Loop = false;
						}						
					}
					def.SendMessageTo(from, def.ToolBrokeMessage);
				}
                }
				/*
                        BonusHarvestResource bonus = def.GetBonusResource();
                        Item bonusItem = null;

                        if (bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill)
                        {
							if (bonus.RequiredMap == null || bonus.RequiredMap == from.Map)
							{
							    bonusItem = Construct(bonus.Type, from, tool);
                                Caddellite.OnHarvest(from, tool, this, bonusItem);

                                if (Give(from, bonusItem, true))	//Bonuses always allow placing at feet, even if pack is full irregrdless of def
								{
                                    bonus.SendSuccessTo(from);
								}
								else
								{
                                    bonusItem.Delete();
								}
							}
                        }
                        EventSink.InvokeResourceHarvestSuccess(new ResourceHarvestSuccessEventArgs(from, tool, item, bonusItem, this));
                    }

                    #region High Seas
                    OnToolUsed(from, tool, item != null);
                    #endregion
                }
				point *= 2;
                // Siege rules will take into account axes and polearms used for lumberjacking
                if (tool is IUsesRemaining && (tool is BaseHarvestTool || tool is Pickaxe || tool is SturdyPickaxe || tool is GargoylesPickaxe || Siege.SiegeShard))
                {
                    IUsesRemaining toolWithUses = (IUsesRemaining)tool;

                    toolWithUses.ShowUsesRemaining = true;

                    if (toolWithUses.UsesRemaining > 0)
                        --toolWithUses.UsesRemaining;

                    if (toolWithUses.UsesRemaining < 1)
                    {
                        tool.Delete();
                        def.SendMessageTo(from, def.ToolBrokeMessage);
                    }
                }
            }

            if (type == null)
                def.SendMessageTo(from, def.FailMessage);

			from.CheckSkill(def.Skill, point );
			*/

			//배고픔
			if( from is PlayerMobile )
			{
				int hunger = 100;
				PlayerMobile pm = from as PlayerMobile;
				pm.MacroCheck += 10;
				if( def.Skill is SkillName.Fishing )
					hunger *= 2;
				from.Hunger -= hunger;
				pm.LastTarget = tool;
			}
						
            OnHarvestFinished(from, tool, def, vein, bank, resource, toHarvest);
			//자동 캐기
			/*
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				if( tool != null && pm.Loop && pm.TimerList[71] == 0 )
				{
					pm.LoopCheck = false;
					pm.LastTarget = tool;
					tool.OnDoubleClick( from );
				}
			}
			*/
        }

        public virtual bool CheckHarvestSkill(Map map, Point3D loc, Mobile from, HarvestResource resource, HarvestDefinition def)
        {
            return from.Skills[def.Skill].Value >= resource.ReqSkill && from.CheckSkill(def.Skill, resource.MinSkill, resource.MaxSkill);
        }

        public virtual void OnToolUsed(Mobile from, Item tool, bool caughtSomething)
        {
        }

        public virtual void OnHarvestFinished(Mobile from, Item tool, HarvestDefinition def, HarvestVein vein, HarvestBank bank, HarvestResource resource, object harvested)
        {
        }

        public virtual bool SpecialHarvest(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc)
        {
            return false;
        }

        public virtual Item Construct(Type type, Mobile from, Item tool)
        {
            try
            {
                return Activator.CreateInstance(type) as Item;
            }
            catch
            {
                return null;
            }
        }

        public virtual HarvestVein MutateVein(Mobile from, Item tool, HarvestDefinition def, HarvestBank bank, object toHarvest, HarvestVein vein)
        {
            return vein;
        }

        public virtual void SendSuccessTo(Mobile from, Item item, HarvestResource resource)
        {
            resource.SendSuccessTo(from);
        }

        public virtual void SendPackFullTo(Mobile from, Item item, HarvestDefinition def, HarvestResource resource)
        {
            def.SendMessageTo(from, def.PackFullMessage);
        }

        public virtual bool Give(Mobile m, Item item, bool placeAtFeet)
        {
            if (m.PlaceInBackpack(item))
                return true;

            if (!placeAtFeet)
                return false;

            Map map = m.Map;

            if (map == null || map == Map.Internal)
                return false;

            List<Item> atFeet = new List<Item>();

            IPooledEnumerable eable = m.GetItemsInRange(0);

            foreach (Item obj in eable)
                atFeet.Add(obj);

            eable.Free();

            for (int i = 0; i < atFeet.Count; ++i)
            {
                Item check = atFeet[i];

                if (check.StackWith(m, item, false))
                    return true;
            }

            ColUtility.Free(atFeet);

            item.MoveToWorld(m.Location, map);
            return true;
        }

        public virtual Type MutateType(Type type, Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource, out double chance, out double point, out bool fail, out Type basic, out Type upgrade)
        {
			chance = 0;
			point = 0;
			fail = false;
			basic = null;
			upgrade = null;
            return from.Region.GetResource(type);
        }

        public virtual Type GetResourceType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
        {
            if (resource.Types.Length > 0)
                return resource.Types[Utility.Random(resource.Types.Length)];

            return null;
        }

        public virtual HarvestResource MutateResource(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestVein vein, HarvestResource primary, HarvestResource fallback)
        {
            bool racialBonus = (def.RaceBonus && from.Race == Race.Elf);

            if (vein.ChanceToFallback > (Utility.RandomDouble() + (racialBonus ? .20 : 0)))
                return fallback;

            double skillValue = from.Skills[def.Skill].Value;

            if (fallback != null && (skillValue < primary.ReqSkill || skillValue < primary.MinSkill))
                return fallback;

            return primary;
        }

        public virtual bool OnHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked, bool last)
        {
            if (!CheckHarvest(from, tool))
            {
                from.EndAction(locked);
                return false;
            }

            int tileID;
            Map map;
            Point3D loc;

            if (!GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
            {
                from.EndAction(locked);
                OnBadHarvestTarget(from, tool, toHarvest);
                return false;
            }
            else if (!def.Validate(tileID) && !def.ValidateSpecial(tileID))
            {
                from.EndAction(locked);
                OnBadHarvestTarget(from, tool, toHarvest);
                return false;
            }
            else if (!CheckRange(from, tool, def, map, loc, true))
            {
                from.EndAction(locked);
                return false;
            }
            else if (!CheckResources(from, tool, def, map, loc, true))
            {
                from.EndAction(locked);
                return false;
            }
            else if (!CheckHarvest(from, tool, def, toHarvest))
            {
                from.EndAction(locked);
                return false;
            }

            DoHarvestingEffect(from, tool, def, map, loc);

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
                if (Core.SA)
                {
                    from.Animate(AnimationType.Attack, Utility.RandomList(def.EffectActions));
                }
                else
                {
                    from.Animate(Utility.RandomList(def.EffectActions), 5, 1, true, false, 0);
                }
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

                if (check.Validate(tileID))
                    def = check;
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

                if (check.ValidateSpecial(tileID))
                    def = check;
            }

            return def;
        }
        #endregion

        public virtual void StartHarvesting(Mobile from, Item tool, object toHarvest)
        {
            if (!CheckHarvest(from, tool))
                return;

            int tileID;
            Map map;
            Point3D loc;

            if (!GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }

            HarvestDefinition def = GetDefinition(tileID, tool);

            if (def == null)
            {
                OnBadHarvestTarget(from, tool, toHarvest);
                return;
            }

            if (!CheckRange(from, tool, def, map, loc, false))
                return;
            else if (!CheckResources(from, tool, def, map, loc, false))
                return;
            else if (!CheckHarvest(from, tool, def, toHarvest))
                return;

            object toLock = GetLock(from, tool, def, toHarvest);

            if (!from.BeginAction(toLock))
            {
                OnConcurrentHarvest(from, tool, def, toHarvest);
                return;
            }

            new HarvestTimer(from, tool, this, def, toHarvest, toLock).Start();
			
            //OnHarvestStarted(from, tool, def, toHarvest);
        }

		static string resourceName = "";
		
        public virtual bool GetHarvestDetails(Mobile from, Item tool, object toHarvest, out int tileID, out Map map, out Point3D loc)
        {
            if (toHarvest is Static && !((Static)toHarvest).Movable)
            {
                Static obj = (Static)toHarvest;

                tileID = (obj.ItemID & 0x3FFF) | 0x4000;
                map = obj.Map;
                loc = obj.GetWorldLocation();
            }
            else if (toHarvest is StaticTarget) //자원 채취 설정 코드
            {
                StaticTarget obj = (StaticTarget)toHarvest;

                tileID = (obj.ItemID & 0x3FFF) | 0x4000;
                map = from.Map;
                loc = obj.Location;
				resourceName = obj.Name;
				//if( obj.Name == "cave floor" || obj.Name == "Yew tree" )
				//if( tileID != 1360 && tileID >= 1339 && tileID <= 1363 ) ( id >= 4789 && id <= 4807 ) )
				//if( tileID != 0x4550 && ( tileID >= 0x453B && tileID <= 0x4553 ) || ( tileID >= 0x52B5 && tileID <= 0x52C7 ) )
				//{
				//	HarvestLoop = true;
				//}				
            }
            else if (toHarvest is LandTarget)
            {
                LandTarget obj = (LandTarget)toHarvest;

                tileID = obj.TileID;
                map = from.Map;
                loc = obj.Location;
            }
            else
            {
                tileID = 0;
                map = null;
                loc = Point3D.Zero;
                return false;
            }

            return (map != null && map != Map.Internal);
        }

        #region Enhanced Client
        public static void TargetByResource(TargetByResourceMacroEventArgs e)
        {
            Mobile m = e.Mobile;
            Item tool = e.Tool;

            HarvestSystem system = null;
            HarvestDefinition def = null;
            object toHarvest;

            if (tool is IHarvestTool)
            {
                system = ((IHarvestTool)tool).HarvestSystem;
            }

            if (system != null)
            {
                switch (e.ResourceType)
                {
                    case 0: // ore
                        if (system is Mining)
                            def = ((Mining)system).OreAndStone;
                        break;
                    case 1: // sand
                        if (system is Mining)
                            def = ((Mining)system).Sand;
                        break;
                    case 2: // wood
                        if (system is Lumberjacking)
                            def = ((Lumberjacking)system).Definition;
                        break;
                    case 3: // grave
                        if (TryHarvestGrave(m))
                            return;
                        break;
                    case 4: // red shrooms
                        if (TryHarvestShrooms(m))
                            return;
                        break;
                }

                if (def != null && FindValidTile(m, def, out toHarvest))
                {
                    system.StartHarvesting(m, tool, toHarvest);
                    return;
                }

                system.OnBadHarvestTarget(m, tool, new LandTarget(new Point3D(0, 0, 0), Map.Felucca));
            }
        }
		static bool HarvestLoop = false;
        private static bool FindValidTile(Mobile m, HarvestDefinition definition, out object toHarvest)
        {
            Map map = m.Map;
            toHarvest = null;

            if (m == null || map == null || map == Map.Internal)
                return false;
			
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

							//if( id != 1360 && ( id >= 1339 && id <= 1363 ) || ( id >= 4789 && id <= 4807 ) )
                            if (definition.Validate(id))
                            {
                                toHarvest = new StaticTarget(new Point3D(x, y, tile.Z), tile.ID);
                                return true;
                            }
                        }
                    }

                    LandTile lt = map.Tiles.GetLandTile(x, y);

                    if (definition.Validate(lt.ID))
                    {
                        toHarvest = new LandTarget(new Point3D(x, y, lt.Z), map);
						//if( lt.ID >= 1981 && lt.ID <= 2000 )
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryHarvestGrave(Mobile m)
        {
            Map map = m.Map;

            if (map == null)
                return false;

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
                            PlayerMobile player = m as PlayerMobile;

                            if (player != null)
                            {
                                QuestSystem qs = player.Quest;

                                if (qs is WitchApprenticeQuest)
                                {
                                    FindIngredientObjective obj = qs.FindObjective(typeof(FindIngredientObjective)) as FindIngredientObjective;

                                    if (obj != null && !obj.Completed && obj.Ingredient == Ingredient.Bones)
                                    {
                                        player.SendLocalizedMessage(1055037); // You finish your grim work, finding some of the specific bones listed in the Hag's recipe.
                                        obj.Complete();

                                        return true;
                                    }
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

            if (map == null)
                return false;

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
                            PlayerMobile player = m as PlayerMobile;

                            if (player != null)
                            {
                                QuestSystem qs = player.Quest;

                                if (qs is WitchApprenticeQuest)
                                {
                                    FindIngredientObjective obj = qs.FindObjective(typeof(FindIngredientObjective)) as FindIngredientObjective;

                                    if (obj != null && !obj.Completed && obj.Ingredient == Ingredient.RedMushrooms)
                                    {
                                        player.SendLocalizedMessage(1055036); // You slice a red cap mushroom from its stem.
                                        obj.Complete();

                                        return true;
                                    }
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
        public FurnitureAttribute()
        {
        }        

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
            if (item == null)
            {
                return false;
            }
			
			if (IsNotChoppables(item))
			{
				return false;
			}

            if (item.GetType().IsDefined(typeof(FurnitureAttribute), false))
            {
                return true;
            }

            if (item is AddonComponent && ((AddonComponent)item).Addon != null && ((AddonComponent)item).Addon.GetType().IsDefined(typeof(FurnitureAttribute), false))
            {
                return true;
            }

            return false;
        }
    }
}
