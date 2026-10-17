using System;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Guilds
{
    public class GuildVaultGump : BaseGuildGump
    {
        public GuildVaultGump(PlayerMobile pm, Guild g)
            : base(pm, g)
        {
            PopulateGump();
        }

        public override void PopulateGump()
        {
            base.PopulateGump();

            // 1. 길드 자원 창고 상단 배너
            AddImageTiled(25, 75, 550, 28, 0x283E);
            AddHtml(30, 80, 540, 22, "<basefont color=#FFD700><center><b>길드 및 세력 금고 (GUILD RESOURCE VAULT)</b></center></basefont>", false, false);

            // 2. NPC 시민/모험가 상납 자원 원장 (인출 불가 - 업그레이드 전용)
            AddBackground(25, 110, 270, 240, 0x2422);
            AddHtml(35, 120, 250, 24, "<basefont color=#33FF99><b>가상 시민·모험가 상납 자원</b></basefont>", false, false);
            AddHtml(35, 142, 250, 36, "<basefont color=#B0B0B0><small>* NPC들이 공헌한 자원입니다.<br>* 유저 인출 불가 (길드 강화 전용)</small></basefont>", false, false);

            AddImageTiled(35, 180, 250, 2, 0x2711);

            // NPC 골드
            AddHtml(40, 195, 110, 22, "<basefont color=#FFD700><b>상납 골드:</b></basefont>", false, false);
            AddHtml(150, 195, 130, 22, string.Format("<basefont color=#FFFFFF>{0:N0} GP</basefont>", guild.NpcGold), false, false);

            // NPC 목재
            AddHtml(40, 230, 110, 22, "<basefont color=#CD853F><b>상납 목재:</b></basefont>", false, false);
            AddHtml(150, 230, 130, 22, string.Format("<basefont color=#FFFFFF>{0:N0} 개</basefont>", guild.NpcWood), false, false);

            // NPC 철광석
            AddHtml(40, 265, 110, 22, "<basefont color=#B0C4DE><b>상납 철광:</b></basefont>", false, false);
            AddHtml(150, 265, 130, 22, string.Format("<basefont color=#FFFFFF>{0:N0} 개</basefont>", guild.NpcIron), false, false);

            AddHtml(35, 305, 250, 36, "<basefont color=#888888><small>도시 경제 활동 및 원정을 통해<br>자동으로 길드 창고에 누적됩니다.</small></basefont>", false, false);

            // 3. 플레이어 가문 예치 자원 원장 (입출금 가능)
            AddBackground(305, 110, 270, 240, 0x2422);
            AddHtml(315, 120, 250, 24, "<basefont color=#66CCFF><b>가문(유저) 예치 자원</b></basefont>", false, false);
            AddHtml(315, 142, 250, 36, "<basefont color=#B0B0B0><small>* 길드원이 자발적으로 예치한 자원입니다.<br>* 언제든 자유롭게 회수 가능합니다.</small></basefont>", false, false);

            AddImageTiled(315, 180, 250, 2, 0x2711);

            // 플레이어 골드
            AddHtml(320, 195, 100, 22, "<basefont color=#FFD700><b>예치 골드:</b></basefont>", false, false);
            AddHtml(420, 195, 140, 22, string.Format("<basefont color=#FFFFFF>{0:N0} GP</basefont>", guild.PlayerGold), false, false);

            // 플레이어 목재
            AddHtml(320, 230, 100, 22, "<basefont color=#CD853F><b>예치 목재:</b></basefont>", false, false);
            AddHtml(420, 230, 140, 22, string.Format("<basefont color=#FFFFFF>{0:N0} 개</basefont>", guild.PlayerWood), false, false);

            // 플레이어 철광석
            AddHtml(320, 265, 100, 22, "<basefont color=#B0C4DE><b>예치 철광:</b></basefont>", false, false);
            AddHtml(420, 265, 140, 22, string.Format("<basefont color=#FFFFFF>{0:N0} 개</basefont>", guild.PlayerIron), false, false);

            // 자원 예치 / 회수 버튼
            AddBackground(320, 305, 115, 30, 0x2486);
            AddButton(325, 310, 0x845, 0x846, 20, GumpButtonType.Reply, 0); // 예치 (10,000gp)
            AddHtml(348, 310, 85, 20, "<basefont color=#FFD700>1만GP 예치</basefont>", false, false);

            AddBackground(445, 305, 115, 30, 0x2486);
            AddButton(450, 310, 0x845, 0x846, 21, GumpButtonType.Reply, 0); // 회수 (10,000gp)
            AddHtml(473, 310, 85, 20, "<basefont color=#FF9999>1만GP 회수</basefont>", false, false);

            // 하단 정보 요약
            AddImageTiled(25, 360, 550, 2, 0x2711);

            int totalGold = guild.NpcGold + guild.PlayerGold;
            AddHtml(35, 370, 530, 22, string.Format("<basefont color=#E0E0E0>길드 총 보유 자산: <basefont color=#FFD700><b>{0:N0} GP</b></basefont> | 본거지: <basefont color=#FFFFFF><b>{1}</b></basefont></basefont>", totalGold, guild.HomeTown ?? "Britain"), false, false);

            AddHtml(35, 395, 530, 22, "<basefont color=#888888>* 이 자원들은 향후 길드 요새 구축, NPC 사병 무장, 도시 시설 투자에 사용됩니다.</basefont>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            base.OnResponse(sender, info);

            PlayerMobile pm = sender.Mobile as PlayerMobile;
            if (pm == null || !IsMember(pm, guild))
                return;

            switch (info.ButtonID)
            {
                case 20: // 10,000 GP 예치
                    {
                        const int depositAmount = 10000;
                        if (Banker.Withdraw(pm, depositAmount))
                        {
                            guild.PlayerGold += depositAmount;
                            pm.SendMessage("은행에서 {0:N0} GP를 인출하여 길드 창고에 예치했습니다. (현재 예치금: {1:N0} GP)", depositAmount, guild.PlayerGold);
                        }
                        else
                        {
                            pm.SendMessage("은행 잔고가 부족합니다. (필요: {0:N0} GP)", depositAmount);
                        }
                        pm.SendGump(new GuildVaultGump(pm, guild));
                        break;
                    }
                case 21: // 10,000 GP 회수
                    {
                        const int withdrawAmount = 10000;
                        if (guild.PlayerGold >= withdrawAmount)
                        {
                            guild.PlayerGold -= withdrawAmount;
                            Banker.Deposit(pm, withdrawAmount);
                            pm.SendMessage("길드 창고에서 {0:N0} GP를 회수하여 은행에 입금했습니다. (남은 예치금: {1:N0} GP)", withdrawAmount, guild.PlayerGold);
                        }
                        else
                        {
                            pm.SendMessage("회수할 수 있는 유저 예치 골드가 부족합니다. (현재 예치금: {0:N0} GP, NPC 상납금은 회수할 수 없습니다)", guild.PlayerGold);
                        }
                        pm.SendGump(new GuildVaultGump(pm, guild));
                        break;
                    }
            }
        }
    }
}
