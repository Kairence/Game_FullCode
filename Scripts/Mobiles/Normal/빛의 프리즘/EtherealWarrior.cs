using System;
using Server.Gumps;

namespace Server.Mobiles 
{ 
    [CorpseName("an ethereal warrior corpse")] 
    public class EtherealWarrior : BaseCreature 
    { 
        private static readonly TimeSpan ResurrectDelay = TimeSpan.FromSeconds(2.0);
        private DateTime m_NextResurrect;
        [Constructable] 
        public EtherealWarrior()
            : base(AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4)
        { 
            this.Name = NameList.RandomName("ethereal warrior");
            this.Body = 123;

			/* [Ethereal Warrior - Fame 16,000 / Normal / Weight 1.25]
			   - 빛의 프리즘 던전 영적 수호자
			   - 영체 컨셉: 물리 저항 극대화, 정교한 무술, 독/에너지에 취약
			   - 지능형 영혼: 테이밍 불가 (200 숙련도 고려)
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 정예)

			// [Attributes] (기본 보너스 * 1배 * 1.25) - 기본 보너스
			// Str: 보너스 약 1,530 -> 최종 Set 약 380-450
			this.SetStr(380, 450); 

			// Hits: 보너스 약 40,500 -> 최종 Set 약 10,000-11,500
			this.SetHits(10000, 11500); 

			this.SetDex(180, 220); // 영체다운 매우 빠른 반응 속도
			this.SetInt(200, 300); 

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Energy, 80); // 영적인 에너지 타격

			// [Resistances] 영체 컨셉 (물리/냉기 특화, 독/에너지 약점)
			this.SetResistance(ResistanceType.Physical, 70, 75); // ★ 실체가 없어 물리 공격을 흘림
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 65, 75);    // 영적인 차가움
			this.SetResistance(ResistanceType.Poison, 15, 25);    // ★ 영적 결합을 해치는 부식/독에 매우 취약
			this.SetResistance(ResistanceType.Energy, 30, 45);   // ★ 순수 에너지 간섭에 의한 불안정화(약점)

			// [Skills] 고대 무술의 달인
			this.SetSkill(SkillName.Wrestling, 120.0, 135.0); 
			this.SetSkill(SkillName.Tactics, 120.0, 135.0);
			this.SetSkill(SkillName.Anatomy, 120.0, 135.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Parry, 100.0, 120.0); // 영적인 방어술

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 14;

			this.Fame = 16000;
			this.Karma = 0; // 중립적인 수호령 설정
        }

        public EtherealWarrior(Serial serial)
            : base(serial)
        { 
        }

        public override bool InitialInnocent
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return Core.AOS ? 5 : 0;
            }
        }

        public override TribeType Tribe { get { return TribeType.Fey; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override int Feathers
        {
            get
            {
                return 100;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich, 3);
            this.AddLoot(LootPack.Gems);
        }

        public override void OnMovement(Mobile from, Point3D oldLocation)
        {
            if (!from.Alive && (from is PlayerMobile))
            {
                if (!from.Frozen && (DateTime.UtcNow >= this.m_NextResurrect) && this.InRange(from, 4) && !this.InRange(oldLocation, 4) && this.InLOS(from))
                {
                    this.m_NextResurrect = DateTime.UtcNow + ResurrectDelay;
                    if (!from.Criminal && (from.Kills < 5) && (from.Karma > 0))
                    {
                        if (from.Map != null && from.Map.CanFit(from.Location, 16, false, false))
                        {
                            this.Direction = this.GetDirectionTo(from);
                            from.PlaySound(0x1F2);
                            from.FixedEffect(0x376A, 10, 16);
                            from.CloseGump(typeof(ResurrectGump));
                            from.SendGump(new ResurrectGump(from, ResurrectMessage.Healer));
                        }
                    }
                }
            }
        }

        public override int GetAngerSound()
        {
            return 0x2F8;
        }

        public override int GetIdleSound()
        {
            return 0x2F8;
        }

        public override int GetAttackSound()
        {
            return Utility.Random(0x2F5, 2);
        }

        public override int GetHurtSound()
        {
            return 0x2F9;
        }

        public override int GetDeathSound()
        {
            return 0x2F7;
        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (0.25 > Utility.RandomDouble())
            {
                int toSap = Utility.RandomMinMax(20, 30);

                switch (Utility.Random(3))
                {
                    case 0:
                        defender.Damage(toSap, this);
                        Hits += toSap;
                        break;
                    case 1:
                        defender.Stam -= toSap;
                        Stam += toSap;
                        break;
                    case 2:
                        defender.Mana -= toSap;
                        Mana += toSap;
                        break;
                }
            }
            /*defender.Damage(Utility.Random(10, 10), this);
            defender.Stam -= Utility.Random(10, 10);
            defender.Mana -= Utility.Random(10, 10);*/
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            /*attacker.Damage(Utility.Random(10, 10), this);
            attacker.Stam -= Utility.Random(10, 10);
            attacker.Mana -= Utility.Random(10, 10);*/
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
        }
    }
}
