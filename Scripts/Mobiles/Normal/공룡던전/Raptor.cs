using System;
using System.Collections.Generic;
using Server;
using Server.Items;
namespace Server.Mobiles
{
	[CorpseName("a raptor corpse")]
	public class Raptor : BaseCreature
	{
		private const int MaxFriends = 2;

		private bool m_IsFriend;
		private List<Mobile> m_Friends = new List<Mobile>();
		private InternalTimer m_FriendsTimer;

		[Constructable]
		public Raptor()
			: this(false)
		{
		}

		[Constructable]
		public Raptor(bool isFriend)
			: base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.175, 0.350)
		{
			m_IsFriend = isFriend;

			Name = "a raptor";
			Body = 730;

			/* [Raptor - Fame 5,500 / Dinosaur / Weight 1.24]
			   - 스킬 200 마스터 서버용 '중상급 민첩형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (5,500/1000) + 2.5 = 8
			   - 테이밍 난이도: 80.0 ~ 90.0 (공룡 테이머의 주력 유닛)
			   -------------------------------------------------- */

			// [Attributes] 명성 5,500 보너스 + 가중치 1.24 반영
			this.SetStr(100, 130); 
			this.SetHits(2200, 2800); 
			this.SetDex(20, 30);
			this.SetInt(20, 30);

			// [Combat Options] 찢고 발기는 갈고리 발톱
			this.SetDamage(25, 40);
			this.SetAttackSpeed(1.8); // 매우 빠른 공격 속도 (연타 컨셉)

			// [Damage Types] 90% 물리 + 10% 출혈(Energy 속성 대체)
			this.SetDamageType(ResistanceType.Physical, 90);
			this.SetDamageType(ResistanceType.Energy, 10);

			// [Resistances] 질긴 가죽과 야성 (최대 저항 75% 캡 준수)
			this.SetResistance(ResistanceType.Physical, 45, 55); 
			this.SetResistance(ResistanceType.Fire, 30, 40);      
			this.SetResistance(ResistanceType.Cold, 30, 40);    
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] 유저 스킬 90 ~ 120 구간 (GM 이상 유저용)
			this.SetSkill(SkillName.Wrestling, 90.0, 110.0); 
			this.SetSkill(SkillName.Tactics, 90.0, 110.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 120.0);    // 치명타 유도
			this.SetSkill(SkillName.MagicResist, 80.0, 100.0);

			// [Taming & Food] ★ 가상 방어구 상단 배치

			this.ControlSlots = 2; // 강력한 개체이므로 2슬롯 점유
			this.MinTameSkill = 80.0; // 랩터 군단을 부리기 위한 숙련된 테이머의 기준

			// [Misc]
			this.VirtualArmor = 8;

			this.Fame = 5500;
			this.Karma = -5500;

			Tamable = !isFriend;


            SetWeaponAbility(WeaponAbility.BleedAttack);
		}
		
		public override int TreasureMapLevel { get { return 3; } }

		public override int Meat
		{
			get { return 7; }
		}

		public override int Hides
		{
			get { return 11; }
		}

		public override HideType HideType
		{
			get { return HideType.Horned; }
		}

		public override PackInstinct PackInstinct
		{
			get { return PackInstinct.Ostard; }
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Rich, 2);
		}

		public override int GetIdleSound()
		{
			return 1573;
		}

		public override int GetAngerSound()
		{
			return 1570;
		}

		public override int GetHurtSound()
		{
			return 1572;
		}

		public override int GetDeathSound()
		{
			return 1571;
		}


		public override void OnCombatantChange()
		{
			if (!m_IsFriend && !Controlled && Combatant != null && m_FriendsTimer == null)
			{
				m_FriendsTimer = new InternalTimer(this);
				m_FriendsTimer.Start();
			}
		}

		public void CheckFriends()
		{
			if (!Alive || Combatant == null || Controlled || Map == null || Map == Map.Internal)
			{
				m_Friends.ForEach(f => f.Delete());
				m_Friends.Clear();

				m_FriendsTimer.Stop();
				m_FriendsTimer = null;
			}
			else
			{
				int count = 0;

				for (int i = 0; i < m_Friends.Count; i++)
				{
					// remove dead friends

					Mobile friend = m_Friends[i];

					if (friend == null || friend.Deleted)
						m_Friends.Remove(friend);
					else
						count++;
				}

				for (int i = count; i < MaxFriends; i++)
				{
					// spawn new friends

					BaseCreature friend = new Raptor(true);
					var loc = Location;
					var validLocation = false;
					for (var j = 0; !validLocation && j < 10; ++j)
					{
						var x = X + Utility.Random(3) - 1;
						var y = Y + Utility.Random(3) - 1;
						var z = Map.GetAverageZ(x, y);

						if (validLocation = Map.CanFit(x, y, Z, 16, false, false))
							loc = new Point3D(x, y, Z);
						else if (validLocation = Map.CanFit(x, y, z, 16, false, false))
							loc = new Point3D(x, y, z);
					}

					friend.MoveToWorld(loc, Map);
					friend.Combatant = Combatant;

					if (friend.AIObject != null)
						friend.AIObject.Action = ActionType.Combat;

					m_Friends.Add(friend);
				}
			}
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			if (!Controlled && Utility.RandomDouble() < 0.25)
			{
				c.DropItem(new AncientPotteryFragments());
			}
            
            if (!Controlled && Utility.RandomDouble() <= 0.005)
			{
				c.DropItem(new RaptorClaw());
			}
		}

		public Raptor(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)2);

			writer.Write((bool)m_IsFriend);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if (version > 0)
				m_IsFriend = reader.ReadBool();

            if(version == 1)
                SetWeaponAbility(WeaponAbility.BleedAttack);

			if (m_IsFriend)
				Delete();
		}

		private class InternalTimer : Timer
		{
			private Raptor m_Owner;

			public InternalTimer(Raptor owner)
				: base(TimeSpan.Zero, TimeSpan.FromSeconds(30.0))
			{
				m_Owner = owner;
			}

			protected override void OnTick()
			{
				m_Owner.CheckFriends();
			}
		}
	}
}
