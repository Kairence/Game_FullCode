using System;
using System.Collections.Generic;
using Server;
using Server.Commands.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Targeting;

namespace Server.Commands
{
	public class ItemSaveInfoCommand
	{
		private static int target_number = 0;

		public static void Initialize()
		{
			CommandSystem.Register("Item", AccessLevel.Player, new CommandEventHandler(ItemSaveInfo_OnCommand));
		}

		[Usage("Status")]
		[Description("���� �ݰ� ��� Ȯ��.")]
		public static void ItemSaveInfo_OnCommand(CommandEventArgs e)
		{
			var sub = "";
			if (e.Length > 0)
				sub = e.GetString(0);

			if (e.Mobile is PlayerMobile)
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				if (pm.IsStaff() && sub == "")
				{
					for (int i = 0; i < 100; i++)
					{
						if (pm.ItemSave[i] == null)
							continue;
						else if (pm.ItemSave[i] is Item)
						{
							Item item = pm.ItemSave[i] as Item;
							string name = item.Name;
							if (item.Name == null)
								name = String.Format("#{0}", item.LabelNumber);
							pm.SendMessage("{0}�� : {1}", i + 1, name);
						}
					}
				}
				if (sub != "")
				{
					sub.ToLower();
					bool savecheck = sub.Contains("save");
					bool loadcheck = sub.Contains("load");
					Int32.TryParse(sub.Substring(4), out target_number);

					if (target_number > 100)
						return;

					if (savecheck)
					{
						e.Mobile.Target = new InternalTarget();
					}
					else if (loadcheck && target_number >= 1 && pm.ItemSave[target_number - 1] is Item)
					{
						Item item = pm.ItemSave[target_number - 1] as Item;
						if (e.Mobile.InRange(item.GetWorldLocation(), 1))
							item.OnDoubleClick(e.Mobile);
						else
						{
							bool check = false;
							Container pack = pm.Backpack;
							if (pack != null)
							{
								List<Item> Search = pack.FindItemsByType<Item>();
								for (int i = Search.Count - 1; i >= 0; i--)
								{
									Item Next = Search[i];
									if (Next.GetType() == item.GetType())
									{
										check = true;
										pm.ItemSave[target_number - 1] = Next;
										Next.OnDoubleClick(e.Mobile);
										break;
									}
								}
							}
							if (!check)
							{
								pm.ItemSave[target_number - 1] = null;
								pm.SendMessage("�ش� �������� ���ų� ����� �� �����ϴ�!");
							}
						}
					}
				}

				/*
				else if( sub == "save1" )
				{
					//Ÿ��1 ����
					target_number = 1;
				}
				else if( pm.ItemSave[target_number -1] != null && sub == "load1" )
				{
					if( pm.ItemSave[target_number -1] is Item )
					{
						Item i = pm.ItemSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);

					}
				}
				else if( sub == "save2" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 2;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load2" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				else if( sub == "save3" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 3;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load3" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				else if( sub == "save4" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 4;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load4" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				else if( sub == "save5" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 5;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load5" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				else if( sub == "save6" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 6;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load6" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				else if( sub == "save7" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 7;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load7" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				else if( sub == "save8" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 8;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load8" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				else if( sub == "save9" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 9;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load9" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				else if( sub == "save10" )
				{
					//Ÿ��1 ����
					e.Mobile.Target = new InternalTarget();
					target_number = 10;
				}
				else if( pm.ObjectSave[target_number -1] != null && sub == "load10" )
				{
					if( pm.ObjectSave[target_number -1] is Item )
					{
						Item i = pm.ObjectSave[target_number -1] as Item;
						if (e.Mobile.InRange(i.GetWorldLocation(), 1))
							i.OnDoubleClick(e.Mobile);
						else
							pm.SendMessage("�� �� ������ �־�� �մϴ�!");
					}
				}
				*/
			}
		}

		private class InternalTarget : Target
		{
			public InternalTarget()
				: base(8, false, TargetFlags.None) { }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (target_number > 0 && from is PlayerMobile)
				{
					PlayerMobile pm = from as PlayerMobile;
					if (targeted is Item)
					{
						Item item = targeted as Item;
						pm.ItemSave[target_number - 1] = item;
					}
					else
						pm.SendMessage("�������� �ƴմϴ�!");
				}
			}
		}
	}

	public class MonsterSaveInfoCommand
	{
		private static int target_number = 0;

		public static void Initialize()
		{
			CommandSystem.Register("Monster", AccessLevel.Player, new CommandEventHandler(MonsterSaveInfo_OnCommand));
		}

		[Usage("Status")]
		[Description("���� �ݰ� ��� Ȯ��.")]
		public static void MonsterSaveInfo_OnCommand(CommandEventArgs e)
		{
			var sub = "";
			if (e.Length > 0)
				sub = e.GetString(0);

			if (e.Mobile is PlayerMobile)
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				if (pm.IsStaff() && sub == "")
				{
					for (int i = 0; i < 100; i++)
					{
						if (pm.MonsterSave[i] == null)
							continue;
						else
						{
							string item = pm.MonsterSave[i];
							pm.SendMessage("{0}�� : {1}", i + 1, item);
						}
					}
				}
				if (sub != "")
				{
					sub.ToLower();
					bool savecheck = sub.Contains("save");
					bool loadcheck = sub.Contains("load");
					Int32.TryParse(sub.Substring(4), out target_number);

					if (target_number > 100)
						return;

					if (savecheck)
					{
						e.Mobile.Target = new InternalTarget();
					}
					else if (loadcheck && target_number >= 1)
					{
						string item = pm.MonsterSave[target_number - 1];

						List<Mobile> Search = new List<Mobile>();
						IPooledEnumerable eable = pm.GetMobilesInRange(20);

						bool check = false;

						foreach (Mobile Next in eable)
						{
							if (Next.GetType().Name == item && pm.CanBeHarmful(Next) && pm.CanSee(Next))
							{
								Search.Add(Next);
								//e.Mobile.OnDoubleClick(bc);
								break;
							}
						}
						eable.Free();
						if (Search.Count > 0)
						{
							if (Search[0] is BaseCreature)
							{
								BaseCreature bc = Search[0] as BaseCreature;
								if (bc.IsMonster)
								{
									bc.OnDoubleClick(pm);
									if (pm.Target != null)
									{
										pm.Target.Invoke(pm, bc);
										pm.ClearTarget();
										NetState ns = pm.NetState;
										if (pm != null)
											ns.Send(CancelTarget.Instance);
									}
									pm.Combatant = bc;
								}
							}
						}
						else
							pm.SendMessage("�ش� ���Ͱ� �����ϴ�!");
					}
				}
			}
		}

		private class InternalTarget : Target
		{
			public InternalTarget()
				: base(8, false, TargetFlags.None) { }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (target_number > 0 && from is PlayerMobile)
				{
					PlayerMobile pm = from as PlayerMobile;
					if (targeted is Mobile)
					{
						Mobile item = targeted as Mobile;
						if (item is BaseCreature)
						{
							BaseCreature bc = item as BaseCreature;
							if (bc.IsMonster)
							{
								pm.MonsterSave[target_number - 1] = bc.GetType().Name;
								pm.Combatant = bc;
								/*
								bc.OnDoubleClick(pm);
								if( pm.Target != null )
								{
									pm.Target.Invoke( from, targeted );
									NetState ns = pm.NetState;
									if( pm != null )
										ns.Send(CancelTarget.Instance);
								}
								*/
							}
							else
								pm.SendMessage("���Ͱ� �ƴմϴ�!");
						}
						else
							pm.SendMessage("���Ͱ� �ƴմϴ�!");
					}
					else
						pm.SendMessage("���Ͱ� �ƴմϴ�!");
				}
			}
		}
	}

	public class PetSaveInfoCommand
	{
		private static int target_number = 0;

		public static void Initialize()
		{
			CommandSystem.Register("Pet", AccessLevel.Player, new CommandEventHandler(PetSaveInfo_OnCommand));
		}

		[Usage("Status")]
		[Description("���� �ݰ� ��� Ȯ��.")]
		public static void PetSaveInfo_OnCommand(CommandEventArgs e)
		{
			var sub = "";
			if (e.Length > 0)
				sub = e.GetString(0);

			if (e.Mobile is PlayerMobile)
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				if (pm.IsStaff() && sub == "")
				{
					for (int i = 0; i < 100; i++)
					{
						if (pm.PetSave[i] == null)
							continue;
						else
						{
							Mobile item = pm.PetSave[i];
							pm.SendMessage("{0}�� : {1}", i + 1, item);
						}
					}
				}
				if (sub != "")
				{
					sub.ToLower();
					bool savecheck = sub.Contains("save");
					bool loadcheck = sub.Contains("load");
					Int32.TryParse(sub.Substring(4), out target_number);

					if (target_number > 100)
						return;

					if (savecheck)
					{
						e.Mobile.Target = new InternalTarget();
					}
					else if (loadcheck && target_number >= 1)
					{
						Mobile item = pm.PetSave[target_number - 1];

						if (
							item != null
							&& (pm.Mount == item || pm.InRange(item.Location, 20) && pm.CanSee(item))
							&& item is BaseCreature
						)
						{
							BaseCreature bc = item as BaseCreature;
							if (!bc.IsMonster)
							{
								if (pm.Mount == item)
									pm.OnDoubleClick(pm);
								else
									bc.OnDoubleClick(pm);
								if (pm.Target != null)
								{
									pm.Target.Invoke(pm, bc);
									pm.ClearTarget();
									NetState ns = pm.NetState;
									if (pm != null)
										ns.Send(CancelTarget.Instance);
								}
							}
						}
						else
							pm.SendMessage("�ش� ���� �����ϴ�!");
					}
				}
			}
		}

		private class InternalTarget : Target
		{
			public InternalTarget()
				: base(8, false, TargetFlags.None) { }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (target_number > 0 && from is PlayerMobile)
				{
					PlayerMobile pm = from as PlayerMobile;
					if (targeted is Mobile)
					{
						Mobile item = targeted as Mobile;
						if (item is BaseCreature)
						{
							BaseCreature bc = item as BaseCreature;
							if (!bc.IsMonster)
							{
								pm.PetSave[target_number - 1] = bc;
								/*
								bc.OnDoubleClick(pm);
								if( pm.Target != null )
								{
									pm.Target.Invoke( from, targeted );
									NetState ns = pm.NetState;
									if( pm != null )
										ns.Send(CancelTarget.Instance);
								}
								*/
							}
							else
								pm.SendMessage("���� �ƴմϴ�!");
						}
						else
							pm.SendMessage("���� �ƴմϴ�!");
					}
					else
						pm.SendMessage("���� �ƴմϴ�!");
				}
			}
		}
	}

	public class PlayerSaveInfoCommand
	{
		private static int target_number = 0;

		public static void Initialize()
		{
			CommandSystem.Register("Player", AccessLevel.Player, new CommandEventHandler(PlayerSaveInfo_OnCommand));
		}

		[Usage("Status")]
		[Description("���� �ݰ� ��� Ȯ��.")]
		public static void PlayerSaveInfo_OnCommand(CommandEventArgs e)
		{
			var sub = "";
			if (e.Length > 0)
				sub = e.GetString(0);

			if (e.Mobile is PlayerMobile)
			{
				PlayerMobile pm = e.Mobile as PlayerMobile;
				if (pm.IsStaff() && sub == "")
				{
					for (int i = 0; i < 100; i++)
					{
						if (pm.PlayerSave[i] == null)
							continue;
						else
						{
							Mobile item = pm.PlayerSave[i];
							pm.SendMessage("{0}�� : {1}", i + 1, item);
						}
					}
				}
				if (sub != "")
				{
					sub.ToLower();
					bool savecheck = sub.Contains("save");
					bool loadcheck = sub.Contains("load");
					Int32.TryParse(sub.Substring(4), out target_number);

					if (target_number > 100)
						return;

					if (savecheck)
					{
						e.Mobile.Target = new InternalTarget();
					}
					else if (loadcheck && target_number >= 1)
					{
						Mobile item = pm.PlayerSave[target_number - 1];

						if (item != null && pm.InRange(item.Location, 20) && pm.CanSee(item) && item is PlayerMobile)
						{
							PlayerMobile bc = item as PlayerMobile;
							bc.OnDoubleClick(pm);
							if (pm.Spell != null)
							{
								pm.Target.Invoke(pm, bc);
								pm.ClearTarget();
								NetState ns = pm.NetState;
								if (pm != null)
									ns.Send(CancelTarget.Instance);
							}
						}
						else
							pm.SendMessage("�ش� ������ �����ϴ�!");
					}
				}
			}
		}

		private class InternalTarget : Target
		{
			public InternalTarget()
				: base(8, false, TargetFlags.None) { }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (target_number > 0 && from is PlayerMobile)
				{
					PlayerMobile pm = from as PlayerMobile;
					if (targeted is Mobile)
					{
						Mobile item = targeted as Mobile;
						if (item is PlayerMobile)
						{
							PlayerMobile bc = item as PlayerMobile;
							pm.PlayerSave[target_number - 1] = bc;
							/*
							bc.OnDoubleClick(pm);
							if( pm.Target != null )
							{
								pm.Target.Invoke( from, targeted );
								NetState ns = pm.NetState;
								if( pm != null )
									ns.Send(CancelTarget.Instance);
							}
							*/
						}
						else
							pm.SendMessage("������ �ƴմϴ�!");
					}
					else
						pm.SendMessage("������ �ƴմϴ�!");
				}
			}
		}
	}
}
