using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class OrcBrute : BaseCreature
    {
        [Constructable]
        public OrcBrute()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 189;

            Name = "an orc brute";
            BaseSoundID = 0x45A;

			Boss = true;

			/* [Orc Dungeon Level 2 Boss - Orc Brute - Fame 24,000 / Weight 1.28]
			   - 컨셉: 무식한 힘의 상징 (물리 파괴형)
			   - VirtualArmor: (24,000/1000) + 1 = 25 (단단한 피부 보정)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 20,700 (오우거 로드에 버금가는 파워)
			this.SetStr(17300, 17700); 

			// 최종 Hits 약 460,000 (민맥 편차 2,000 고정)
			this.SetHits(387000, 389000); 

			// 최종 Dex/Int 약 4,100
			this.SetDex(3450, 3550);
			this.SetInt(3450, 3550);

			// 최종 Stam/Mana 약 4,350
			this.SetStam(3650, 3750);
			this.SetMana(3650, 3750);

			SetAttackSpeed(3.0);
			SetDamage(90, 130);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 65, 75); // 강철 같은 육체
			this.SetResistance(ResistanceType.Fire, 40, 50);
			this.SetResistance(ResistanceType.Cold, 40, 50);
			this.SetResistance(ResistanceType.Poison, 50, 60);
			this.SetResistance(ResistanceType.Energy, 30, 40);    // 마법 저항력은 상대적 취약

			// [Skills] 최종 276.4 부근
			this.SetSkill(SkillName.Wrestling, 166.0, 171.0);
			this.SetSkill(SkillName.Tactics, 166.0, 171.0);
			this.SetSkill(SkillName.Anatomy, 166.0, 171.0);
			this.SetSkill(SkillName.MagicResist, 140.0, 150.0);

			// 가방 방어력: (24,000/1000) + 1 = 25
			this.VirtualArmor = 25;

			this.Fame = 24000;
			this.Karma = -24000;
        }

        public OrcBrute(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 2;
            }
        }

        public override TribeType Tribe { get { return TribeType.Orc; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.SavagesAndOrcs;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override bool AutoDispel
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Rich);
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
                return false;

            return base.IsEnemy(m);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            base.AggressiveAction(aggressor, criminal);

            Item item = aggressor.FindItemOnLayer(Layer.Helm);

            if (item is OrcishKinMask)
            {
                AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
                item.Delete();
                aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                aggressor.PlaySound(0x307);
            }
        }

        public override void OnDamagedBySpell(Mobile caster)
        {
            if (caster == this || Controlled || Summoned)
                return;

            SpawnOrcLord(caster);
        }
		public int orcs = 0;
		public bool regenBonus = false;
        public void SpawnOrcLord(Mobile target)
        {
            Map map = target.Map;

            if (map == null)
                return;


            IPooledEnumerable eable = GetMobilesInRange(10);

            foreach (Mobile m in eable)
            {
                if (m is OrcishLord)
                    ++orcs;
            }

            eable.Free();

            if (orcs < 10)
            {
                BaseCreature orc = new SpawnedOrcishLord();

                orc.Team = Team;

                Point3D loc = target.Location;
                bool validLocation = false;

                for (int j = 0; !validLocation && j < 10; ++j)
                {
                    int x = target.X + Utility.Random(3) - 1;
                    int y = target.Y + Utility.Random(3) - 1;
                    int z = map.GetAverageZ(x, y);

                    if (validLocation = map.CanFit(x, y, Z, 16, false, false))
                        loc = new Point3D(x, y, Z);
                    else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                        loc = new Point3D(x, y, z);
                }

                orc.MoveToWorld(loc, map);

                orc.Combatant = target;
            }
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
