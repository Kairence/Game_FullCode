using System;
using System.Collections;

namespace Server.Mobiles
{
    [CorpseName("a rai-ju corpse")]
    public class RaiJu : BaseCreature
    {
        private static readonly Hashtable m_Table = new Hashtable();
        [Constructable]
        public RaiJu()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a Rai-Ju";
            this.Body = 199;
            this.BaseSoundID = 0x346;

			/* [Rai-Ju - Holy City Dungeon / Original Wiki & Keep Formula]
			   - 명성: 7,000 / 카르마: -7,000 (칼둔 아님)
			   - 슬롯: 2 (조합형 펫)
			   - 가방 방어력: 5 (VA 공식 적용)
			   -------------------------------------------------- */

			// [Attributes] 가중치 1.15 적용
			this.SetStr(200, 300); 
			this.SetHits(3500, 4500); // 저항이 낮은 대신 체력으로 승부
			this.SetDex(175, 250);    // 위키 고증: 매우 높은 민첩성
			this.SetInt(150, 250);

			// [Combat Options] 물리 50% / 에너지 50% (위키 고증)
			this.SetDamage(30, 50); 
			this.SetAttackSpeed(2.0); 
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			// [Resistances] 위키 기반 + 형님표 75% 상한선 준수
			this.SetResistance(ResistanceType.Physical, 35, 50); // 사냥 타격감 확보
			this.SetResistance(ResistanceType.Fire, 30, 40);     
			this.SetResistance(ResistanceType.Cold, 10, 20);      // 위키 고증: 냉기 취약
			this.SetResistance(ResistanceType.Poison, 30, 40); 
			this.SetResistance(ResistanceType.Energy, 60, 70);    // 에너지 특화 (70% 미만 유지)

			// [Skills]
			this.SetSkill(SkillName.Wrestling, 105.0, 120.0); 
			this.SetSkill(SkillName.Tactics, 105.0, 120.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 115.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Magery, 90.0, 105.0); // 번개 마법 시전

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; 
			this.MinTameSkill = 125.1; 
			this.VirtualArmor = 5; // 공식: (7000/1000) - 2

			this.Fame = 7000;
			this.Karma = -7000;
		}
        public RaiJu(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich, 2);
            this.AddLoot(LootPack.Gems, 2);
        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (0.1 > Utility.RandomDouble() && !this.IsStunned(defender))
            {
                /* Lightning Fist
                * Cliloc: 1070839
                * Effect: Type: "3" From: "0x57D4F5B" To: "0x0" ItemId: "0x37B9" ItemIdName: "glow" FromLocation: "(884 715, 10)" ToLocation: "(884 715, 10)" Speed: "10" Duration: "5" FixedDirection: "True" Explode: "False"
                * Damage: 35-65, 100% energy, resistable
                * Freezes for 4 seconds
                * Effect cannot stack
                */
                defender.FixedEffect(0x37B9, 10, 5);
                defender.SendLocalizedMessage(1070839); // The creature attacks with stunning force!
 
                // This should be done in place of the normal attack damage.
                //AOS.Damage( defender, this, Utility.RandomMinMax( 35, 65 ), 0, 0, 0, 0, 100 );

                defender.Frozen = true; 

                ExpireTimer timer = new ExpireTimer(defender, TimeSpan.FromSeconds(4.0));
                timer.Start();
                m_Table[defender] = timer;
            }
        }

        public bool IsStunned(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        private class ExpireTimer : Timer
        {
            private readonly Mobile m_Mobile;
            public ExpireTimer(Mobile m, TimeSpan delay)
                : base(delay)
            {
                this.m_Mobile = m;
                this.Priority = TimerPriority.TwoFiftyMS;
            }

            public void DoExpire()
            {
                this.m_Mobile.Frozen = false;
                this.Stop();
                m_Table.Remove(this.m_Mobile);
            }

            protected override void OnTick()
            {
                this.m_Mobile.SendLocalizedMessage(1005603); // You can move again!
                this.DoExpire();
            }
        }
    }
}