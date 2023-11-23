using System;
using System.Collections.Generic;
using System.Linq;

using Server.Engines.SphynxFortune;
using Server.Engines.XmlSpawner2;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Second;
using Server.Spells.Fifth;
using Server.Spells.Bushido;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Chivalry;
using Server.Spells.Necromancy;
using Server.Spells.Spellweaving;
using Server.SkillHandlers;
using Server.Engines.CityLoyalty;
using Server.Services.Virtues;
using Server.Spells.SkillMasteries;
using Server.Regions;

namespace Server
{
    public enum DamageType
    {
        Melee,
        Ranged,
        Spell,
        SpellAOE
    }

    public class AOS
    {
        public static void DisableStatInfluences()
        {
            for (int i = 0; i < SkillInfo.Table.Length; ++i)
            {
                SkillInfo info = SkillInfo.Table[i];

                info.StrScale = 0.0;
                info.DexScale = 0.0;
                info.IntScale = 0.0;
                info.StatTotal = 0.0;
            }
        }

        public static int Damage(IDamageable m, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy)
        {
            return Damage(m, null, damage, ignoreArmor, phys, fire, cold, pois, nrgy);
        }

        public static int Damage(IDamageable m, int damage, int phys, int fire, int cold, int pois, int nrgy)
        {
            return Damage(m, null, damage, phys, fire, cold, pois, nrgy);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, false);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, 0, false);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, false);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy)
        {
            return Damage(m, from, damage, ignoreArmor, phys, fire, cold, pois, nrgy, 0, 0, false);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, bool keepAlive)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, keepAlive);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, bool keepAlive, bool archer, bool deathStrike)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, keepAlive, archer ? DamageType.Ranged : DamageType.Melee); // old deathStrike damage, kept for compatibility
        }

        public static int Damage(IDamageable m, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, bool keepAlive, bool archer, bool deathStrike, int aggro)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, keepAlive, archer ? DamageType.Ranged : DamageType.Melee, aggro); // old deathStrike damage, kept for compatibility
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, DamageType type)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, false, type);
        }

        public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, DamageType type)
        {
            return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, false, type);
        }

        public static int Damage(IDamageable damageable, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, bool keepAlive, DamageType type = DamageType.Melee, int aggro = 100)
        {
			//from : 공격자
			//m : 방어자
            Mobile m = damageable as Mobile;
			Server.Engines.Craft.AutoCraftTimer.EndTimer(from);
			Server.Engines.Craft.AutoCraftTimer.EndTimer(m);

            if (damageable == null)// || damageable.Deleted || !damageable.Alive )
                return 0;

            if (m != null && phys == 0 && fire == 100 && cold == 0 && pois == 0 && nrgy == 0)
                Mobiles.MeerMage.StopEffect(m, true);

            if (!Core.AOS)
            {
                if(m != null)
                    m.Damage(damage, from);

                return damage;
            }

            #region Mondain's Legacy
            if (m != null)
            {
                m.Items.ForEach(i =>
                {
                    ITalismanProtection prot = i as ITalismanProtection;

                    if (prot != null)
                        damage = prot.Protection.ScaleDamage(from, damage);
                });
            }
            #endregion

            Fix(ref phys);
            Fix(ref fire);
            Fix(ref cold);
            Fix(ref pois);
            Fix(ref nrgy);
            Fix(ref chaos);
            Fix(ref direct);

            bool ranged = type == DamageType.Ranged;
            BaseQuiver quiver = null;

            if (ranged && from.Race != Race.Gargoyle)
                quiver = from.FindItemOnLayer(Layer.Cloak) as BaseQuiver;

            int totalDamage;

            if (!ignoreArmor)
            {
				//옵션 계산
				int physDamage = (damage + SAAbsorptionAttributes.GetValue(from, SAAbsorptionAttribute.EaterDamage ) ) * phys * (100 - damageable.PhysicalResistance);
				int fireDamage = (damage + SAAbsorptionAttributes.GetValue(from, SAAbsorptionAttribute.EaterFire ) ) * fire * (100 - damageable.FireResistance);
				int coldDamage = (damage + SAAbsorptionAttributes.GetValue(from, SAAbsorptionAttribute.EaterCold ) ) * cold * (100 - damageable.ColdResistance);
				int poisonDamage = (damage + SAAbsorptionAttributes.GetValue(from, SAAbsorptionAttribute.EaterPoison ) ) * pois * (100 - damageable.PoisonResistance);
				int energyDamage = (damage + SAAbsorptionAttributes.GetValue(from, SAAbsorptionAttribute.EaterEnergy ) ) * nrgy * (100 - damageable.EnergyResistance);
				int chaosDamage = damage * chaos * (100 - damageable.ChaosResistance );
				int directDamage = damage * direct * (100 - damageable.DirectResistance );

				if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					fireDamage = (int)( fireDamage * ( 1 + pm.SilverPoint[11] * 0.1 + SAAbsorptionAttributes.GetValue(pm, SAAbsorptionAttribute.ResonanceFire ) * 0.001 ));
					coldDamage = (int)( coldDamage * ( 1 + pm.SilverPoint[12] * 0.1 + SAAbsorptionAttributes.GetValue(pm, SAAbsorptionAttribute.ResonanceCold ) * 0.001 ));
					poisonDamage = (int)( poisonDamage * ( 1 + pm.SilverPoint[13] * 0.1 + SAAbsorptionAttributes.GetValue(pm, SAAbsorptionAttribute.ResonancePoison ) * 0.001 ));
					energyDamage = (int)( energyDamage * ( 1 + pm.SilverPoint[14] * 0.1 + SAAbsorptionAttributes.GetValue(pm, SAAbsorptionAttribute.ResonanceEnergy ) * 0.001 ));
				}
				
				
				totalDamage = physDamage + fireDamage + coldDamage + poisonDamage + energyDamage + chaosDamage + directDamage;
				totalDamage /= 10000;
            }
            else if (Core.ML && m is PlayerMobile)
            {
                if (quiver != null)
                    damage += damage * quiver.DamageIncrease / 100;

                totalDamage = Math.Min(damage, Core.TOL && ranged ? 30 : 35);	// Direct Damage cap of 30/35
            }
            else
            {
                totalDamage = damage;

                if (Core.ML && quiver != null)
                    totalDamage += totalDamage * quiver.DamageIncrease / 100;
            }

            // object being damaged is not a mobile, so we will end here
            if (damageable is Item)
            {
                return damageable.Damage(totalDamage, from);
            }

            #region Evil Omen, Blood Oath and reflect physical
            if (EvilOmenSpell.TryEndEffect(m))
            {
                totalDamage = (int)(totalDamage * 1.25);
            }

			if( m.Frozen )
			{
				totalDamage *= 150;
				totalDamage /= 100;
			}

			if( m.Combatant == null && from is Mobile )
			{
				m.Combatant = from;
			}
			
			if( from is BaseCreature )
			{
				BaseCreature bc = from as BaseCreature;
				if( bc.ControlMaster == null && bc.SummonMaster == null )
				{
					if( !bc.Boss && bc.attackcharge >= 300 )
						totalDamage *= 10;

					if( Utility.RandomDouble() < 0.1 * Util.MonsterGrade(bc.Grade) )
					{
						bc.RawDex += Util.MonsterGrade(bc.Grade);
						bc.RawStr += Util.MonsterGrade(bc.Grade);
						bc.RawInt += Util.MonsterGrade(bc.Grade);
						bc.HitsMaxSeed += Util.MonsterGrade(bc.Grade);
						bc.Hits += Util.MonsterGrade(bc.Grade);
						bc.StamMaxSeed += Util.MonsterGrade(bc.Grade);
						bc.Stam++;
						bc.ManaMaxSeed++;
						bc.Mana++;
						if( bc.Fame > 0 && from.Fame < bc.Fame && bc.Fame < 17500 + Util.MonsterGrade(bc.Grade) * 2500 )
							bc.Fame++;
					}
				
					for( int i = 0; i < 10000; i++ )
					{
						if( m != null && ( bc.AggroMobile[i] == m || bc.AggroMobile[i] == null ) )
						{
							int doublecal = 4;
							bc.AggroMobile[i] = m;
							BaseShield shield = m.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
							if( shield != null )
								doublecal = 2;

							if( aggro > 0 )
							{
								double aggrobonus = 1;
								double aggrominus = 1;
								if( m is PlayerMobile )
								{
									PlayerMobile pm = m as PlayerMobile;
									aggrobonus += pm.SilverPoint[3] * 0.004;
									aggrominus -= pm.SilverPoint[4] * 0.0015;
									if( aggrominus == 0 )
										aggrominus = 0.0025;
								}							
								int realaggro = (int)( (( totalDamage / doublecal ) * aggro * aggrobonus / 100 ) / aggrominus);
								bc.AggroScore[i] += realaggro / doublecal;
							}
							break;
						}
					}
				}
				else if ( bc.ControlMaster != null )
				{
					if( m is BaseCreature )
					{
						BaseCreature monster = m as BaseCreature;
						if( monster.ControlMaster == null && monster.SummonMaster == null )
						{
							if( bc.ControlSlots > 0 )
							{
								int maxStat = (int)( ( 50 + bc.ControlMaster.Skills.AnimalLore.Value ) * bc.ControlSlots );
								int maxHits = (int)( bc.ControlMaster.Skills.AnimalLore.Value * 20 + bc.ControlSlots * 1000 );
								int maxStamMana = maxHits / 10;
								if( maxStat < bc.RawStr )
								{
									if( Misc.Util.PetStat( bc, bc.StatExp[0], bc.RawStr, monster.RawStr ) )
										bc.RawStr++;
								}
								if( maxStat < bc.RawDex )
								{
									if( Misc.Util.PetStat( bc, bc.StatExp[1], bc.RawDex, monster.RawDex ) )
										bc.RawDex++;
								}
								if( maxStat < bc.RawInt )
								{
									if( Misc.Util.PetStat( bc, bc.StatExp[2], bc.RawInt, monster.RawInt ) )
										bc.RawInt++;
								}
								if( maxHits < bc.HitsMaxSeed )
								{
									if( Misc.Util.PetStat( bc, bc.StatExp[3], bc.HitsMaxSeed, monster.HitsMaxSeed ) )
										bc.HitsMaxSeed++;
								}
								if( maxStamMana < bc.StamMaxSeed )
								{
									if( Misc.Util.PetStat( bc, bc.StatExp[4], bc.StamMaxSeed, monster.StamMaxSeed ) )
										bc.StamMaxSeed++;
								}
								if( maxStamMana < bc.ManaMaxSeed )
								{
									if( Misc.Util.PetStat( bc, bc.StatExp[5], bc.ManaMaxSeed, monster.ManaMaxSeed ) )
										bc.ManaMaxSeed++;
								}
							}
								
						}
						
					}
					
				}
			}

			if( m is BaseCreature )
			{
				BaseCreature bc = m as BaseCreature;
				
				if( bc.ControlMaster == null && bc.SummonMaster == null )
				{
					//totalDamage -= bc.VirtualArmor;
					
					if( bc is GolemController )
					{
						if ( totalDamage > 200 )
						{
							totalDamage = 200 + totalDamage / 50;
						}
					}
					
					if( bc is OrcBrute )
					{
						OrcBrute ob = bc as OrcBrute;
						if( ob.Hits < 2000 && !ob.regenBonus )
						{
							Util.Good_Effect( ob );
							ob.regenBonus = true;
							ob.Say("전투는 지금부터다!");
							ob.Hits = ob.HitsMaxSeed;
							ob.Stam = ob.StamMaxSeed;
						}
					}
					bc.attackcharge++;
					if( Utility.RandomDouble() < 0.05 * Util.MonsterGrade(bc.Grade) )
					{
						bc.RawDex += Util.MonsterGrade(bc.Grade);
						bc.RawStr += Util.MonsterGrade(bc.Grade);
						bc.RawInt += Util.MonsterGrade(bc.Grade);
						bc.HitsMaxSeed += Util.MonsterGrade(bc.Grade);
						bc.Hits += Util.MonsterGrade(bc.Grade);
						bc.StamMaxSeed += Util.MonsterGrade(bc.Grade);
						bc.Stam++;
						bc.ManaMaxSeed++;
						bc.Mana++;
						if( bc.Fame > 0 && from.Fame < bc.Fame && bc.Fame < 17500 + Util.MonsterGrade(bc.Grade) * 2500 )
							bc.Fame += Util.MonsterGrade(bc.Grade);
					}
					for( int i = 0; i < 10000; i++ )
					{
						if( (from != null ) && ( bc.AggroMobile[i] == from || bc.AggroMobile[i] == null ) )
						{
							bc.AggroMobile[i] = from;

							int doublecal = 1;
							
							BaseShield shield = from.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
							if( shield != null )
								doublecal = 2;
							
							if( aggro > 0 )
							{
								double aggrobonus = 1;
								double aggrominus = 1;
								if( from is PlayerMobile )
								{
									PlayerMobile pm = from as PlayerMobile;
									aggrobonus += pm.SilverPoint[3] * 0.005;
									aggrominus -= pm.SilverPoint[4] * 0.0025;
									if( aggrominus == 0 )
										aggrominus = 0.0025;
								}							
								int realaggro = (int)(( ( totalDamage / doublecal ) * aggro * aggrobonus / 100 ) / aggrominus);
								bc.AggroScore[i] += realaggro * doublecal;
							}
							break;
						}
					}
				}
			}

			if( from is BloodElemental )
			{
				if( !from.InRange(m, 1) )
				{
                    m.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
					m.PlaySound(0x1FE);
					from.MoveToWorld(m.Location, m.Map);
				}
			}
            if (from != null && !from.Deleted && from.Alive && !from.IsDeadBondedPet)
            {
                if (!ignoreArmor && from != m)
                {
                    int reflectPhys = Math.Min(300, AosAttributes.GetValue(m, AosAttribute.ReflectPhysical));

                    if (reflectPhys != 0)
                    {
                        if (from is ExodusMinion && ((ExodusMinion)from).FieldActive || from is ExodusOverseer && ((ExodusOverseer)from).FieldActive)
                        {
                            from.FixedParticles(0x376A, 20, 10, 0x2530, EffectLayer.Waist);
                            from.PlaySound(0x2F4);
                            m.SendAsciiMessage("Your weapon cannot penetrate the creature's magical barrier");
                        }
                        else
                        {
                            from.Damage(Scale((damage * phys * (100 - (ignoreArmor ? 0 : m.PhysicalResistance))) / 10000, reflectPhys), m);
                        }
                    }
                }
            }
            #endregion

			if( from is BaseCreature )
			{
				BaseCreature bc = from as BaseCreature;

				List<Mobile> list = new List<Mobile>();
				IPooledEnumerable eable = bc.GetMobilesInRange(10);
				int targetcount = 0;
				foreach (Mobile targets in eable)
				{
					if ( bc == targets || !bc.CanBeHarmful( targets ) )
						continue;
					else
						list.Add( targets );
				}
				eable.Free();

				if (list.Count > 0)
				{
					for( int i = 0; i < list.Count; i++ )
					{
						Mobile target = list[i] as Mobile;
						if( !target.Deleted && target.Alive && target.Combatant != null && bc.Combatant == target.Combatant )
							targetcount++;
					}
				}
				totalDamage *= 100 + targetcount * 10;
				totalDamage /= 100;
			}
			if ( m.Spell != null && m.Spell.IsCasting )
			{
				if ( m.MeleeDamageAbsorb == 1 || m.MeleeDamageAbsorb == 3 )
				{
					totalDamage *= 150;
					totalDamage /= 100;
				}
				else
					totalDamage *= 2;
			}
			totalDamage *= 2;
			
			#region Pet Training
            if (from is BaseCreature || m is BaseCreature)
            {
				SpecialAbility.CheckCombatTrigger(from, m, ref totalDamage, type);

                if (PetTrainingHelper.Enabled)
                {
                    if (from is BaseCreature && m is BaseCreature)
                    {
                        var profile = PetTrainingHelper.GetTrainingProfile((BaseCreature)from);

                        if (profile != null)
                        {
                            profile.CheckProgress((BaseCreature)m);
                        }

                        profile = PetTrainingHelper.GetTrainingProfile((BaseCreature)m);

                        if (profile != null && 0.3 > Utility.RandomDouble())
                        {
                            profile.CheckProgress((BaseCreature)from);
                        }
                    }

                    if (from is BaseCreature && ((BaseCreature)from).Controlled && m.Player)
                    {
                        totalDamage /= 2;
                    }
                }
            }
            #endregion

            if (type <= DamageType.Ranged)
            {
                AttuneWeaponSpell.TryAbsorb(m, ref totalDamage);
            }

            if (keepAlive && totalDamage > m.Hits)
            {
                totalDamage = m.Hits;
            }

            if (from is BaseCreature && type <= DamageType.Ranged)
            {
                ((BaseCreature)from).AlterMeleeDamageTo(m, ref totalDamage);
            }

            if (m is BaseCreature && type <= DamageType.Ranged)
            {
                ((BaseCreature)m).AlterMeleeDamageFrom(from, ref totalDamage);
            }

            if (m is BaseCreature)
            {
                ((BaseCreature)m).OnBeforeDamage(from, ref totalDamage, type);
            }

			
            if (totalDamage <= 0)
            {
                return 0;
            }
			if( totalDamage > 60000 )
				totalDamage = 60000;

            if (from != null && m != null)
            {
                DoLeech(totalDamage, from, m);
            }

			totalDamage = m.Damage(totalDamage, from, true, false);
			/*
            if (Core.SA && type == DamageType.Melee && from is BaseCreature &&
                (m is PlayerMobile || (m is BaseCreature && !((BaseCreature)m).IsMonster)))
            {
                from.RegisterDamage(totalDamage / 4, m);
            }
			*/
            SpiritSpeak.CheckDisrupt(m);

            #region Stygian Abyss
            if (m.Spell != null)
                ((Spell)m.Spell).CheckCasterDisruption(true, phys, fire, cold, pois, nrgy);

            //BattleLust.IncreaseBattleLust(m, totalDamage);

            //if (ManaPhasingOrb.IsInManaPhase(m))
            //    ManaPhasingOrb.RemoveFromTable(m);

            //SoulChargeContext.CheckHit(from, m, totalDamage);

            //Spells.Mysticism.SleepSpell.OnDamage(m);
            //Spells.Mysticism.PurgeMagicSpell.OnMobileDoDamage(from);
            #endregion

            BaseCostume.OnDamaged(m);
			
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;
				if( from is PlayerMobile )
					pm.TimerList[65] = 300;
				//else if( from is BaseCreature )
				//	pm.TimerList[64] = 100;
			}


            return totalDamage;
        }


        public static void Fix(ref int val)
        {
            if (val < 0)
                val = 0;
        }

        public static int Scale2(int input, int percent)
        {
            return (input * percent) / 1000;
        }
		
        public static int Scale(int input, int percent)
        {
            return (input * percent) / 100;
        }

        public static void DoLeech(int damageGiven, Mobile from, Mobile target)
        {
			//신규 흡혈 설정
			int lifeLeech = (int)(AosWeaponAttributes.GetValue(from, AosWeaponAttribute.HitLeechHits));
			int stamLeech = (int)(AosWeaponAttributes.GetValue(from, AosWeaponAttribute.HitLeechStam));
			int manaLeech =(int)(AosWeaponAttributes.GetValue(from, AosWeaponAttribute.HitLeechMana));				
			if (lifeLeech != 0)
			{
				from.Hits += Scale2(damageGiven, lifeLeech);
				Effects.SendPacket(target.Location, target.Map, new Network.ParticleEffect(Network.EffectType.FixedFrom, target.Serial, Serial.Zero, 0x377A, target.Location, target.Location, 1, 15, false, false, 1926, 0, 0, 9502, 1, target.Serial, 16, 0));
				Effects.SendPacket(target.Location, target.Map, new Network.ParticleEffect(Network.EffectType.FixedFrom, target.Serial, Serial.Zero, 0x3728, target.Location, target.Location, 1, 12, false, false, 1963, 0, 0, 9042, 1, target.Serial, 16, 0));
			}
			if (manaLeech != 0)
			{
				from.Mana += Scale2(damageGiven, manaLeech);
			}
			if (stamLeech != 0)
			{
				from.Stam += Scale2(damageGiven, stamLeech);
			}
			if (lifeLeech != 0 || stamLeech != 0 || manaLeech != 0)
			{
				from.PlaySound(0x44D);
			}
			
			/*
            TransformContext context = TransformationSpellHelper.GetContext(from);

            if (context != null)
            {
                if (context.Type == typeof(WraithFormSpell))
                {
                    int manaLeech = AOS.Scale(damageGiven, Math.Min(target.Mana, (int)from.Skills.SpiritSpeak.Value / 5)); // Wraith form gives 5-20% mana leech

                    if (manaLeech != 0)
                    {
                        from.Mana += manaLeech;
                        from.PlaySound(0x44D);

                        target.Mana -= manaLeech;
                    }
                }
                else if (context.Type == typeof(VampiricEmbraceSpell))
                {
                    #region High Seas
                    if (target is BaseCreature && ((BaseCreature)target).TaintedLifeAura)
                    {
                        AOS.Damage(from, target, AOS.Scale(damageGiven, 20), false, 0, 0, 0, 0, 0, 0, 100, false, false, false);
                        from.SendLocalizedMessage(1116778); //The tainted life force energy damages you as your body tries to absorb it.
                    }
                    #endregion
                    else
                    {
                        from.Hits += AOS.Scale(damageGiven, 20);
                        from.PlaySound(0x44D);
                    }
                }
            }
			*/
        }

        #region AOS Status Bar
        public static int GetStatus( Mobile from, int index )
		{
			switch ( index )
			{
				case 0: return from.GetMaxResistance( ResistanceType.Physical );
				case 1: return from.GetMaxResistance( ResistanceType.Fire );
				case 2: return from.GetMaxResistance( ResistanceType.Cold );
				case 3: return from.GetMaxResistance( ResistanceType.Poison );
				case 4: return from.GetMaxResistance( ResistanceType.Energy );
                case 5: return Math.Min(999, AosAttributes.GetValue(from, AosAttribute.DefendChance));
                case 6: return 999;
                case 7: return Math.Min(999, AosAttributes.GetValue(from, AosAttribute.AttackChance));
                case 8: return Math.Min(999, AosAttributes.GetValue(from, AosAttribute.WeaponSpeed));
                case 9: return Math.Min(1000, AosAttributes.GetValue(from, AosAttribute.WeaponDamage));
                case 10: return Math.Min(100, AosAttributes.GetValue(from, AosAttribute.LowerRegCost));
                case 11: return AosAttributes.GetValue(from, AosAttribute.SpellDamage);
                case 12: return Math.Min(999, AosAttributes.GetValue(from, AosAttribute.CastRecovery));
                case 13: return Math.Min(999, AosAttributes.GetValue(from, AosAttribute.CastSpeed));
                case 14: return Math.Min(80, AosAttributes.GetValue(from, AosAttribute.LowerManaCost) ); // + BaseArmor.GetInherentLowerManaCost(from);
                
                case 15: return (int)RegenRates.Mobile_HitsRegenRate(from); // HP   REGEN
                case 16: return (int)RegenRates.Mobile_StamRegenRate(from); // Stam REGEN
                case 17: return (int)RegenRates.Mobile_ManaRegenRate(from); // MANA REGEN
                case 18: return Math.Min(1000, AosAttributes.GetValue(from, AosAttribute.ReflectPhysical)); // reflect phys
                case 19: return Math.Min(1000, AosAttributes.GetValue(from, AosAttribute.EnhancePotions)); // enhance pots

                case 20: return AosAttributes.GetValue(from, AosAttribute.BonusStr) + from.GetStatOffset(StatType.Str); // str inc
                case 21: return AosAttributes.GetValue(from, AosAttribute.BonusDex) + from.GetStatOffset(StatType.Dex); // dex inc
                case 22: return AosAttributes.GetValue(from, AosAttribute.BonusInt) + from.GetStatOffset(StatType.Int); // int inc

                case 23: return 0; // hits neg
                case 24: return 0; // stam neg
                case 25: return 0; // mana neg

                case 26: return AosAttributes.GetValue(from, AosAttribute.BonusHits); // hits inc
                case 27: return AosAttributes.GetValue(from, AosAttribute.BonusStam); // stam inc
                case 28: return AosAttributes.GetValue(from, AosAttribute.BonusMana); // mana inc
				default: return 0;
			}
        }
        #endregion
    }

    [Flags]
    public enum AosAttribute
    {
        RegenHits = 0x00000001, //체력 재생
        RegenStam = 0x00000002, //기력 재생
        RegenMana = 0x00000004, //마나 재생
        DefendChance = 0x00000008, //방어율 증가
        AttackChance = 0x00000010, //명중률 증가
        BonusStr = 0x00000020, //힘 증가
        BonusDex = 0x00000040, //민첩 증가
        BonusInt = 0x00000080, //지능 증가
        BonusHits = 0x00000100, //체력 증가
        BonusStam = 0x00000200, //기력 증가
        BonusMana = 0x00000400, //마나 증가
        WeaponDamage = 0x00000800, //피해 증가%
        WeaponSpeed = 0x00001000, //공격 속도 증가%
        SpellDamage = 0x00002000, //주문 피해 증가%
        CastRecovery = 0x00004000, //주문 치명타 확률 증가
        CastSpeed = 0x00008000, //주문 속도 증가%
        LowerManaCost = 0x00010000, //제작 경험치 증가%
        LowerRegCost = 0x00020000, //채집 경험치 증가%
        ReflectPhysical = 0x00040000, //물리데미지반사
        EnhancePotions = 0x00080000, //치유량 증가%
        Luck = 0x00100000, 			 //운 증가
        SpellChanneling = 0x00200000, //마법 치명타 피해 증가
        NightSight = 0x00400000,		//금화 획득 증가
        IncreasedKarmaLoss = 0x00800000, //카르마감소증가
        Brittle = 0x01000000,			 //물리 치명타 피해 증가
        LowerAmmoCost = 0x02000000,		 //전투 경험치 증가
        BalancedWeapon = 0x04000000,	 //물리 피해 증가%
		WeaponDamageBonus = 0x08000000,	 //무기 데미지증가+
		SpellDamageBonus = 0x10000000,   //마법 데미지증가+
		HealBonus = 0x20000000,          //치유량 증가
		WeaponCritical = 0x40000000, //물리 치명타 확률 증가
    }

    public sealed class AosAttributes : BaseAttributes
    {
        public static bool IsValid(AosAttribute attribute)
        {
            if (!Core.AOS)
            {
                return false;
            }

            if (!Core.ML && attribute == AosAttribute.IncreasedKarmaLoss)
            {
                return false;
            }

            return true;
        }

        public static int[] GetValues(Mobile m, params AosAttribute[] attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static int[] GetValues(Mobile m, IEnumerable<AosAttribute> attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosAttribute> attributes)
        {
            return attributes.Select(a => GetValue(m, a));
        }

		private static bool IdentifiedCheck( Item checkitem )
		{
			if( checkitem is BaseWeapon )
			{
				BaseWeapon item = checkitem as BaseWeapon;
				return item.Identified;
			}
			if( checkitem is BaseArmor )
			{
				BaseArmor item = checkitem as BaseArmor;
				return item.Identified;
			}
			if( checkitem is BaseClothing )
			{
				BaseClothing item = checkitem as BaseClothing;
				return item.Identified;
			}
			if( checkitem is BaseJewel )
			{
				BaseJewel item = checkitem as BaseJewel;
				return item.Identified;
			}
			if( checkitem is Spellbook )
			{
				Spellbook item = checkitem as Spellbook;
				return item.Identified;
			}
			return false;
		}
		
        public static int GetValue(Mobile m, AosAttribute attribute)
        {
            if (World.Loading || !IsValid(attribute))
            {
                return 0;
            }

            int value = 0;

            if (attribute == AosAttribute.Luck || attribute == AosAttribute.RegenMana || attribute == AosAttribute.DefendChance || attribute == AosAttribute.EnhancePotions)
                value += SphynxFortune.GetAosAttributeBonus(m, attribute);

            #region Enhancement
            value += Enhancement.GetValue(m, attribute);
            #endregion
			
            for (int i = 0; i < m.Items.Count; ++i)
            {
                Item obj = m.Items[i];

                AosAttributes attrs = RunicReforging.GetAosAttributes(obj);
				
                if (attrs != null && IdentifiedCheck( obj ) )
                    value += attrs[attribute];

                if (attribute == AosAttribute.Luck)
                {
                    if (obj is BaseWeapon)
                        value += ((BaseWeapon)obj).GetLuckBonus();

                    if (obj is BaseArmor)
                        value += ((BaseArmor)obj).GetLuckBonus();
                }

                if (obj is ISetItem)
                {
                    ISetItem item = (ISetItem)obj;

                    attrs = item.SetAttributes;

                    if (attrs != null && item.LastEquipped)
                        value += attrs[attribute];
                }
            }

            #region Malus/Buff Handler

            #region Skill Mastery
            value += SkillMasterySpell.GetAttributeBonus(m, attribute);
            #endregion

			/*
			//전투 포인트
			if( m is PlayerMobile )
			{
				PlayerMobile pm = m as PlayerMobile;

				if( attribute == AosAttribute.BonusStr )
					value += pm.SilverPoint[1] + pm.ArtifactPoint[1] * 6;
				else if( attribute == AosAttribute.BonusDex )
					value += pm.SilverPoint[2] + pm.ArtifactPoint[2] * 6;
				else if( attribute == AosAttribute.BonusInt )
					value += pm.SilverPoint[3] + pm.ArtifactPoint[3] * 6;
				else if( attribute == AosAttribute.Luck )
					value += pm.SilverPoint[4] + pm.ArtifactPoint[4] * 6;
				else if( attribute == AosAttribute.BonusHits )
					value += pm.SilverPoint[5] + pm.ArtifactPoint[5] * 6;
				else if( attribute == AosAttribute.BonusStam )
					value += pm.SilverPoint[6] + pm.ArtifactPoint[6] * 6;
				else if( attribute == AosAttribute.BonusMana )
					value += pm.SilverPoint[7] + pm.ArtifactPoint[7] * 6;
				else if( attribute == AosAttribute.RegenHits )
					value += pm.SilverPoint[8];
				else if( attribute == AosAttribute.RegenStam )
					value += pm.SilverPoint[9];
				else if( attribute == AosAttribute.RegenMana )
					value += pm.SilverPoint[10];
			}
			*/
            if (attribute == AosAttribute.WeaponDamage)
            {
                if (BaseMagicalFood.IsUnderInfluence(m, MagicalFood.GrapesOfWrath))
                    value += 35;

                // attacker gets 10% bonus when they're under divine fury
                if (DivineFurySpell.UnderEffect(m))
                    value += DivineFurySpell.GetDamageBonus(m);

                // Horrific Beast transformation gives a +25% bonus to damage.
                if (TransformationSpellHelper.UnderTransformation(m, typeof(HorrificBeastSpell)))
                    value += 25;

                int defenseMasteryMalus = 0;
                int discordanceEffect = 0;

                // Defense Mastery gives a -50%/-80% malus to damage.
                if (Server.Items.DefenseMastery.GetMalus(m, ref defenseMasteryMalus))
                    value -= defenseMasteryMalus;

                // Discordance gives a -2%/-48% malus to damage.
                if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
                    value -= discordanceEffect * 2;

                if (Block.IsBlocking(m))
                    value -= 30;

                #region SA
                if (m is PlayerMobile && m.Race == Race.Gargoyle)
                {
                    value += ((PlayerMobile)m).GetRacialBerserkBuff(false);
                }
                #endregion

                #region High Seas
                if (BaseFishPie.IsUnderEffects(m, FishPieEffect.WeaponDam))
                    value += 5;
                #endregion
            }
            else if (attribute == AosAttribute.SpellDamage)
            {
                if (BaseMagicalFood.IsUnderInfluence(m, MagicalFood.GrapesOfWrath))
                    value += 15;

                if (PsychicAttack.Registry.ContainsKey(m))
                    value -= PsychicAttack.Registry[m].SpellDamageMalus;

                TransformContext context = TransformationSpellHelper.GetContext(m);

                if (context != null && context.Spell is ReaperFormSpell)
                    value += ((ReaperFormSpell)context.Spell).SpellDamageBonus;

                value += ArcaneEmpowermentSpell.GetSpellBonus(m, true);

                #region SA
                if (m is PlayerMobile && m.Race == Race.Gargoyle)
                {
                    value += ((PlayerMobile)m).GetRacialBerserkBuff(true);
                }
                #endregion

                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.GuildOfArcaneArts))
                    value += 5;
                #endregion

                #region High Seas
                if (BaseFishPie.IsUnderEffects(m, FishPieEffect.SpellDamage))
                    value += 5;
                #endregion
            }
            else if (attribute == AosAttribute.CastSpeed)
            {
                if (HowlOfCacophony.IsUnderEffects(m) || AuraOfNausea.UnderNausea(m))
                    value -= 5;

                if (EssenceOfWindSpell.IsDebuffed(m))
                    value -= EssenceOfWindSpell.GetFCMalus(m);

                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.BardicCollegium))
                    value += 1;
                #endregion

                #region SA
                if (Spells.Mysticism.SleepSpell.IsUnderSleepEffects(m))
                    value -= 2;

                if (TransformationSpellHelper.UnderTransformation(m, typeof(Spells.Mysticism.StoneFormSpell)))
                    value -= 2;
                #endregion
            }
            else if (attribute == AosAttribute.CastRecovery)
            {
                if (HowlOfCacophony.IsUnderEffects(m))
                    value -= 5;

                value -= ThunderstormSpell.GetCastRecoveryMalus(m);

                #region SA
                if (Spells.Mysticism.SleepSpell.IsUnderSleepEffects(m))
                    value -= 3;
                #endregion
            }
            else if (attribute == AosAttribute.WeaponSpeed)
            {
                if (HowlOfCacophony.IsUnderEffects(m) || AuraOfNausea.UnderNausea(m))
                    value -= 60;

                if (DivineFurySpell.UnderEffect(m))
                    value += DivineFurySpell.GetWeaponSpeedBonus(m);

                value += HonorableExecution.GetSwingBonus(m);

                TransformContext context = TransformationSpellHelper.GetContext(m);

                if (context != null && context.Spell is ReaperFormSpell)
                    value += ((ReaperFormSpell)context.Spell).SwingSpeedBonus;

                int discordanceEffect = 0;

                // Discordance gives a malus of -0/-28% to swing speed.
                if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
                    value -= discordanceEffect;

                if (EssenceOfWindSpell.IsDebuffed(m))
                    value -= EssenceOfWindSpell.GetSSIMalus(m);

                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.GuildOfAssassins))
                    value += 5;
                #endregion

                #region SA
                if (Spells.Mysticism.SleepSpell.IsUnderSleepEffects(m))
                    value -= 45;

                if (TransformationSpellHelper.UnderTransformation(m, typeof(Spells.Mysticism.StoneFormSpell)))
                    value -= 10;

                if (StickySkin.IsUnderEffects(m))
                    value -= 30;
                #endregion
            }
            else if (attribute == AosAttribute.AttackChance)
            {
                if (AuraOfNausea.UnderNausea(m))
                    value -= 60;

                if (DivineFurySpell.UnderEffect(m))
                    value += DivineFurySpell.GetAttackBonus(m);                   

                if (BaseWeapon.CheckAnimal(m, typeof(GreyWolf)) || BaseWeapon.CheckAnimal(m, typeof(BakeKitsune)))
                    value += 20; // attacker gets 20% bonus when under Wolf or Bake Kitsune form

                if (HitLower.IsUnderAttackEffect(m))
                    value -= 25; // Under Hit Lower Attack effect -> 25% malus

                WeaponAbility ability = WeaponAbility.GetCurrentAbility(m);

                if (ability != null)
                    value += ability.AccuracyBonus;

                SpecialMove move = SpecialMove.GetCurrentMove(m);

                if (move != null)
                    value += move.GetAccuracyBonus(m);

                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.WarriorsGuild))
                    value += 5;
                #endregion

                #region SA
                if (Spells.Mysticism.SleepSpell.IsUnderSleepEffects(m))
                    value -= 45;

                if (m.Race == Race.Gargoyle)
                    value += 5;  //Gargoyles get a +5 HCI
                #endregion

                #region High Seas
                if (BaseFishPie.IsUnderEffects(m, FishPieEffect.HitChance))
                    value += 8;
                #endregion
            }
            else if (attribute == AosAttribute.DefendChance)
            {
                if (AuraOfNausea.UnderNausea(m))
                    value -= 60;

                if (DivineFurySpell.UnderEffect(m))
                    value -= DivineFurySpell.GetDefendMalus(m);

                value -= HitLower.GetDefenseMalus(m);

                int discordanceEffect = 0;
                int surpriseMalus = 0;

                value += Block.GetBonus(m);

                if (SurpriseAttack.GetMalus(m, ref surpriseMalus))
                    value -= surpriseMalus;

                // Defender loses -0/-28% if under the effect of Discordance.
                if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
                    value -= discordanceEffect;

                #region High Seas
                if (BaseFishPie.IsUnderEffects(m, FishPieEffect.DefChance))
                    value += 8;
                #endregion
            }
            else if (attribute == AosAttribute.RegenHits)
            {
                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.MaritimeGuild))
                    value += 2;
                #endregion

                #region High Seas
                if (m is PlayerMobile && BaseFishPie.IsUnderEffects(m, FishPieEffect.HitsRegen))
                    value += 3;

                if (SurgeShield.IsUnderEffects(m, SurgeType.Hits))
                    value += 10;

                if (SearingWeaponContext.HasContext(m))
                    value -= m is PlayerMobile ? 20 : 60;
                #endregion

                //Virtue Artifacts
                value += AnkhPendant.GetHitsRegenModifier(m);
            }
            else if (attribute == AosAttribute.RegenStam)
            {
                #region High Seas
                if (m is PlayerMobile && BaseFishPie.IsUnderEffects(m, FishPieEffect.StamRegen))
                    value += 3;

                if (SurgeShield.IsUnderEffects(m, SurgeType.Stam))
                    value += 10;
                #endregion

                //Virtue Artifacts
                value += AnkhPendant.GetStamRegenModifier(m);
            }
            else if (attribute == AosAttribute.RegenMana)
            {
                #region City Loyalty
                if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.MerchantsAssociation))
                    value += 1;
                #endregion

                #region High Seas
                if (m is PlayerMobile && BaseFishPie.IsUnderEffects(m, FishPieEffect.ManaRegen))
                    value += 3;

                if (SurgeShield.IsUnderEffects(m, SurgeType.Mana))
                    value += 10;
                #endregion

                //Virtue Artifacts
                value += AnkhPendant.GetManaRegenModifier(m);
            }
            #endregion

			value /= 100;
			
            return value;
        }

        public override void SetValue(int bitmask, int value)
        {
            if (Core.SA && bitmask == (int)AosAttribute.WeaponSpeed && Owner is BaseWeapon)
            {
                ((BaseWeapon)Owner).WeaponAttributes.ScaleLeech(value);
            }

            base.SetValue(bitmask, value);
        }

        public AosAttributes(Item owner)
            : base(owner)
        {
        }

        public AosAttributes(Item owner, AosAttributes other)
            : base(owner, other)
        {
        }

        public AosAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }


        public int this[AosAttribute attribute]
        {
            get
            {
                return ExtendedGetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public int ExtendedGetValue(int bitmask)
        {
            int value = GetValue(bitmask);

            XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

            if (xaos != null)
            {
                value += xaos.GetValue(bitmask);
            }

            return (value);
        }

        public override string ToString()
        {
            return "...";
        }

        public void AddStatBonuses(Mobile to)
        {
            int strBonus = BonusStr;
            int dexBonus = BonusDex;
            int intBonus = BonusInt;

            if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
            {
                string modName = Owner.Serial.ToString();

                if (strBonus != 0)
                    to.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    to.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    to.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            to.CheckStatTimers();
        }

        public void RemoveStatBonuses(Mobile from)
        {
            string modName = Owner.Serial.ToString();

            from.RemoveStatMod(modName + "Str");
            from.RemoveStatMod(modName + "Dex");
            from.RemoveStatMod(modName + "Int");

            from.CheckStatTimers();
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenHits
        {
            get
            {
                return this[AosAttribute.RegenHits];
            }
            set
            {
                this[AosAttribute.RegenHits] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenStam
        {
            get
            {
                return this[AosAttribute.RegenStam];
            }
            set
            {
                this[AosAttribute.RegenStam] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenMana
        {
            get
            {
                return this[AosAttribute.RegenMana];
            }
            set
            {
                this[AosAttribute.RegenMana] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DefendChance
        {
            get
            {
                return this[AosAttribute.DefendChance];
            }
            set
            {
                this[AosAttribute.DefendChance] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AttackChance
        {
            get
            {
                return this[AosAttribute.AttackChance];
            }
            set
            {
                this[AosAttribute.AttackChance] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusStr
        {
            get
            {
                return this[AosAttribute.BonusStr];
            }
            set
            {
                this[AosAttribute.BonusStr] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusDex
        {
            get
            {
                return this[AosAttribute.BonusDex];
            }
            set
            {
                this[AosAttribute.BonusDex] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusInt
        {
            get
            {
                return this[AosAttribute.BonusInt];
            }
            set
            {
                this[AosAttribute.BonusInt] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusHits
        {
            get
            {
                return this[AosAttribute.BonusHits];
            }
            set
            {
                this[AosAttribute.BonusHits] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusStam
        {
            get
            {
                return this[AosAttribute.BonusStam];
            }
            set
            {
                this[AosAttribute.BonusStam] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BonusMana
        {
            get
            {
                return this[AosAttribute.BonusMana];
            }
            set
            {
                this[AosAttribute.BonusMana] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponDamage
        {
            get
            {
                return this[AosAttribute.WeaponDamage];
            }
            set
            {
                this[AosAttribute.WeaponDamage] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponSpeed
        {
            get
            {
                return this[AosAttribute.WeaponSpeed];
            }
            set
            {
                this[AosAttribute.WeaponSpeed] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellDamage
        {
            get
            {
                return this[AosAttribute.SpellDamage];
            }
            set
            {
                this[AosAttribute.SpellDamage] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CastRecovery
        {
            get
            {
                return this[AosAttribute.CastRecovery];
            }
            set
            {
                this[AosAttribute.CastRecovery] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CastSpeed
        {
            get
            {
                return this[AosAttribute.CastSpeed];
            }
            set
            {
                this[AosAttribute.CastSpeed] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerManaCost
        {
            get
            {
                return this[AosAttribute.LowerManaCost];
            }
            set
            {
                this[AosAttribute.LowerManaCost] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerRegCost
        {
            get
            {
                return this[AosAttribute.LowerRegCost];
            }
            set
            {
                this[AosAttribute.LowerRegCost] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReflectPhysical
        {
            get
            {
                return this[AosAttribute.ReflectPhysical];
            }
            set
            {
                this[AosAttribute.ReflectPhysical] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EnhancePotions
        {
            get
            {
                return this[AosAttribute.EnhancePotions];
            }
            set
            {
                this[AosAttribute.EnhancePotions] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Luck
        {
            get
            {
                return this[AosAttribute.Luck];
            }
            set
            {
                this[AosAttribute.Luck] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellChanneling
        {
            get
            {
                return this[AosAttribute.SpellChanneling];
            }
            set
            {
                this[AosAttribute.SpellChanneling] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int NightSight
        {
            get
            {
                return this[AosAttribute.NightSight];
            }
            set
            {
                this[AosAttribute.NightSight] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int IncreasedKarmaLoss
        {
            get
            {
                return this[AosAttribute.IncreasedKarmaLoss];
            }
            set
            {
                this[AosAttribute.IncreasedKarmaLoss] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Brittle
        {
            get
            {
                return this[AosAttribute.Brittle];
            }
            set
            {
                this[AosAttribute.Brittle] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerAmmoCost
        {
            get
            {
                return this[AosAttribute.LowerAmmoCost];
            }
            set
            {
                this[AosAttribute.LowerAmmoCost] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BalancedWeapon
        {
            get
            {
                return this[AosAttribute.BalancedWeapon];
            }
            set
            {
                this[AosAttribute.BalancedWeapon] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponDamageBonus
        {
            get
            {
                return this[AosAttribute.WeaponDamageBonus];
            }
            set
            {
                this[AosAttribute.WeaponDamageBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellDamageBonus
        {
            get
            {
                return this[AosAttribute.SpellDamageBonus];
            }
            set
            {
                this[AosAttribute.SpellDamageBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int HealBonus
        {
            get
            {
                return this[AosAttribute.HealBonus];
            }
            set
            {
                this[AosAttribute.HealBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int WeaponCritical
        {
            get
            {
                return this[AosAttribute.WeaponCritical];
            }
            set
            {
                this[AosAttribute.WeaponCritical] = value;
            }
        }
	}
    [Flags]
    public enum AosWeaponAttribute : long
    {
        LowerStatReq = 0x00000001,  		//장비 요구치 감소
        SelfRepair = 0x00000002,			//자가 수리
        HitLeechHits = 0x00000004,			//체력 흡수
        HitLeechStam = 0x00000008,			//기력 흡수
        HitLeechMana = 0x00000010,			//마나 흡수
        HitLowerAttack = 0x00000020,		//공격력 감소
        HitLowerDefend = 0x00000040,		//방어력 감소
        HitMagicArrow = 0x00000080,			//매직 화살 발동
        HitHarm = 0x00000100,				//함 발동
        HitFireball = 0x00000200, 			//파이어볼 발동
        HitLightning = 0x00000400,			//라이트닝 발동
        HitDispel = 0x00000800,				//위더 발동
        HitColdArea = 0x00001000,			//광역 냉기 데미지 증가%	
        HitFireArea = 0x00002000,			//광역 화염 데미지 증가%
        HitPoisonArea = 0x00004000,			//광역 독 데미지 증가%
        HitEnergyArea = 0x00008000,			//광역 에너지 데미지 증가%
        HitPhysicalArea = 0x00010000,		//광역 물리 데미지 증가%
        ResistPhysicalBonus = 0x00020000,
        ResistFireBonus = 0x00040000,
        ResistColdBonus = 0x00080000,
        ResistPoisonBonus = 0x00100000,
        ResistEnergyBonus = 0x00200000,
        UseBestSkill = 0x00400000,			//%확률로 2배 데미지
        MageWeapon = 0x00800000,			//네임드 몬스터 피해량 20% 증가
        DurabilityBonus = 0x01000000,
        BloodDrinker = 0x02000000,			//충격 데미지 -> 출혈 데미지
        BattleLust = 0x04000000,			//무기 데미지 -> 출혈 데미지
        HitCurse = 0x08000000,				//독 레벨 +1 확률%
        HitFatigue = 0x10000000,			//충격 데미지 -> 관통 데미지
        HitManaDrain = 0x20000000,			//무기 데미지 -> 출혈 데미지
        SplinteringWeapon = 0x40000000,		//무기 데미지 -> 충격 데미지
        ReactiveParalyze =  0x80000000,		//
    }

    public sealed class AosWeaponAttributes : BaseAttributes
    {
        public static bool IsValid(AosWeaponAttribute attribute)
        {
            if (!Core.AOS)
            {
                return false;
            }

            if (!Core.SA && attribute >= AosWeaponAttribute.BloodDrinker)
            {
                return false;
            }

            return true;
        }

        public static int[] GetValues(Mobile m, params AosWeaponAttribute[] attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static int[] GetValues(Mobile m, IEnumerable<AosWeaponAttribute> attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosWeaponAttribute> attributes)
        {
            return attributes.Select(a => GetValue(m, a));
        }

        public static int GetValue(Mobile m, AosWeaponAttribute attribute)
        {
            if (World.Loading || !IsValid(attribute))
            {
                return 0;
            }

            int value = 0;

            #region Enhancement
            value += Enhancement.GetValue(m, attribute);
            #endregion

            for (int i = 0; i < m.Items.Count; ++i)
            {
                AosWeaponAttributes attrs = RunicReforging.GetAosWeaponAttributes(m.Items[i]);

                if (attrs != null)
                    value += attrs[attribute];
            }

			value /= 100;
            return value;
        }

        public override void SetValue(int bitmask, int value)
        {
            if (bitmask == (int)AosWeaponAttribute.DurabilityBonus && Owner is BaseWeapon)
            {
                ((BaseWeapon)Owner).UnscaleDurability();
            }

            base.SetValue(bitmask, value);

            if (bitmask == (int)AosWeaponAttribute.DurabilityBonus && Owner is BaseWeapon)
            {
                ((BaseWeapon)Owner).ScaleDurability();
            }
        }

        public AosWeaponAttributes(Item owner)
            : base(owner)
        {
        }

        public AosWeaponAttributes(Item owner, AosWeaponAttributes other)
            : base(owner, other)
        {
        }

        public AosWeaponAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public int this[AosWeaponAttribute attribute]
        {
            get
            {
                return ExtendedGetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public int ExtendedGetValue(int bitmask)
        {
            int value = GetValue(bitmask);

            XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

            if (xaos != null)
            {
                value += xaos.GetValue(bitmask);
            }

            return (value);
        }

        public void ScaleLeech(int weaponSpeed)
        {
            BaseWeapon wep = Owner as BaseWeapon;

            if (wep == null || wep.IsArtifact)
                return;

            if (HitLeechHits > 0)
            {
                double postcap = (double)HitLeechHits / (double)ItemPropertyInfo.GetMaxIntensity(wep, AosWeaponAttribute.HitLeechHits);
                if (postcap < 1.0) postcap = 1.0;

                int newhits = (int)((wep.MlSpeed * 2500 / (100 + weaponSpeed)) * postcap);

                if (wep is BaseRanged)
                    newhits /= 2;

                if(HitLeechHits > newhits)
                    HitLeechHits = newhits;
            }

            if (HitLeechMana > 0)
            {
                double postcap = (double)HitLeechMana / (double)ItemPropertyInfo.GetMaxIntensity(wep, AosWeaponAttribute.HitLeechMana);
                if (postcap < 1.0) postcap = 1.0;

                int newmana = (int)((wep.MlSpeed * 2500 / (100 + weaponSpeed)) * postcap);

                if (wep is BaseRanged)
                    newmana /= 2;

                if(HitLeechMana > newmana)
                    HitLeechMana = newmana;
            }
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerStatReq
        {
            get
            {
                return this[AosWeaponAttribute.LowerStatReq];
            }
            set
            {
                this[AosWeaponAttribute.LowerStatReq] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SelfRepair
        {
            get
            {
                return this[AosWeaponAttribute.SelfRepair];
            }
            set
            {
                this[AosWeaponAttribute.SelfRepair] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLeechHits
        {
            get
            {
                return this[AosWeaponAttribute.HitLeechHits];
            }
            set
            {
                this[AosWeaponAttribute.HitLeechHits] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLeechStam
        {
            get
            {
                return this[AosWeaponAttribute.HitLeechStam];
            }
            set
            {
                this[AosWeaponAttribute.HitLeechStam] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLeechMana
        {
            get
            {
                return this[AosWeaponAttribute.HitLeechMana];
            }
            set
            {
                this[AosWeaponAttribute.HitLeechMana] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLowerAttack
        {
            get
            {
                return this[AosWeaponAttribute.HitLowerAttack];
            }
            set
            {
                this[AosWeaponAttribute.HitLowerAttack] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLowerDefend
        {
            get
            {
                return this[AosWeaponAttribute.HitLowerDefend];
            }
            set
            {
                this[AosWeaponAttribute.HitLowerDefend] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitMagicArrow
        {
            get
            {
                return this[AosWeaponAttribute.HitMagicArrow];
            }
            set
            {
                this[AosWeaponAttribute.HitMagicArrow] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitHarm
        {
            get
            {
                return this[AosWeaponAttribute.HitHarm];
            }
            set
            {
                this[AosWeaponAttribute.HitHarm] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitFireball
        {
            get
            {
                return this[AosWeaponAttribute.HitFireball];
            }
            set
            {
                this[AosWeaponAttribute.HitFireball] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitLightning
        {
            get
            {
                return this[AosWeaponAttribute.HitLightning];
            }
            set
            {
                this[AosWeaponAttribute.HitLightning] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitDispel
        {
            get
            {
                return this[AosWeaponAttribute.HitDispel];
            }
            set
            {
                this[AosWeaponAttribute.HitDispel] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitColdArea
        {
            get
            {
                return this[AosWeaponAttribute.HitColdArea];
            }
            set
            {
                this[AosWeaponAttribute.HitColdArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitFireArea
        {
            get
            {
                return this[AosWeaponAttribute.HitFireArea];
            }
            set
            {
                this[AosWeaponAttribute.HitFireArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoisonArea
        {
            get
            {
                return this[AosWeaponAttribute.HitPoisonArea];
            }
            set
            {
                this[AosWeaponAttribute.HitPoisonArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitEnergyArea
        {
            get
            {
                return this[AosWeaponAttribute.HitEnergyArea];
            }
            set
            {
                this[AosWeaponAttribute.HitEnergyArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPhysicalArea
        {
            get
            {
                return this[AosWeaponAttribute.HitPhysicalArea];
            }
            set
            {
                this[AosWeaponAttribute.HitPhysicalArea] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistPhysicalBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistPhysicalBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistPhysicalBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistFireBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistFireBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistFireBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistColdBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistColdBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistColdBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistPoisonBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistPoisonBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistPoisonBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResistEnergyBonus
        {
            get
            {
                return this[AosWeaponAttribute.ResistEnergyBonus];
            }
            set
            {
                this[AosWeaponAttribute.ResistEnergyBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UseBestSkill
        {
            get
            {
                return this[AosWeaponAttribute.UseBestSkill];
            }
            set
            {
                this[AosWeaponAttribute.UseBestSkill] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MageWeapon
        {
            get
            {
                return this[AosWeaponAttribute.MageWeapon];
            }
            set
            {
                this[AosWeaponAttribute.MageWeapon] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DurabilityBonus
        {
            get
            {
                return this[AosWeaponAttribute.DurabilityBonus];
            }
            set
            {
                this[AosWeaponAttribute.DurabilityBonus] = value;
            }
        }

        #region SA
        [CommandProperty(AccessLevel.GameMaster)]
        public int BloodDrinker
        {
            get
            {
                return this[AosWeaponAttribute.BloodDrinker];
            }
            set
            {
                this[AosWeaponAttribute.BloodDrinker] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BattleLust
        {
            get
            {
                return this[AosWeaponAttribute.BattleLust];
            }
            set
            {
                this[AosWeaponAttribute.BattleLust] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitCurse
        {
            get
            {
                return this[AosWeaponAttribute.HitCurse];
            }
            set
            {
                this[AosWeaponAttribute.HitCurse] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitFatigue
        {
            get
            {
                return this[AosWeaponAttribute.HitFatigue];
            }
            set
            {
                this[AosWeaponAttribute.HitFatigue] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitManaDrain
        {
            get
            {
                return this[AosWeaponAttribute.HitManaDrain];
            }
            set
            {
                this[AosWeaponAttribute.HitManaDrain] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SplinteringWeapon
        {
            get
            {
                return this[AosWeaponAttribute.SplinteringWeapon];
            }
            set
            {
                this[AosWeaponAttribute.SplinteringWeapon] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReactiveParalyze
        {
            get
            {
                return this[AosWeaponAttribute.ReactiveParalyze];
            }
            set
            {
                this[AosWeaponAttribute.ReactiveParalyze] = value;
            }
        }
        #endregion
    }

	//특수 데미지
    [Flags]
    public enum ExtendedWeaponAttribute
    {
        BoneBreaker     = 0x00000001, //
        HitSwarm        = 0x00000002, //
        HitSparks       = 0x00000004, //감전 데미지 증가
        Bane            = 0x00000008, //부식 데미지 증가
        MysticWeapon    = 0x00000010, //
        AssassinHoned   = 0x00000020, //어그로 감소
        Focus           = 0x00000040, //
        HitExplosion    = 0x00000080, //연소 데미지 증가
        Freezing		= 0x00000100, //동상 데미지 증가
		InfectionBonus	= 0x00000200,  //인팩팅 데미지 증가
		LightningBonus	= 0x00000400  //라이트닝(4써클, 7써클) 데미지 증가
    }

    public sealed class ExtendedWeaponAttributes : BaseAttributes
    {
        public ExtendedWeaponAttributes(Item owner)
            : base(owner)
        {
        }

        public ExtendedWeaponAttributes(Item owner, ExtendedWeaponAttributes other)
            : base(owner, other)
        {
        }

        public ExtendedWeaponAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public static int GetValue(Mobile m, ExtendedWeaponAttribute attribute)
        {
            if (!Core.AOS)
                return 0;

            int value = 0;

            #region Enhancement
            value += Enhancement.GetValue(m, attribute);
            #endregion

            for (int i = 0; i < m.Items.Count; ++i)
            {
                Item obj = m.Items[i];

                if (obj is BaseWeapon)
                {
                    ExtendedWeaponAttributes attrs = ((BaseWeapon)obj).ExtendedWeaponAttributes;

                    if (attrs != null)
                        value += attrs[attribute];
                }
            }

			value /= 100;
            return value;
        }

        public int this[ExtendedWeaponAttribute attribute]
        {
            get
            {
                return GetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BoneBreaker
        {
            get
            {
                return this[ExtendedWeaponAttribute.BoneBreaker];
            }
            set
            {
                this[ExtendedWeaponAttribute.BoneBreaker] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitSwarm
        {
            get
            {
                return this[ExtendedWeaponAttribute.HitSwarm];
            }
            set
            {
                this[ExtendedWeaponAttribute.HitSwarm] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitSparks
        {
            get
            {
                return this[ExtendedWeaponAttribute.HitSparks];
            }
            set
            {
                this[ExtendedWeaponAttribute.HitSparks] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Bane
        {
            get
            {
                return this[ExtendedWeaponAttribute.Bane];
            }
            set
            {
                this[ExtendedWeaponAttribute.Bane] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MysticWeapon
        {
            get
            {
                return this[ExtendedWeaponAttribute.MysticWeapon];
            }
            set
            {
                this[ExtendedWeaponAttribute.MysticWeapon] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AssassinHoned
        {
            get
            {
                return this[ExtendedWeaponAttribute.AssassinHoned];
            }
            set
            {
                this[ExtendedWeaponAttribute.AssassinHoned] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Focus
        {
            get
            {
                return this[ExtendedWeaponAttribute.Focus];
            }
            set
            {
                this[ExtendedWeaponAttribute.Focus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitExplosion
        {
            get
            {
                return this[ExtendedWeaponAttribute.HitExplosion];
            }
            set
            {
                this[ExtendedWeaponAttribute.HitExplosion] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int Freezing
        {
            get
            {
                return this[ExtendedWeaponAttribute.Freezing];
            }
            set
            {
                this[ExtendedWeaponAttribute.Freezing] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int LightningBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.LightningBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.LightningBonus] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int InfectionBonus
        {
            get
            {
                return this[ExtendedWeaponAttribute.InfectionBonus];
            }
            set
            {
                this[ExtendedWeaponAttribute.InfectionBonus] = value;
            }
        }
	}

    [Flags]
    public enum AosArmorAttribute
    {
        LowerStatReq = 0x00000001,
        SelfRepair = 0x00000002,
        MageArmor = 0x00000004,
        DurabilityBonus = 0x00000008,
        #region Stygian Abyss
        ReactiveParalyze = 0x00000010,
        SoulCharge = 0x00000020,	//회복량+
		PierceResist = 0x00000040, //관통 저항력
		ShockResist = 0x00000080, //충격 저항력
		BleedResist = 0x00000100, //출혈 저항력
		WeaponDefense = 0x00000200, //방어력
		MagicDefense = 0x00000400 //마법 방어력
        #endregion
    }

    public sealed class AosArmorAttributes : BaseAttributes
    {
        public static bool IsValid(AosArmorAttribute attribute)
        {
            if (!Core.AOS)
            {
                return false;
            }

            if (!Core.SA && attribute >= AosArmorAttribute.ReactiveParalyze)
            {
                return false;
            }

            return true;
        }

        public static int[] GetValues(Mobile m, params AosArmorAttribute[] attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static int[] GetValues(Mobile m, IEnumerable<AosArmorAttribute> attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosArmorAttribute> attributes)
        {
            return attributes.Select(a => GetValue(m, a));
        }

        public static int GetValue(Mobile m, AosArmorAttribute attribute)
        {
            if (World.Loading || !IsValid(attribute))
            {
                return 0;
            }

            int value = 0;

            for (int i = 0; i < m.Items.Count; ++i)
            {
                AosArmorAttributes attrs = RunicReforging.GetAosArmorAttributes(m.Items[i]);

                if (attrs != null)
                    value += attrs[attribute];
            }

			value /= 100;
            return value;
        }

        public override void SetValue(int bitmask, int value)
        {
            if (bitmask == (int)AosArmorAttribute.DurabilityBonus)
            {
                if (Owner is BaseArmor)
                {
                    ((BaseArmor)Owner).UnscaleDurability();
                }
                else if (Owner is BaseClothing)
                {
                    ((BaseClothing)Owner).UnscaleDurability();
                }
            }

            base.SetValue(bitmask, value);

            if (bitmask == (int)AosArmorAttribute.DurabilityBonus)
            {
                if (Owner is BaseArmor)
                {
                    ((BaseArmor)Owner).ScaleDurability();
                }
                else if (Owner is BaseClothing)
                {
                    ((BaseClothing)Owner).ScaleDurability();
                }
            }
        }

        public AosArmorAttributes(Item owner)
            : base(owner)
        {
        }

        public AosArmorAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public AosArmorAttributes(Item owner, AosArmorAttributes other)
            : base(owner, other)
        {
        }

        public int this[AosArmorAttribute attribute]
        {
            get
            {
                return ExtendedGetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public int ExtendedGetValue(int bitmask)
        {
            int value = GetValue(bitmask);

            XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

            if (xaos != null)
            {
                value += xaos.GetValue(bitmask);
            }

            return (value);
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LowerStatReq
        {
            get
            {
                return this[AosArmorAttribute.LowerStatReq];
            }
            set
            {
                this[AosArmorAttribute.LowerStatReq] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SelfRepair
        {
            get
            {
                return this[AosArmorAttribute.SelfRepair];
            }
            set
            {
                this[AosArmorAttribute.SelfRepair] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MageArmor
        {
            get
            {
                return this[AosArmorAttribute.MageArmor];
            }
            set
            {
                this[AosArmorAttribute.MageArmor] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DurabilityBonus
        {
            get
            {
                return this[AosArmorAttribute.DurabilityBonus];
            }
            set
            {
                this[AosArmorAttribute.DurabilityBonus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ReactiveParalyze
        {
            get
            {
                return this[AosArmorAttribute.ReactiveParalyze];
            }
            set
            {
                this[AosArmorAttribute.ReactiveParalyze] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulCharge
        {
            get
            {
                return this[AosArmorAttribute.SoulCharge];
            }
            set
            {
                this[AosArmorAttribute.SoulCharge] = value;
            }
        }
    

        [CommandProperty(AccessLevel.GameMaster)]
        public int PierceResist
        {
            get
            {
                return this[AosArmorAttribute.PierceResist];
            }
            set
            {
                this[AosArmorAttribute.PierceResist] = value;
            }
        }
    
        [CommandProperty(AccessLevel.GameMaster)]
        public int ShockResist
        {
            get
            {
                return this[AosArmorAttribute.ShockResist];
            }
            set
            {
                this[AosArmorAttribute.ShockResist] = value;
            }
        }
    
        [CommandProperty(AccessLevel.GameMaster)]
        public int BleedResist
        {
            get
            {
                return this[AosArmorAttribute.BleedResist];
            }
            set
            {
                this[AosArmorAttribute.BleedResist] = value;
            }
        }
    }

    public sealed class AosSkillBonuses : BaseAttributes
    {
        private List<SkillMod> m_Mods;

        public AosSkillBonuses(Item owner)
            : base(owner)
        {
        }

        public AosSkillBonuses(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public AosSkillBonuses(Item owner, AosSkillBonuses other)
            : base(owner, other)
        {
        }

		public int GetSkillName(SkillName skill)
		{
			return GetLabel(skill);
		}
		
        public void GetProperties(ObjectPropertyList list)
        {
            for (int i = 0; i < 10; ++i)
            {
                SkillName skill;
                double bonus;

                if (!GetValues(i, out skill, out bonus))
                    continue;

				if( i <= 4 )
					list.Add(1060451 + i, "#{0}\t{1}", GetLabel(skill), bonus);
				else
					list.Add(1063510 + i, "#{0}\t{1}", GetLabel(skill), bonus);
            }
        }
		
        public static int GetLabel(SkillName skill)
        {
            switch (skill)
            {
                case SkillName.EvalInt:
                    return 1002070; // Evaluate Intelligence
                case SkillName.Forensics:
                    return 1002078; // Forensic Evaluation
                case SkillName.Lockpicking:
                    return 1002097; // Lockpicking
                default:
                    return 1044060 + (int)skill;
            }
        }

        public void AddTo(Mobile m)
        {
            if (Discordance.UnderPVPEffects(m))
            {
                return;
            }

            Remove();

            for (int i = 0; i < 10; ++i)
            {
                SkillName skill;
                double bonus;

                if (!GetValues(i, out skill, out bonus))
                    continue;

                if (m_Mods == null)
                    m_Mods = new List<SkillMod>();

                SkillMod sk = new DefaultSkillMod(skill, true, bonus);
                sk.ObeyCap = true;
                m.AddSkillMod(sk);
                m_Mods.Add(sk);
            }
        }

        public void Remove()
        {
            if (m_Mods == null)
                return;

            for (int i = 0; i < m_Mods.Count; ++i)
            {
                Mobile m = m_Mods[i].Owner;
                m_Mods[i].Remove();

                if (Core.ML)
                    CheckCancelMorph(m);
            }
            m_Mods = null;
        }

        public override void SetValue(int bitmask, int value)
        {
            base.SetValue(bitmask, value);

            if (Owner != null && Owner.Parent is Mobile)
            {
                Remove();
                AddTo((Mobile)Owner.Parent);
            }
        }

        public bool GetValues(int index, out SkillName skill, out double bonus)
        {
            int v = GetValue(1 << index);
            int vSkill = 0;
            int vBonus = 0;

            for (int i = 0; i < 16; ++i)
            {
                vSkill <<= 1;
                vSkill |= (v & 1);
                v >>= 1;

                vBonus <<= 1;
                vBonus |= (v & 1);
                v >>= 1;
            }

            skill = (SkillName)vSkill;
            bonus = (double)vBonus / 10;

            return (bonus != 0);
        }

        public void SetValues(int index, SkillName skill, double bonus)
        {
            int v = 0;
            int vSkill = (int)skill;
            int vBonus = (int)(bonus * 10);

            for (int i = 0; i < 16; ++i)
            {
                v <<= 1;
                v |= (vBonus & 1);
                vBonus >>= 1;

                v <<= 1;
                v |= (vSkill & 1);
                vSkill >>= 1;
            }

            SetValue(1 << index, v);
        }

        public SkillName GetSkill(int index)
        {
            SkillName skill;
            double bonus;

            GetValues(index, out skill, out bonus);

            return skill;
        }

        public void SetSkill(int index, SkillName skill)
        {
            SetValues(index, skill, GetBonus(index));
        }

        public double GetBonus(int index)
        {
            SkillName skill;
            double bonus;

            GetValues(index, out skill, out bonus);

            return bonus;
        }

        public void SetBonus(int index, double bonus)
        {
            SetValues(index, GetSkill(index), bonus);
        }

        public override string ToString()
        {
            return "...";
        }

        public void CheckCancelMorph(Mobile m)
        {
            if (m == null)
                return;

            double minSkill, maxSkill;

            AnimalFormContext acontext = AnimalForm.GetContext(m);
            TransformContext context = TransformationSpellHelper.GetContext(m);

            if (context != null)
            {
                Spell spell = context.Spell as Spell;
                spell.GetCastSkills(out minSkill, out maxSkill);

                if (m.Skills[spell.CastSkill].Value < minSkill)
                {
                    TransformationSpellHelper.RemoveContext(m, context, true);
                }
            }

            if (acontext != null)
            {
                if (acontext.Type == typeof(WildWhiteTiger) && m.Skills[SkillName.Ninjitsu].Value < 90)
                {
                    AnimalForm.RemoveContext(m, true);
                }
                else
                {
                    int i;

                    for (i = 0; i < AnimalForm.Entries.Length; ++i)
                    {
                        if (AnimalForm.Entries[i].Type == acontext.Type)
                            break;
                    }

                    if (i < AnimalForm.Entries.Length && m.Skills[SkillName.Ninjitsu].Value < AnimalForm.Entries[i].ReqSkill)
                    {
                        AnimalForm.RemoveContext(m, true);
                    }
                }
            }
            if (!m.CanBeginAction(typeof(PolymorphSpell)) && m.Skills[SkillName.Magery].Value < 66.1)
            {
                m.BodyMod = 0;
                m.HueMod = -1;
                m.NameMod = null;
                m.EndAction(typeof(PolymorphSpell));
                BaseArmor.ValidateMobile(m);
                BaseClothing.ValidateMobile(m);
            }
            if (!m.CanBeginAction(typeof(IncognitoSpell)) && m.Skills[SkillName.Magery].Value < 38.1)
            {
                if (m is PlayerMobile)
                    ((PlayerMobile)m).SetHairMods(-1, -1);
                m.BodyMod = 0;
                m.HueMod = -1;
                m.NameMod = null;
                m.EndAction(typeof(IncognitoSpell));
                BaseArmor.ValidateMobile(m);
                BaseClothing.ValidateMobile(m);
                BuffInfo.RemoveBuff(m, BuffIcon.Incognito);
            }
            return;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_1_Value
        {
            get
            {
                return GetBonus(0);
            }
            set
            {
                SetBonus(0, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_1_Name
        {
            get
            {
                return GetSkill(0);
            }
            set
            {
                SetSkill(0, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_2_Value
        {
            get
            {
                return GetBonus(1);
            }
            set
            {
                SetBonus(1, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_2_Name
        {
            get
            {
                return GetSkill(1);
            }
            set
            {
                SetSkill(1, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_3_Value
        {
            get
            {
                return GetBonus(2);
            }
            set
            {
                SetBonus(2, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_3_Name
        {
            get
            {
                return GetSkill(2);
            }
            set
            {
                SetSkill(2, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_4_Value
        {
            get
            {
                return GetBonus(3);
            }
            set
            {
                SetBonus(3, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_4_Name
        {
            get
            {
                return GetSkill(3);
            }
            set
            {
                SetSkill(3, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_5_Value
        {
            get
            {
                return GetBonus(4);
            }
            set
            {
                SetBonus(4, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_5_Name
        {
            get
            {
                return GetSkill(4);
            }
            set
            {
                SetSkill(4, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_6_Value
        {
            get
            {
                return GetBonus(5);
            }
            set
            {
                SetBonus(5, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_6_Name
        {
            get
            {
                return GetSkill(5);
            }
            set
            {
                SetSkill(5, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_7_Value
        {
            get
            {
                return GetBonus(6);
            }
            set
            {
                SetBonus(6, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_7_Name
        {
            get
            {
                return GetSkill(6);
            }
            set
            {
                SetSkill(6, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_8_Value
        {
            get
            {
                return GetBonus(7);
            }
            set
            {
                SetBonus(7, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_8_Name
        {
            get
            {
                return GetSkill(7);
            }
            set
            {
                SetSkill(7, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_9_Value
        {
            get
            {
                return GetBonus(8);
            }
            set
            {
                SetBonus(8, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_9_Name
        {
            get
            {
                return GetSkill(8);
            }
            set
            {
                SetSkill(8, value);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double Skill_10_Value
        {
            get
            {
                return GetBonus(9);
            }
            set
            {
                SetBonus(9, value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill_10_Name
        {
            get
            {
                return GetSkill(9);
            }
            set
            {
                SetSkill(9, value);
            }
        }
    }

    #region Stygian Abyss
    [Flags]
    public enum SAAbsorptionAttribute
    {
        EaterFire = 0x00000001, //불 피해 증가
        EaterCold = 0x00000002, //냉기 피해 증가
        EaterPoison = 0x00000004, //독 피해 증가
        EaterEnergy = 0x00000008, //에너지 피해 증가
        EaterKinetic = 0x00000010, //충격 피해 증가
        EaterDamage = 0x00000020, //물리 피해 증가
        ResonanceFire = 0x00000040, //불 피해 증가%
        ResonanceCold = 0x00000080, //냉기 피해 증가%
        ResonancePoison = 0x00000100, //독 피해 증가%
        ResonanceEnergy = 0x00000200, //에너지 피해 증가%
        ResonanceKinetic = 0x00000400, //충격 피해 증가%
        /*Soul Charge is wrong. 
         * Do not use these types. 
         * Use AosArmorAttribute type only.
         * Fill these in with any new attributes.*/
        SoulChargeFire = 0x00000800, //연소 저항력%
        SoulChargeCold = 0x00001000, //동상 저항력%
        SoulChargePoison = 0x00002000, //부식 저항력%
        SoulChargeEnergy = 0x00004000, //감전 저항력%
        SoulChargeKinetic = 0x00008000, //회복량 증가%
        CastingFocus = 0x00010000, //피해의 %프로만큼 마나 회복
		EaterPierce = 0x00020000, //관통 피해 증가
        ResonancePierce = 0x00040000, //관통 피해 증가%
		EaterBleed = 0x00080000, //출혈 피해 증가
        ResonanceBleed = 0x00100000, //출혈 피해 증가%
		HumanoidDamage = 0x00200000, //영장류 피해 증가%
		UndeadDamage = 0x00400000, //언데드 피해 증가%
		ElementalDamage = 0x00800000, //정령 피해 증가%
		AbyssDamage = 0x01000000, //악마 피해 증가%
		ArachnidDamage = 0x02000000, //거미류 피해 증가%
		ReptilianDamage = 0x04000000, //파충류 피해 증가%
		FeyDamage = 0x08000000 //요정 피해 증가%
    }

    public sealed class SAAbsorptionAttributes : BaseAttributes
    {
        public static bool IsValid(SAAbsorptionAttribute attribute)
        {
            if (!Core.SA)
            {
                return false;
            }

            return true;
        }

        public static int[] GetValues(Mobile m, params SAAbsorptionAttribute[] attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static int[] GetValues(Mobile m, IEnumerable<SAAbsorptionAttribute> attributes)
        {
            return EnumerateValues(m, attributes).ToArray();
        }

        public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<SAAbsorptionAttribute> attributes)
        {
            return attributes.Select(a => GetValue(m, a));
        }

        public static int GetValue(Mobile m, SAAbsorptionAttribute attribute)
        {
            if (World.Loading || !IsValid(attribute))
            {
                return 0;
            }

            int value = 0;

            #region Enhancement
            value += Enhancement.GetValue(m, attribute);
            #endregion

            for (int i = 0; i < m.Items.Count; ++i)
            {
                SAAbsorptionAttributes attrs = RunicReforging.GetSAAbsorptionAttributes(m.Items[i]);

                if (attrs != null)
                    value += attrs[attribute];
            }

            value += SkillMasterySpell.GetAttributeBonus(m, attribute);

			value /= 100;
            return value;
        }

        public SAAbsorptionAttributes(Item owner)
            : base(owner)
        {
        }

        public SAAbsorptionAttributes(Item owner, SAAbsorptionAttributes other)
            : base(owner, other)
        {
        }

        public SAAbsorptionAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public int this[SAAbsorptionAttribute attribute]
        {
            get
            {
                return GetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterFire
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterFire];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterFire] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterCold
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterCold];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterCold] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterPoison
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterPoison];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterPoison] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterEnergy
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterEnergy];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterEnergy] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterKinetic
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterKinetic];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterKinetic] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterDamage] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceFire
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceFire];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceFire] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceCold
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceCold];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceCold] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonancePoison
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonancePoison];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonancePoison] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceEnergy
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceEnergy];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceEnergy] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceKinetic
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceKinetic];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceKinetic] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargeFire
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargeFire];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargeFire] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargeCold
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargeCold];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargeCold] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargePoison
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargePoison];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargePoison] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargeEnergy
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargeEnergy];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargeEnergy] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoulChargeKinetic
        {
            get
            {
                return this[SAAbsorptionAttribute.SoulChargeKinetic];
            }
            set
            {
                this[SAAbsorptionAttribute.SoulChargeKinetic] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CastingFocus
        {
            get
            {
                return this[SAAbsorptionAttribute.CastingFocus];
            }
            set
            {
                this[SAAbsorptionAttribute.CastingFocus] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterPierce
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterPierce];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterPierce] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonancePierce
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonancePierce];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonancePierce] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int EaterBleed
        {
            get
            {
                return this[SAAbsorptionAttribute.EaterBleed];
            }
            set
            {
                this[SAAbsorptionAttribute.EaterBleed] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ResonanceBleed
        {
            get
            {
                return this[SAAbsorptionAttribute.ResonanceBleed];
            }
            set
            {
                this[SAAbsorptionAttribute.ResonanceBleed] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int HumanoidDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.HumanoidDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.HumanoidDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int UndeadDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.UndeadDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.UndeadDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ElementalDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.ElementalDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.ElementalDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int AbyssDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.AbyssDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.AbyssDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ArachnidDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.ArachnidDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.ArachnidDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ReptilianDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.ReptilianDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.ReptilianDamage] = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int FeyDamage
        {
            get
            {
                return this[SAAbsorptionAttribute.FeyDamage];
            }
            set
            {
                this[SAAbsorptionAttribute.FeyDamage] = value;
            }
        }
	}
    #endregion

    [Flags]
    public enum AosElementAttribute
    {
        Physical = 0x00000001,
        Fire = 0x00000002,
        Cold = 0x00000004,
        Poison = 0x00000008,
        Energy = 0x00000010,
        Chaos = 0x00000020,
        Direct = 0x00000040
    }

    public sealed class AosElementAttributes : BaseAttributes
    {
        public AosElementAttributes(Item owner)
            : base(owner)
        {
        }

        public AosElementAttributes(Item owner, AosElementAttributes other)
            : base(owner, other)
        {
        }

        public AosElementAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public int this[AosElementAttribute attribute]
        {
            get
            {
                return ExtendedGetValue((int)attribute);
            }
            set
            {
                SetValue((int)attribute, value);
            }
        }

        public int ExtendedGetValue(int bitmask)
        {
            int value = GetValue(bitmask);

            XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

            if (xaos != null)
            {
                value += xaos.GetValue(bitmask);
            }

            return (value);
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Physical
        {
            get
            {
                return this[AosElementAttribute.Physical];
            }
            set
            {
                this[AosElementAttribute.Physical] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Fire
        {
            get
            {
                return this[AosElementAttribute.Fire];
            }
            set
            {
                this[AosElementAttribute.Fire] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Cold
        {
            get
            {
                return this[AosElementAttribute.Cold];
            }
            set
            {
                this[AosElementAttribute.Cold] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Poison
        {
            get
            {
                return this[AosElementAttribute.Poison];
            }
            set
            {
                this[AosElementAttribute.Poison] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Energy
        {
            get
            {
                return this[AosElementAttribute.Energy];
            }
            set
            {
                this[AosElementAttribute.Energy] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Chaos
        {
            get
            {
                return this[AosElementAttribute.Chaos];
            }
            set
            {
                this[AosElementAttribute.Chaos] = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Direct
        {
            get
            {
                return this[AosElementAttribute.Direct];
            }
            set
            {
                this[AosElementAttribute.Direct] = value;
            }
        }
    }

    [Flags]
    public enum NegativeAttribute
    {
        Brittle = 0x00000001,
        Prized = 0x00000002,
        Massive = 0x00000004,
        Unwieldly = 0x00000008,
        Antique = 0x00000010,
        NoRepair = 0x00000020
    }

    public sealed class NegativeAttributes : BaseAttributes
    {
        public NegativeAttributes(Item owner)
            : base(owner)
        {
        }

        public NegativeAttributes(Item owner, NegativeAttributes other)
            : base(owner, other)
        {
        }

        public NegativeAttributes(Item owner, GenericReader reader)
            : base(owner, reader)
        {
        }

        public void GetProperties(ObjectPropertyList list, Item item)
        {
            if (NoRepair > 0)
                list.Add(1151782);

			/*
            if (Brittle > 0 ||
                item is BaseWeapon && ((BaseWeapon)item).Attributes.Brittle > 0 ||
                item is BaseArmor && ((BaseArmor)item).Attributes.Brittle > 0 ||
                item is BaseJewel && ((BaseJewel)item).Attributes.Brittle > 0 ||
                item is BaseClothing && ((BaseClothing)item).Attributes.Brittle > 0)
                list.Add(1116209);
			*/
            if (Prized > 0)
                list.Add(1154910);

            //if (Massive > 0)
            //    list.Add(1038003);

            //if (Unwieldly > 0)
            //    list.Add(1154909);

            if (Antique > 0)
                list.Add(1076187);
        }

        public const double CombatDecayChance = 0.02;

        public static void OnCombatAction(Mobile m)
        {
            if (m == null || !m.Alive)
                return;

            var list = new List<Item>();

            foreach (var item in m.Items.Where(i => i is IDurability))
            {
                NegativeAttributes attrs = RunicReforging.GetNegativeAttributes(item);

                if (attrs != null && attrs.Antique > 0 && CombatDecayChance > Utility.RandomDouble())
                {
                    list.Add(item);
                }
            }

            foreach (var item in list)
            {
                IDurability dur = item as IDurability;

                if (dur == null)
                    continue;

                if (dur.HitPoints >= 1)
                {
                    if (dur.HitPoints >= 4)
                    {
                        dur.HitPoints -= 4;
                    }
                    else
                    {
                        dur.HitPoints = 0;
                    }
                }
                else
                {
                    if (dur.MaxHitPoints > 1)
                    {
                        dur.MaxHitPoints--;

                        if (item.Parent is Mobile)
                            ((Mobile)item.Parent).LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                    }
                    else
                    {
                        item.Delete();
                    }
                }
            }

            ColUtility.Free(list);
        }

        public int this[NegativeAttribute attribute]
        {
            get { return GetValue((int)attribute); }
            set { SetValue((int)attribute, value); }
        }

        public override string ToString()
        {
            return "...";
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Brittle { get { return this[NegativeAttribute.Brittle]; } set { this[NegativeAttribute.Brittle] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Prized { get { return this[NegativeAttribute.Prized]; } set { this[NegativeAttribute.Prized] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Massive { get { return this[NegativeAttribute.Massive]; } set { this[NegativeAttribute.Massive] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Unwieldly { get { return this[NegativeAttribute.Unwieldly]; } set { this[NegativeAttribute.Unwieldly] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Antique { get { return this[NegativeAttribute.Antique]; } set { this[NegativeAttribute.Antique] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int NoRepair { get { return this[NegativeAttribute.NoRepair]; } set { this[NegativeAttribute.NoRepair] = value; } }
    }

    [PropertyObject]
    public abstract class BaseAttributes
    {
        private readonly Item m_Owner;
        private uint m_Names;
        private int[] m_Values;

        private static readonly int[] m_Empty = new int[0];

        public bool IsEmpty
        {
            get
            {
                return (m_Names == 0);
            }
        }
        public Item Owner
        {
            get
            {
                return m_Owner;
            }
        }

        public BaseAttributes(Item owner)
        {
            m_Owner = owner;
            m_Values = m_Empty;
        }

        public BaseAttributes(Item owner, BaseAttributes other)
        {
            m_Owner = owner;
            m_Values = new int[other.m_Values.Length];
            other.m_Values.CopyTo(m_Values, 0);
            m_Names = other.m_Names;
        }

        public BaseAttributes(Item owner, GenericReader reader)
        {
            m_Owner = owner;

            int version = reader.ReadByte();

            switch (version)
            {
                case 1:
                    {
                        m_Names = reader.ReadUInt();
                        m_Values = new int[reader.ReadEncodedInt()];

                        for (int i = 0; i < m_Values.Length; ++i)
                            m_Values[i] = reader.ReadEncodedInt();

                        break;
                    }
                case 0:
                    {
                        m_Names = reader.ReadUInt();
                        m_Values = new int[reader.ReadInt()];

                        for (int i = 0; i < m_Values.Length; ++i)
                            m_Values[i] = reader.ReadInt();

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((byte)1); // version;

            writer.Write((uint)m_Names);
            writer.WriteEncodedInt((int)m_Values.Length);

            for (int i = 0; i < m_Values.Length; ++i)
                writer.WriteEncodedInt((int)m_Values[i]);
        }

        public int GetValue(int bitmask)
        {
            if (!Core.AOS)
                return 0;

            uint mask = (uint)bitmask;

            if ((m_Names & mask) == 0)
                return 0;

            int index = GetIndex(mask);

            if (index >= 0 && index < m_Values.Length)
                return m_Values[index];

            return 0;
        }

        public virtual void SetValue(int bitmask, int value)
        {
            uint mask = (uint)bitmask;

            if (value != 0)
            {
                if ((m_Names & mask) != 0)
                {
                    int index = GetIndex(mask);

                    if (index >= 0 && index < m_Values.Length)
                        m_Values[index] = value;
                }
                else
                {
                    int index = GetIndex(mask);

                    if (index >= 0 && index <= m_Values.Length)
                    {
                        int[] old = m_Values;
                        m_Values = new int[old.Length + 1];

                        for (int i = 0; i < index; ++i)
                            m_Values[i] = old[i];

                        m_Values[index] = value;

                        for (int i = index; i < old.Length; ++i)
                            m_Values[i + 1] = old[i];

                        m_Names |= mask;
                    }
                }
            }
            else if ((m_Names & mask) != 0)
            {
                int index = GetIndex(mask);

                if (index >= 0 && index < m_Values.Length)
                {
                    m_Names &= ~mask;

                    if (m_Values.Length == 1)
                    {
                        m_Values = m_Empty;
                    }
                    else
                    {
                        int[] old = m_Values;
                        m_Values = new int[old.Length - 1];

                        for (int i = 0; i < index; ++i)
                            m_Values[i] = old[i];

                        for (int i = index + 1; i < old.Length; ++i)
                            m_Values[i - 1] = old[i];
                    }
                }
            }

            if (m_Owner != null && m_Owner.Parent is Mobile)
            {
                Mobile m = (Mobile)m_Owner.Parent;

                m.CheckStatTimers();
                m.UpdateResistances();
                m.Delta(MobileDelta.Stat | MobileDelta.WeaponDamage | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);
            }

            if (m_Owner != null)
                m_Owner.InvalidateProperties();
        }

        private int GetIndex(uint mask)
        {
            int index = 0;
            uint ourNames = m_Names;
            uint currentBit = 1;

            while (currentBit != mask)
            {
                if ((ourNames & currentBit) != 0)
                    ++index;

                if (currentBit == 0x80000000)
                    return -1;

                currentBit <<= 1;
            }

            return index;
        }
    }
}
