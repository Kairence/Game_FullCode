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
            Name = "Boss a bone demon";
            Body = 308;
            BaseSoundID = 0x48D;

            SetStr(1000, 1500);
            SetDex(251, 375);
            SetInt(1001, 1020);

            SetHits(73100, 75200);
			SetStam(750, 1000);
			SetMana(1000, 2500);
			
			Boss = true;
			
            SetDamage(52, 120);

			SetAttackSpeed( 2.5 );

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Cold, 50);

            SetResistance(ResistanceType.Physical, 75);
            SetResistance(ResistanceType.Fire, 60);
            SetResistance(ResistanceType.Cold, 90);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 60);

            SetSkill(SkillName.Wrestling, 250.0, 320.0);
            SetSkill(SkillName.Tactics, 275.0, 285.0);
            SetSkill(SkillName.MagicResist, 270.1, 275.0);
            SetSkill(SkillName.DetectHidden, 275.0, 285.0);
            SetSkill(SkillName.Magery, 270.6, 275.5);
            SetSkill(SkillName.EvalInt, 270.6, 275.5);
            SetSkill(SkillName.Meditation, 270.0, 275.0);

            Fame = 22000;
            Karma = -22000;

            VirtualArmor = 24;
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
