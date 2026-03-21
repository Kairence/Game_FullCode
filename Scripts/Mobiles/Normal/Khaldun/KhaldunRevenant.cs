using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    public class KhaldunRevenant : BaseCreature
    {
        private static readonly Hashtable m_Table = new Hashtable();
        private readonly Mobile m_Target;
        private readonly DateTime m_ExpireTime;
        public KhaldunRevenant(Mobile target)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.18, 0.36)
        {
            this.Name = "a revenant";
            this.Body = 0x3CA;
            this.Hue = 0x41CE;

            this.m_Target = target;
            this.m_ExpireTime = DateTime.UtcNow + TimeSpan.FromMinutes(10.0);

			/* [Khaldun Revenant - Fame 12,000 / Khaldun / Weight 1.25]
			   - 스킬 200 마스터 서버용 '상급 추격자' 밸런스 적용
			   - 카르마 보정: 명성(12,000) + 2,000 보정 = -14,000
			   - 가상 방어력(VirtualArmor): (12,000/1000) + 3.0 = 15 (강화된 원혼의 외피)
			   -------------------------------------------------- */

			// [Attributes] 명성 12,000 보너스 + 가중치 1.25 반영
			this.SetStr(280, 350); 
			this.SetHits(6500, 7500); 
			this.SetDex(50, 70);
			this.SetInt(50, 70);

			SetAttackSpeed(1.8);
			SetDamage(55, 85);

			// [Damage Types] 40% 물리 + 60% 냉기 (죽음의 한기)
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Cold, 60);

			// [Resistances] 언데드 상위 저항 (불에 타지 않는 원한)
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 30, 40);      // 칼둔 리벤넌트는 불에도 잘 안 탐
			this.SetResistance(ResistanceType.Cold, 75);         // 냉기 완전 면역 (Max 75)
			this.SetResistance(ResistanceType.Poison, 75);      // 독 면역
			this.SetResistance(ResistanceType.Energy, 50, 60);

			// [Skills] ★ 스킬 200 서버 기준 - 상급 유저를 위협하는 추격자 (재설계)
			// 유저 스킬 120 ~ 150 구간 사냥에 최적화
			this.SetSkill(SkillName.Wrestling, 115.0, 135.0); 
			this.SetSkill(SkillName.Tactics, 115.0, 135.0);
			this.SetSkill(SkillName.Anatomy, 120.0, 140.0);    // 치명적인 추격 공격
			this.SetSkill(SkillName.MagicResist, 130.0, 150.0); // 마법으로 떨쳐내기 불가능급

			// [Misc]
			this.VirtualArmor = 15;

			this.Fame = 12000;
			this.Karma = -14000; // 칼둔 최대 보정 적용 (-12,000 - 2,000)

            Halberd weapon = new Halberd();
            weapon.Hue = 0x41CE;
            weapon.Movable = false;

            this.AddItem(weapon);
        }

        public KhaldunRevenant(Serial serial)
            : base(serial)
        {
        }

        public override bool DeleteCorpseOnDeath
        {
            get
            {
                return true;
            }
        }
        public override Mobile ConstantFocus
        {
            get
            {
                return this.m_Target;
            }
        }
        public override bool AlwaysAttackable
        {
            get
            {
                return true;
            }
        }
        public override bool BardImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public static void Initialize()
        {
            EventSink.PlayerDeath += new PlayerDeathEventHandler(EventSink_PlayerDeath);
        }

        public static void EventSink_PlayerDeath(PlayerDeathEventArgs e)
        {
            Mobile m = e.Mobile;
            Mobile lastKiller = m.LastKiller;

            if (lastKiller is BaseCreature)
                lastKiller = ((BaseCreature)lastKiller).GetMaster();

            if (IsInsideKhaldun(m) && IsInsideKhaldun(lastKiller) && lastKiller.Player && !m_Table.Contains(lastKiller))
            {
                foreach (AggressorInfo ai in m.Aggressors)
                {
                    if (ai.Attacker == lastKiller && ai.CanReportMurder)
                    {
                        SummonRevenant(m, lastKiller);
                        break;
                    }
                }
            }
        }

        public static void SummonRevenant(Mobile victim, Mobile killer)
        {
            KhaldunRevenant revenant = new KhaldunRevenant(killer);

            revenant.MoveToWorld(victim.Location, victim.Map);
            revenant.Combatant = killer;
            revenant.FixedParticles(0, 0, 0, 0x13A7, EffectLayer.Waist);
            Effects.PlaySound(revenant.Location, revenant.Map, 0x29);

            m_Table.Add(killer, null);
        }

        public static bool IsInsideKhaldun(Mobile from)
        {
            return from != null && from.Region != null && from.Region.IsPartOf("Khaldun");
        }

        public override void DisplayPaperdollTo(Mobile to)
        {
        }

        public override int GetIdleSound()
        {
            return 0x1BF;
        }

        public override int GetAngerSound()
        {
            return 0x107;
        }

        public override int GetDeathSound()
        {
            return 0xFD;
        }

        public override void OnThink()
        {
            if (!this.m_Target.Alive || DateTime.UtcNow > this.m_ExpireTime)
            {
                this.Delete();
                return;
            }

            //Combatant = m_Target;
            //FocusMob = m_Target;

            if (this.AIObject != null)
                this.AIObject.Action = ActionType.Combat;

            base.OnThink();
        }

        public override bool OnBeforeDeath()
        {
            Effects.SendLocationEffect(this.Location, this.Map, 0x376A, 10, 1);
            return true;
        }

        public override void OnDelete()
        {
            if (this.m_Target != null)
                m_Table.Remove(this.m_Target);

            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            this.Delete();
        }
    }
}