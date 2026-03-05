using System;
using Server;
using Server.Network;
using Server.Gumps;

namespace Server.Misc
{
    public class SPMOverride
    {
        public static void Initialize()
        {
            // 0xB1은 Gump 버튼 클릭 패킷입니다. 
            // 0x11은 구버전 클라이언트의 Gump 응답 패킷입니다.
            // 둘 다 등록해서 어느 쪽으로 오든 낚아챕니다.
            PacketHandlers.Register(0xB1, 9, true, new OnPacketReceive(OnGumpResponse));
            PacketHandlers.Register(0x11, 9, true, new OnPacketReceive(OnGumpResponse));

            Console.WriteLine("### [DEBUG] SPM Packet Hook Loaded ###");
        }

        public static void OnGumpResponse(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;
            if (from == null) return;

            // 패킷 데이터를 직접 읽습니다. (참조 에러 발생 소지 없음)
            // GumpID(4바이트)를 건너뛰고 ButtonID(4바이트)를 읽음
            pvSrc.Seek(8, System.IO.SeekOrigin.Begin);
            int buttonID = pvSrc.ReadInt32();

            // 오리온 OJS에서 설정한 7000번 버튼인지 체크
            if (buttonID == 7000)
            {
                // Scripts 영역의 Gump를 호출
                from.CloseGump(typeof(CustomSPMGump));
                from.SendGump(new CustomSPMGump(from));
                
                from.SendMessage(0x48, "커스텀 전투 기술창이 열렸습니다.");
            }
        }
    }
}