using System;
using System.Collections;
using Server.Engines.CannedEvil;
using Server.Items;
using System.Collections.Generic;
using Server.Network;
using System.Linq;

namespace Server.Mobiles
{
    [CorpseName("a bone demon corpse")]
    public class BoneDemon : BaseCreature
    {
        private DateTime m_NextAbilityTime;
        [Constructable]
        public BoneDemon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a bone demon";
            Body = 308;
            BaseSoundID = 0x48D;

			Boss = true;

            /* [Bone Demon - Fame 20,000 / Boss / Weight 1.26]
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   - VirtualArmor: (20,000/1000) + 5 = 25
			   -------------------------------------------------- */

			// 최종 Str 약 15,750 (민맥 편차 축소)
			this.SetStr(13100, 13400); 

			// 최종 Hits 약 349,000 (안정적인 탱킹 체력 확보)
			this.SetHits(292800, 294800); 

			// 최종 Dex/Int 약 3,150
			this.SetDex(2600, 2700);
			this.SetInt(2600, 2700);

			// 최종 Stam/Mana 약 3,320
			this.SetStam(2750, 2850);
			this.SetMana(2750, 2850);

			// [Combat Options]
			SetAttackSpeed(2.5);
			SetDamage(75, 110);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 60, 70);
			this.SetResistance(ResistanceType.Fire, 25, 35);
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 70, 75);
			this.SetResistance(ResistanceType.Energy, 35, 45);

			// [Skills] 최종 210.0 부근 (편차 축소)
			this.SetSkill(SkillName.Wrestling, 125.0, 130.0);
			this.SetSkill(SkillName.Tactics, 125.0, 130.0);
			this.SetSkill(SkillName.Anatomy, 125.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 140.0);

			// 가방 방어력: (20,000/1000) + 5 = 25
			this.VirtualArmor = 25;

			this.Fame = 20000;
			this.Karma = -20000;
        }
        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (DateTime.Now > m_NextAbilityTime )
            {
				Lightning();
            }
            m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds(Utility.RandomMinMax(4, 6));
        }		
		
        #region Lightning
        private void Lightning()
        {
            int count = 0;

            IPooledEnumerable eable = GetMobilesInRange(16);
            foreach (Mobile m in eable)
            {
                if (m.IsPlayer() && GetDistanceToSqrt(m) <= 16 && CanBeHarmful(m))
                {
                    if (m is AncientLich || m is BoneDemon)
                        continue;

                    DoHarmful(m);

                    Effects.SendBoltEffect(m, false, 0);
                    Effects.PlaySound(m, m.Map, 0x51D);

                    double damage = m.Hits * 0.6;

                    if (damage < 300.0)
                        damage = 300.0;
                    else if (damage > 4000.0)
                        damage = 4000.0;

                    AOS.Damage(m, this, (int)damage, 0, 0, 0, 0, 100);

                    count++;

                    if (count >= 6)
                        break;
                }
            }

            eable.Free();
        }
        #endregion
        public BoneDemon(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get
            {
                return !Core.SE;
            }
        }
        public override bool Unprovokable
        {
            get
            {
                return Core.SE;
            }
        }
        public override bool AreaPeaceImmune
        {
            get
            {
                return Core.SE;
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
                return 1;
            }
        }
        public override void GenerateLoot()
        {

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
