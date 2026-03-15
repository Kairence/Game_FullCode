using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Targeting;
using Server.Network;

namespace Server.Items
{
    public class Bandage : Item, IDyable, ICommodity
    {
        public static int Range = 1;
        public override double DefaultWeight => 0.1;

		#region ICommodity 구현 (CS0535 에러 해결)
        // [수정] 인터페이스 멤버 구현
        public TextDefinition Description => LabelNumber;
        public bool IsDeedable => true;
        #endregion

        [Constructable]
        public Bandage() : this(1) { }
        [Constructable]
        public Bandage(int amount) : base(0xE21) { Stackable = true; Amount = amount; }
        public Bandage(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            // [기획] 기력 체크 없이 즉시 타겟 창 출력
            if (from.InRange(GetWorldLocation(), Range))
            {
                from.RevealingAction();
                from.SendLocalizedMessage(500948); // 누구에게 사용하시겠습니까?
                from.Target = new InternalTarget(this);
            }
            else
                from.SendLocalizedMessage(500295);
        }

        private class InternalTarget : Target
        {
            private readonly Bandage m_Bandage;

            public InternalTarget(Bandage bandage) : base(Bandage.Range, false, TargetFlags.Beneficial)
            { m_Bandage = bandage; }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile target && from.InRange(m_Bandage.GetWorldLocation(), Range))
                {
                    bool isPet = (target is BaseCreature);
                    
                    // [기획] 유저 타겟이면 회복술, 펫 타겟이면 수의학 기준으로 기력 소모량 결정
                    double checkSkill = isPet ? from.Skills.Veterinary.Value : from.Skills.Healing.Value;
                    int stamLoss = (checkSkill >= 50.0) ? 5 : 10;

                    // [기획] 실제 대상을 찍었을 때 최종 기력 체크
                    if (from.Stam < stamLoss)
                    {
                        from.SendLocalizedMessage(1156036, stamLoss.ToString()); // 기력이 부족합니다.
                        return;
                    }

                    // [기획] 회복술 100 미만 자신 치료 불가
                    if (from == target && from.Skills.Healing.Value < 100.0)
                    {
                        from.SendLocalizedMessage(503407); // 자신을 치료할 수 없습니다.
                        return;
                    }

                    if (BandageContext.BeginHeal(from, target) != null)
                    {
                        from.Stam -= stamLoss;
                        m_Bandage.Consume();
                    }
                }
            }
        }

        public virtual bool Dye(Mobile from, DyeTub sender) { Hue = sender.DyedHue; return true; }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }

    public class BandageContext
    {
        private readonly Mobile m_Healer;
        private readonly Mobile m_Patient;
        private int m_RemainingTicks;
        private Timer m_Timer;

        private static readonly Dictionary<Mobile, BandageContext> m_Table = new Dictionary<Mobile, BandageContext>();

        public void Slip()
        {
            // [기획] 회복술 150 이상 이동 패널티 삭제
            if (m_Healer.Skills.Healing.Value >= 150.0)
                return;

            m_RemainingTicks--;
            m_Healer.SendLocalizedMessage(500961);
            if (m_RemainingTicks <= 0) StopHeal();
        }

        public BandageContext(Mobile healer, Mobile patient)
        {
            m_Healer = healer;
            m_Patient = patient;
            m_RemainingTicks = 10;

            // [기획] 수의학 150 보너스: VASave/VirtualArmor +5
            if (m_Patient is BaseCreature bc && bc.ControlMaster == m_Healer && m_Healer.Skills.Veterinary.Value >= 150.0)
            {
                bc.VASave += 5;
                bc.VirtualArmor += 5;
            }

            // [기획] 회복술 200 미만 중복 치료 불가 카운트 증가
            if (m_Patient is PlayerMobile pm && m_Healer.Skills.Healing.Value < 200.0)
                pm.UseBandage++;

            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(10.0), TimeSpan.FromSeconds(10.0), 10, OnTick);
        }

        private void OnTick()
        {
            if (!m_Healer.Alive || !m_Healer.InRange(m_Patient, Bandage.Range)) { StopHeal(); return; }

            bool isPet = (m_Patient is BaseCreature);
            double mainSkill = isPet ? m_Healer.Skills.Veterinary.Value : m_Healer.Skills.Healing.Value;

            // [기획] 유지 기력 체크 (50기준 1 or 2)
            if (m_Healer.Stam < (mainSkill >= 50.0 ? 1 : 2))
            {
                m_Healer.SendLocalizedMessage(1156036);
                StopHeal();
                return;
            }

            if (m_RemainingTicks > 0)
            {
                // [기획] 수의학 100 본디드 펫 부활
                if (isPet && m_Patient is BaseCreature bc && bc.IsDeadPet && bc.IsBonded && m_Healer.Skills.Veterinary.Value >= 100.0)
                {
                    bc.ResurrectPet();
                    m_Healer.SendLocalizedMessage(503256);
                    StopHeal();
                    return;
                }

                // [기획] 회복량 계산
                double subSkill = isPet ? m_Healer.Skills.AnimalLore.Value : m_Healer.Skills.Anatomy.Value;
                int heal = Utility.RandomMinMax(50, 100) + (int)(Utility.RandomMinMax(1, 2) * (subSkill / 10.0));

                // [기획] 중독/200레벨 보정
				bool skillBonus = false;
				if (m_Patient.Poisoned && mainSkill >= 200.0)
					skillBonus = true;

                // [기획] 펫 2배 회복
                if (isPet) heal *= 2;

				Spells.SpellHelper.Heal(heal, m_Patient, m_Healer, false, skillBonus );
                m_RemainingTicks--;
            }

            m_Patient.PlaySound(0x57);
            if (m_RemainingTicks <= 0) StopHeal();
        }

        public void StopHeal()
        {
            if (m_Timer != null) m_Timer.Stop();
            m_Timer = null;
            m_Table.Remove(m_Healer);

            // [기획] VA 복구
            if (m_Patient is BaseCreature bc && bc.VASave > 0)
            {
                bc.VirtualArmor -= bc.VASave;
                bc.VASave = 0;
            }

            // [기획] 중복 카운트 초기화
            if (m_Patient is PlayerMobile pm && m_Healer.Skills.Healing.Value < 200.0)
                pm.UseBandage = 0;

            if (m_RemainingTicks <= 0) 
            {
                // [기획] 해부학 150 코마 회복
                if (!(m_Patient is BaseCreature) && m_Patient is PlayerMobile targetPm && targetPm.Coma && m_Healer.Skills.Anatomy.Value >= 150.0)
                    targetPm.Coma = false;

                m_Healer.SendLocalizedMessage(503409); // 치료 완료
            }
        }

        public static BandageContext BeginHeal(Mobile healer, Mobile patient)
        {
            if (healer == patient && healer.Skills.Healing.Value < 100.0) return null;
            if (patient is PlayerMobile pm && pm.UseBandage > 0 && healer.Skills.Healing.Value < 200.0)
            {
                healer.SendLocalizedMessage(503408);
                return null;
            }

            if (healer.CanBeBeneficial(patient, true, true))
            {
                healer.DoBeneficial(patient);
                BandageContext context = GetContext(healer);
                if (context != null) context.StopHeal();

                context = new BandageContext(healer, patient);
                m_Table[healer] = context;
                healer.SendLocalizedMessage(500956); // 시작
                return context;
            }
            return null;
        }

        public static BandageContext GetContext(Mobile healer)
        {
            BandageContext bc = null;
            m_Table.TryGetValue(healer, out bc);
            return bc;
        }
    }
}