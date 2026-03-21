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
			Name = "an ancient lich";
            Body = 830;
			Boss = true;
            //BaseSoundID = 412;

            /* [Ancient Lich - Fame 26,000 / Boss / Weight 1.21]
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 극소화)
			   - VirtualArmor: (26,000/1000) - 1 = 25
			   -------------------------------------------------- */

			// 최종 Str 약 22,000 (민맥 차이 500 내외)
			this.SetStr(18100, 18600); 

			// 최종 Hits 약 488,000 (편차 2,000 이내로 고정)
			this.SetHits(406500, 408500); 

			// 최종 Dex/Int 약 4,400 
			this.SetDex(3600, 3750);
			this.SetInt(3600, 3750);

			// 최종 Stam/Mana 약 4,600
			this.SetStam(3800, 3950);
			this.SetMana(3800, 3950);

			// [Combat Options]
			SetAttackSpeed(7.0);
			SetDamage(75, 110);

			// [Resistances] 최고 저항 75 이하 준수
			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 50, 60);
			this.SetResistance(ResistanceType.Cold, 70, 75);
			this.SetResistance(ResistanceType.Poison, 60, 70);
			this.SetResistance(ResistanceType.Energy, 65, 75);

			// [Skills] 최종 293.5 부근 (편차 축소)
			this.SetSkill(SkillName.Wrestling, 170.0, 175.0);
			this.SetSkill(SkillName.Magery, 170.0, 175.0);
			this.SetSkill(SkillName.EvalInt, 170.0, 175.0);
			this.SetSkill(SkillName.Meditation, 170.0, 175.0);
			this.SetSkill(SkillName.MagicResist, 170.0, 175.0);

			// 가방 방어력: (26,000/1000) - 1 = 25
			this.VirtualArmor = 25;

			this.Fame = 26000;
			this.Karma = -26000;      
			this.SpecialType2 = 4;
			this.SpecialChance2 = 0.30;	
			
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
