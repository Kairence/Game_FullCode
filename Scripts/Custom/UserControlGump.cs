using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Commands;
using Server.Targeting;

namespace Server.Custom
{
    public enum UserControlTab
    {
        Hotkeys = 0,    // 1. 단축키
        Combat = 0,     // (호환성 유지용 별칭)
        Utility = 1,    // 2. 편의
        Account = 2,    // 3. 계정
        Achievement = 3 // 4. 업적
    }

    public class UserControlCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("menu", AccessLevel.Player, new CommandEventHandler(OnCommand));
            CommandSystem.Register("menu]", AccessLevel.Player, new CommandEventHandler(OnCommand));
            CommandSystem.Register("관리", AccessLevel.Player, new CommandEventHandler(OnCommand));
            CommandSystem.Register("관리]", AccessLevel.Player, new CommandEventHandler(OnCommand));
            CommandSystem.Register("통합관리", AccessLevel.Player, new CommandEventHandler(OnCommand));

            CommandSystem.Register("hotkey", AccessLevel.Player, new CommandEventHandler(OnHotkeyCommand));
            CommandSystem.Register("단축키", AccessLevel.Player, new CommandEventHandler(OnHotkeyCommand));
            CommandSystem.Register("hotbar", AccessLevel.Player, new CommandEventHandler(OnHotbarCommand));
            CommandSystem.Register("핫바", AccessLevel.Player, new CommandEventHandler(OnHotbarCommand));
            CommandSystem.Register("combatbook", AccessLevel.Player, new CommandEventHandler(OnCombatBookCommand));
            CommandSystem.Register("무기책", AccessLevel.Player, new CommandEventHandler(OnCombatBookCommand));
        }

        [Usage("menu")]
        [Description("카이렌스 통합 관리 창을 엽니다.")]
        public static void OnCommand(CommandEventArgs e)
        {
            if (e.Mobile is PlayerMobile pm)
            {
                pm.CloseGump(typeof(UserControlGump));
                pm.SendGump(new UserControlGump(pm, UserControlTab.Hotkeys));
            }
        }

        public static void OnHotkeyCommand(CommandEventArgs e)
        {
            if (e.Mobile is PlayerMobile pm)
                pm.SendMessage(0x35, "[단축키 매니저] 클라이언트 단축키 매니저를 엽니다.");
        }

        public static void OnHotbarCommand(CommandEventArgs e)
        {
            if (e.Mobile is PlayerMobile pm)
                pm.SendMessage(0x42, "[단축키 액션바] 화면 1~10 액션바를 토글합니다.");
        }

        public static void OnCombatBookCommand(CommandEventArgs e)
        {
            if (e.Mobile is PlayerMobile pm)
                pm.SendMessage(0x55, "[전사 무기책] 전사 무기 특수기 도감을 엽니다.");
        }
    }

    public class HotkeySlot
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public int ActionId { get; set; }

        public HotkeySlot(string category, string name, int actionId)
        {
            Category = category;
            Name = name;
            ActionId = actionId;
        }

        public bool IsEmpty => string.IsNullOrEmpty(Name) || Name == "[대기 상태]" || Name == "[대기]";
    }

    public static class HotkeySystem
    {
        private static readonly System.Collections.Generic.Dictionary<Mobile, HotkeySlot[]> m_Slots = new System.Collections.Generic.Dictionary<Mobile, HotkeySlot[]>();

        public static HotkeySlot[] GetSlots(Mobile m)
        {
            if (m == null) return new HotkeySlot[10];

            if (!m_Slots.TryGetValue(m, out HotkeySlot[] slots))
            {
                slots = new HotkeySlot[10];
                slots[0] = new HotkeySlot("전투", "방어구 무시", 1);
                slots[1] = new HotkeySlot("전투", "출혈 공격", 2);
                slots[2] = new HotkeySlot("마법", "화염 오오라", 101);
                slots[3] = new HotkeySlot("펫", "펫 일점사", 201);
                slots[4] = new HotkeySlot("생존", "붕대 치료", 301);
                for (int i = 5; i < 10; i++)
                {
                    slots[i] = new HotkeySlot("대기", "[대기]", 0);
                }
                m_Slots[m] = slots;
            }
            return slots;
        }

        public static void SetSlot(Mobile m, int index, string category, string name, int actionId)
        {
            if (m == null || index < 0 || index >= 10) return;
            var slots = GetSlots(m);
            slots[index] = new HotkeySlot(category, name, actionId);
        }

        public static void ClearSlot(Mobile m, int index)
        {
            if (m == null || index < 0 || index >= 10) return;
            var slots = GetSlots(m);
            slots[index] = new HotkeySlot("대기", "[대기]", 0);
        }
    }

    public class HotkeyItemDef
    {
        public int ActionId { get; set; }
        public int CategoryIndex { get; set; }
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }

        public HotkeyItemDef(int actionId, int catIdx, string catName, string name, string tag)
        {
            ActionId = actionId;
            CategoryIndex = catIdx;
            CategoryName = catName;
            Name = name;
            Tag = tag;
        }
    }

    public class UserControlGump : Gump
    {
        private readonly PlayerMobile m_Player;
        private readonly UserControlTab m_CurrentTab;
        private readonly int m_SelectedSlot;

        public static readonly HotkeyItemDef[] AllHotkeyItems = new HotkeyItemDef[]
        {
            // Category 0: 전사 특수기
            new(1,  0, "전투", "방어구 무시", "기력 60"),
            new(2,  0, "전투", "출혈 공격", "기력 40"),
            new(3,  0, "전투", "충격파", "기력 50"),
            new(4,  0, "전투", "파괴 일격", "기력 60"),
            new(5,  0, "전투", "무장 해제", "기력 60"),
            new(6,  0, "전투", "낙마 공격", "기력 60"),
            new(7,  0, "전투", "연속 공격", "기력 100"),
            new(13, 0, "전투", "소용돌이", "기력 100"),

            // Category 1: 오오라 / 마법
            new(101, 1, "마법", "화염 오오라", "마나 100"),
            new(102, 1, "마법", "냉기 오오라", "마나 100"),
            new(103, 1, "마법", "번개 오오라", "마나 100"),
            new(104, 1, "마법", "독 오오라", "마나 100"),
            new(105, 1, "마법", "원소 결계", "방어 강화"),
            new(106, 1, "마법", "자연의 회복", "지속 재생"),

            // Category 2: 펫 지정 / 명령
            new(201, 2, "펫", "펫 일점사", "All Kill"),
            new(202, 2, "펫", "펫 집합", "Follow Me"),
            new(203, 2, "펫", "펫 정지", "All Stop"),
            new(204, 2, "펫", "펫 호위", "Guard Me"),
            new(205, 2, "펫", "펫 탑승", "Mount"),
            new(206, 2, "펫", "펫 상태창", "Status"),

            // Category 3: 오브젝트 / 생존
            new(301, 3, "생존", "붕대 치료", "자가 치료"),
            new(302, 3, "생존", "물약 섭취", "체/기/마"),
            new(303, 3, "생존", "적 자동 조준", "근접 우선"),
            new(304, 3, "생존", "선택 대상 공격", "즉시 공격"),
            new(305, 3, "생존", "마지막 적 공격", "재공격"),
            new(306, 3, "생존", "마지막 마법", "재시전")
        };

        private const int GumpWidth = 620;
        private const int GumpHeight = 480;

        public UserControlGump(PlayerMobile pm, UserControlTab tab = UserControlTab.Hotkeys, int selectedSlot = 0) : base(80, 80)
        {
            m_Player = pm;
            m_CurrentTab = tab;
            m_SelectedSlot = Math.Clamp(selectedSlot, 0, 9);

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            // Page 0: 모든 탭에서 공통으로 보이는 배경 및 상단 헤더
            AddPage(0);

            // 메인 외곽 프레임 (어두운 가젯/보더 스타일)
            AddBackground(0, 0, GumpWidth, GumpHeight, 5054);
            AddImageTiled(10, 10, GumpWidth - 20, GumpHeight - 20, 2624);
            AddAlphaRegion(10, 10, GumpWidth - 20, GumpHeight - 20);

            // 상단 타이틀 바
            AddImageTiled(15, 15, GumpWidth - 30, 32, 9274);
            AddHtml(15, 20, GumpWidth - 30, 24, "<CENTER><BASEFONT COLOR=#FFD700><BIG><B>카이렌스 통합 관리 창</B></BIG></BASEFONT></CENTER>", false, false);

            // 닫기 버튼 (우측 상단 Quit 버튼: 눌러도 Mail로 바뀌지 않도록 동일 그래픽 0x158B 유지)
            AddButton(GumpWidth - 45, 18, 0x158B, 0x158B, 0, GumpButtonType.Reply, 0);

            // 내부 컨텐츠 프레임 (Y: 95 ~ 465)
            AddImageTiled(15, 95, GumpWidth - 30, GumpHeight - 110, 9204);
            AddAlphaRegion(18, 98, GumpWidth - 36, GumpHeight - 116);

            // 탭 목록과 각 탭의 시작 페이지 번호 매핑
            // Page 1: 1. 단축키
            // Page 2~3: 2. 편의 (2: 1페이지, 3: 2페이지)
            // Page 4: 3. 계정
            // Page 5: 4. 업적
            string[] tabNames = { "1. 단축키", "2. 편의", "3. 계정", "4. 업적" };
            int[] tabStartPages = { 1, 2, 4, 5 };
            int tabCount = tabNames.Length;
            int startX = 18;
            int tabWidth = (GumpWidth - 36) / tabCount;
            int tabY = 56;

            for (int p = 1; p <= 5; p++)
            {
                AddPage(p);

                // 현재 페이지가 속한 활성 탭 인덱스 계산
                int activeTabIndex = 0;
                if (p == 1) activeTabIndex = 0;
                else if (p == 2 || p == 3) activeTabIndex = 1;
                else if (p == 4) activeTabIndex = 2;
                else if (p == 5) activeTabIndex = 3;

                // 탭 버튼 헤더 렌더링
                for (int i = 0; i < tabCount; i++)
                {
                    bool isSelected = (i == activeTabIndex);
                    int x = startX + (i * tabWidth);

                    if (isSelected)
                    {
                        // 활성 탭: 밝은 대리석 바 + 골드 체크 라디오 + 골드 텍스트
                        AddImageTiled(x, tabY, tabWidth - 4, 30, 9274);
                        AddImage(x + 10, tabY + 7, 4006);
                        AddLabel(x + 34, tabY + 6, 1153, tabNames[i]);
                    }
                    else
                    {
                        // 비활성 탭: 어두운 바 + 클릭 가능한 라디오 버튼
                        AddImageTiled(x, tabY, tabWidth - 4, 30, 9204);
                        AddButton(x + 10, tabY + 7, 4005, 4007, 0, GumpButtonType.Page, tabStartPages[i]);
                        AddLabel(x + 34, tabY + 6, 1152, tabNames[i]);
                    }
                }

                // 페이지별 컨텐츠
                switch (p)
                {
                    case 1:
                        RenderHotkeysTab();
                        break;
                    case 2:
                        RenderUtilityTab(1);
                        break;
                    case 3:
                        RenderUtilityTab(2);
                        break;
                    case 4:
                        RenderAccountTab();
                        break;
                    case 5:
                        RenderAchievementTab();
                        break;
                }
            }
        }

        #region 탭별 렌더링

        // 1. 단축키 탭 (심플 2대 카드)
        private void RenderHotkeysTab()
        {
            // 카드 1: 통합 단축키 매니저 (1~10 슬롯 & 액션바 & 보관함)
            int c1Y = 120;
            AddImageTiled(25, c1Y, 560, 95, 9274);
            AddAlphaRegion(27, c1Y + 2, 556, 91);
            AddHtml(45, c1Y + 18, 380, 24, "<BASEFONT COLOR=#00FFCC><BIG><B>통합 단축키 매니저 (1~10 슬롯 & 보관함)</B></BIG></BASEFONT>", false, false);
            AddHtml(45, c1Y + 48, 380, 20, "<BASEFONT COLOR=#CCCCCC>1~10번 액션바 슬롯 및 특수기 / 몬스터 / 펫 / 아이템 보관함</BASEFONT>", false, false);
            AddButton(440, c1Y + 30, 4005, 4007, 101, GumpButtonType.Reply, 0);
            AddHtml(475, c1Y + 32, 100, 20, "<BASEFONT COLOR=#FFFF77><B>매니저 열기</B></BASEFONT>", false, false);

            // 카드 2: 전사 무기 특수기 도감 (무기책)
            int c2Y = 235;
            AddImageTiled(25, c2Y, 560, 95, 9274);
            AddAlphaRegion(27, c2Y + 2, 556, 91);
            AddHtml(45, c2Y + 18, 380, 24, "<BASEFONT COLOR=#00FFCC><BIG><B>전사 무기 특수기 도감 (무기책)</B></BIG></BASEFONT>", false, false);
            AddHtml(45, c2Y + 48, 380, 20, "<BASEFONT COLOR=#CCCCCC>30종 전사 무기 특수기 상세 스펙 및 쿨타임 도감</BASEFONT>", false, false);
            AddButton(440, c2Y + 30, 4005, 4007, 103, GumpButtonType.Reply, 0);
            AddHtml(475, c2Y + 32, 100, 20, "<BASEFONT COLOR=#FFFFFF><B>도감 열기</B></BASEFONT>", false, false);
        }


        private void RenderUtilityTab(int subPage)
        {
            int contentX = 30;
            int contentY = 105;

            AddHtml(contentX, contentY, 550, 24, "<BASEFONT COLOR=#FFCC00><BIG><B>[ 편의 & 자동화 관리 ]</B></BIG></BASEFONT>", false, false);

            if (subPage == 1)
            {
                contentY += 28;

                // 1. Loop 자동 반복 섹션 (H: 66)
                AddImageTiled(contentX, contentY, 550, 66, 9274);
                AddAlphaRegion(contentX + 2, contentY + 2, 546, 62);

                bool isLoopOn = m_Player.Loop;
                string loopStatus = isLoopOn ? "<BASEFONT COLOR=#00FF00>[ 실행 중 (ON) ]</BASEFONT>" : "<BASEFONT COLOR=#FF4444>[ 정지 (OFF) ]</BASEFONT>";
                AddHtml(contentX + 15, contentY + 8, 380, 20, $"<BASEFONT COLOR=#FFFFFF><B>1. 자동 반복 작업 (Loop) :</B></BASEFONT> {loopStatus}", false, false);
                AddHtml(contentX + 15, contentY + 34, 400, 20, $"<BASEFONT COLOR=#AAAAAA>설정 반복 횟수: {m_Player.LoopCount:#,0}회 | 제작/채집/허수아비 자동 수련</BASEFONT>", false, false);

                int loopBtnGump = isLoopOn ? 4017 : 4005;
                AddButton(contentX + 430, contentY + 10, loopBtnGump, loopBtnGump + 1, 201, GumpButtonType.Reply, 0);
                AddHtml(contentX + 465, contentY + 12, 75, 20, isLoopOn ? "<BASEFONT COLOR=#FF7777>중단</BASEFONT>" : "<BASEFONT COLOR=#77FF77>시작</BASEFONT>", false, false);

                contentY += 72;

                // 2. AutoFood 자동 식사 섹션 (H: 66)
                AddImageTiled(contentX, contentY, 550, 66, 9274);
                AddAlphaRegion(contentX + 2, contentY + 2, 546, 62);

                bool isFoodOn = (m_Player.AutoFood != null);
                string foodStatus = isFoodOn ? "<BASEFONT COLOR=#00FF00>[ 작동 중 (ON) ]</BASEFONT>" : "<BASEFONT COLOR=#FF4444>[ 미사용 (OFF) ]</BASEFONT>";
                AddHtml(contentX + 15, contentY + 8, 380, 20, $"<BASEFONT COLOR=#FFFFFF><B>2. 자동 음식 섭취 (Auto Food) :</B></BASEFONT> {foodStatus}", false, false);
                AddHtml(contentX + 15, contentY + 34, 400, 20, $"<BASEFONT COLOR=#AAAAAA>배고픔 25% 이하 시 가방 안의 음식을 스스로 찾아 섭취합니다.</BASEFONT>", false, false);

                int foodBtnGump = isFoodOn ? 4017 : 4005;
                AddButton(contentX + 430, contentY + 10, foodBtnGump, foodBtnGump + 1, 202, GumpButtonType.Reply, 0);
                AddHtml(contentX + 465, contentY + 12, 75, 20, isFoodOn ? "<BASEFONT COLOR=#FF7777>끄기</BASEFONT>" : "<BASEFONT COLOR=#77FF77>켜기</BASEFONT>", false, false);

                contentY += 72;

                // 3. 집 자원 연동 제작 섹션 (H: 66)
                AddImageTiled(contentX, contentY, 550, 66, 9274);
                AddAlphaRegion(contentX + 2, contentY + 2, 546, 62);

                bool isHouseRes = HouseCraftSystem.IsHouseResourceEnabled(m_Player);
                string resStatus = isHouseRes ? "<BASEFONT COLOR=#00FF00>[ 연동 중 (ON) ]</BASEFONT>" : "<BASEFONT COLOR=#FF4444>[ 미사용 (OFF) ]</BASEFONT>";
                AddHtml(contentX + 15, contentY + 8, 350, 20, $"<BASEFONT COLOR=#FFFFFF><B>3. 집 자원 연동 제작 :</B></BASEFONT> {resStatus}", false, false);
                AddHtml(contentX + 15, contentY + 34, 400, 20, "<BASEFONT COLOR=#AAAAAA>집 안에서 제작 시 가방에 재료가 없으면 시큐어 상자 자원 사용</BASEFONT>", false, false);

                int resBtnGump = isHouseRes ? 4017 : 4005;
                AddButton(contentX + 430, contentY + 10, resBtnGump, resBtnGump + 1, 203, GumpButtonType.Reply, 0);
                AddHtml(contentX + 465, contentY + 12, 75, 20, isHouseRes ? "<BASEFONT COLOR=#FF7777>끄기</BASEFONT>" : "<BASEFONT COLOR=#77FF77>켜기</BASEFONT>", false, false);

                contentY += 72;

                // 4. 제작품 자동 보관 섹션 (H: 72)
                AddImageTiled(contentX, contentY, 550, 72, 9274);
                AddAlphaRegion(contentX + 2, contentY + 2, 546, 68);

                bool isAutoStore = HouseCraftSystem.IsAutoStoreEnabled(m_Player);
                Container targetBox = HouseCraftSystem.GetStoreContainer(m_Player);
                string boxName = targetBox != null ? (!string.IsNullOrEmpty(targetBox.Name) ? targetBox.Name : targetBox.GetType().Name) : "[ 미지정 ]";
                string storeStatus = isAutoStore ? "<BASEFONT COLOR=#00FF00>[ 보관 중 (ON) ]</BASEFONT>" : "<BASEFONT COLOR=#FF4444>[ 미사용 (OFF) ]</BASEFONT>";

                AddHtml(contentX + 15, contentY + 8, 335, 20, $"<BASEFONT COLOR=#FFFFFF><B>4. 제작품 자동 보관 :</B></BASEFONT> {storeStatus}", false, false);
                AddHtml(contentX + 15, contentY + 32, 400, 18, $"<BASEFONT COLOR=#FFD700>현재 지정 상자 : </BASEFONT><BASEFONT COLOR=#00FFCC>{boxName}</BASEFONT>", false, false);
                AddHtml(contentX + 15, contentY + 48, 400, 18, "<BASEFONT COLOR=#888888>* 집 안에서 완성된 아이템을 지정된 보관함으로 자동 적재</BASEFONT>", false, false);

                // 상자 지정 버튼
                AddButton(contentX + 355, contentY + 8, 4005, 4007, 205, GumpButtonType.Reply, 0);
                AddHtml(contentX + 385, contentY + 10, 60, 20, "<BASEFONT COLOR=#FFFFFF>상자지정</BASEFONT>", false, false);

                // ON/OFF 토글 버튼
                int storeBtnGump = isAutoStore ? 4017 : 4005;
                AddButton(contentX + 445, contentY + 8, storeBtnGump, storeBtnGump + 1, 204, GumpButtonType.Reply, 0);
                AddHtml(contentX + 475, contentY + 10, 55, 20, isAutoStore ? "<BASEFONT COLOR=#FF7777>끄기</BASEFONT>" : "<BASEFONT COLOR=#77FF77>켜기</BASEFONT>", false, false);

                // 하단 서브 페이지 네비게이션 (1 / 2)
                AddButton(contentX + 190, 432, 4014, 4016, 0, GumpButtonType.Page, 5);
                AddHtml(contentX + 230, 434, 80, 20, "<CENTER><BASEFONT COLOR=#FFD700><B>1 / 2</B></BASEFONT></CENTER>", false, false);
                AddButton(contentX + 315, 432, 4005, 4007, 0, GumpButtonType.Page, 6);
            }
            else // subPage == 2 (추가 확장 슬롯)
            {
                contentY += 35;

                AddImageTiled(contentX, contentY, 550, 260, 9274);
                AddAlphaRegion(contentX + 2, contentY + 2, 546, 256);

                AddHtml(contentX + 20, contentY + 25, 500, 22, "<BASEFONT COLOR=#00FFCC><BIG><B>◆ 추가 편의 기능 슬롯 (준비 중)</B></BIG></BASEFONT>", false, false);
                AddHtml(contentX + 20, contentY + 60, 500, 20, "<BASEFONT COLOR=#CCCCCC>5. 스마트 타겟팅 필터링 옵션 (예정)</BASEFONT>", false, false);
                AddHtml(contentX + 20, contentY + 90, 500, 20, "<BASEFONT COLOR=#CCCCCC>6. 자동 전리품 분류 및 골드 정리 (예정)</BASEFONT>", false, false);
                AddHtml(contentX + 20, contentY + 120, 500, 20, "<BASEFONT COLOR=#CCCCCC>7. 장비 원클릭 수리 및 보관 키트 (예정)</BASEFONT>", false, false);
                AddHtml(contentX + 20, contentY + 150, 500, 20, "<BASEFONT COLOR=#CCCCCC>8. 추가 자동화 도구 슬롯</BASEFONT>", false, false);

                AddHtml(contentX + 20, contentY + 200, 500, 20, "<BASEFONT COLOR=#888888>* 유저 피드백 및 편의 기능 요청에 따라 순차적으로 확장 배치됩니다.</BASEFONT>", false, false);

                // 하단 서브 페이지 네비게이션 (2 / 2)
                AddButton(contentX + 190, 432, 4014, 4016, 0, GumpButtonType.Page, 5);
                AddHtml(contentX + 230, 434, 80, 20, "<CENTER><BASEFONT COLOR=#FFD700><B>2 / 2</B></BASEFONT></CENTER>", false, false);
                AddButton(contentX + 315, 432, 4005, 4007, 0, GumpButtonType.Page, 6);
            }
        }

        private void RenderAccountTab()
        {
            int contentX = 30;
            int contentY = 110;

            AddHtml(contentX, contentY, 550, 24, "<BASEFONT COLOR=#FFCC00><BIG><B>[ 계정 정보 & 비밀번호 변경 ]</B></BIG></BASEFONT>", false, false);
            contentY += 30;

            // 1. 계정 기본 정보
            AddImageTiled(contentX, contentY, 550, 70, 9274);
            AddAlphaRegion(contentX + 2, contentY + 2, 546, 66);

            string acctName = m_Player.Account != null ? m_Player.Account.Username : "None";
            AddHtml(contentX + 15, contentY + 12, 500, 20, $"<BASEFONT COLOR=#FFFFFF><B>계정 아이디 :</B></BASEFONT> <BASEFONT COLOR=#00FFCC>{acctName}</BASEFONT>", false, false);
            AddHtml(contentX + 15, contentY + 38, 500, 20, $"<BASEFONT COLOR=#FFFFFF><B>캐릭터 이름 :</B></BASEFONT> <BASEFONT COLOR=#FFFF77>{m_Player.Name}</BASEFONT>", false, false);

            contentY += 85;

            // 2. 비밀번호 변경 폼
            AddImageTiled(contentX, contentY, 550, 215, 9274);
            AddAlphaRegion(contentX + 2, contentY + 2, 546, 211);

            AddHtml(contentX + 15, contentY + 15, 520, 20, "<BASEFONT COLOR=#FF7777><B>◆ 계정 비밀번호 변경</B></BASEFONT>", false, false);

            // 현재 비밀번호
            AddHtml(contentX + 20, contentY + 48, 140, 20, "<BASEFONT COLOR=#FFFFFF>현재 비밀번호 :</BASEFONT>", false, false);
            AddImageTiled(contentX + 160, contentY + 46, 200, 24, 0xBBC);
            AddTextEntry(contentX + 165, contentY + 48, 190, 20, 0, 1001, "");

            // 새 비밀번호
            AddHtml(contentX + 20, contentY + 83, 140, 20, "<BASEFONT COLOR=#FFFFFF>새 비밀번호 :</BASEFONT>", false, false);
            AddImageTiled(contentX + 160, contentY + 81, 200, 24, 0xBBC);
            AddTextEntry(contentX + 165, contentY + 83, 190, 20, 0, 1002, "");

            // 새 비밀번호 확인
            AddHtml(contentX + 20, contentY + 118, 140, 20, "<BASEFONT COLOR=#FFFFFF>새 비밀번호 확인 :</BASEFONT>", false, false);
            AddImageTiled(contentX + 160, contentY + 116, 200, 24, 0xBBC);
            AddTextEntry(contentX + 165, contentY + 118, 190, 20, 0, 1003, "");

            // 변경 실행 버튼
            AddButton(contentX + 160, contentY + 155, 4005, 4006, 301, GumpButtonType.Reply, 0);
            AddHtml(contentX + 195, contentY + 157, 180, 20, "<BASEFONT COLOR=#FFCC00><B>비밀번호 변경 실행</B></BASEFONT>", false, false);

            AddHtml(contentX + 15, contentY + 188, 520, 20, "<BASEFONT COLOR=#888888>* 비밀번호는 변경 즉시 서버 암호화되어 안전하게 보관됩니다.</BASEFONT>", false, false);
        }

        private void RenderAchievementTab()
        {
            int contentX = 30;
            int contentY = 105;

            AddHtml(contentX, contentY, 550, 24, "<BASEFONT COLOR=#FFCC00><BIG><B>[ 카이렌스 업적 & 숙련도 시스템 ]</B></BIG></BASEFONT>", false, false);
            contentY += 28;

            Server.Accounting.Account acc = m_Player.Account as Server.Accounting.Account;

            int col1X = contentX;
            int col2X = contentX + 280;
            int startRowY = contentY;

            // 1. 채집 숙련도
            AddImageTiled(col1X, startRowY, 270, 48, 9274);
            AddAlphaRegion(col1X + 2, startRowY + 2, 266, 44);
            AddButton(col1X + 10, startRowY + 14, 4005, 4007, 401, GumpButtonType.Reply, 0);
            AddHtml(col1X + 40, startRowY + 6, 120, 20, "<BASEFONT COLOR=#00FA9A><B>채집 숙련도</B></BASEFONT>", false, false);
            AddHtml(col1X + 160, startRowY + 6, 95, 20, "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>자원 채집</BASEFONT></DIV>", false, false);
            AddHtml(col1X + 40, startRowY + 26, 220, 18, "<BASEFONT COLOR=#888888>채집 활동 시 누적 숙련도 확인</BASEFONT>", false, false);

            // 2. 제작 숙련도
            int row2Y = startRowY + 52;
            AddImageTiled(col1X, row2Y, 270, 48, 9274);
            AddAlphaRegion(col1X + 2, row2Y + 2, 266, 44);
            AddButton(col1X + 10, row2Y + 14, 4005, 4007, 402, GumpButtonType.Reply, 0);
            AddHtml(col1X + 40, row2Y + 6, 120, 20, "<BASEFONT COLOR=#00FA9A><B>제작 숙련도</B></BASEFONT>", false, false);
            AddHtml(col1X + 160, row2Y + 6, 95, 20, "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>아이템 제작</BASEFONT></DIV>", false, false);
            AddHtml(col1X + 40, row2Y + 26, 220, 18, "<BASEFONT COLOR=#888888>제작 레벨 및 레시피 숙련 현황</BASEFONT>", false, false);

            // 3. 전투 숙련도
            int row3Y = row2Y + 52;
            AddImageTiled(col1X, row3Y, 270, 48, 9274);
            AddAlphaRegion(col1X + 2, row3Y + 2, 266, 44);
            AddButton(col1X + 10, row3Y + 14, 4005, 4007, 403, GumpButtonType.Reply, 0);
            AddHtml(col1X + 40, row3Y + 6, 120, 20, "<BASEFONT COLOR=#FF4500><B>전투 숙련도</B></BASEFONT>", false, false);
            AddHtml(col1X + 160, row3Y + 6, 95, 20, "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>상세 정보</BASEFONT></DIV>", false, false);
            AddHtml(col1X + 40, row3Y + 26, 220, 18, "<BASEFONT COLOR=#888888>슈퍼 슬레이어 & 몬스터 도감</BASEFONT>", false, false);

            // 4. 던전 마스터리
            int row4Y = row3Y + 52;
            AddImageTiled(col1X, row4Y, 270, 48, 9274);
            AddAlphaRegion(col1X + 2, row4Y + 2, 266, 44);
            AddButton(col1X + 10, row4Y + 14, 4005, 4007, 404, GumpButtonType.Reply, 0);
            AddHtml(col1X + 40, row4Y + 6, 120, 20, "<BASEFONT COLOR=#FFD700><B>던전 마스터리</B></BASEFONT>", false, false);
            AddHtml(col1X + 160, row4Y + 6, 95, 20, "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>능력치 투자</BASEFONT></DIV>", false, false);
            AddHtml(col1X + 40, row4Y + 26, 220, 18, "<BASEFONT COLOR=#888888>대륙별 던전 포인트 및 패시브</BASEFONT>", false, false);

            // Right Column
            // 5. 가문 시스템
            string familyPt = (acc != null && acc.Point != null && acc.Point.Length > 0) ? $"{acc.Point[0]:#,0} Pt" : "0 Pt";
            AddImageTiled(col2X, startRowY, 270, 48, 9274);
            AddAlphaRegion(col2X + 2, startRowY + 2, 266, 44);
            AddButton(col2X + 10, startRowY + 14, 4005, 4007, 405, GumpButtonType.Reply, 0);
            AddHtml(col2X + 40, startRowY + 6, 120, 20, "<BASEFONT COLOR=#87CEFA><B>가문 시스템</B></BASEFONT>", false, false);
            AddHtml(col2X + 155, startRowY + 6, 100, 20, $"<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF><B>{familyPt}</B></BASEFONT></DIV>", false, false);
            AddHtml(col2X + 40, startRowY + 26, 220, 18, "<BASEFONT COLOR=#888888>가문 계정 업적 & 혜택 관리</BASEFONT>", false, false);

            // 6. 유물 포인트
            string artPt = (m_Player.ArtifactPoint != null && m_Player.ArtifactPoint.Length > 0) ? $"{m_Player.ArtifactPoint[0]:#,0} Pt" : "0 Pt";
            AddImageTiled(col2X, row2Y, 270, 48, 9274);
            AddAlphaRegion(col2X + 2, row2Y + 2, 266, 44);
            AddButton(col2X + 10, row2Y + 14, 4005, 4007, 406, GumpButtonType.Reply, 0);
            AddHtml(col2X + 40, row2Y + 6, 120, 20, "<BASEFONT COLOR=#DA70D6><B>유물 포인트</B></BASEFONT>", false, false);
            AddHtml(col2X + 155, row2Y + 6, 100, 20, $"<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF><B>{artPt}</B></BASEFONT></DIV>", false, false);
            AddHtml(col2X + 40, row2Y + 26, 220, 18, "<BASEFONT COLOR=#888888>희귀 유물 수집 및 포인트 현황</BASEFONT>", false, false);

            // 7. 스킬 포인트
            AddImageTiled(col2X, row3Y, 270, 48, 9274);
            AddAlphaRegion(col2X + 2, row3Y + 2, 266, 44);
            AddButton(col2X + 10, row3Y + 14, 4005, 4007, 407, GumpButtonType.Reply, 0);
            AddHtml(col2X + 40, row3Y + 6, 120, 20, "<BASEFONT COLOR=#F0E68C><B>스킬 포인트</B></BASEFONT>", false, false);
            AddHtml(col2X + 160, row3Y + 6, 95, 20, "<DIV ALIGN=RIGHT><BASEFONT COLOR=#FFFFFF>스킬 강화</BASEFONT></DIV>", false, false);
            AddHtml(col2X + 40, row3Y + 26, 220, 18, "<BASEFONT COLOR=#888888>스킬 포인트 투자 및 특화 강화</BASEFONT>", false, false);

            // 8. 명성 & 카르마 미니 카드
            AddImageTiled(col2X, row4Y, 270, 48, 9274);
            AddAlphaRegion(col2X + 2, row4Y + 2, 266, 44);
            AddHtml(col2X + 12, row4Y + 6, 120, 18, $"<BASEFONT COLOR=#AAAAAA>명성(Fame):</BASEFONT> <BASEFONT COLOR=#FFFFFF>{m_Player.Fame:#,0}</BASEFONT>", false, false);
            AddHtml(col2X + 138, row4Y + 6, 125, 18, $"<BASEFONT COLOR=#AAAAAA>카르마(Karma):</BASEFONT> <BASEFONT COLOR=#FFFFFF>{m_Player.Karma:#,0}</BASEFONT>", false, false);
            AddHtml(col2X + 12, row4Y + 26, 245, 18, "<BASEFONT COLOR=#888888>소사리아 명예도 및 성향치</BASEFONT>", false, false);

            // 하단 통합 자산 & 경제 현황 카드 (Y: 345 ~ 430, H: 85)
            int botY = row4Y + 52;
            AddImageTiled(contentX, botY, 550, 85, 9274);
            AddAlphaRegion(contentX + 2, botY + 2, 546, 81);

            long totalPlat = acc != null ? acc.TotalPlat : 0;
            long totalGold = acc != null ? acc.TotalGold : 0;
            long packGold = m_Player.Backpack != null ? m_Player.Backpack.GetAmount(typeof(Server.Items.Gold)) : 0;

            AddHtml(contentX + 15, botY + 8, 520, 20, "<BASEFONT COLOR=#FFD700><B>◆ 계정 금고 및 경제 잔고 (Account Vault & Gold)</B></BASEFONT>", false, false);
            AddHtml(contentX + 20, botY + 32, 250, 20, $"<BASEFONT COLOR=#CCCCCC>계정 금고 (Plat):</BASEFONT> <BASEFONT COLOR=#00FFCC><B>{totalPlat:#,0}</B> Plat</BASEFONT>", false, false);
            AddHtml(contentX + 280, botY + 32, 250, 20, $"<BASEFONT COLOR=#CCCCCC>계정 금고 (Gold):</BASEFONT> <BASEFONT COLOR=#FFD700><B>{totalGold:#,0}</B> Gold</BASEFONT>", false, false);
            AddHtml(contentX + 20, botY + 56, 500, 20, $"<BASEFONT COLOR=#CCCCCC>가방 소지금 (Backpack):</BASEFONT> <BASEFONT COLOR=#FFFF77><B>{packGold:#,0}</B> Gold</BASEFONT>", false, false);
        }

        #endregion

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Player == null || m_Player.Deleted)
                return;

            int btnId = info.ButtonID;

            // 닫기
            if (btnId == 0)
                return;

            // 1. 단축키 탭 버튼 액션
            if (btnId == 101) // 통합 단축키 매니저 (보관함)
            {
                m_Player.SendMessage(0x35, "[단축키 매니저] 클라이언트 단축키 매니저를 엽니다.");
                return;
            }
            else if (btnId == 102) // 화면 1~10 단축키 액션바 (HUD) On/Off
            {
                m_Player.SendMessage(0x42, "[단축키 액션바] 화면 1~10 액션바를 토글합니다.");
                m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Hotkeys));
                return;
            }
            else if (btnId == 103) // 전사 특수기 도감 (무기책)
            {
                m_Player.SendMessage(0x55, "[전사 무기책] 전사 무기 특수기 도감을 엽니다.");
                return;
            }

            // 편의 탭 액션
            if (btnId == 201) // Loop 토글
            {
                m_Player.Loop = !m_Player.Loop;
                if (m_Player.Loop)
                {
                    m_Player.LoopCount = 50000;
                    m_Player.SendMessage(0x35, "반복 작업을 시작합니다. (50,000회)");
                }
                else
                {
                    m_Player.SendMessage(0x22, "반복 작업을 중단합니다.");
                }
                m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Utility));
                return;
            }
            else if (btnId == 202) // AutoFood 토글
            {
                if (m_Player.AutoFood == null)
                {
                    m_Player.SendMessage(0x35, "자동으로 섭취할 음식을 가방에서 선택하세요.");
                    m_Player.Target = new AutoFoodTarget();
                }
                else
                {
                    m_Player.AutoFood = null;
                    m_Player.SendMessage(0x22, "자동 음식 섭취가 해제되었습니다.");
                    m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Utility));
                }
                return;
            }
            else if (btnId == 203) // 집 자원 자동 연동 토글
            {
                bool current = HouseCraftSystem.IsHouseResourceEnabled(m_Player);
                HouseCraftSystem.SetHouseResourceEnabled(m_Player, !current);
                m_Player.SendMessage(!current ? 0x42 : 0x22, $"[하우징 자원 연동] 집 안 시큐어 상자 자원 사용이 {(!current ? "활성화" : "비활성화")}되었습니다.");
                m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Utility));
                return;
            }
            else if (btnId == 204) // 제작품 자동 상자 보관 토글
            {
                bool current = HouseCraftSystem.IsAutoStoreEnabled(m_Player);
                if (!current && HouseCraftSystem.GetStoreContainer(m_Player) == null)
                {
                    m_Player.SendMessage(0x35, "먼저 제작품을 보관할 집 안의 시큐어 상자를 지정해 주세요.");
                    m_Player.Target = new HouseCraftSystem.StoreContainerTarget();
                }
                else
                {
                    HouseCraftSystem.SetAutoStoreEnabled(m_Player, !current);
                    m_Player.SendMessage(!current ? 0x42 : 0x22, $"[제작품 자동 보관] 완성품 자동 상자 보관이 {(!current ? "활성화" : "비활성화")}되었습니다.");
                    m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Utility));
                }
                return;
            }
            else if (btnId == 205) // 제작품 보관함 상자 지정 타겟
            {
                m_Player.SendMessage(0x35, "제작품을 자동으로 보관할 집 안의 시큐어 상자를 선택하세요.");
                m_Player.Target = new HouseCraftSystem.StoreContainerTarget();
                return;
            }

            // 계정 탭 액션: 비밀번호 변경
            if (btnId == 301)
            {
                TextRelay curPassEntry = info.GetTextEntry(1001);
                TextRelay newPassEntry = info.GetTextEntry(1002);
                TextRelay confPassEntry = info.GetTextEntry(1003);

                string curPass = curPassEntry != null ? curPassEntry.Text.Trim() : "";
                string newPass = newPassEntry != null ? newPassEntry.Text.Trim() : "";
                string confPass = confPassEntry != null ? confPassEntry.Text.Trim() : "";

                if (m_Player.Account == null)
                {
                    m_Player.SendMessage(0x22, "계정 정보를 찾을 수 없습니다.");
                    return;
                }

                if (string.IsNullOrEmpty(curPass) || string.IsNullOrEmpty(newPass))
                {
                    m_Player.SendMessage(0x22, "현재 비밀번호와 새 비밀번호를 모두 입력해 주세요.");
                    m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Account));
                    return;
                }

                if (!m_Player.Account.CheckPassword(curPass))
                {
                    m_Player.SendMessage(0x22, "현재 비밀번호가 일치하지 않습니다.");
                    m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Account));
                    return;
                }

                if (newPass != confPass)
                {
                    m_Player.SendMessage(0x22, "새 비밀번호와 확인 입력이 일치하지 않습니다.");
                    m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Account));
                    return;
                }

                if (newPass.Length < 4)
                {
                    m_Player.SendMessage(0x22, "새 비밀번호는 최소 4자리 이상이어야 합니다.");
                    m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Account));
                    return;
                }

                // 비밀번호 변경 실행
                m_Player.Account.SetPassword(newPass);
                m_Player.SendMessage(0x44, "계정 비밀번호가 성공적으로 변경되었습니다!");
                m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Account));
                return;
            }

            // 업적 탭 서브 시스템 호출 (401 ~ 407)
            if (btnId == 401) // 1. 채집 숙련도
            {
                m_Player.SendGump(new HarvestMasteryGump(m_Player, 0));
                return;
            }
            else if (btnId == 402) // 2. 제작 숙련도
            {
                m_Player.SendGump(new CraftMasteryGump(m_Player, 0));
                return;
            }
            else if (btnId == 403) // 3. 전투 숙련도
            {
                m_Player.SendGump(new CombatMasteryGump(m_Player));
                return;
            }
            else if (btnId == 404) // 4. 던전 마스터리
            {
                m_Player.SendGump(new DungeonPointGump(m_Player));
                return;
            }
            else if (btnId == 405) // 5. 가문 시스템
            {
                m_Player.SendGump(new FamilyGump(m_Player, 0));
                return;
            }
            else if (btnId == 406) // 6. 유물 포인트
            {
                int artPt = (m_Player.ArtifactPoint != null && m_Player.ArtifactPoint.Length > 0) ? m_Player.ArtifactPoint[0] : 0;
                m_Player.SendMessage(0x35, $"현재 보유 유물 포인트: {artPt:#,0} Pt 입니다.");
                m_Player.SendGump(new UserControlGump(m_Player, UserControlTab.Achievement));
                return;
            }
            else if (btnId == 407) // 7. 스킬 포인트
            {
                m_Player.SendGump(new SkillPointGump(m_Player));
                return;
            }
        }
    }

    public class AutoFoodTarget : Target
    {
        public AutoFoodTarget() : base(2, false, TargetFlags.None)
        {
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (from == null || from.Deleted)
                return;

            if (from is PlayerMobile pm)
            {
                if (targeted is Food food)
                {
                    pm.AutoFood = food;
                    string foodName = !string.IsNullOrEmpty(food.Name) ? food.Name : food.GetType().Name;
                    pm.SendMessage(0x42, $"[자동 식사] [{foodName}]이(가) 자동 섭취 음식으로 등록되었습니다.");
                    pm.SendGump(new UserControlGump(pm, UserControlTab.Utility));
                }
                else
                {
                    pm.SendMessage(0x22, "음식 아이템만 지정할 수 있습니다.");
                    pm.SendGump(new UserControlGump(pm, UserControlTab.Utility));
                }
            }
        }
    }
}
