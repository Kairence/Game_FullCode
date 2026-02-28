using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Commands;

namespace Server.Commands
{
    public class SPMCommand
    {
        public static void Initialize()
        {
            // [중요] 패킷 리스너는 작동하지 않으므로 제거하고, 명령어만 등록합니다.
            CommandSystem.Register("spm", AccessLevel.Player, new CommandEventHandler(OnSPMCommand));
            
            Console.WriteLine("=== [성공] SPM 기술창 시스템 로드 완료 (명령어: [spm) ===");
        }

        [Usage("spm")]
        public static void OnSPMCommand(CommandEventArgs e)
        {
            // 1. 기술창을 띄워줍니다.
            e.Mobile.CloseGump(typeof(CustomSPMGump));
            e.Mobile.SendGump(new CustomSPMGump(e.Mobile));

            // 2. [선택사항] 만약 페이퍼돌 버튼 클릭 효과를 내고 싶다면 감지기를 띄웁니다.
            // e.Mobile.SendGump(new InvisibleTriggerGump()); 
        }
    }
}

namespace Server.Gumps
{
    // --- 실제 기술창 껌프 ---
    public class CustomSPMGump : Gump
    {
        private Mobile m_From;

        public CustomSPMGump(Mobile from) : base(150, 150)
        {
            m_From = from;
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 240, 120, 9270);
            AddLabel(60, 10, 1152, "--- 무기 기술창 ---");

            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon != null)
            {
                DisplayAbility(weapon.PrimaryAbility, 20, 40, 1);
                DisplayAbility(weapon.SecondaryAbility, 20, 75, 2);
            }
            else
            {
                AddLabel(45, 60, 0x21, "무기를 장착해 주세요.");
            }
        }

        private void DisplayAbility(WeaponAbility ability, int x, int y, int buttonID)
        {
            if (ability == null) return;
            AddButton(x, y, 0x15E1, 0x15E5, buttonID, GumpButtonType.Reply, 0);
            AddLabel(x + 30, y, 0x481, ability.ToString());
            AddLabel(x + 30, y + 15, 0x3E5, "클릭 시 즉시 시전");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1 || info.ButtonID == 2)
			{
				// ... 기술 시전 로직 ...
				int cooldownSeconds = 5; // 예시 5초
				
				// [핵심] 오리온 스크립트용 전용 메시지 전송
				// 컬러 코드를 특이하게(예: 0x35) 주면 일반 채팅과 구분이 쉽습니다.
				sender.Mobile.SendAsciiMessage(0x35, "COOLDOWN|{0}|{1}", info.ButtonID, cooldownSeconds);
			}
		}
    }

    // --- [핵심] 페이퍼돌 버튼 위치에 덧씌울 투명 감지기 ---
    public class InvisibleTriggerGump : Gump
    {
        public InvisibleTriggerGump() : base(0, 0)
        {
            Closable = false; // 유저가 실수로 닫지 못하게 함
            Disposable = false;
            Dragable = false;

            AddPage(0);
            // 페이퍼돌 전투북 좌표(156, 200)에 20x20 크기의 투명 버튼 배치
            // 그래픽 번호 0x4D2 등 빈 이미지를 사용하거나, 테스트 시에는 0x15E1(화살표)로 위치를 잡으세요.
            AddButton(156, 200, 0x4D2, 0x4D2, 9999, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 9999)
            {
                // 페이퍼돌 위치를 누르면 기술창을 띄워줌
                sender.Mobile.SendGump(new CustomSPMGump(sender.Mobile));
                // 감지기를 다시 띄워 유지시킴
                sender.Mobile.SendGump(new InvisibleTriggerGump());
            }
        }
    }
}