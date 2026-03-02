#region References
using System;
using System.Collections;
using Server.Engines.XmlSpawner2;
using Server.Factions;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Spellweaving;
using Server.Targeting;
#endregion

namespace Server.SkillHandlers
{
	public class AnimalTaming
	{
		private static readonly Hashtable m_BeingTamed = new Hashtable();

		public static bool DisableMessage { get; set; }
		public static bool DeferredTarget { get; set; }

		static AnimalTaming()
		{
			DeferredTarget = true;
			DisableMessage = false;
		}

		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.AnimalTaming].Callback = OnUse;
		}

		public static TimeSpan OnUse(Mobile m)
		{
			m.RevealingAction();

			if (!DisableMessage)
			{
				m.SendLocalizedMessage(502789); // Tame which animal?
			}

			if (DeferredTarget)
			{
				Timer.DelayCall(() => m.Target = new InternalTarget(m));
			}
			else
			{
				m.Target = new InternalTarget(m);
			}

			return TimeSpan.FromSeconds(40.0);
		}

		public static bool MustBeSubdued(BaseCreature bc)
		{
			if (bc.Owners.Count > 0)
			{
				return false;
			}
			return bc.SubdueBeforeTame && (bc.Hits > ((double)bc.HitsMax / 10));
		}

		public static void ScaleStats(BaseCreature bc)
		{
			// 강인함 1당 0.1% 보정 (400이면 1.4배, -400이면 0.6배)
			double scalar = 1.0 + (bc.Loyalty / 1000.0);
			if (scalar < 0.1)
				scalar = 0.1; // 최소치 방어

			if (bc.RawStr > 0)
				bc.RawStr = (int)Math.Max(1, bc.RawStr * scalar);
			if (bc.RawDex > 0)
				bc.RawDex = (int)Math.Max(1, bc.RawDex * scalar);
			if (bc.RawInt > 0)
				bc.RawInt = (int)Math.Max(1, bc.RawInt * scalar);

			if (bc.HitsMaxSeed > 0)
				bc.HitsMaxSeed = (int)Math.Max(1, bc.HitsMaxSeed * scalar);
			if (bc.StamMaxSeed > 0)
				bc.StamMaxSeed = (int)Math.Max(1, bc.StamMaxSeed * scalar);
			if (bc.ManaMaxSeed > 0)
				bc.ManaMaxSeed = (int)Math.Max(1, bc.ManaMaxSeed * scalar);

			bc.Hits = bc.HitsMax;
			bc.Stam = bc.StamMax;
			bc.Mana = bc.ManaMax;
		}

		// [수정] 기획 반영: 스킬은 고정 (기존 scalar 로직 제거)
		public static void ScaleSkills(BaseCreature bc, double scalar, bool firstTame)
		{
			// 기획에 따라 스킬은 성장 및 변동이 없으므로 아무 작업도 하지 않음
			return;
		}

		public static void ScaleSkills(BaseCreature bc, double scalar, double capScalar, bool firstTame)
		{
			// 기획에 따라 스킬은 성장 및 변동이 없으므로 아무 작업도 하지 않음
			return;
			for (int i = 0; i < bc.Skills.Length; ++i)
			{
				if (!Core.TOL || firstTame)
				{
					bc.Skills[i].Cap = Math.Max(100.0, bc.Skills[i].Base * capScalar);
				}
				bc.Skills[i].Base *= scalar;
				if (bc.Skills[i].Base > bc.Skills[i].Cap)
				{
					bc.Skills[i].Cap = bc.Skills[i].Base;
				}
			}
		}

		private class InternalTarget : Target
		{
			private bool m_SetSkillTime = true;

			public InternalTarget(Mobile m)
				: base(Core.AOS ? 3 : 2, false, TargetFlags.None)
			{
				BeginTimeout(m, TimeSpan.FromSeconds(30.0));
			}

			protected override void OnTargetFinish(Mobile from)
			{
				if (m_SetSkillTime)
				{
					from.NextSkillTime = Core.TickCount;
				}
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				from.RevealingAction();

				if (targeted is Mobile)
				{
					if (targeted is BaseCreature)
					{
						BaseCreature creature = (BaseCreature)targeted;

						if (!creature.Tamable || creature.Region is DungeonRegion)
						{
							creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1049655, from.NetState);
						}
						else if (creature.Controlled)
						{
							creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502804, from.NetState);
						}
						else if (from.Followers + creature.ControlSlots > from.FollowersMax)
						{
							from.SendLocalizedMessage(1049611);
						}
						else if (creature.Owners.Count >= BaseCreature.MaxOwners && !creature.Owners.Contains(from))
						{
							creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1005615, from.NetState);
						}
						else if (MustBeSubdued(creature))
						{
							creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1054025, from.NetState);
						}
						else if (
							DarkWolfFamiliar.CheckMastery(from, creature)
							|| from.Skills[SkillName.AnimalTaming].Value >= creature.CurrentTameSkill
						)
						{
							if (m_BeingTamed.Contains(targeted))
							{
								creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502802, from.NetState);
							}
							else if (creature.CanAngerOnTame && 0.95 >= Utility.RandomDouble())
							{
								creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502805, from.NetState);
								creature.PlaySound(creature.GetAngerSound());
								creature.Direction = creature.GetDirectionTo(from);
								if (from is PlayerMobile)
									creature.Combatant = from;
							}
							else
							{
								m_SetSkillTime = false;
								m_BeingTamed[targeted] = from;

								from.LocalOverheadMessage(MessageType.Emote, 0x59, 1010597); // You start to tame...

								new InternalTimer(from, creature).Start();
							}
						}
						else
						{
							creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502806, from.NetState);
						}
					}
				}
			}

			private class InternalTimer : Timer
			{
				private readonly Mobile m_Tamer;
				private readonly BaseCreature m_Creature;
				private readonly DateTime m_StartTime;
				private readonly int m_MaxSuccessRequired;
				private int m_CurrentSuccessCount = 0;
				private bool m_Paralyzed;

				public InternalTimer(Mobile tamer, BaseCreature creature)
					: base(TimeSpan.FromSeconds(3.0), TimeSpan.FromSeconds(3.0))
				{
					m_Tamer = tamer;
					m_Creature = creature;
					m_Paralyzed = creature.Paralyzed;
					m_StartTime = DateTime.UtcNow;

					// [기획] 필요 성공 횟수 설정 (스킬 50당 1회 추가)
					int req = 1 + (int)(creature.CurrentTameSkill / 50);
					m_MaxSuccessRequired = req;

					// [보너스] 150레벨 이상 시 1단계를 자동으로 통과 (단, 최소 1회 시도는 고정)
					if (tamer.Skills[SkillName.AnimalTaming].Value >= 150.0 && req > 1)
					{
						m_CurrentSuccessCount = 1;
						//tamer.SendMessage(0x35, "조련의 거장으로서 동물의 경계를 즉시 무너뜨리고 교감을 시작합니다.");
					}

					Priority = TimerPriority.TwoFiftyMS;
				}

				protected override void OnTick()
				{
					DamageEntry de = m_Creature.FindMostRecentDamageEntry(false);
					bool alreadyOwned = m_Creature.Owners.Contains(m_Tamer);
					double minSkill = m_Creature.CurrentTameSkill + (m_Creature.Owners.Count * 6.0);

					if (!m_Tamer.InRange(m_Creature, Core.AOS ? 7 : 6))
					{
						StopAndRemove(502795);
						return;
					}
					else if (!m_Tamer.CheckAlive())
					{
						StopAndRemove(502796);
						return;
					}
					else if (m_Creature.Controlled || !m_Creature.Tamable)
					{
						StopAndRemove(502804);
						return;
					}
					else if (de != null && de.LastDamage > m_StartTime)
					{
						StopAndRemove(502794);
						return;
					}

					m_Tamer.RevealingAction();

					double tamingSkillValue = m_Tamer.Skills[SkillName.AnimalTaming].Value;
					double baseChance = 80.0 + (tamingSkillValue - minSkill);
					double currentChance = baseChance / Math.Pow(2, m_CurrentSuccessCount);
					double critChance = m_Tamer.Int * 0.01;

					bool isSuccess = false;
					bool isCritical = false;

					if (Utility.RandomDouble() * 100 < critChance)
					{
						isSuccess = true;
						isCritical = true;
					}
					else if (Utility.RandomDouble() * 100 < currentChance)
					{
						isSuccess = true;
					}

					if (isSuccess)
					{
						// [Cliloc 적용] 0~5 인덱스 범위 내에서 현재 단계 메시지 출력
						int stepIdx = Math.Min(m_CurrentSuccessCount, 5);
						int clilocNum = isCritical ? (1080907 + stepIdx) : (1080901 + stepIdx);
						int hue = isCritical ? 0x35 : 0x59;

						m_Tamer.PublicOverheadMessage(MessageType.Regular, hue, clilocNum);

						m_CurrentSuccessCount++;

						// 경험치 획득 로직
						double expGain = minSkill * (1.1 - (currentChance * 0.01));
						m_Tamer.CheckSkill(SkillName.AnimalTaming, expGain);

						if (!alreadyOwned)
							m_Tamer.CheckTargetSkill(SkillName.AnimalLore, m_Creature, 0.0, 200.0);

						if (m_Creature.Paralyzed)
							m_Paralyzed = true;

						if (m_CurrentSuccessCount >= m_MaxSuccessRequired)
						{
							CompleteTaming(minSkill, alreadyOwned);
						}
					}
					else
					{
						// [실패 Cliloc 적용] 1080919 ~ 1080924
						int failIdx = Math.Min(m_CurrentSuccessCount, 5);
						int failCliloc = 1080919 + failIdx;
						m_Creature.PrivateOverheadMessage(MessageType.Regular, 0x22, failCliloc, m_Tamer.NetState);
						StopAndRemove(-1);
					}
				}

				private void CompleteTaming(double minSkill, bool alreadyOwned)
				{
					m_Tamer.NextSkillTime = Core.TickCount;
					m_BeingTamed.Remove(m_Creature);

					m_Creature.Loyalty = -Math.Abs(m_Creature.Loyalty);
					AnimalTaming.ScaleStats(m_Creature);

					/*
					if (m_Creature.Owners.Count == 0)
					{
						if (m_Paralyzed) ScaleSkills(m_Creature, 0.86, true);
						else ScaleSkills(m_Creature, 0.90, true);
					}
					else
					{
						ScaleSkills(m_Creature, 0.90, false);
					}
					*/
					// 최종 성공 Cliloc (1080913)
					m_Creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1080913, m_Tamer.NetState);

					m_Creature.SetControlMaster(m_Tamer);

					// [스킬 보너스 100레벨 고려] 50레벨 이상이면 즉시 본디드 (유저님 기획)
					if (m_Tamer.Skills[SkillName.AnimalTaming].Value >= 100.0)
						m_Creature.IsBonded = true;

					m_Creature.OnAfterTame(m_Tamer);

					if (!m_Creature.Owners.Contains(m_Tamer))
						m_Creature.Owners.Add(m_Tamer);

					EventSink.InvokeTameCreature(new TameCreatureEventArgs(m_Tamer, m_Creature));
					Stop();
				}

				private void StopAndRemove(int messageNum)
				{
					if (messageNum != -1)
						m_Creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, messageNum, m_Tamer.NetState);

					m_BeingTamed.Remove(m_Creature);
					m_Tamer.NextSkillTime = Core.TickCount;
					Stop();
				}

				private bool CanPath()
				{
					IPoint3D p = m_Tamer;
					if (p == null)
						return false;
					if (m_Creature.InRange(new Point3D(p), 1))
						return true;
					MovementPath path = new MovementPath(m_Creature, new Point3D(p));
					return path.Success;
				}
			}
		}
	}
}
