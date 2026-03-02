using System;
using System.Collections.Generic;
using Server;
using Server.Commands.Generic;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Commands
{
	public class BaseCreatureAggroScoreInfoCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register(
				"AggroScore",
				AccessLevel.GameMaster,
				new CommandEventHandler(BaseCreatureAggroScoreInfo_OnCommand)
			);
		}

		[Usage("BaseCreatureDelete")]
		[Description("���� ���� �ڵ�.")]
		public static void BaseCreatureAggroScoreInfo_OnCommand(CommandEventArgs e)
		{
			e.Mobile.Target = new InternalTarget();
		}

		private class InternalTarget : Target
		{
			public InternalTarget()
				: base(8, false, TargetFlags.None) { }

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is BaseCreature)
				{
					BaseCreature bc = targeted as BaseCreature;

					for (int i = 0; i < 10000; i++)
					{
						if (bc.AggroMobile[i] == null)
							break;
						from.SendMessage("��׷� {0}, ���� {1}", bc.AggroMobile[i].Name, bc.AggroScore[i]);
					}
				}
			}
		}
	}
}
