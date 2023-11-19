using System;
using System.Collections;
using Server.Engines.CannedEvil;
using Server.Items;
using System.Collections.Generic;
using Server.Network;
using System.Linq;

namespace Server.Mobiles
{
    [CorpseName("an ancient liche's corpse")]
    public class AncientLich : BaseCreature
    {
        private DateTime m_NextDiscordTime;
        private DateTime m_NextAbilityTime;

        [Constructable]
        public AncientLich()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            //Name = NameList.RandomName("ancient lich");
			Name = "Boss an ancient lich";
            Body = 830;
            //BaseSoundID = 412;

            SetStr(1416, 1505);
            SetDex(1596, 1615);
            SetInt(5766, 5845);

            SetHits(93060, 95509);
			SetStam(300, 999);
			SetMana(1000, 1500);

            this.SetDamage(280, 300);

			SetAttackSpeed( 20.0 );

            SetDamageType(ResistanceType.Physical, 20);
            SetDamageType(ResistanceType.Cold, 40);
            SetDamageType(ResistanceType.Energy, 40);

            SetResistance(ResistanceType.Physical, 85, 95);
            SetResistance(ResistanceType.Fire, 25, 30);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 80, 90);
            SetResistance(ResistanceType.Energy, 25, 30);

            SetSkill(SkillName.EvalInt, 310.1, 325.0);
            SetSkill(SkillName.Magery, 310.1, 325.0);
            SetSkill(SkillName.Meditation, 310.1, 321.0);
            SetSkill(SkillName.Poisoning, 310.1, 321.0);
            SetSkill(SkillName.MagicResist, 310.2, 325.0);
            SetSkill(SkillName.Tactics, 310.1, 325.0);
            SetSkill(SkillName.Wrestling, 380.1, 400.0);

            Fame = 26000;
            Karma = -26000;

			Boss = true;
			
            VirtualArmor = 260;
        }
        public override int GetAttackSound() { return 0x61E; }
        public override int GetDeathSound() { return 0x61F; }
        public override int GetHurtSound() { return 0x620; }
        public override int GetIdleSound() { return 0x621; }

        public override bool CanRummageCorpses { get { return true; } }
        public override bool BleedImmune { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Lethal; } }
        public override bool ShowFameTitle { get { return false; } }
        public override bool ClickTitle { get { return false; } }
		
        public AncientLich(Serial serial)
            : base(serial)
        {
        }

        public void ChangeCombatant()
        {
            ForceReacquire();
            BeginFlee(TimeSpan.FromSeconds(2.5));
        } 
        public override void OnThink()
        {
            if (m_NextDiscordTime <= DateTime.UtcNow)
            {
                Mobile target = Combatant as Mobile;

                if (target != null && target.InRange(this, 16) && CanBeHarmful(target))
                    Discord(target);
            }
        }
        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (DateTime.UtcNow > m_NextAbilityTime )
            {
				BlastRadius();
            }
            m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(4, 6));
        }		
		
        #region Blast Radius
        private static readonly int BlastRange = 16;

        private static readonly double[] BlastChance = new double[]
            {
                0.0, 0.0, 0.05, 0.95, 0.95, 0.95, 0.05, 0.95, 0.95,
                0.95, 0.05, 0.95, 0.95, 0.95, 0.05, 0.95, 0.95
            };

        private void BlastRadius()
        {
            // TODO: Based on OSI taken videos, not accurate, but an aproximation

            Point3D loc = Location;

            for (int x = -BlastRange; x <= BlastRange; x++)
            {
                for (int y = -BlastRange; y <= BlastRange; y++)
                {
                    Point3D p = new Point3D(loc.X + x, loc.Y + y, loc.Z);
                    int dist = (int)Math.Round(Utility.GetDistanceToSqrt(loc, p));

                    if (dist <= BlastRange && BlastChance[dist] > Utility.RandomDouble())
                    {
                        Timer.DelayCall(TimeSpan.FromSeconds(0.1 * dist), new TimerCallback(
                            delegate
                            {
                                int hue = Utility.RandomList(90, 95);

                                Effects.SendPacket(loc, Map, new HuedEffect(EffectType.FixedXYZ, Serial.Zero, Serial.Zero, 0x3709, p, p, 77, 88, true, false, hue, 4));
                            }
                        ));
                    }
                }
            }

            PlaySound(0x64C);

            IPooledEnumerable eable = GetMobilesInRange(BlastRange);
            foreach (Mobile m in eable)
            {
                if (this != m && GetDistanceToSqrt(m) <= BlastRange && CanBeHarmful(m))
                {
                    if (m is ShadowDweller || m is BoneDemon || m is AncientLich)
                        continue;

                    DoHarmful(m);

                    double damage = m.Hits * 0.6;

                    if (damage < 400.0)
                        damage = 400.0;
                    else if (damage > 6000.0)
                        damage = 6000.0;

                    DoHarmful(m);

                    AOS.Damage(m, this, (int)damage, 0, 0, 0, 0, 100);
                }
            }

            eable.Free();
        }
        #endregion


        #region Unholy Touch
        private static Dictionary<Mobile, Timer> m_UnholyTouched = new Dictionary<Mobile, Timer>();

        public void Discord(Mobile target)
        {
            if (!m_UnholyTouched.ContainsKey(target))
            {
                int scalar = 100;

                ArrayList mods = new ArrayList();

                if (target.PhysicalResistance > 0)
                {
                    mods.Add(new ResistanceMod(ResistanceType.Physical, -(target.PhysicalResistance - scalar)));
                }

                if (target.FireResistance > 0)
                {
                    mods.Add(new ResistanceMod(ResistanceType.Fire, -(target.FireResistance - scalar)));
                }

                if (target.ColdResistance > 0)
                {
                    mods.Add(new ResistanceMod(ResistanceType.Cold, -(target.ColdResistance - scalar)));
                }

                if (target.PoisonResistance > 0)
                {
                    mods.Add(new ResistanceMod(ResistanceType.Poison, -(target.PoisonResistance - scalar)));
                }

                if (target.EnergyResistance > 0)
                {
                    mods.Add(new ResistanceMod(ResistanceType.Energy, -(target.EnergyResistance - scalar)));
                }

                for (int i = 0; i < target.Skills.Length; ++i)
                {
                    if (target.Skills[i].Value > 0)
                    {
                        mods.Add(new DefaultSkillMod((SkillName)i, true, -(target.Skills[i].Value - scalar)));                        
                    }
                }
                
                target.PlaySound(0x458);

                ApplyMods(target, mods);

                m_UnholyTouched[target] = Timer.DelayCall(TimeSpan.FromSeconds(30), new TimerCallback(
                    delegate
                    {
                        ClearMods(target, mods);

                        m_UnholyTouched.Remove(target);
                    }));
            }

            m_NextDiscordTime = DateTime.UtcNow + TimeSpan.FromSeconds(5 + Utility.RandomDouble() * 22);
        }

        private static void ApplyMods(Mobile from, ArrayList mods)
        {
            for (int i = 0; i < mods.Count; ++i)
            {
                object mod = mods[i];

                if (mod is ResistanceMod)
                    from.AddResistanceMod((ResistanceMod)mod);
                else if (mod is StatMod)
                    from.AddStatMod((StatMod)mod);
                else if (mod is SkillMod)
                    from.AddSkillMod((SkillMod)mod);
            }
        }

        private static void ClearMods(Mobile from, ArrayList mods)
        {
            for (int i = 0; i < mods.Count; ++i)
            {
                object mod = mods[i];

                if (mod is ResistanceMod)
                    from.RemoveResistanceMod((ResistanceMod)mod);
                else if (mod is StatMod)
                    from.RemoveStatMod(((StatMod)mod).Name);
                else if (mod is SkillMod)
                    from.RemoveSkillMod((SkillMod)mod);
            }
        }
        #endregion		
		
        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override bool Unprovokable
        {
            get
            {
                return true;
            }
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich, 3);
            this.AddLoot(LootPack.MedScrolls, 2);
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
