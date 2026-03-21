using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a plant corpse")]
    public class BogThing : BaseCreature
    {
        [Constructable]
        public BogThing()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.6, 1.2)
        {
            this.Name = "a bog thing";
            this.Body = 780;

			Boss = true;

			/* [Orc Dungeon Level 1 Boss - Bog Thing - Fame 12,500 / Weight 1.22]
			   - 컨셉: 늪지의 파수꾼 (생명력/방어 특화)
			   - VirtualArmor: (12,500/1000) + 3 = 15 (진흙 껍질 보정 +3)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 8,100
			this.SetStr(6600, 6900); 

			// 최종 Hits 약 160,000 (민맥 편차 2,000 고정)
			this.SetHits(132600, 134600); 

			// 최종 Dex/Int 약 1,600 (느리지만 묵직한 움직임)
			this.SetDex(1300, 1400);
			this.SetInt(1300, 1400);

			// 최종 Stam/Mana 약 1,500
			this.SetStam(1200, 1300);
			this.SetMana(1200, 1300);

			// [Combat Options]
			SetAttackSpeed(4.5);
			SetDamage(60, 90);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 60, 70); // 진흙과 이끼로 덮인 외피
			this.SetResistance(ResistanceType.Fire, 15, 25);      // 약점: 화염 (수분이 증발하며 타격)
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 75);       // 늪지 생물 (독 면역 Max 75)
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] 최종 107.8 부근
			this.SetSkill(SkillName.Wrestling, 61.0, 66.0);
			this.SetSkill(SkillName.Tactics, 61.0, 66.0);
			this.SetSkill(SkillName.Anatomy, 61.0, 66.0);
			this.SetSkill(SkillName.MagicResist, 75.0, 85.0);

			// 가방 방어력: (12,500/1000) + 3 = 15
			this.VirtualArmor = 15;

			this.Fame = 12500;
			this.Karma = -12500;
        }

        public BogThing(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average, 2);
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

        public void SpawnBogling(Mobile m)
        {
            Map map = this.Map;

            if (map == null)
                return;

            SpawnedBogling spawned = new SpawnedBogling();

            spawned.Team = this.Team;

            bool validLocation = false;
            Point3D loc = this.Location;

            for (int j = 0; !validLocation && j < 10; ++j)
            {
                int x = this.X + Utility.Random(3) - 1;
                int y = this.Y + Utility.Random(3) - 1;
                int z = map.GetAverageZ(x, y);

                if (validLocation = map.CanFit(x, y, this.Z, 16, false, false))
                    loc = new Point3D(x, y, this.Z);
                else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                    loc = new Point3D(x, y, z);
            }

            spawned.MoveToWorld(loc, map);
            spawned.Combatant = m;
			spawned.Home = new Point3D( this.X, this.Y, this.Z );
			spawned.RangeHome = 20;
        }

        public void EatBoglings()
        {
            ArrayList toEat = new ArrayList();
            IPooledEnumerable eable = GetMobilesInRange(5);

            foreach (Mobile m in eable)
            {
                if (m is Bogling)
                    toEat.Add(m);
            }
            eable.Free();

            if (toEat.Count > 0)
            {
                this.PlaySound(Utility.Random(0x3B, 2)); // Eat sound

                foreach (Mobile m in toEat)
                {
                    this.Hits += (m.Hits / 2);
                    m.Delete();
                }
            }
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            if (this.Hits > (this.HitsMax / 4))
            {
                if (0.25 >= Utility.RandomDouble())
                    this.SpawnBogling(attacker);
            }
            else
            {
                this.EatBoglings();
            }
        }
    }
}