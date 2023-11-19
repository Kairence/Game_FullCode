using Server;
using System;
using Server.Mobiles;
using Server.Engines.Despise;
using System.Collections.Generic;
using System.Linq;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class CityTeleporter : Teleporter
    {
        [Constructable]
        public CityTeleporter()
        {
			Name = "도시 귀환 게이트";
			ItemID = 3948;
			Hue = 1150;
			Visible = true;
        }
		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
			if (from.InRange(GetWorldLocation(), 1))
				CanTeleport(from);
		}			
        public override bool CanTeleport(Mobile m)
        {
            if (m is BaseCreature)
                return false;

            return base.CanTeleport(m);
        }

        public override void DoTeleport(Mobile m)
        {
			if (m.IsPlayer() || !m.Hidden)
				m.Send(new PlaySound(0x20E, m.Location));

			m.CloseGump(typeof(CitygateConfirmGump));
			m.SendGump(new CitygateConfirmGump(m, this));			
        }

		public virtual void EndConfirmation(Mobile m)
		{
            Map map = MapDest;

            if (map == null || map == Map.Internal)
                map = m.Map;

            Point3D p = PointDest;

            if (p == Point3D.Zero)
                p = m.Location;

            TeleportPets(m, p, map);

            bool sendEffect = (!m.Hidden || m.AccessLevel == AccessLevel.Player);

            if (SourceEffect && sendEffect)
                Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);

			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				pm.PlayerMove(false);
				if (DestEffect && sendEffect)
					Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);

				if (SoundID > 0 && sendEffect)
					Effects.PlaySound(m.Location, m.Map, SoundID);
			}
		}			

        public static void TeleportPets(Mobile master, Point3D loc, Map map)
        {
            var move = new List<Mobile>();
            IPooledEnumerable eable = master.GetMobilesInRange(3);

            foreach (Mobile m in eable)
            {
                if (m is BaseCreature && !(m is DespiseCreature))
                {
                    BaseCreature pet = (BaseCreature)m;

                    if (pet.Controlled && pet.ControlMaster == master)
                    {
                        if (pet.ControlOrder == OrderType.Guard || pet.ControlOrder == OrderType.Follow ||
                            pet.ControlOrder == OrderType.Come)
                        {
                            move.Add(pet);
                        }
                    }
                }
            }

            eable.Free();

            foreach (Mobile m in move)
            {
                m.MoveToWorld(loc, map);
            }

            move.Clear();
            move.TrimExcess();
        }

        public CityTeleporter(Serial serial)
            : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
		}
    }
	
	public class CitygateConfirmGump : Gump
	{
		private readonly Mobile m_From;
		private readonly CityTeleporter m_Gate;

		public CitygateConfirmGump(Mobile from, CityTeleporter gate)
			: base(Core.AOS ? 110 : 20, Core.AOS ? 100 : 30)
		{
			m_From = from;
			m_Gate = gate;

			if (Core.AOS)
			{
				Closable = false;

				AddPage(0);

				AddBackground(0, 0, 420, 280, 5054);

				AddImageTiled(10, 10, 400, 20, 2624);
				AddAlphaRegion(10, 10, 400, 20);

				AddHtmlLocalized(10, 10, 400, 20, 1062051, 30720, false, false); // Gate Warning

				AddImageTiled(10, 40, 400, 200, 2624);
				AddAlphaRegion(10, 40, 400, 200);

				AddHtmlLocalized(
					10,
					40,
					400,
					200,
					1062049,
					32512,
					false,
					true); // Dost thou wish to step into the moongate? Continue to enter the gate, Cancel to stay here

				AddImageTiled(10, 250, 400, 20, 2624);
				AddAlphaRegion(10, 250, 400, 20);

				AddButton(10, 250, 4005, 4007, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(40, 250, 170, 20, 1011036, 32767, false, false); // OKAY

				AddButton(210, 250, 4005, 4007, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(240, 250, 170, 20, 1011012, 32767, false, false); // CANCEL
			}
			else
			{
				AddPage(0);

				AddBackground(0, 0, 420, 400, 5054);
				AddBackground(10, 10, 400, 380, 3000);

				AddHtml(
					20,
					40,
					380,
					60,
					@"도시로 돌아가시려면 도시 게이트를 타세요.",
					false,
					false);

				AddHtmlLocalized(55, 110, 290, 20, 1011012, false, false); // CANCEL
				AddButton(20, 110, 4005, 4007, 0, GumpButtonType.Reply, 0);

				AddHtmlLocalized(55, 140, 290, 40, 1011011, false, false); // CONTINUE
				AddButton(20, 140, 4005, 4007, 1, GumpButtonType.Reply, 0);
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info.ButtonID == 1)
				m_Gate.EndConfirmation(m_From);
		}
	}	
}