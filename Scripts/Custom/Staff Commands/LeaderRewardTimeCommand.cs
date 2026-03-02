using System;
using Server;
using Server.Accounting;
using Server.Commands.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Commands
{
	public class LeaderRewardTimeInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("LRT", AccessLevel.Owner, new CommandEventHandler(LeaderRewardTimeInfo_OnCommand));
		}

		[Usage("Status")]
		[Description("��ȣ ���� ����")]
		public static void LeaderRewardTimeInfo_OnCommand(CommandEventArgs e)
		{
			if (Server.Event.fsc == null)
			{
				foreach (Item item in World.Items.Values)
				{
					if (item is FirstSkillCheck)
					{
						FirstSkillCheck fs = item as FirstSkillCheck;
						Server.Event.fsc = fs;
						break;
					}
				}
			}
			else if (Server.Event.fsc != null)
			{
				FirstSkillCheck fs = Server.Event.fsc;
				e.Mobile.SendMessage(fs.LeaderRewardTime.ToString());
			}
		}
	}

	public class LeaderRewardTimeResetInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register(
				"LRTS",
				AccessLevel.Owner,
				new CommandEventHandler(LeaderRewardTimeResetInfo_OnCommand)
			);
		}

		[Usage("Status")]
		[Description("��ȣ ���� ����")]
		public static void LeaderRewardTimeResetInfo_OnCommand(CommandEventArgs e)
		{
			if (Server.Event.fsc == null)
			{
				foreach (Item item in World.Items.Values)
				{
					if (item is FirstSkillCheck)
					{
						FirstSkillCheck fs = item as FirstSkillCheck;
						Server.Event.fsc = fs;
						break;
					}
				}
			}
			else if (Server.Event.fsc != null)
			{
				FirstSkillCheck fs = Server.Event.fsc;
				fs.LeaderRewardTime = DateTime.Now - TimeSpan.FromDays(8);
				e.Mobile.SendMessage(fs.LeaderRewardTime.ToString());
			}
		}
	}
}
