using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an overseer's corpse")]
    public class ExodusOverseer : BaseCreature
    {
        private bool m_FieldActive;
        [Constructable]
        public ExodusOverseer()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "exodus overseer";
            this.Body = 0x2F4;

			/* [Exodus Overseer - Normal - Fame 15,000 / Weight 1.25]
			   - 엑소더스 군단 중형 기계 / 일반 던전
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 20 (기본 15 + 합금 보정 5)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(410, 435); 
			this.SetHits(9200, 9500); 
			this.SetDex(80, 90);
			this.SetInt(80, 90);

			// [Combat Options] 강력한 에너지 충격파 (물리 40% / 에너지 60%)
			this.SetDamage(45, 75);
			this.SetAttackSpeed(2.8);
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Energy, 60);

			// [Resistances] 75% 캡 준수 및 약점(화염/냉기) 설정
			this.SetResistance(ResistanceType.Physical, 55, 65); 
			this.SetResistance(ResistanceType.Fire, 35, 45);      // 냉각 시스템 약점
			this.SetResistance(ResistanceType.Cold, 35, 45);    
			this.SetResistance(ResistanceType.Poison, 60, 70); 
			this.SetResistance(ResistanceType.Energy, 65, 75);   // 에너지 방막 (Max 75)

			// [Skills] 기본 100~120에 역산 보너스(14.0) 가산
			this.SetSkill(SkillName.Wrestling, 115.0, 130.0); 
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);
			this.SetSkill(SkillName.Anatomy, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);

			this.Tamable = false;
			this.VirtualArmor = 20;
			this.Fame = 15000;
			this.Karma = -15000;

            if (Utility.Random(2) == 0)
                this.PackItem(new PowerCrystal());
            else
                this.PackItem(new ArcaneGem());

            this.m_FieldActive = this.CanUseField;
        }

        public ExodusOverseer(Serial serial)
            : base(serial)
        {
        }

        public bool FieldActive
        {
            get
            {
                return this.m_FieldActive;
            }
        }
        public bool CanUseField
        {
            get
            {
                return this.Hits >= this.HitsMax * 9 / 10;
            }
        }// TODO: an OSI bug prevents to verify this
        public override bool IsScaredOfScaryThings
        {
            get
            {
                return false;
            }
        }
        public override bool IsScaryToPets
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
                return !Core.AOS;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
        }

        public override int GetIdleSound()
        {
            return 0xFD;
        }

        public override int GetAngerSound()
        {
            return 0x26C;
        }

        public override int GetDeathSound()
        {
            return 0x211;
        }

        public override int GetAttackSound()
        {
            return 0x23B;
        }

        public override int GetHurtSound()
        {
            return 0x140;
        }

        public override void AlterMeleeDamageFrom(Mobile from, ref int damage)
        {
            if (this.m_FieldActive)
                damage = 0; // no melee damage when the field is up
        }

        public override void AlterSpellDamageFrom(Mobile caster, ref int damage)
        {
            if (!this.m_FieldActive)
                damage = 0; // no spell damage when the field is down
        }

        public override void OnDamagedBySpell(Mobile from)
        {
            if (from != null && from.Alive && 0.4 > Utility.RandomDouble())
            {
                this.SendEBolt(from);
            }

            if (!this.m_FieldActive)
            {
                // should there be an effect when spells nullifying is on?
                this.FixedParticles(0, 10, 0, 0x2522, EffectLayer.Waist);
            }
            else if (this.m_FieldActive && !this.CanUseField)
            {
                this.m_FieldActive = false;

                // TODO: message and effect when field turns down; cannot be verified on OSI due to a bug
                this.FixedParticles(0x3735, 1, 30, 0x251F, EffectLayer.Waist);
            }
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            if (this.m_FieldActive)
            {
                this.FixedParticles(0x376A, 20, 10, 0x2530, EffectLayer.Waist);

                this.PlaySound(0x2F4);

                attacker.SendAsciiMessage("Your weapon cannot penetrate the creature's magical barrier");
            }

            if (attacker != null && attacker.Alive && attacker.Weapon is BaseRanged && 0.4 > Utility.RandomDouble())
            {
                this.SendEBolt(attacker);
            }
        }

        public override void OnThink()
        {
            base.OnThink();

            // TODO: an OSI bug prevents to verify if the field can regenerate or not
            if (!this.m_FieldActive && !this.IsHurt())
                this.m_FieldActive = true;
        }

        public override bool Move(Direction d)
        {
            bool move = base.Move(d);

            if (move && this.m_FieldActive && this.Combatant != null)
                this.FixedParticles(0, 10, 0, 0x2530, EffectLayer.Waist);

            return move;
        }

        public void SendEBolt(Mobile to)
        {
            this.MovingParticles(to, 0x379F, 7, 0, false, true, 0xBE3, 0xFCB, 0x211);
            to.PlaySound(0x229);
            this.DoHarmful(to);
            AOS.Damage(to, this, 50, 0, 0, 0, 0, 100);
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

            this.m_FieldActive = this.CanUseField;
        }
    }
}