using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a meer's corpse")]
    public class MeerEternal : BaseCreature
    {
        private DateTime m_NextAbilityTime;
        [Constructable]
        public MeerEternal()
            : base(AIType.AI_Spellweaving, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a meer eternal";
            this.Body = 772;

			/* [MeerEternal - Fame 12,000 / Normal / Weight 1.28]
			   - 미어 종족의 고위 대마법사 (선족 설정)
			   - 지능형 아인종: 테이밍 불가 (200 숙련도 고려)
			   - 종족 특성: 에너지/화염 저항 취약 (마법 방어력은 높음)
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 정예 몬스터)

			// [Attributes] (기본 보너스 * 1배 * 1.28) - 기본 보너스
			// Str: 보너스 약 1,130 -> 최종 Set 약 300-350
			this.SetStr(300, 350); 

			// Hits: 보너스 약 27,950 -> 최종 Set 약 7500-8500
			this.SetHits(7500, 8500); 

			this.SetDex(150, 180); // 영적인 존재다운 빠른 캐스팅 속도
			this.SetInt(350, 450); // 대마법사 컨셉 (높은 마나 통)

			// [Combat Options] 물리 공격보다는 마법 위주의 설계
			this.SetDamage(25, 40);
			this.SetAttackSpeed(2.2);
			this.SetDamageType(ResistanceType.Physical, 20); // 물리 대미지는 낮음
			this.SetDamageType(ResistanceType.Energy, 80);   // 영적인 에너지 타격

			// [Resistances] 마법 저항은 높으나 종족 약점(화염/에너지) 유지
			this.SetResistance(ResistanceType.Physical, 45, 55); 
			this.SetResistance(ResistanceType.Fire, 30, 40);      // ★ 화염 취약점
			this.SetResistance(ResistanceType.Cold, 50, 60);    
			this.SetResistance(ResistanceType.Poison, 50, 60); 
			this.SetResistance(ResistanceType.Energy, 35, 45);   // 에너지 저항 보강(영적 존재)

			// [Skills] 대마법사다운 높은 마법 관련 스킬
			this.SetSkill(SkillName.Wrestling, 110.0, 120.0); 
			this.SetSkill(SkillName.Tactics, 100.0, 110.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 145.0); // 마법 저항력 극대화
			this.SetSkill(SkillName.Magery, 130.0, 140.0);      // 강력한 8서클 마법 구사
			this.SetSkill(SkillName.EvalInt, 125.0, 135.0);
			this.SetSkill(SkillName.Meditation, 120.0, 140.0);

			// [Misc]
			this.Tamable = false; // 고지능 지도자급 (200 기준 불가)
			this.VirtualArmor = 15;

			this.Fame = 12000;
			this.Karma = 12000; // 선족 설정

			switch (Utility.Random(12))
            {
                case 0: PackItem(new StrangleScroll()); break;
                case 1: PackItem(new WitherScroll()); break;
                case 2: PackItem(new VampiricEmbraceScroll()); break;
			}

            this.m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(2, 5));
        }

        public MeerEternal(Serial serial)
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
        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override bool CanRummageCorpses
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
        public override int TreasureMapLevel
        {
            get
            {
                return Core.AOS ? 5 : 4;
            }
        }
        public override bool InitialInnocent
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich, 2);
            this.AddLoot(LootPack.MedScrolls, 2);
            this.AddLoot(LootPack.HighScrolls, 2);
        }

        public override int GetHurtSound()
        {
            return 0x167;
        }

        public override int GetDeathSound()
        {
            return 0xBC;
        }

        public override int GetAttackSound()
        {
            return 0x28B;
        }

        public override void OnThink()
        {
            if (DateTime.UtcNow >= this.m_NextAbilityTime)
            {
                Mobile combatant = this.Combatant as Mobile;

                if (combatant != null && combatant.Map == this.Map && combatant.InRange(this, 12))
                {
                    this.m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(10, 15));

                    int ability = Utility.Random(4);

                    switch ( ability )
                    {
                        case 0:
                            this.DoFocusedLeech(combatant, "Thine essence will fill my withering body with strength!");
                            break;
                        case 1:
                            this.DoFocusedLeech(combatant, "I rebuke thee, worm, and cleanse thy vile spirit of its tainted blood!");
                            break;
                        case 2:
                            this.DoFocusedLeech(combatant, "I devour your life's essence to strengthen my resolve!");
                            break;
                        case 3:
                            this.DoAreaLeech();
                            break;
                    // TODO: Resurrect ability
                    }
                }
            }

            base.OnThink();
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

        private void DoAreaLeech()
        {
            this.m_NextAbilityTime += TimeSpan.FromSeconds(2.5);

            this.Say(true, "Beware, mortals!  You have provoked my wrath!");
            this.FixedParticles(0x376A, 10, 10, 9537, 33, 0, EffectLayer.Waist);

            Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerCallback(DoAreaLeech_Finish));
        }

        private void DoAreaLeech_Finish()
        {
            ArrayList list = new ArrayList();
            IPooledEnumerable eable = GetMobilesInRange(6);

            foreach (Mobile m in eable)
            {
                if (this.CanBeHarmful(m) && this.IsEnemy(m))
                    list.Add(m);
            }
            eable.Free();

            if (list.Count == 0)
            {
                this.Say(true, "Bah! You have escaped my grasp this time, mortal!");
            }
            else
            {
                double scalar;

                if (list.Count == 1)
                    scalar = 0.75;
                else if (list.Count == 2)
                    scalar = 0.50;
                else
                    scalar = 0.25;

                for (int i = 0; i < list.Count; ++i)
                {
                    Mobile m = (Mobile)list[i];

                    int damage = (int)(m.Hits * scalar);

                    damage += Utility.RandomMinMax(-5, 5);

                    if (damage < 1)
                        damage = 1;

                    m.MovingParticles(this, 0x36F4, 1, 0, false, false, 32, 0, 9535, 1, 0, (EffectLayer)255, 0x100);
                    m.MovingParticles(this, 0x0001, 1, 0, false, true, 32, 0, 9535, 9536, 0, (EffectLayer)255, 0);

                    this.DoHarmful(m);
                    this.Hits += AOS.Damage(m, this, damage, 100, 0, 0, 0, 0);
                }

                this.Say(true, "If I cannot cleanse thy soul, I will destroy it!");
            }
        }

        private void DoFocusedLeech(Mobile combatant, string message)
        {
            this.Say(true, message);

            Timer.DelayCall(TimeSpan.FromSeconds(0.5), new TimerStateCallback(DoFocusedLeech_Stage1), combatant);
        }

        private void DoFocusedLeech_Stage1(object state)
        {
            Mobile combatant = (Mobile)state;

            if (this.CanBeHarmful(combatant))
            {
                this.MovingParticles(combatant, 0x36FA, 1, 0, false, false, 1108, 0, 9533, 1, 0, (EffectLayer)255, 0x100);
                this.MovingParticles(combatant, 0x0001, 1, 0, false, true, 1108, 0, 9533, 9534, 0, (EffectLayer)255, 0);
                this.PlaySound(0x1FB);

                Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(DoFocusedLeech_Stage2), combatant);
            }
        }

        private void DoFocusedLeech_Stage2(object state)
        {
            Mobile combatant = (Mobile)state;

            if (this.CanBeHarmful(combatant))
            {
                combatant.MovingParticles(this, 0x36F4, 1, 0, false, false, 32, 0, 9535, 1, 0, (EffectLayer)255, 0x100);
                combatant.MovingParticles(this, 0x0001, 1, 0, false, true, 32, 0, 9535, 9536, 0, (EffectLayer)255, 0);

                this.PlaySound(0x209);
                this.DoHarmful(combatant);
                this.Hits += AOS.Damage(combatant, this, Utility.RandomMinMax(30, 40) - (Core.AOS ? 0 : 10), 100, 0, 0, 0, 0);
            }
        }
    }
}