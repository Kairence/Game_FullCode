using System;
using System.Collections;
using System.Collections.Generic;
using Server.Network;
using Server.Spells;
using Server.Mobiles;
using Server.Misc;

namespace Server.Items
{
    public abstract class WeaponAbility
    {
        public virtual int BaseMana => m_Specs.TryGetValue(this, out var s) ? s.BaseMana : 0;
        public virtual int BaseStam => m_Specs.TryGetValue(this, out var s) ? s.BaseStam : 0;
        public virtual TimeSpan Cooldown => m_Specs.TryGetValue(this, out var s) ? s.Cooldown : TimeSpan.FromSeconds(5.0);
        public virtual int[] AllowedCategories => m_Specs.TryGetValue(this, out var s) ? s.AllowedCategories : null;
        public virtual bool IsUltimate => m_Specs.TryGetValue(this, out var s) ? s.IsUltimate : false;


        public virtual int AccuracyBonus
        {
            get
            {
                return 0;
            }
        }
        public virtual double DamageScalar
        {
            get
            {
                return 1.0;
            }
        }

        public virtual bool RequiresSE
        {
            get
            {
                return false;
            }
        }

		/// <summary>
		///		Return false to make this special ability consume no ammo from ranged weapons
		/// </summary>
		public virtual bool ConsumeAmmo
		{
			get
			{
				return true;
			}
		}

		// 광역 즉발 특수기 여부 (true면 타겟 없이 즉발 광역 시전)
		public virtual bool IsAreaAbility => false;

		public virtual void BeforeAttack(Mobile attacker, Mobile defender, int damage)
		{
		}
		
        public virtual void OnHit(Mobile attacker, Mobile defender, int damage)
        {
			OnHit(attacker, defender, damage, 0, 0 );
        }

        public virtual void OnHit(Mobile attacker, Mobile defender, int damage, int level)
        {
			OnHit(attacker, defender, damage, 0, 0 );
        }

        public virtual void OnHit(Mobile attacker, Mobile defender, int damage, int level, double bonus)
        {
        }

        public virtual void OnMiss(Mobile attacker, Mobile defender)
        {
        }

        public virtual bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            // Here because you must be sure you can use the skill before calling CheckHit if the ability has a HCI bonus for example
            return true;
        }

        public virtual bool OnBeforeSwing(Mobile attacker, Mobile defender, int level)
        {
            // Here because you must be sure you can use the skill before calling CheckHit if the ability has a HCI bonus for example
            return true;
        }

        public virtual bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            return true;
        }

        public virtual bool OnBeforeDamage(Mobile attacker, Mobile defender, int level)
        {
            return true;
        }

        public virtual bool RequiresSecondarySkill(Mobile from)
        {
            return true;
        }

        public virtual double GetRequiredSkill(Mobile from)
        {
			return 0.0;
			/*
            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon != null && (weapon.PrimaryAbility == this ) )
                return 0.0;
            else if (weapon != null && (weapon.SecondaryAbility == this ) )
                return 150.0;

            return 200.0;
			*/
        }

        public virtual double GetRequiredSecondarySkill(Mobile from)
        {
            //if (!RequiresSecondarySkill(from))
            return 0.0;

            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon != null && (weapon.PrimaryAbility == this ) )
                return 0.0;
            else if (weapon != null && (weapon.SecondaryAbility == this ) )
                return 100.0;

            return 200.0;
        }

        public virtual SkillName GetSecondarySkill(Mobile from)
        {
            return SkillName.Tactics;
        }

		public virtual bool CalculateStam(Mobile from, int baseskill, int levelskill, int level, bool tactics )
		{
			if( from is BaseCreature )
				return true;
			int stam = baseskill + levelskill;
			if( from.Skills.Focus.Value >= 150 )
				stam -= 25;
			int loss = AosArmorAttributes.GetValue(from, AosArmorAttribute.DefenseStam);
			stam = (int)( stam * ( 1 - loss * 0.000001 ) );
			if( from.Stam < stam )
				return false;
			else
			{
				if( tactics && Utility.RandomDouble() < 0.25 )
					return true;

				from.Stam -= stam;
				return true;
			}
		}

        public virtual int CalculateMana(Mobile from)
        {
			if( from is BaseCreature )
				return 0;
			if( from is PlayerMobile )
			{
				BaseWeapon weapon = from.Weapon as BaseWeapon;
				int stamloss = ( weapon.MinDamage + weapon.MaxDamage ) / 2;
				if( !(weapon is Fists ))
				{
					stamloss += weapon.StrRequirement / 20;
					stamloss /= 4;
				}
				return stamloss;
			}
			int mana = BaseMana;
			
			//BaseWeapon weapon = from.Weapon as BaseWeapon;
			//if( weapon.SecondaryAbility == this )
			//	mana *= 2;

			/*
            double skillTotal = GetSkillTotal(from);

            if (skillTotal >= 300.0)
                mana -= 10;
            else if (skillTotal >= 200.0)
                mana -= 5;
            double scalar = 1.0;

            if (!Server.Spells.Necromancy.MindRotSpell.GetMindRotScalar(from, ref scalar))
            {
                scalar = 1.0;
            }

            if (Server.Spells.Mysticism.PurgeMagicSpell.IsUnderCurseEffects(from))
            {
                scalar += .5;
            }

            // Lower Mana Cost = 40%
            int lmc = Math.Min(AosAttributes.GetValue(from, AosAttribute.LowerManaCost), 40);

            lmc += BaseArmor.GetInherentLowerManaCost(from);

            scalar -= (double)lmc / 100;
            mana = (int)(mana * scalar);
			*/

            // Using a special move within 3 seconds of the previous special move costs double mana 
            //if (GetContext(from) != null)
            //    mana *= 2;

            return mana;
        }

        public virtual bool CheckWeaponSkill(Mobile from)
        {
            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon == null)
                return false;

            Skill skill = from.Skills[weapon.Skill];

            double reqSkill = GetRequiredSkill(from);
            double reqSecondarySkill = GetRequiredSecondarySkill(from);
            SkillName secondarySkill = SkillName.Tactics;

            if (Core.ML && from.Skills[secondarySkill].Value < reqSecondarySkill)
            {
                int loc = GetSkillLocalization(secondarySkill);

                if (loc == 1060184)
                {
                    from.SendLocalizedMessage(loc);
                }
                else
                {
                    from.SendLocalizedMessage(loc, reqSecondarySkill.ToString());
                }

                return false;
            }

            if (skill != null && skill.Base >= reqSkill)
                return true;

            /* <UBWS> */
            if (weapon.WeaponAttributes.UseBestSkill > 0 && (from.Skills[SkillName.Swords].Base >= reqSkill || from.Skills[SkillName.Macing].Base >= reqSkill || from.Skills[SkillName.Fencing].Base >= reqSkill))
                return true;
            /* </UBWS> */

            if (reqSecondarySkill != 0.0 && !Core.TOL)
            {
                from.SendLocalizedMessage(1079308, reqSkill.ToString()); // You need ~1_SKILL_REQUIREMENT~ weapon and tactics skill to perform that attack
            }
            else
            {
                from.SendLocalizedMessage(1060182, reqSkill.ToString()); // You need ~1_SKILL_REQUIREMENT~ weapon skill to perform that attack
            }

            return false;
        }

        private int GetSkillLocalization(SkillName skill)
        {
            switch (skill)
            {
                default: return Core.TOL ? 1157351 : 1079308;
                    // You need ~1_SKILL_REQUIREMENT~ weapon and tactics skill to perform that attack                                                             
                    // You need ~1_SKILL_REQUIREMENT~ tactics skill to perform that attack
                case SkillName.Bushido:
                case SkillName.Ninjitsu: return 1063347;
                    // You need ~1_SKILL_REQUIREMENT~ Bushido or Ninjitsu skill to perform that attack!
                case SkillName.Poisoning: return 1060184;
                    // You lack the required poisoning to perform that attack
            }
        }

        public virtual bool CheckSkills(Mobile from)
        {
            return CheckWeaponSkill(from);
        }

        public virtual double GetSkillTotal(Mobile from)
        {
            return GetSkill(from, SkillName.Swords) + GetSkill(from, SkillName.Macing) +
                   GetSkill(from, SkillName.Fencing) + GetSkill(from, SkillName.Archery) + GetSkill(from, SkillName.Parry) +
                   GetSkill(from, SkillName.Lumberjacking) + GetSkill(from, SkillName.Stealth) + GetSkill(from, SkillName.Throwing) +
                   GetSkill(from, SkillName.Poisoning) + GetSkill(from, SkillName.Bushido) + GetSkill(from, SkillName.Ninjitsu);
        }

        public virtual double GetSkill(Mobile from, SkillName skillName)
        {
            Skill skill = from.Skills[skillName];

            if (skill == null)
                return 0.0;

            return skill.Value;
        }

        public virtual int CalculateMana(Mobile from, int type)
        {
			if( from is BaseCreature )
				return 0;
			BaseWeapon weapon = from.Weapon as BaseWeapon;
			if( from is PlayerMobile )
			{
				int stamloss = ( weapon.MinDamage + weapon.MaxDamage ) / 2;
				if( !(weapon is Fists ))
				{
					stamloss += weapon.StrRequirement / 20;
					stamloss /= 4;
				}
				return stamloss;
			}
            int mana = BaseMana;
			
			if( weapon.SecondaryAbility == this )
				mana *= 2;

            return mana;
        }

		public virtual bool CheckMana(Mobile from, int type, double time )
		{
            int mana = CalculateMana(from);
			//mana *= ActivebyOnHit(from);
			
            if (from.Stam < mana)
            {
                from.SendMessage("기력이 {0} 필요합니다.", mana.ToString()); // You need ~1_MANA_REQUIREMENT~ mana to perform that attack
				ClearCurrentAbility(from);
				
                return false;
            }
			/*
			if ( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				if( pm.Hunger < mana * 4 )
				{
					from.SendMessage("당신은 허기부터 해결해야 합니다.");
					return false;
				}
				else
					pm.Hunger -= mana * 4;
			}
			*/
            BaseWeapon weapon = from.Weapon as BaseWeapon;

			if (weapon != null && weapon.PrimaryAbility == this && GetContext(from) == null)
			{
				SPMTime = DateTime.Now + TimeSpan.FromSeconds(time);
				Timer timer = new WeaponAbilityTimer(from, time);
				timer.Start();

				AddContext(from, new WeaponAbilityContext(timer));
			}
			if (weapon != null && weapon.SecondaryAbility == this && GetContext2(from) == null)
			{
				SPMTime = DateTime.Now + TimeSpan.FromSeconds(time);
				Timer timer = new WeaponAbilityTimer2(from, time);
				timer.Start();

				AddContext2(from, new WeaponAbilityContext2(timer));
			}

			if (ManaPhasingOrb.IsInManaPhase(from))
				ManaPhasingOrb.RemoveFromTable(from);
			else
				from.Stam -= mana;

            return true;
		}
		
		private DateTime SPMTime;
		
        public virtual bool CheckMana(Mobile from, bool consume, int type = 1, double time = 60.0)
        {
            int mana = CalculateMana(from);
			//mana *= 2;//ActivebyOnHit(from);

            if (from.Mana < mana)
            {
                from.SendMessage("기력이 {0} 필요합니다.", mana.ToString()); // You need mana to perform that attack
                return false;
            }
			
            if (consume)
            {
				BaseWeapon weapon = from.Weapon as BaseWeapon;
                if (weapon != null && weapon.PrimaryAbility == this && GetContext(from) == null)
                {
					SPMTime = DateTime.Now + TimeSpan.FromSeconds(time);
					Timer timer = new WeaponAbilityTimer(from, time);
                    timer.Start();

                    AddContext(from, new WeaponAbilityContext(timer));
                }
				if (weapon != null && weapon.SecondaryAbility == this && GetContext2(from) == null)
				{
					SPMTime = DateTime.Now + TimeSpan.FromSeconds(time);
					Timer timer = new WeaponAbilityTimer2(from, time);
					timer.Start();

					AddContext2(from, new WeaponAbilityContext2(timer));
				}
                if (ManaPhasingOrb.IsInManaPhase(from))
                    ManaPhasingOrb.RemoveFromTable(from);
                else
                    from.Stam -= mana;
            }

            return true;
        }

        public virtual bool Validate(Mobile from)
        {
            //if (!from.Player && (!Core.TOL || CheckMana(from, false)))
            //    return true;

			/*
			BaseWeapon weapon = from.Weapon as BaseWeapon;
            if (weapon != null && weapon.PrimaryAbility == this && GetContext(from) != null)
			{
				string time = "첫 번째 특수기의 재사용 시간이 " + Server.Misc.Util.TimeCal(SPMTime, DateTime.Now ) + " 남았습니다";
				from.SendMessage(time);
                //from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
				return false;
			}
            if (weapon != null && weapon.SecondaryAbility == this && GetContext2(from) != null)
			{
				string time = "두 번째 특수기의 재사용 시간이 " + Server.Misc.Util.TimeCal(SPMTime, DateTime.Now ) + " 남았습니다";
				from.SendMessage(time);
                //from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
				return false;
			}
			*/
            NetState state = from.NetState;

            if (state == null)
                return false;

            if (RequiresSE && !state.SupportsExpansion(Expansion.SE))
            {
                from.SendLocalizedMessage(1063456); // You must upgrade to Samurai Empire in order to use that ability.
                return false;
            }

            if (Spells.Bushido.HonorableExecution.IsUnderPenalty(from) || Spells.Ninjitsu.AnimalForm.UnderTransformation(from))
            {
                from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
                return false;
            }

            if (Core.ML && from.Spell != null)
            {
                from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
                return false;
            }

            return true; //CheckSkills(from) && CheckMana(from, false);
        }

		
		private static int ActivebyOnHit(Mobile from)
		{
			WeaponAbility a = WeaponAbility.GetCurrentAbility(from);
			if( a == ArmorIgnore || a == ConcussionBlow || a == m_Abilities[7] || a == m_Abilities[8] || a == m_Abilities[9] || a == m_Abilities[28] || a == m_Abilities[26] )
				return 2;
			
			if( a == BleedAttack || a == CrushingBlow || a == m_Abilities[5] || a == m_Abilities[6] || a == m_Abilities[10] || a == m_Abilities[11] || a == m_Abilities[12] || a == m_Abilities[13] || a == m_Abilities[25] || a == m_Abilities[27] )
				return 3;
			return 10;
		}
		
        private static readonly WeaponAbility[] m_Abilities = new WeaponAbility[34]
        {
            null,
            new ArmorIgnore(),
            new BleedAttack(),
            new ConcussionBlow(),
            new CrushingBlow(),
            new Disarm(),
            new Dismount(),
            new DoubleStrike(),
            new InfectiousStrike(),
            new MortalStrike(),
            new MovingShot(),
            new ParalyzingBlow(),
            new ShadowStrike(),
            new WhirlwindAttack(),
            new RidingSwipe(),
            new FrenziedWhirlwind(),
            new Block(),
            new DefenseMastery(),
            new NerveStrike(),
            new TalonStrike(),
            new Feint(),
            new DualWield(),
            new DoubleShot(),
            new ArmorPierce(),
            new Bladeweave(),
            new ForceArrow(),
            new LightningArrow(),
            new PsychicAttack(),
            new SerpentArrow(),
            new ForceOfNature(),
            new InfusedThrow(),
            new MysticArc(),
            new Disrobe(),
            new ColdWind()
        };

        public static WeaponAbility[] Abilities
        {
            get
            {
                return m_Abilities;
            }
        }

        private static readonly Hashtable m_Table = new Hashtable();

        public static Hashtable Table
        {
            get
            {
                return m_Table;
            }
        }

        public static readonly WeaponAbility ArmorIgnore = m_Abilities[1];
        public static readonly WeaponAbility BleedAttack = m_Abilities[2];
        public static readonly WeaponAbility ConcussionBlow = m_Abilities[3];
        public static readonly WeaponAbility CrushingBlow = m_Abilities[4];
        public static readonly WeaponAbility Disarm = m_Abilities[5];
        public static readonly WeaponAbility Dismount = m_Abilities[6];
        public static readonly WeaponAbility DoubleStrike = m_Abilities[7];
        public static readonly WeaponAbility InfectiousStrike = m_Abilities[8];
        public static readonly WeaponAbility MortalStrike = m_Abilities[9];
        public static readonly WeaponAbility MovingShot = m_Abilities[10];
        public static readonly WeaponAbility ParalyzingBlow = m_Abilities[11];
        public static readonly WeaponAbility ShadowStrike = m_Abilities[12];
        public static readonly WeaponAbility WhirlwindAttack = m_Abilities[13];

        public static readonly WeaponAbility RidingSwipe = m_Abilities[14];
        public static readonly WeaponAbility FrenziedWhirlwind = m_Abilities[15];
        public static readonly WeaponAbility Block = m_Abilities[16];
        public static readonly WeaponAbility DefenseMastery = m_Abilities[17];
        public static readonly WeaponAbility NerveStrike = m_Abilities[18];
        public static readonly WeaponAbility TalonStrike = m_Abilities[19];
        public static readonly WeaponAbility Feint = m_Abilities[20];
        public static readonly WeaponAbility DualWield = m_Abilities[21];
        public static readonly WeaponAbility DoubleShot = m_Abilities[22];
        public static readonly WeaponAbility ArmorPierce = m_Abilities[23];

        public static readonly WeaponAbility Bladeweave = m_Abilities[24];
        public static readonly WeaponAbility ForceArrow = m_Abilities[25];
        public static readonly WeaponAbility LightningArrow = m_Abilities[26];
        public static readonly WeaponAbility PsychicAttack = m_Abilities[27];
        public static readonly WeaponAbility SerpentArrow = m_Abilities[28];
        public static readonly WeaponAbility ForceOfNature = m_Abilities[29];

        public static readonly WeaponAbility InfusedThrow = m_Abilities[30];
        public static readonly WeaponAbility MysticArc = m_Abilities[31];

        public static readonly WeaponAbility Disrobe = m_Abilities[32];
        public static readonly WeaponAbility ColdWind = m_Abilities[33];

        #region [전사 특수기 중앙 스펙 및 쿨타임 관리]
        public class AbilitySpec
        {
            public int BaseStam { get; set; }
            public int BaseMana { get; set; }
            public TimeSpan Cooldown { get; set; }
            public int[] AllowedCategories { get; set; }
            public bool IsUltimate { get; set; }

            public AbilitySpec(int stam, int mana, double cdSecs, int[] cats, bool ultimate = false)
            {
                BaseStam = stam;
                BaseMana = mana;
                Cooldown = TimeSpan.FromSeconds(cdSecs);
                AllowedCategories = cats;
                IsUltimate = ultimate;
            }
        }

        private static readonly Dictionary<WeaponAbility, AbilitySpec> m_Specs = new Dictionary<WeaponAbility, AbilitySpec>();
        private static readonly Dictionary<(Mobile, WeaponAbility), DateTime> m_Cooldowns = new Dictionary<(Mobile, WeaponAbility), DateTime>();

        private static void RegisterSpec(WeaponAbility ability, int stam, int mana, double cdSecs, int[] cats, bool ultimate = false)
        {
            if (ability != null)
            {
                m_Specs[ability] = new AbilitySpec(stam, mana, cdSecs, cats, ultimate);
            }
        }

        static WeaponAbility()
        {
            // [공식 기획서 기준 전사 특수기 스펙 및 사용 가능 무기군 등록]
            // 무기군: 0:한손검, 1:양손검, 2:도끼, 3:한손둔기, 4:양손둔기, 5:한손펜싱, 6:양손펜싱, 7:활, 8:석궁, 9:맨손
            RegisterSpec(ArmorIgnore, stam: 60, mana: 0, cdSecs: 6.0, cats: new[] { 0, 1, 5, 6 });
            RegisterSpec(BleedAttack, stam: 40, mana: 0, cdSecs: 5.0, cats: new[] { 0, 2, 6, 7 });
            RegisterSpec(ConcussionBlow, stam: 50, mana: 0, cdSecs: 5.0, cats: new[] { 3, 4, 9 }); // 충격파 (5초 스턴 + 무기 100%)
            RegisterSpec(CrushingBlow, stam: 60, mana: 0, cdSecs: 6.0, cats: new[] { 1, 2, 4, 8 }); // 파괴 일격 (기력 삭감)
            RegisterSpec(Disarm, stam: 60, mana: 0, cdSecs: 10.0, cats: new[] { 0, 3, 9 }); // 무장 해제 (30초간 공격력 50% 감소)
            RegisterSpec(Dismount, stam: 60, mana: 0, cdSecs: 10.0, cats: new[] { 2, 4 }); // 낙마 (탑승/비행 시 30초간 피해 50% 증가)
            RegisterSpec(DoubleStrike, stam: 100, mana: 0, cdSecs: 12.0, cats: new[] { 5, 6, 7 }); // 연속 공격
            RegisterSpec(InfectiousStrike, stam: 60, mana: 0, cdSecs: 6.0, cats: new[] { 5, 7 }); // 독 바르기
            RegisterSpec(MortalStrike, stam: 120, mana: 0, cdSecs: 12.0, cats: new[] { 8 }); // 급소 가격 (400% 추가 공격)
            RegisterSpec(MovingShot, stam: 60, mana: 0, cdSecs: 8.0, cats: new[] { 7, 8 });
            RegisterSpec(ParalyzingBlow, stam: 80, mana: 0, cdSecs: 10.0, cats: new[] { 1, 3, 6, 8, 9 }); // 마비 일격
            RegisterSpec(ShadowStrike, stam: 80, mana: 0, cdSecs: 10.0, cats: new[] { 5 }); // 그림자 일격 (어그로 0, 200% 추가)
            RegisterSpec(WhirlwindAttack, stam: 100, mana: 0, cdSecs: 12.0, cats: new[] { 1, 2, 4 }); // 소용돌이 일격 (광역 300% 추가)
            RegisterSpec(RidingSwipe, stam: 80, mana: 0, cdSecs: 10.0, cats: new[] { 1, 4, 6 });
            RegisterSpec(Block, stam: 50, mana: 0, cdSecs: 10.0, cats: new[] { 0, 3, 5 });
            RegisterSpec(Feint, stam: 50, mana: 0, cdSecs: 8.0, cats: new[] { 0, 5 });
            RegisterSpec(DualWield, stam: 60, mana: 0, cdSecs: 8.0, cats: new[] { 0, 5 });
            RegisterSpec(DoubleShot, stam: 80, mana: 0, cdSecs: 10.0, cats: new[] { 7, 8 });
            RegisterSpec(ArmorPierce, stam: 50, mana: 0, cdSecs: 5.0, cats: new[] { 8 }); // 방어구 관통 (석궁 60% 방어무시)
            RegisterSpec(Bladeweave, stam: 100, mana: 0, cdSecs: 12.0, cats: new[] { 0 }); // 칼날 매듭 (한손검 300% 추가)
            RegisterSpec(LightningArrow, stam: 100, mana: 0, cdSecs: 10.0, cats: new[] { 7 }); // 전격 화살 (활 5타일 광역 300% 추가)
            RegisterSpec(PsychicAttack, stam: 60, mana: 0, cdSecs: 8.0, cats: new[] { 3, 9 }); // 정신 공격 (마법 피해 50% 증가)
            RegisterSpec(SerpentArrow, stam: 60, mana: 0, cdSecs: 8.0, cats: new[] { 7 });
            RegisterSpec(ForceOfNature, stam: 80, mana: 0, cdSecs: 10.0, cats: new[] { 3, 4 });
            RegisterSpec(InfusedThrow, stam: 60, mana: 0, cdSecs: 8.0, cats: new[] { 7, 8 });
            RegisterSpec(MysticArc, stam: 80, mana: 0, cdSecs: 10.0, cats: new[] { 7, 8 });

            // [무기술 200 레전더리 특수기 5종]
            RegisterSpec(NerveStrike, stam: 0, mana: 100, cdSecs: 30.0, cats: new[] { 0, 1, 2 }, ultimate: true); // 신경 공격 (검/도끼)
            RegisterSpec(FrenziedWhirlwind, stam: 0, mana: 100, cdSecs: 30.0, cats: new[] { 3, 4 }, ultimate: true); // 뇌진탕 일격 (둔기)
            RegisterSpec(TalonStrike, stam: 0, mana: 100, cdSecs: 30.0, cats: new[] { 5, 6 }, ultimate: true); // 갈퀴 발톱 (펜싱)
            RegisterSpec(ForceArrow, stam: 0, mana: 100, cdSecs: 30.0, cats: new[] { 7, 8 }, ultimate: true); // 힘의 화살 (궁술)
            RegisterSpec(DefenseMastery, stam: 0, mana: 100, cdSecs: 30.0, cats: new[] { 9 }, ultimate: true); // 완전 방어 (맨손)
        }


        public static TimeSpan GetCooldownRemaining(Mobile m, WeaponAbility a)
        {
            if (m != null && a != null && m_Cooldowns.TryGetValue((m, a), out DateTime until))
            {
                DateTime now = DateTime.UtcNow;
                if (until > now)
                    return until - now;
            }
            return TimeSpan.Zero;
        }

        public static void SetCooldown(Mobile m, WeaponAbility a, TimeSpan cd)
        {
            if (m != null && a != null)
            {
                m_Cooldowns[(m, a)] = DateTime.UtcNow + cd;
            }
        }

        public virtual bool CanUseWith(BaseWeapon weapon, Mobile attacker)
        {
            if (AllowedCategories == null || AllowedCategories.Length == 0)
                return true;

            int cat = BaseWeapon.GetWeaponCategoryID(weapon, attacker);
            for (int i = 0; i < AllowedCategories.Length; i++)
            {
                if (AllowedCategories[i] == cat)
                    return true;
            }
            return false;
        }

        public virtual void GetActualCost(Mobile attacker, out int stamCost, out int manaCost)
        {
            stamCost = BaseStam;
            manaCost = BaseMana;

            if (attacker is PlayerMobile pm)
            {
                // 서버 공식 커스텀 옵션 (ItemOptionCreator): LowerStamCost(65), LowerManaCost(64), LowerAllCost(66)
                int rawStam = ItemOptionCreator.GetAttributeValue(pm, CustomOption.LowerStamCost);
                int rawMana = ItemOptionCreator.GetAttributeValue(pm, CustomOption.LowerManaCost);
                int rawAll  = ItemOptionCreator.GetAttributeValue(pm, CustomOption.LowerAllCost);

                // 1%당 10,000 (ValueScale = 10000). 최대 할인율 50% 캡
                double stamDiscount = Math.Min(0.50, Math.Max(0.0, (rawStam + rawAll) / 1000000.0));
                double manaDiscount = Math.Min(0.50, Math.Max(0.0, (rawMana + rawAll) / 1000000.0));

                if (stamCost > 0)
                    stamCost = (int)Math.Max(0, Math.Round(stamCost * (1.0 - stamDiscount)));

                if (manaCost > 0)
                    manaCost = (int)Math.Max(0, Math.Round(manaCost * (1.0 - manaDiscount)));
            }
        }
        #endregion

        public static bool IsWeaponAbility(Mobile m, WeaponAbility a)
        {
            if (a == null)
                return true;

            if (!m.Player)
                return true;

            BaseWeapon weapon = m.Weapon as BaseWeapon ?? BaseWeapon.Fists;
            return a.CanUseWith(weapon, m);
        }

        public virtual bool DealsOwnDamage
        {
            get
            {
                return (this == ArmorIgnore || this == ArmorPierce || this == Bladeweave ||
                        this == BleedAttack || this == ConcussionBlow || this == CrushingBlow ||
                        this == DoubleStrike || this == DoubleShot || this == ForceOfNature ||
                        this == FrenziedWhirlwind || this == InfectiousStrike || this == LightningArrow ||
                        this == MortalStrike || this == MovingShot || this == MysticArc ||
                        this == NerveStrike || this == PsychicAttack || this == ShadowStrike ||
                        this == TalonStrike || this == WhirlwindAttack || this == ForceArrow);
            }
        }

        public virtual bool ValidatesDuringHit
        {
            get
            {
                return true;
            }
        }

        public static WeaponAbility GetCurrentAbility(Mobile m)
        {
            if (!Core.AOS)
            {
                ClearCurrentAbility(m);
                return null;
            }

            WeaponAbility a = (WeaponAbility)m_Table[m];

            if (!IsWeaponAbility(m, a))
            {
                ClearCurrentAbility(m);
                return null;
            }

            return a;
        }

        public static bool SetCurrentAbility(Mobile m, WeaponAbility a)
        {
            if (!Core.AOS)
            {
                ClearCurrentAbility(m);
                return false;
            }

            if (!IsWeaponAbility(m, a))
            {
                m.SendMessage(0x22, "현재 무기로는 사용할 수 없는 특수기입니다.");
                ClearCurrentAbility(m);
                return false;
            }

            if (a == null)
            {
                ClearCurrentAbility(m);
                return true;
            }

            // 액티브 즉발 시전 실행!
            return a.ExecuteActive(m);
        }

        public static void ClearCurrentAbility(Mobile m)
        {
            m_Table.Remove(m);

            if (Core.AOS && m.NetState != null)
                m.Send(ClearWeaponAbility.Instance);
        }

        #region 액티브 즉시 시전 엔진 (LoL 평캔 + 자동 타겟팅 시스템)
        public virtual bool ExecuteActive(Mobile attacker)
        {
            if (attacker == null || !attacker.Alive || attacker.Frozen || attacker.Paralyzed)
                return false;

            BaseWeapon weapon = attacker.Weapon as BaseWeapon ?? BaseWeapon.Fists;
            if (weapon == null)
                return false;

            // 1. 무기 적합성 검증
            if (!CanUseWith(weapon, attacker))
            {
                attacker.SendMessage(0x22, "현재 무기로는 사용할 수 없는 특수기입니다.");
                ClearCurrentAbility(attacker);
                return false;
            }

            // 2. 레전더리 특수기 스킬 요구치 검증 (무기술 200.0)
            if (IsUltimate)
            {
                SkillName reqSkill = weapon.Skill;
                if (weapon is Fists || weapon == null) reqSkill = SkillName.Wrestling;
                if (attacker.Skills[reqSkill].Value < 200.0)
                {
                    attacker.SendMessage(0x22, $"{reqSkill} 스킬이 200.0 이상이어야 사용할 수 있습니다.");
                    ClearCurrentAbility(attacker);
                    return false;
                }
            }

            // 3. 개별 쿨타임 검증
            TimeSpan remaining = GetCooldownRemaining(attacker, this);
            if (remaining > TimeSpan.Zero)
            {
                attacker.SendMessage(0x22, $"[{this}] 재사용 대기 중입니다. ({remaining.TotalSeconds:F1}초 남음)");
                ClearCurrentAbility(attacker);
                return false;
            }

            // 4. 자원 소모량 계산 (서버 옵션 LowerStamCost / LowerManaCost / LowerAllCost 정밀 적용)
            int stamCost = 0, manaCost = 0;
            GetActualCost(attacker, out stamCost, out manaCost);

            if (attacker.Stam < stamCost)
            {
                attacker.SendMessage(0x22, $"기력이 부족합니다. (필요 기력: {stamCost})");
                ClearCurrentAbility(attacker);
                return false;
            }

            if (attacker.Mana < manaCost)
            {
                attacker.SendMessage(0x22, $"마나가 부족합니다. (필요 마나: {manaCost})");
                ClearCurrentAbility(attacker);
                return false;
            }

            // 5. 타겟 결정 (범위기 vs 단일기)
            Mobile target = null;
            int maxRange = Math.Max(weapon.MaxRange, 1);

            if (IsAreaAbility)
            {
                // 범위 즉발 공격: 주변 적 유무 체크
                target = null;
            }
            else
            {
                // 1순위: 현재 교전 중인 대상 (Combatant)
                Mobile c = attacker.Combatant as Mobile;
                if (c != null && c.Alive && !c.Deleted && c.Map == attacker.Map && attacker.InRange(c.Location, maxRange) && attacker.InLOS(c) && attacker.CanBeHarmful(c, false))
                {
                    target = c;
                }

                // 2순위: 1순위 대상이 없거나 사거리 밖이면, 사거리 내 가장 가까운 적대 대상 자동 탐색
                if (target == null)
                {
                    double bestDist = double.MaxValue;
                    IPooledEnumerable eable = attacker.GetMobilesInRange(maxRange);
                    foreach (Mobile m in eable)
                    {
                        if (m == attacker || !m.Alive || m.Deleted || m.Map != attacker.Map)
                            continue;

                        if (!attacker.CanSee(m) || !attacker.InLOS(m))
                            continue;

                        // 피아식별: 자신에게 적대 가능한 대상만
                        if (!attacker.CanBeHarmful(m, false))
                            continue;

                        // 본인의 소환수나 제어 중인 펫은 제외
                        if (m is BaseCreature bc && (bc.Controlled || bc.Summoned) && (bc.ControlMaster == attacker || bc.SummonMaster == attacker))
                            continue;

                        double dist = attacker.GetDistanceToSqrt(m);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            target = m;
                        }
                    }
                    eable.Free();
                }

                if (target == null)
                {
                    attacker.SendMessage(0x3B2, "공격할 대상이 사거리 내에 없습니다.");
                    ClearCurrentAbility(attacker);
                    return false;
                }

                // 타겟 방향으로 캐릭터 회전 및 교전 상대 지정
                attacker.Direction = attacker.GetDirectionTo(target);
                attacker.Combatant = target;
            }

            // 6. 자원 차감 및 개별 쿨타임 등록
            if (attacker is PlayerMobile)
            {
                if (stamCost > 0)
                    attacker.Stam -= stamCost;
                if (manaCost > 0)
                    attacker.Mana -= manaCost;

                SetCooldown(attacker, this, Cooldown);
            }

            // 7. 특수기 장전 등록
            SpecialMove.ClearCurrentMove(attacker);
            m_Table[attacker] = this;

            // 8. 은신 해제 및 공격 애니메이션
            attacker.RevealingAction();

            // 9. 강제 타격 파이프라인 호출
            if (IsAreaAbility)
            {
                weapon.PlaySwingAnimation(attacker);

                // 광역 즉발기: 주변 모든 적 타격
                int baseDamage = (int)weapon.ScaleDamageAOS(attacker, (weapon.MinDamage + weapon.MaxDamage) / 2, false);
                OnHit(attacker, null, Math.Max(baseDamage, 1));
                attacker.SendMessage(0x35, $"[{this}] 광역 특수기를 발동했습니다!");
                ClearCurrentAbility(attacker);
            }
            else
            {
                attacker.DoHarmful(target);

                if (attacker.NetState != null)
                {
                    attacker.Send(new Swing(0, attacker, target));
                }

                // 단일 타겟: BaseWeapon.OnHit 강제 호출
                weapon.OnHit(attacker, target, 1.0);
                attacker.SendMessage(0x35, $"[{this}] 특수기를 적중시켰습니다!");
            }

            // 10. [평캔] 다음 평타는 무기 딜레이 1회 이후로 밀어줌
            long nextSwing = Core.TickCount + (long)weapon.GetDelay(attacker).TotalMilliseconds;
            if (attacker.NextCombatTime < nextSwing)
                attacker.NextCombatTime = nextSwing;

            return true;
        }
        #endregion

        public static void Initialize()
        {
            EventSink.SetAbility += new SetAbilityEventHandler(EventSink_SetAbility);
        }

        public WeaponAbility()
        {
        }

        private static void EventSink_SetAbility(SetAbilityEventArgs e)
        {
            int index = e.Index;
            Mobile m = e.Mobile;

            if (m == null)
                return;

            if (index == 0)
            {
                ClearCurrentAbility(m);
                return;
            }

            WeaponAbility ability = null;

            // 표준 UO 특수기 인덱스 (1 ~ 33) 매핑
            if (index >= 1 && index < m_Abilities.Length)
            {
                ability = m_Abilities[index];
            }

            if (ability != null)
                SetCurrentAbility(m, ability);
            else
                ClearCurrentAbility(m);
        }

        private static readonly Hashtable m_PlayersTable = new Hashtable();

        private static void AddContext(Mobile m, WeaponAbilityContext context)
        {
            m_PlayersTable[m] = context;
        }

        public static void RemoveContext(Mobile m)
        {
            WeaponAbilityContext context = GetContext(m);

            if (context != null)
                RemoveContext(m, context);
        }

        private static void RemoveContext(Mobile m, WeaponAbilityContext context)
        {
            m_PlayersTable.Remove(m);

            context.Timer.Stop();
        }

        private static WeaponAbilityContext GetContext(Mobile m)
        {
            return (m_PlayersTable[m] as WeaponAbilityContext);
        }

        private class WeaponAbilityTimer : Timer
        {
            private readonly Mobile m_Mobile;

            public WeaponAbilityTimer(Mobile from, double time)
                : base(TimeSpan.FromSeconds(time))
            {
                m_Mobile = from;
                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                RemoveContext(m_Mobile);
            }
        }

        private class WeaponAbilityContext
        {
            private readonly Timer m_Timer;

            public Timer Timer
            {
                get
                {
                    return m_Timer;
                }
            }

            public WeaponAbilityContext(Timer timer)
            {
                m_Timer = timer;
            }
        }
		
		
        private static readonly Hashtable m_PlayersTable2 = new Hashtable();

        private static void AddContext2(Mobile m, WeaponAbilityContext2 context)
        {
            m_PlayersTable2[m] = context;
        }

        public static void RemoveContext2(Mobile m)
        {
            WeaponAbilityContext2 context = GetContext2(m);

            if (context != null)
                RemoveContext2(m, context);
        }

        private static void RemoveContext2(Mobile m, WeaponAbilityContext2 context)
        {
            m_PlayersTable2.Remove(m);

            context.Timer.Stop();
        }

        private static WeaponAbilityContext2 GetContext2(Mobile m)
        {
            return (m_PlayersTable2[m] as WeaponAbilityContext2);
        }

        private class WeaponAbilityTimer2 : Timer
        {
            private readonly Mobile m_Mobile;

            public WeaponAbilityTimer2(Mobile from, double time)
                : base(TimeSpan.FromSeconds(time))
            {
                m_Mobile = from;
                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                RemoveContext2(m_Mobile);
            }
        }

        private class WeaponAbilityContext2
        {
            private readonly Timer m_Timer;

            public Timer Timer
            {
                get
                {
                    return m_Timer;
                }
            }

            public WeaponAbilityContext2(Timer timer)
            {
                m_Timer = timer;
            }
        }		
    }
}
