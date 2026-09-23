using System;
using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.Misc;

namespace Server.Items
{
    public class PetSigil : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int SigilCategory { get; set; } // 0: 공격(Offense), 1: 방어(Defense), 2: 유틸(Utility)

        [CommandProperty(AccessLevel.GameMaster)]
        public int SigilTier { get; set; } // 1: 일반, 2: 희귀, 3: 영웅, 4: 서사, 5: 전설, 6: 신화

        [CommandProperty(AccessLevel.GameMaster)]
        public int OptionID { get; set; } // CustomOption ID

        [CommandProperty(AccessLevel.GameMaster)]
        public int OptionValue { get; set; } // 효과 수치

        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges { get; set; } // 잔여 횟수

        [Constructable]
        public PetSigil() : base(0x14F0) // 0x14F0 = Deed or scroll graphic
        {
            Weight = 1.0;
            Name = "마법 문양";
            Hue = 0x480;
        }

        public PetSigil(Serial serial) : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            base.AddNameProperty(list);
            
            string cat = SigilCategory == 0 ? "공격" : SigilCategory == 1 ? "방어" : "유틸";
            string tierStr = SigilTier == 6 ? "신화" : SigilTier == 5 ? "전설" : SigilTier == 4 ? "서사" : SigilTier == 3 ? "영웅" : SigilTier == 2 ? "희귀" : "일반";
            
            list.Add(1060658, "종류\t{0} 문양 ({1})", cat, tierStr); 
            // Cliloc을 이용한 옵션 표기 (예: 무기 피해 +50%)
            // ItemOptionCreator.BaseCliloc + OptionID 형태로 나중에 매핑 가능
            list.Add(1060659, "효과\tOption ID: {0} (+{1})", OptionID, OptionValue); 
            list.Add(1060660, "잔여 횟수\t{0}", Charges);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            from.SendMessage("문양을 새길 펫을 선택하세요.");
            from.Target = new SigilTarget(this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(SigilCategory);
            writer.Write(SigilTier);
            writer.Write(OptionID);
            writer.Write(OptionValue);
            writer.Write(Charges);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            SigilCategory = reader.ReadInt();
            SigilTier = reader.ReadInt();
            OptionID = reader.ReadInt();
            OptionValue = reader.ReadInt();
            Charges = reader.ReadInt();
        }

        private class SigilTarget : Target
        {
            private PetSigil m_Sigil;

            public SigilTarget(PetSigil sigil) : base(12, false, TargetFlags.None)
            {
                m_Sigil = sigil;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Sigil == null || m_Sigil.Deleted) return;

                if (targeted is BaseCreature bc)
                {
                    if (!bc.Controlled || bc.ControlMaster != from)
                    {
                        from.SendMessage("당신의 펫에게만 사용할 수 있습니다.");
                        return;
                    }

                    // 테이머 팔로우(ControlSlots) 기준 등급 제한
                    int allowedTier = 1;
                    if (bc.ControlSlots >= 5) allowedTier = 6;
                    else if (bc.ControlSlots == 4) allowedTier = 5;
                    else if (bc.ControlSlots == 3) allowedTier = 4;
                    else if (bc.ControlSlots == 2) allowedTier = 3;
                    else if (bc.ControlSlots == 1) allowedTier = 2; // 유저 기획: 1슬롯 희귀(2) 가능

                    if (m_Sigil.SigilTier > allowedTier)
                    {
                        from.SendMessage("이 펫의 슬롯 요구치로는 이 강력한 문양을 받아들일 수 없습니다.");
                        return;
                    }

                    if (bc.ActiveSigils == null || bc.ActiveSigils.Length != 12)
                        bc.ActiveSigils = new int[12];

                    int offset = m_Sigil.SigilCategory * 4;
                    bc.ActiveSigils[offset] = m_Sigil.OptionID;
                    bc.ActiveSigils[offset + 1] = m_Sigil.OptionValue;
                    bc.ActiveSigils[offset + 2] = m_Sigil.Charges;
                    bc.ActiveSigils[offset + 3] = m_Sigil.SigilTier;

                    from.SendMessage("펫에게 성공적으로 문양을 새겼습니다!");
                    bc.PlaySound(0x1F3);
                    bc.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);

                    m_Sigil.Delete();
                }
                else
                {
                    from.SendMessage("올바른 대상이 아닙니다.");
                }
            }
        }
    }
}
