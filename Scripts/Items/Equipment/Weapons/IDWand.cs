using System;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class IDWand : BaseWand
    {
        private int m_Grade; // 0:일반, 1:희귀, 2:영웅, 3:서사, 4:전설, 5:신화

        [CommandProperty(AccessLevel.GameMaster)]
        public int Grade
        {
            get { return m_Grade; }
            set { m_Grade = value; InvalidateProperties(); }
        }

        [Constructable]
        public IDWand() : this(0)
        {
        }

        public IDWand(int grade) : base(WandEffect.Identification, 100, 100) // 차지 100 고정
        {
            m_Grade = grade;
            Name = "아이템 감정 완드";
        }

        public IDWand(Serial serial) : base(serial)
        {
        }

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
		}

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            string[] gradeNames = { "일반", "희귀", "영웅", "서사", "전설", "신화" };
            string name = (m_Grade >= 0 && m_Grade < gradeNames.Length) ? gradeNames[m_Grade] : "알 수 없음";

            // 완드의 성능 표시
            list.Add(1060659, "감정 가능 한계\t{0} 등급", name);
        }

        public override bool OnWandTarget(Mobile from, object o)
        {
            if (o is Item item && item is IEquipOption equip)
            {
				if (!from.BeginAction(typeof(ItemIdentification)))
				{
					from.SendMessage(0x22, "이미 아이템을 분석 중입니다.");
					return false; // 차지도 안 깎이고 타겟 종료
				}
                if (item.RootParent != from)
                {
                    from.SendMessage("가방에 있는 아이템만 강화/감정할 수 있습니다.");
					from.EndAction(typeof(ItemIdentification));
                    return false;
                }

                // 1. 완드 등급과 아이템 등급 비교 (아이템 등급이 완드 등급보다 높으면 불가)
                int itemGrade = equip.SuffixOption[1];

                if (m_Grade < itemGrade)
                {
                    from.SendMessage(0x22, "이 완드의 등급으로는 해당 아이템을 감정할 수 없습니다.");
					from.EndAction(typeof(ItemIdentification));
                    return false;
                }

                // 2. 차지 체크 및 강화 시퀀스 시작
                if (Charges > 0)
                {
                    // ItemIdentification에 정의된 강화/감정 로직 호출
                    int currentStep = equip.SuffixOption[10];
                    int iterations = ItemIdentification.GetIterations(currentStep);
                    
                    from.SendMessage(0x35, "완드의 마력을 집중하여 아이템을 분석합니다...");
                    
					if (item != null && equip != null)
					{
						// ... 차지 소모 및 메시지 ...
						from.NextSkillTime = Core.TickCount + (int)TimeSpan.FromSeconds((iterations + 1) * 2.1).TotalMilliseconds;
						new ItemIdentification.EnhanceTimer(from, item, iterations).Start();
					}
                }
                else
                {
                    from.SendMessage("완드의 차지가 모두 소모되었습니다.");
					from.EndAction(typeof(ItemIdentification));
					return false;
                }

                return true;
            }

            from.SendMessage("이 아이템에는 완드를 사용할 수 없습니다.");
			from.EndAction(typeof(ItemIdentification));
            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
            writer.Write(m_Grade);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Grade = reader.ReadInt();
        }
    }
}