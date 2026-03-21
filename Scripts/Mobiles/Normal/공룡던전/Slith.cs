using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a slith corpse")]
    public class Slith : BaseCreature
    {
        [Constructable]
        public Slith() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a slith";
            Body = 734;

			/* [Slith - Fame 3,500 / Dinosaur / Weight 1.18]
			   - 스킬 200 마스터 서버용 '중급 공격형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (3,500/1000) + 1.5 = 5
			   - 테이밍 불가능 (야생 개체)
			   -------------------------------------------------- */

			// [Attributes] 명성 3,500 보너스 + 가중치 1.18 반영
			this.SetStr(45, 60); 
			this.SetHits(1000, 1300); 
			this.SetDex(10, 15);
			this.SetInt(10, 15);

			// [Combat Options] 날카로운 이빨과 산성 침
			this.SetDamage(18, 30);
			this.SetAttackSpeed(2.4);

			// [Damage Types] 60% 물리 + 40% 독
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Poison, 40);

			// [Resistances] 파충류의 질긴 가죽 (최대 저항 75% 캡 준수)
			this.SetResistance(ResistanceType.Physical, 35, 45); 
			this.SetResistance(ResistanceType.Fire, 20, 30);      
			this.SetResistance(ResistanceType.Cold, 30, 40);    
			this.SetResistance(ResistanceType.Poison, 50, 60); 
			this.SetResistance(ResistanceType.Energy, 20, 30);

			// [Skills] 유저 스킬 70 ~ 100 구간 수련 최적화
			this.SetSkill(SkillName.Wrestling, 75.0, 95.0); 
			this.SetSkill(SkillName.Tactics, 75.0, 95.0);
			this.SetSkill(SkillName.MagicResist, 60.0, 80.0);
			this.SetSkill(SkillName.Poisoning, 80.0, 100.0); // 타격 시 중독 효과

			// [Taming] ★ 테이밍 불가능 설정
			this.Tamable = false;

			// [Misc]
			this.VirtualArmor = 5;

			this.Fame = 3500;
			this.Karma = -3500;

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public Slith(Serial serial) : base(serial)
        {
        }

        public override int DragonBlood { get { return 8; } }

		public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }
        public override int Meat
        {
            get { return 6; }
        }

        public override int Hides
        {
            get { return 10; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (!Controlled && Utility.RandomDouble() < 0.05)
            {
                c.DropItem(new SlithEye());
            }

            if (!Controlled && Utility.RandomDouble() < 0.25)
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        c.DropItem(new AncientPotteryFragments());
                        break;
                    case 1:
                        c.DropItem(new TatteredAncientScroll());
                        break;
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}
