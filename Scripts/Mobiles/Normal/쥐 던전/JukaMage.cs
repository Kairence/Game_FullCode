using System;
using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
    [CorpseName("a juka corpse")] 
    public class JukaMage : BaseCreature
    {
        private DateTime m_NextAbilityTime;
		
        [Constructable]
        public JukaMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a juka mage";
            Body = 765;

			/* [Juka Mage - Normal - Fame 12,000 / Weight 1.22]
			   - 정글 던전의 쥬카 술사 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 10 (명성/1000 - 2 보정)
			   - 특이사항: 강력한 원거리 마법 및 높은 마나 회복력
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(270, 285); 
			this.SetHits(6100, 6200); 
			this.SetDex(60, 75);
			this.SetInt(250, 270); // 높은 지능으로 인한 강력한 마법 데미지

			// [Combat Options] 물리 20% / 에너지 80% (마력 깃든 지팡이)
			this.SetDamage(25, 45);
			this.SetAttackSpeed(2.5); 
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Energy, 80);

			// [Resistances] 최고 저항 75 이하 준수 / 물리 약점 설정
			this.SetResistance(ResistanceType.Physical, 25, 35); // ★ 확실한 약점 (근접전에 취약)
			this.SetResistance(ResistanceType.Fire, 50, 65);      
			this.SetResistance(ResistanceType.Cold, 50, 65);    
			this.SetResistance(ResistanceType.Poison, 50, 65); 
			this.SetResistance(ResistanceType.Energy, 70, 75);  // 마법 에너지 내성 특화

			// [Skills] 기본 110~120에 역산 보너스(9.2) 가산
			// 최종 숙련도 약 120~130대의 숙련된 술사
			this.SetSkill(SkillName.Wrestling, 119.0, 129.0); 
			this.SetSkill(SkillName.Tactics, 119.0, 129.0);
			this.SetSkill(SkillName.Magery, 125.0, 140.0);      // 상급 마법 구사
			this.SetSkill(SkillName.EvalInt, 125.0, 140.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 145.0); // 높은 마법 저항력

			this.Tamable = false;
			this.VirtualArmor = 10;
			this.Fame = 12000;
			this.Karma = -12000;

            Container bag = new Bag();

            int count = Utility.RandomMinMax(10, 20);

            for (int i = 0; i < count; ++i)
            {
                Item item = Loot.RandomReagent();

                if (item == null)
                    continue;

                if (!bag.TryDropItem(this, item, false))
                    item.Delete();
            }

            PackItem(bag);

            PackItem(new ArcaneGem());

            if (Core.ML && Utility.RandomDouble() < .33)
                PackItem(Engines.Plants.Seed.RandomPeculiarSeed(2));

            m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(2, 5));
        }

        public JukaMage(Serial serial)
            : base(serial)
        {
        }

        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
		
		public override int TreasureMapLevel { get { return 3; } }
		
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
            AddLoot(LootPack.MedScrolls, 2);
        }

        public override int GetIdleSound()
        {
            return 0x1AC;
        }

        public override int GetAngerSound()
        {
            return 0x1CD;
        }

        public override int GetHurtSound()
        {
            return 0x1D0;
        }

        public override int GetDeathSound()
        {
            return 0x28D;
        }

        public override void OnThink()
        {
            if (DateTime.UtcNow >= this.m_NextAbilityTime)
            {
                JukaLord toBuff = null;
                IPooledEnumerable eable = GetMobilesInRange(8);

                foreach (Mobile m in eable)
                {
                    if (m is JukaLord && this.IsFriend(m) && m.Combatant != null && this.CanBeBeneficial(m) && m.CanBeginAction(typeof(JukaMage)) && this.InLOS(m))
                    {
                        toBuff = (JukaLord)m;
                        break;
                    }
                }
                eable.Free();

                if (toBuff != null)
                {
                    if (this.CanBeBeneficial(toBuff) && toBuff.BeginAction(typeof(JukaMage)))
                    {
                        this.m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 60));

                        toBuff.Say(true, "Give me the power to destroy my enemies!");
                        this.Say(true, "Fight well my lord!");

                        this.DoBeneficial(toBuff);

                        object[] state = new object[] { toBuff, toBuff.HitsMaxSeed, toBuff.RawStr, toBuff.RawDex };

                        SpellHelper.Turn(this, toBuff);

                        int toScale = toBuff.HitsMaxSeed;

                        if (toScale > 0)
                        {
                            toBuff.HitsMaxSeed += AOS.Scale(toScale, 75);
                            toBuff.Hits += AOS.Scale(toScale, 75);
                        }

                        toScale = toBuff.RawStr;

                        if (toScale > 0)
                            toBuff.RawStr += AOS.Scale(toScale, 50);

                        toScale = toBuff.RawDex;

                        if (toScale > 0)
                        {
                            toBuff.RawDex += AOS.Scale(toScale, 50);
                            toBuff.Stam += AOS.Scale(toScale, 50);
                        }

                        toBuff.Hits = toBuff.Hits;
                        toBuff.Stam = toBuff.Stam;

                        toBuff.FixedParticles(0x375A, 10, 15, 5017, EffectLayer.Waist);
                        toBuff.PlaySound(0x1EE);

                        Timer.DelayCall(TimeSpan.FromSeconds(20.0), new TimerStateCallback(Unbuff), state);
                    }
                }
                else
                {
                    this.m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(2, 5));
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

        private void Unbuff(object state)
        {
            object[] states = (object[])state;

            JukaLord toDebuff = (JukaLord)states[0];

            toDebuff.EndAction(typeof(JukaMage));

            if (toDebuff.Deleted)
                return;

            toDebuff.HitsMaxSeed = (int)states[1];
            toDebuff.RawStr = (int)states[2];
            toDebuff.RawDex = (int)states[3];

            toDebuff.Hits = toDebuff.Hits;
            toDebuff.Stam = toDebuff.Stam;
        }
    }
}