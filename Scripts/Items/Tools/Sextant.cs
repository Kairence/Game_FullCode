using System;
using Server.Network;
using Server.Targeting;
using Server.Regions;
using Server.Engines.Craft;
using Server.Mobiles;

namespace Server.Items
{
    public class Sextant : Item
    {
        private int m_UsesRemaining;

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get { return m_UsesRemaining; }
            set { m_UsesRemaining = value; InvalidateProperties(); }
        }

        [Constructable]
        public Sextant() : base(0x1058)
        {
            this.Weight = 2.0;
            m_UsesRemaining = 50;
        }

        public Sextant(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(this.GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446); // Too far away.
                return;
            }

            string coords = GetCoords(from);
            if (!String.IsNullOrEmpty(coords))
                from.LocalOverheadMessage(MessageType.Regular, from.SpeechHue, false, coords);

            from.SendMessage("지도를 그릴 빈 지도나 스크롤을 선택하세요. (남은 횟수: {0})", m_UsesRemaining);
            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private Sextant m_Sextant;

            public InternalTarget(Sextant sextant) : base(1, false, TargetFlags.None)
            {
                m_Sextant = sextant;
            }

			protected override void OnTarget(Mobile from, object targeted)
            {
                if (!(targeted is BlankMap || targeted is BlankScroll))
                {
                    from.SendMessage("그것에는 지도를 그릴 수 없습니다.");
                    return;
                }

                Item targetItem = (Item)targeted;
                Region reg = from.Region;
                CraftSystem system = DefCartography.CraftSystem;

                double minSkill = 0.0; double maxSkill = 50.0;
                int iRandom = 5; // 무조건 4번 반복 (총 5틱)

                if (reg is TownRegion) { minSkill = 100.0; maxSkill = 150.0; }
                else if (reg is DungeonRegion) { minSkill = 200.0; maxSkill = 250.0; }
                else if (reg.Name != null && (reg.Name.Contains("Ruins") || reg.Name.Contains("Temple")))
                {
                    minSkill = 150.0; maxSkill = 200.0;
                }

                if (from.BeginAction(typeof(CraftSystem)))
                {
                    from.SendMessage("지형을 정밀하게 측량하기 시작합니다...");
                    // 타이머에 m_Sextant 인스턴스를 전달하여 내구도 차감이 가능하게 합니다.
                    new SextantInternalTimer(from, system, targetItem, m_Sextant, iRandom, minSkill, maxSkill).Start();
                }
                else
                {
                    from.SendLocalizedMessage(500119);
                }
            }
        }

		private class SextantInternalTimer : Timer
		{
			private Mobile m_From;
			private CraftSystem m_System;
			private Item m_TargetItem;
			private Sextant m_Sextant;
			private int m_iCount;
			private int m_iCountMax;
			private double m_MinSkill;
			private double m_MaxSkill;
			
			// 시작 위치 저장용 변수
			private Point3D m_StartLocation;

			public SextantInternalTimer(Mobile from, CraftSystem system, Item target, Sextant sextant, int iCountMax, double min, double max)
				: base(TimeSpan.Zero, TimeSpan.FromSeconds(system.Delay), iCountMax)
			{
				m_From = from;
				m_System = system;
				m_TargetItem = target;
				m_Sextant = sextant;
				m_iCount = 0;
				m_iCountMax = iCountMax;
				m_MinSkill = min;
				m_MaxSkill = max;
				
				// 타이머 시작 시점의 위치를 기록
				m_StartLocation = from.Location;
			}

			protected override void OnTick()
			{
				// 1. 이동 체크: 현재 위치가 시작 위치와 다르면 취소
				if (m_From.Location != m_StartLocation)
				{
					m_From.EndAction(typeof(CraftSystem));
					m_From.SendMessage("이동하는 바람에 측량이 중단되었습니다.");
					Stop();
					return;
				}

				m_iCount++;
				m_From.DisruptiveAction(); // 공격 등에 의한 방해 체크

				if (m_iCount < m_iCountMax)
				{
					// 애니메이션(모션)과 사운드 재생
					//m_From.Animate(9, 5, 1, true, false, 0); 
					m_System.PlayCraftEffect(m_From);
				}
				else
				{
					// 마지막 틱: 결과 처리
					m_From.EndAction(typeof(CraftSystem));

					double skillValue = m_From.Skills.Cartography.Value;
					m_From.CheckSkill(SkillName.Cartography, m_MaxSkill * 10);

					double chance = 0.25 + (skillValue - m_MinSkill) * 0.0025;
					chance = Math.Clamp(chance, 0.0, 1.0);
					if (skillValue < m_MinSkill && m_MinSkill > 0) chance = 0.0;

					if (Utility.RandomDouble() < chance)
					{
						ProcessSuccess(skillValue, chance);
					}
					else
					{
						m_TargetItem.Consume();
						m_From.SendLocalizedMessage(1044043);
						ConsumeSextant();
					}
				}
			}

            private void ProcessSuccess(double skillValue, double chance)
            {
                Region reg = m_From.Region;
                PresetMapEntry entry = null;

                bool isSpecial = (reg is TownRegion || reg is DungeonRegion ||
                                 (reg.Name != null && (reg.Name.Contains("Ruins") || reg.Name.Contains("Temple"))));

                if (isSpecial)
                {
                    for (int i = 0; i < PresetMapEntry.Table.Length; i++)
                    {
                        if (PresetMapEntry.Table[i].Bounds.Contains(m_From.Location))
                        {
                            entry = PresetMapEntry.Table[i];
                            break;
                        }
                    }
                }

                if (entry != null && skillValue >= m_MinSkill)
                {
                    m_TargetItem.Consume();
                    m_From.AddToBackpack(new PresetMap(entry));
                    m_From.SendMessage($"{reg.Name}의 정밀 지도를 완성했습니다! (성공률: {chance:P1})");
                    m_From.PlaySound(0x249);
                    ConsumeSextant();
                }
                else
                {
                    // 일반 지도 생성 로직 실행
                    DrawGeneralMap();
                }
            }

            private void DrawGeneralMap()
            {
                m_TargetItem.Consume();
                int mapSize = 100 + (int)m_From.Skills.Cartography.Value;
                MapItem mi = new MapItem();
                int x1 = m_From.X - (mapSize / 2);
                int y1 = m_From.Y - (mapSize / 2);
                int x2 = m_From.X + (mapSize / 2);
                int y2 = m_From.Y + (mapSize / 2);

                mi.SetDisplay(x1, y1, x2, y2, mapSize, mapSize);
                mi.Map = m_From.Map;
                mi.AddPin(m_From.X, m_From.Y);
                mi.Name = $"야외 측량 지도 ({Sextant.GetCoords(m_From)})";

                m_From.AddToBackpack(mi);
                m_From.PlaySound(0x249);
                m_From.SendMessage("주변 지형을 측량하여 일반 지도를 제작했습니다.");
                ConsumeSextant();
            }

            private void ConsumeSextant()
            {
                if (m_Sextant == null) return;
                m_Sextant.UsesRemaining--;
                if (m_Sextant.UsesRemaining <= 0)
                {
                    m_Sextant.Delete();
                    m_From.SendMessage("육분의가 마모되어 부서졌습니다.");
                }
            }
        }

        // --- 좌표 계산 및 포맷팅 로직 (전체 코드) ---

        public static string GetCoords(IEntity e)
        {
            return GetCoords(e.Location, e.Map);
        }

        public static string GetCoords(Point3D location, Map map)
        {
            int xLong = 0, yLat = 0;
            int xMins = 0, yMins = 0;
            bool xEast = false, ySouth = false;

            if (Sextant.Format(location, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
            {
                return String.Format("{0}° {1}'{2}, {3}° {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
            }

            return String.Empty;
        }

        public static bool Format(Point3D p, Map map, ref int xLong, ref int yLat, ref int xMins, ref int yMins, ref bool xEast, ref bool ySouth)
        {
            if (map == null || map == Map.Internal)
                return false;

            int x = p.X, y = p.Y;
            int xCenter, yCenter, xWidth, yHeight;

            if (!ComputeMapDetails(map, x, y, out xCenter, out yCenter, out xWidth, out yHeight))
                return false;

            double absLong = (double)((x - xCenter) * 360) / xWidth;
            double absLat = (double)((y - yCenter) * 360) / yHeight;

            if (absLong > 180.0) absLong = -180.0 + (absLong % 180.0);
            if (absLat > 180.0) absLat = -180.0 + (absLat % 180.0);

            xEast = (absLong >= 0);
            ySouth = (absLat >= 0);

            if (absLong < 0.0) absLong = -absLong;
            if (absLat < 0.0) absLat = -absLat;

            xLong = (int)absLong;
            yLat = (int)absLat;
            xMins = (int)((absLong % 1.0) * 60);
            yMins = (int)((absLat % 1.0) * 60);

            return true;
        }
		public static Point3D ReverseLookup(Map map, int xLong, int yLat, int xMins, int yMins, bool xEast, bool ySouth)
        {
            if (map == null || map == Map.Internal)
                return Point3D.Zero;

            int xCenter, yCenter, xWidth, yHeight;

            if (!ComputeMapDetails(map, 0, 0, out xCenter, out yCenter, out xWidth, out yHeight))
                return Point3D.Zero;

            double absLong = xLong + ((double)xMins / 60);
            double absLat = yLat + ((double)yMins / 60);

            if (!xEast)
                absLong = 360.0 - absLong;

            if (!ySouth)
                absLat = 360.0 - absLat;

            int x = xCenter + (int)((absLong * xWidth) / 360);
            int y = yCenter + (int)((absLat * yHeight) / 360);

            if (x < 0)
                x += xWidth;
            else if (x >= xWidth)
                x -= xWidth;

            if (y < 0)
                y += yHeight;
            else if (y >= yHeight)
                y -= yHeight;

            int z = map.GetAverageZ(x, y);

            return new Point3D(x, y, z);
        }
        public static bool ComputeMapDetails(Map map, int x, int y, out int xCenter, out int yCenter, out int xWidth, out int yHeight)
        {
            xWidth = 5120;
            yHeight = 4096;

            if (map == Map.Trammel || map == Map.Felucca)
            {
                if (x >= 0 && y >= 0 && x < 5120 && y < 4096)
                {
                    xCenter = 1323; yCenter = 1624;
                }
                else if (x >= 5120 && y >= 2304 && x < 6144 && y < 4096)
                {
                    xCenter = 5936; yCenter = 3112;
                }
                else
                {
                    xCenter = 0; yCenter = 0; return false;
                }
            }
            else if (x >= 0 && y >= 0 && x < map.Width && y < map.Height)
            {
                xCenter = 1323; yCenter = 1624;
            }
            else
            {
                xCenter = 0; yCenter = 0; return false;
            }

            return true;
        }

        // --- 저장 및 불러오기 ---

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); // version
            writer.Write((int)m_UsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version >= 1)
                m_UsesRemaining = reader.ReadInt();
            else
                m_UsesRemaining = 50;
        }
    }
}