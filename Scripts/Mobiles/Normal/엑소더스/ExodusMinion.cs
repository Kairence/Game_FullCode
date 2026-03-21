using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a minion's corpse")]
    public class ExodusMinion : BaseCreature
    {
        private bool m_FieldActive;
        [Constructable]
        public ExodusMinion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "exodus minion";
            this.Body = 0x2F5;

			/* [Exodus Minion - Normal - Fame 10,000 / Weight 1.25]
			   - 기계 마법 융합체 / 일반 던전 사양
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 15 (기본 10 + 강철 보정 5)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 적용
			this.SetStr(240, 260); 
			this.SetHits(5400, 5600); 
			this.SetDex(45, 55);
			this.SetInt(45, 55);

			// [Combat Options] 물리/에너지 복합 대미지
			this.SetDamage(35, 65);
			this.SetAttackSpeed(2.5);
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Energy, 40);

			// [Resistances] 최고 저항 75 이하 준수
			this.SetResistance(ResistanceType.Physical, 50, 65); 
			this.SetResistance(ResistanceType.Fire, 45, 55);      
			this.SetResistance(ResistanceType.Cold, 45, 55);    
			this.SetResistance(ResistanceType.Poison, 45, 55); 
			this.SetResistance(ResistanceType.Energy, 60, 75);   // 에너지 저항 특화

			// [Skills] 기본 90~110에 역산 보너스(8.3) 가산
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0); 
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 115.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.Tamable = false;
			this.VirtualArmor = 15;
			this.Fame = 10000;
			this.Karma = -10000; // 일반 던전이므로 명성치와 동일

            this.PackItem(new PowerCrystal());
            this.PackItem(new ArcaneGem());
            this.PackItem(new ClockworkAssembly());

            switch( Utility.Random(3) )
            {
                case 0:
                    this.PackItem(new PowerCrystal());
                    break;
                case 1:
                    this.PackItem(new ArcaneGem());
                    break;
                case 2:
                    this.PackItem(new ClockworkAssembly());
                    break;
            }

            this.m_FieldActive = this.CanUseField;
        }

        public ExodusMinion(Serial serial)
            : base(serial)
        {
        }

        public override bool AutoDispel
        {
            get
            {
                return true;
            }
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
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Rich);
        }

        public override int GetIdleSound()
        {
            return 0x218;
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
            return 0x232;
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

        public override void AlterSpellDamageFrom(Mobile from, ref int damage)
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

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (0.2 > Utility.RandomDouble())
            {
                c.DropItem(new MechanicalComponent());
            }
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