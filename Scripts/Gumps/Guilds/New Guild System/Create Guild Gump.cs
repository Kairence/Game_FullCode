using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Guilds
{
    public class CreateGuildGump : Gump
    {
        // 상인이 상주하는 본거지 가능 도시 전체 목록 (14개 주요 상업 도시)
        public static readonly string[] AvailableTowns = new string[]
        {
            "Britain", "Trinsic", "Minoc", "Moonglow",
            "Yew", "Jhelom", "Skara Brae", "Vesper",
            "Cove", "Nujel'm", "Haven", "Serpent's Hold",
            "Delucia", "Papua"
        };

        private readonly PlayerMobile m_Player;

        public CreateGuildGump(PlayerMobile pm)
            : this(pm, "", "", 0, 0)
        {
        }

        public CreateGuildGump(PlayerMobile pm, string guildName, string guildAbbrev, int townIndex = 0, int tendIndex = 0)
            : base(120, 50)
        {
            m_Player = pm;

            pm.CloseGump(typeof(CreateGuildGump));
            pm.CloseGump(typeof(BaseGuildGump));

            AddPage(0);

            // 1. 메인 윈도우 배경 (가로 560, 세로 400)
            AddBackground(0, 0, 560, 400, 0x24AE);
            AddAlphaRegion(10, 10, 540, 380);

            // 2. 상단 헤더 영역
            AddImageTiled(12, 12, 536, 28, 0x2486);
            AddHtml(15, 15, 530, 22, "<basefont color=#FFD700><center><b>길드 및 세력 창설</b></center></basefont>", false, false);
            AddImageTiled(12, 42, 536, 2, 0x2711);

            // 3. 안내 문구
            AddHtml(25, 47, 510, 20, "<basefont color=#EAEAEA>창설 비용: <basefont color=#FFD700><b>25,000 Gold</b></basefont> (은행 금고에서 자동 출금)</basefont>", false, false);
            AddImageTiled(12, 69, 536, 2, 0x2711);

            // 4. 입력 섹션
            // 4-1. 길드 명칭
            AddHtml(25, 77, 75, 20, "<basefont color=#FFD700><b>길드 명칭</b></basefont>", false, false);
            AddBackground(100, 74, 435, 26, 0x2486);
            AddTextEntry(108, 77, 420, 20, 0x0481, 5, guildName ?? "");

            // 4-2. 길드 약칭 (태그)
            AddHtml(25, 107, 75, 20, "<basefont color=#FFD700><b>길드 약칭</b></basefont>", false, false);
            AddBackground(100, 104, 120, 26, 0x2486);
            AddTextEntry(108, 107, 105, 20, 0x0481, 6, guildAbbrev ?? "");
            AddHtml(230, 107, 290, 20, "<basefont color=#A0A0A0><small>(최대 4자, 머리 위 칭호로 표기)</small></basefont>", false, false);

            AddImageTiled(12, 134, 536, 2, 0x2711);

            // 5. 본거지 도시 선택 (4열 x 4행 = 14개 도시)
            AddHtml(25, 139, 510, 20, "<basefont color=#FFD700><b>본거지 도시 선택</b></basefont>", false, false);

            AddGroup(1); // 도시 선택 그룹

            for (int i = 0; i < AvailableTowns.Length; i++)
            {
                int col = i % 4;
                int row = i / 4;
                int rx = 25 + (col * 130);
                int ry = 161 + (row * 24);

                bool isSelected = (i == townIndex);
                AddRadio(rx, ry, 0xD0, 0xD1, isSelected, 100 + i);
                AddHtml(rx + 24, ry + 2, 100, 20, string.Format("<basefont color=#{0}><b>{1}</b></basefont>", isSelected ? "FFFFFF" : "CCCCCC", AvailableTowns[i]), false, false);
            }

            AddImageTiled(12, 260, 536, 2, 0x2711);

            // 6. 길드 성향 선택 (3열 x 2행 = 6대 성향)
            AddHtml(25, 266, 510, 20, "<basefont color=#FFD700><b>길드 성향 선택</b></basefont>", false, false);

            AddGroup(2); // 성향 선택 그룹

            // 1행: 전투형, 탐험형, 채집형
            AddRadio(25, 290, 0xD0, 0xD1, tendIndex == 0, 200);
            AddHtml(50, 292, 140, 20, "<basefont color=#FF5555><b>전투형</b></basefont>", false, false);

            AddRadio(205, 290, 0xD0, 0xD1, tendIndex == 1, 201);
            AddHtml(230, 292, 140, 20, "<basefont color=#00CCFF><b>탐험형</b></basefont>", false, false);

            AddRadio(385, 290, 0xD0, 0xD1, tendIndex == 2, 202);
            AddHtml(410, 292, 140, 20, "<basefont color=#E0A868><b>채집형</b></basefont>", false, false);

            // 2행: 제작형, 상업형, 친목형
            AddRadio(25, 318, 0xD0, 0xD1, tendIndex == 3, 203);
            AddHtml(50, 320, 140, 20, "<basefont color=#66FF66><b>제작형</b></basefont>", false, false);

            AddRadio(205, 318, 0xD0, 0xD1, tendIndex == 4, 204);
            AddHtml(230, 320, 140, 20, "<basefont color=#FFD700><b>상업형</b></basefont>", false, false);

            AddRadio(385, 318, 0xD0, 0xD1, tendIndex == 5, 205);
            AddHtml(410, 320, 140, 20, "<basefont color=#FF99CC><b>친목형</b></basefont>", false, false);

            AddImageTiled(12, 347, 536, 2, 0x2711);

            // 7. 하단 버튼 영역
            // 취소 버튼
            AddBackground(320, 356, 100, 30, 0x2486);
            AddButton(326, 361, 0x15E3, 0x15E7, 0, GumpButtonType.Reply, 0);
            AddHtml(348, 361, 65, 20, "<basefont color=#E0E0E0><center><b>취소</b></center></basefont>", false, false);

            // 창설 버튼
            AddBackground(430, 356, 105, 30, 0x2486);
            AddButton(436, 361, 0x15E3, 0x15E7, 1, GumpButtonType.Reply, 0);
            AddHtml(458, 361, 65, 20, "<basefont color=#FFD700><center><b>창설</b></center></basefont>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;

            if (pm == null || pm.Guild != null)
                return;

            switch (info.ButtonID)
            {
                case 1: // 창설
                    {
                        TextRelay tName = info.GetTextEntry(5);
                        TextRelay tAbbrev = info.GetTextEntry(6);

                        string guildName = (tName == null) ? "" : tName.Text;
                        string guildAbbrev = (tAbbrev == null) ? "" : tAbbrev.Text;

                        guildName = Utility.FixHtml(guildName.Trim());
                        guildAbbrev = Utility.FixHtml(guildAbbrev.Trim());

                        // 도시 선택값 추출
                        int townIdx = 0;
                        for (int i = 0; i < AvailableTowns.Length; i++)
                        {
                            if (info.IsSwitched(100 + i))
                            {
                                townIdx = i;
                                break;
                            }
                        }
                        string selectedTown = AvailableTowns[townIdx];

                        // 성향 선택값 추출 (0: 전투, 1: 탐험, 2: 채집, 3: 제작, 4: 상업, 5: 친목)
                        GuildTendency selectedTendency = GuildTendency.Combat;
                        int tendIdx = 0;
                        if (info.IsSwitched(201))
                        {
                            selectedTendency = GuildTendency.Exploration;
                            tendIdx = 1;
                        }
                        else if (info.IsSwitched(202))
                        {
                            selectedTendency = GuildTendency.Gathering;
                            tendIdx = 2;
                        }
                        else if (info.IsSwitched(203))
                        {
                            selectedTendency = GuildTendency.Crafting;
                            tendIdx = 3;
                        }
                        else if (info.IsSwitched(204))
                        {
                            selectedTendency = GuildTendency.Commerce;
                            tendIdx = 4;
                        }
                        else if (info.IsSwitched(205))
                        {
                            selectedTendency = GuildTendency.Social;
                            tendIdx = 5;
                        }

                        if (guildName.Length <= 0)
                        {
                            pm.SendMessage("길드 이름을 입력해야 합니다.");
                            pm.SendGump(new CreateGuildGump(pm, guildName, guildAbbrev, townIdx, tendIdx));
                        }
                        else if (guildAbbrev.Length <= 0)
                        {
                            pm.SendMessage("길드 약칭(태그)을 입력해야 합니다.");
                            pm.SendGump(new CreateGuildGump(pm, guildName, guildAbbrev, townIdx, tendIdx));
                        }
                        else if (guildName.Length > Guild.NameLimit)
                        {
                            pm.SendMessage("길드 이름은 최대 {0}자 이하여야 합니다.", Guild.NameLimit);
                            pm.SendGump(new CreateGuildGump(pm, guildName, guildAbbrev, townIdx, tendIdx));
                        }
                        else if (guildAbbrev.Length > Guild.AbbrevLimit)
                        {
                            pm.SendMessage("길드 약칭은 최대 {0}자 이하여야 합니다.", Guild.AbbrevLimit);
                            pm.SendGump(new CreateGuildGump(pm, guildName, guildAbbrev, townIdx, tendIdx));
                        }
                        else if (Guild.FindByAbbrev(guildAbbrev) != null || !BaseGuildGump.CheckProfanity(guildAbbrev))
                        {
                            pm.SendMessage("해당 약칭은 이미 사용 중이거나 사용할 수 없는 문자입니다.");
                            pm.SendGump(new CreateGuildGump(pm, guildName, guildAbbrev, townIdx, tendIdx));
                        }
                        else if (Guild.FindByName(guildName) != null || !BaseGuildGump.CheckProfanity(guildName))
                        {
                            pm.SendMessage("해당 길드 이름은 이미 사용 중이거나 사용할 수 없는 문자입니다.");
                            pm.SendGump(new CreateGuildGump(pm, guildName, guildAbbrev, townIdx, tendIdx));
                        }
                        else if (!Banker.Withdraw(pm, Guild.RegistrationFee))
                        {
                            pm.SendMessage("은행 잔고가 부족합니다. 길드 창설에는 {0} Gold가 필요합니다.", Guild.RegistrationFee);
                            pm.SendGump(new CreateGuildGump(pm, guildName, guildAbbrev, townIdx, tendIdx));
                        }
                        else
                        {
                            pm.SendMessage("{0} Gold가 은행 잔고에서 인출되었습니다.", Guild.RegistrationFee);
                            Guild newGuild = new Guild(pm, guildName, guildAbbrev, selectedTown, selectedTendency);
                            pm.Guild = newGuild;

                            string tendName = selectedTendency switch
                            {
                                GuildTendency.Combat => "전투형",
                                GuildTendency.Exploration => "탐험형",
                                GuildTendency.Gathering => "채집형",
                                GuildTendency.Crafting => "제작형",
                                GuildTendency.Commerce => "상업형",
                                GuildTendency.Social => "친목형",
                                _ => "일반"
                            };

                            pm.SendMessage("새로운 길드 '{0}'[{1}]이(가) 창설되었습니다! 본거지: {2} | 성향: {3}", guildName, guildAbbrev, selectedTown, tendName);
                            pm.SendGump(new GuildInfoGump(pm, newGuild));
                        }

                        break;
                    }
            }
        }
    }
}
