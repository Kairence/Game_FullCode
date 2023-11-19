using Server;
using System;
using Server.Mobiles;
using Server.Engines.Despise;
using System.Collections.Generic;
using System.Linq;
using Server.Gumps;
using Server.Network;
using Server.Regions;

namespace Server.Items
{
    public class HiddenTeleporter : Teleporter
    {
        [Constructable]
        public HiddenTeleporter()
        {
			ItemID = 3948;
			Hue = 0;
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
			if ( m_TeleportPoint < 38 )
				return false;
            if (m is BaseCreature)
                return false;

            return base.CanTeleport(m);
        }

		private Map map = null;
		private Point3D p = Point3D.Zero;

		private int m_TeleportPoint;
		[CommandProperty( AccessLevel.GameMaster )]
		public int TeleportPoint
		{
			get{ return m_TeleportPoint;}
			set{ m_TeleportPoint = value; InvalidateProperties();}
		}		
		private bool TicketCheck(PlayerMobile pm)
		{
			//38 : 코베투스 2층 -> 거미 던전 (이후 테라탄 킵 -> 네이비 둥지까지 이어짐)
			//39 : 코베투스 3층 -> 소서리스 던전
			//40 : 데스파이즈 3층 -> 미궁
			//41 : 디싯 2층 -> 정신병동 지하
			//42 : 디싯 3층 -> 정신병동 옥상
			//43 : 디싯 4층 -> 정신병동 던전
			//44 : 쉐임 2층 -> 타이탄 섬 (배를 꺼내서 숨겨진 크라켄을 잡으면 보너스 있음)
			//45 : 쉐임 3층 -> 비홀더 보스
			//46 : 쉐임 4층 -> 레비아탄 보스
			//47 : 오크 던전 2층 -> 
			for( int i = 38; i < 48; i++ )
			{
				if( pm.SilverPoint[i] >= 1 )
				{
					if( i == 38 ) //5단계 독 확인
					{
						if( pm.Poison == Poison.Lethal )
						{
							pm.Poison = null;
						}
						else
							return false;
					}
					MapDest = MapSelect[i - 38];
					PointDest = LocationSelect[i - 38];
					pm.SilverPoint[i]--;
					return true;
				}
			}
			return false;
		}
		
		private static Map[] MapSelect = 
		{
			Map.Ilshenar, //38 5레벨 독이 걸린 상태에서 가장 큰 거미줄 통과 (이동 시 독 회복 됨)
			Map.Ilshenar, // 찢어진 그림에서 비밀키를 외치면 이동
			Map.Malas, //40 로프를 사용해서 이동
			Map.Malas, //41 악마 뼈를 100개 드레그 해서 이동
			Map.Malas, //42 초가 켜진 상태라면 이동
			Map.Malas, //43 스위치를 더블클릭할 때 배고픔이 0이 되어야 함 (배고픔이 Max가 됨)
			Map.Trammel, //44 지정된 자리에서 낚시를 10회 진행 후 이동
			Map.Trammel, //45 마비 상태에서 책장 클릭 시 이동 (이동 시 마비 해제 됨)
			Map.Trammel, //46 물 정령이 소환된 상태에서 이동
			Map.Trammel, //47 손도끼로 그루터기 클릭
			Map.Trammel  //48
		};
		
		private static Point3D[] LocationSelect = 
		{
			new Point3D( 1785, 994, -29 ), //38
			new Point3D( 566, 454, -3 ), //39 [add static 284
			new Point3D( 338, 1952, 5), //40
			new Point3D( 156, 1625, 0 ), //41
			new Point3D( 172, 1740, 50 ), //42
			new Point3D( 111, 1620, 90 ), //43
			new Point3D( 6444, 1236, 10), //44
			new Point3D( 6861, 1195, 0), //45
			new Point3D( 6848, 1408, 0), //46
			new Point3D( 0, 0, 0), //47
			new Point3D( 6650, 191, 0)  //48
		};
		
        public override void DoTeleport(Mobile m)
        {
			if( m is PlayerMobile && !m.Hidden )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( TicketCheck( pm ) )
				{
					m.Send(new PlaySound(0x20E, m.Location));
					EndConfirmation(m);
				}
			}
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

			m.MoveToWorld(p, map);
			
			/*
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				pm.PlayerMove(false);
				if (DestEffect && sendEffect)
					Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);

				if (SoundID > 0 && sendEffect)
					Effects.PlaySound(m.Location, m.Map, SoundID);
			}
			*/
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

        public HiddenTeleporter(Serial serial)
            : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
			
			writer.Write(m_TeleportPoint);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int v = reader.ReadInt();
			
			m_TeleportPoint = reader.ReadInt();
		}
    }
}