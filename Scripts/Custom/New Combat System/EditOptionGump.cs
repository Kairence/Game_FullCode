using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Commands;
using Server.Targeting;
using Server.Gumps;
using Server.Network;

namespace Server.Misc
{
    class EditOptionCommand
    {
        public static void Initialize()
        {
            // [EditOption 명령어로 실행
            CommandSystem.Register("EditOption", AccessLevel.GameMaster, new CommandEventHandler(OnCommand));
        }

        private static void OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("옵션을 수정할 통합 옵션 아이템을 선택하세요.");
            e.Mobile.Target = new InternalTarget();
        }

        private class InternalTarget : Target
        {
            public InternalTarget() : base(-1, false, TargetFlags.None) { }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is IEquipOption item)
                {
                    // 타겟팅 성공 시 0페이지 Gump 오픈
                    from.SendGump(new EditOptionGump(from, item, 0));
                }
                else
                {
                    from.SendMessage("IEquipOption 속성을 가진 아이템이 아닙니다.");
                }
            }
        }
    }

    class EditOptionGump : Gump
    {
        private readonly Mobile _from;
        private readonly IEquipOption _item;
        private readonly int _page;

        private const int MaxSlots = 100; // Prefix/Suffix 배열의 최대 크기 (필요시 조절)
        private const int SlotsPerPage = 10; // 한 페이지에 10개씩 출력

        public EditOptionGump(Mobile from, IEquipOption item, int page) : base(50, 50)
        {
            _from = from;
            _item = item;
            _page = page;

            AddPage(0);
            AddBackground(0, 0, 420, 430, 5054);
            AddImageTiled(10, 10, 400, 410, 2624);
            AddAlphaRegion(10, 10, 400, 410);

            // 타이틀
            AddHtml(10, 15, 400, 20, $"<CENTER><COLOR=#FFFFFF>통합 옵션 에디터 (Page {_page + 1})</CENTER>", false, false);
            
            if (item is Item itemObj)
            {
                AddHtml(10, 35, 400, 20, $"<CENTER><COLOR=#FFFF00>{itemObj.Name ?? "No Name"} ({itemObj.LabelNumber})</CENTER>", false, false);
            }

            // 테이블 헤더
            AddLabel(30, 65, 1152, "슬롯 (Index)");
            AddLabel(150, 65, 1152, "Prefix (옵션 ID)");
            AddLabel(280, 65, 1152, "Suffix (원시 수치)");

            int startIndex = _page * SlotsPerPage;
            int y = 90;

            // 슬롯 10개 출력 (텍스트 입력창 포함)
            for (int i = 0; i < SlotsPerPage; i++)
            {
                int index = startIndex + i;
                if (index >= MaxSlots) break;

                // 인덱스 번호 (0, 1, 2 ...)
                AddLabel(50, y + 2, 2100, index.ToString());

                // Prefix 입력칸 (TextEntry ID: 1000 + i)
                AddImageTiled(140, y, 80, 20, 0xBBC);
                AddTextEntry(142, y, 76, 20, 0, 1000 + i, _item.PrefixOption[index].ToString());

                // Suffix 입력칸 (TextEntry ID: 2000 + i)
                AddImageTiled(270, y, 100, 20, 0xBBC);
                AddTextEntry(272, y, 96, 20, 0, 2000 + i, _item.SuffixOption[index].ToString());

                y += 25;
            }

            // 하단 컨트롤 버튼
            AddButton(160, 385, 247, 248, 1, GumpButtonType.Reply, 0); // [Save & Close] 버튼

            // 페이징 버튼 (이전 / 다음)
            if (_page > 0)
            {
                AddButton(20, 385, 4014, 4015, 2, GumpButtonType.Reply, 0); // Prev
                AddLabel(55, 385, 1152, "Prev");
            }

            if (startIndex + SlotsPerPage < MaxSlots)
            {
                AddButton(360, 385, 4005, 4006, 3, GumpButtonType.Reply, 0); // Next
                AddLabel(320, 385, 1152, "Next");
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) // 우클릭 닫기
                return;

            // 현재 페이지의 입력값 저장 로직
            int startIndex = _page * SlotsPerPage;
            for (int i = 0; i < SlotsPerPage; i++)
            {
                int index = startIndex + i;
                if (index >= MaxSlots) break;

                TextRelay prefixRelay = info.GetTextEntry(1000 + i);
                TextRelay suffixRelay = info.GetTextEntry(2000 + i);

                if (prefixRelay != null && int.TryParse(prefixRelay.Text, out int pVal))
                    _item.PrefixOption[index] = pVal;

                if (suffixRelay != null && int.TryParse(suffixRelay.Text, out int sVal))
                    _item.SuffixOption[index] = sVal;
            }

            // 우리가 만든 EquipOptionCreate 호출!
            // TotalAttrs 총합 재계산 및 OPL(InvalidateProperties) 자동 갱신됨
            if (_item is Item itemObj)
            {
                ItemOptionCreator.EquipOptionCreate(itemObj);
            }

            // 버튼 동작 분기
            switch (info.ButtonID)
            {
                case 1: // Save & Close
                    _from.SendMessage("아이템 옵션이 성공적으로 저장 및 갱신되었습니다.");
                    break;

                case 2: // Prev
                    _from.SendGump(new EditOptionGump(_from, _item, _page - 1));
                    break;

                case 3: // Next
                    _from.SendGump(new EditOptionGump(_from, _item, _page + 1));
                    break;
            }
        }
    }
}