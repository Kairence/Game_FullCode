using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Items;
using Server.Commands;

namespace Server.Misc
{
    public class SlotMachineGump : Gump
    {
        // 부피감이 큰 아이템 ID 구성 (잉갓 더미 및 ML 보석)
        private static int[] m_ItemPool = new int[] 
        { 
            0x1BE3, // Copper Ingot Stack (구리 잉갓 더미)
            0x1BE9, // Silver Ingot Stack (은 잉갓 더미)
            0x1BEF, // Gold Ingot Stack (금 잉갓 더미)
            0x3192, // ML Jewel: Dark Sapphire (진한 청보석)
            0x3195, // ML Jewel: Fire Ruby (불타는 루비)
            0x3197, // ML Jewel: Ecru Citrine (황금빛 시트린)
            0x3198  // ML Jewel: Perfect Emerald (완벽한 에메랄드 - 잭팟용)
        };

        // 가려진 상태: 큼직한 황금 보물상자 (0x0E40)
        private const int HIDDEN_ICON = 0x0E40; 

        private int[] m_FinalResults; 
        private int m_OpenStage;      

        public static void Initialize()
        {
            CommandSystem.Register("Slot", AccessLevel.Player, new CommandEventHandler(Slot_OnCommand));
        }

        public static void Slot_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new SlotMachineGump(e.Mobile, new int[] { 0, 0, 0 }, 0));
        }

        public SlotMachineGump(Mobile from, int[] final, int stage) : base(100, 100)
        {
            from.CloseGump(typeof(SlotMachineGump));
            m_FinalResults = final;
            m_OpenStage = stage;

            AddPage(0);
            
            // 전체 배경 (약간 더 넓게 조정)
            AddBackground(0, 0, 350, 350, 0xA28); // 고급스러운 석조 배경
            AddAlphaRegion(10, 10, 330, 330);    // 배경 투명도 처리
            
            AddLabel(115, 25, 0x481, "★ HIGH-ROLLER SLOT ★");

            // 슬롯 영역
            for (int i = 0; i < 3; i++)
            {
                // 배경 박스 (0x2436: 어두운 사각 프레임)
                AddImageTiled(35 + (i * 95), 70, 90, 100, 0x2436); 
                
                if (i < m_OpenStage)
                    // 아이템이 작아 보이지 않게 좌표 미세 조정
                    AddItem(50 + (i * 95), 100, m_ItemPool[m_FinalResults[i]]);
                else
                    AddItem(50 + (i * 95), 100, HIDDEN_ICON); 
            }

            if (stage == 0 || stage == 3)
            {
                if (stage == 3)
                {
                    if (final[0] == final[1] && final[1] == final[2])
                        AddLabel(125, 185, 0x35, "★ JACKPOT ★");
                    else
                        AddLabel(100, 185, 0x22, "아쉽네요! 다시 도전?");
                }
                else
                    AddLabel(95, 185, 0x480, "행운을 빕니다! (100 GP)");

                // --- 버튼 크기 대폭 강화 ---
                // 0x15A4, 0x15A6: 아주 큰 'Okay/Check' 스타일의 금색 버튼
                AddButton(135, 220, 0x15A4, 0x15A6, 1, GumpButtonType.Reply, 0);
                AddLabel(155, 280, 0x481, "SPIN!");
            }
            else
            {
                AddLabel(120, 185, 0x481, "두근두근...");
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 1)
            {
                if (from.Backpack != null && from.Backpack.ConsumeTotal(typeof(Gold), 100))
                {
                    int[] final = new int[3];
                    for (int i = 0; i < 3; i++) 
                        final[i] = Utility.Random(m_ItemPool.Length);

                    from.PlaySound(0x41); 
                    new SlotOpenTimer(from, final).Start();
                }
                else
                {
                    from.SendMessage(0x22, "골드가 부족합니다.");
                }
            }
        }

        private class SlotOpenTimer : Timer
        {
            private Mobile m_From;
            private int[] m_Final;
            private int m_CurrentStage = 0;

            public SlotOpenTimer(Mobile from, int[] final) : base(TimeSpan.FromMilliseconds(700), TimeSpan.FromMilliseconds(700))
            {
                m_From = from;
                m_Final = final;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                m_CurrentStage++;

                if (m_CurrentStage <= 3)
                {
                    if (m_CurrentStage < 3) m_From.PlaySound(0x3E5);
                    
                    m_From.SendGump(new SlotMachineGump(m_From, m_Final, m_CurrentStage));

                    if (m_CurrentStage == 3)
                    {
                        if (m_Final[0] == m_Final[1] && m_Final[1] == m_Final[2])
                        {
                            m_From.SendMessage(0x35, "잭팟 달성! 행운의 주인공이 되셨습니다!");
                            m_From.AddToBackpack(new Gold(5000)); // 잉갓 버전이므로 당첨금도 상향!
                            m_From.PlaySound(0x5B5);
                        }
                        else
                        {
                            m_From.PlaySound(0x3E9);
                        }
                        Stop();
                    }
                }
            }
        }
    }
}