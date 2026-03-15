using System;
using Server.Items;
using System.Collections;

namespace Server.Mobiles
{
	[CorpseName( "a giant turkey corpse" )]
	public class GiantTurkey : BaseCreature
	{
		[Constructable]
		public GiantTurkey()
            : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a giant turkey";
			Body = 1026;
			BaseSoundID = 0x66A;

			Boss = true;

            /* [Covetous Level 1 Boss - Giant Turkey - Fame 6,000 / Weight 1.27]
			   - 컨셉: 분노한 거대 조류 (맷집형 탱커)
			   - VirtualArmor: (6,000/1000) + 4 = 10 (지방층 보정 +4)
			   - 편차 수정: 보스급 안정화 룰 적용 (편차 1,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 4,000
			this.SetStr(3200, 3500); 

			// 최종 Hits 약 76,000 (1층 보스치고는 꽤 든든한 맷집)
			this.SetHits(63500, 64500); 

			// 최종 Dex/Int 약 800
			this.SetDex(650, 700);
			this.SetInt(650, 700);

			// 최종 Stam/Mana 약 840 (지치지 않는 쪼기 공격)
			this.SetStam(680, 730);
			this.SetMana(680, 730);

			// [Combat Options]
			SetAttackSpeed(3.5);
			SetDamage(45, 65);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 50, 60);
			this.SetResistance(ResistanceType.Fire, 20, 30);      // 통구이가 되기 쉬운 속성
			this.SetResistance(ResistanceType.Cold, 40, 50);
			this.SetResistance(ResistanceType.Poison, 40, 50);
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] 최종 45.7 부근
			this.SetSkill(SkillName.Wrestling, 25.0, 30.0);
			this.SetSkill(SkillName.Tactics, 25.0, 30.0);
			this.SetSkill(SkillName.Anatomy, 25.0, 30.0);
			this.SetSkill(SkillName.MagicResist, 40.0, 50.0);

			// 가방 방어력: (6,000/1000) + 4 = 10
			this.VirtualArmor = 10;

			this.Fame = 6000;
			this.Karma = -6000;
			summoned = false;
        }

		private bool summoned = false;
		
        public override int Meat { get { return 10; } }
        public override MeatType MeatType { get { return MeatType.Bird; } }
        public override FoodType FavoriteFood { get { return FoodType.GrainsAndHay; } }
        public override int Feathers { get { return 200; } }

		/*
        public override void OnDamagedBySpell(Mobile caster)
        {
            if (caster == this || Controlled || Summoned || summoned )
                return;

            SpawnTurkey(caster);
        }
		
		public override void OnGotMeleeAttack(Mobile attacker)
		{
            if (attacker == this || Controlled || Summoned || summoned )
                return;

            SpawnTurkey(this);
		}
		
        public void SpawnTurkey(Mobile target)
        {
            Map map = target.Map;

            if (map == null)
                return;

			summoned = true;
			
			for( int i = 0; i < 10; ++i)
			{
                BaseCreature turkey = new SummonedTurkey();

                turkey.Team = Team;

				turkey.Home = Home;
				
                Point3D loc = target.Location;
                bool validLocation = false;

                for (int j = 0; !validLocation && j < 10; ++j)
                {
                    int x = target.X + Utility.Random(10) - 1;
                    int y = target.Y + Utility.Random(10) - 1;
                    int z = map.GetAverageZ(x, y);

                    if (validLocation = map.CanFit(x, y, Z, 16, false, false))
                        loc = new Point3D(x, y, Z);
                    else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                        loc = new Point3D(x, y, z);

					turkey.MoveToWorld(loc, map);
					turkey.Combatant = target;
				}
			}
		}
		*/		
        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich);
        }

        public override int GetIdleSound()
        {
            return 0x66A;
        }

        public override int GetAngerSound()
        {
            return 0x66A;
        }

        public override int GetHurtSound()
        {
            return 0x66B;
        }

        public override int GetDeathSound()
        {
            return 0x66B;
        }

		public GiantTurkey(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
			writer.Write((bool) summoned );
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			summoned = reader.ReadBool();
		}
	}
}
