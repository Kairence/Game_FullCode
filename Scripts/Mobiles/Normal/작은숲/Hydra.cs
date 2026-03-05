using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a hydra corpse")]
    public class Hydra : BaseCreature
    {
        [Constructable]
        public Hydra()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a hydra";
            Body = 0x109;
            BaseSoundID = 0x16A;

			/* [Hydra - Normal - Fame 18,000 / Weight 1.25]
			   - 작은 숲 던전 상급 포식자 / 일반 몬스터 상향 버전
			   - 배수: 1x (Normal)
			   - VirtualArmor: 23 (기본 18 + 보정 5)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (중상급 던전 사양)
			this.SetStr(530, 560); 
			this.SetHits(11800, 12200); 
			this.SetDex(100, 120);
			this.SetInt(100, 120);

			// [Combat Options] 5속성 복합 대미지 (전방위 압박)
			this.SetDamage(45, 75);
			this.SetAttackSpeed(2.4);
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Fire, 20);
			this.SetDamageType(ResistanceType.Cold, 20);
			this.SetDamageType(ResistanceType.Poison, 20);
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] 최고 저항 75 이하 준수 / 냉기 약점 유지
			this.SetResistance(ResistanceType.Physical, 55, 65); 
			this.SetResistance(ResistanceType.Fire, 55, 65);      
			this.SetResistance(ResistanceType.Cold, 30, 40);    // ★ 확실한 공략 포인트
			this.SetResistance(ResistanceType.Poison, 60, 70); 
			this.SetResistance(ResistanceType.Energy, 50, 60);   

			// [Skills] 기본 115~125에 역산 보너스(18) 가산
			this.SetSkill(SkillName.Wrestling, 133.0, 143.0); 
			this.SetSkill(SkillName.Tactics, 133.0, 143.0);
			this.SetSkill(SkillName.Anatomy, 133.0, 143.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 4; // 명성 18000에 걸맞은 위력
			this.MinTameSkill = 120.0; // 테이밍 200 시대의 준고급 펫
			this.VirtualArmor = 23;
			this.Fame = 18000;
			this.Karma = -18000;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public Hydra(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get
            {
                return 40;
            }
        }
        public override int Meat
        {
            get
            {
                return 19;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.AosUltraRich, 3);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);		
			
            c.DropItem(new HydraScale());				
			
            if (Utility.RandomDouble() < 0.2)				
                c.DropItem(new ParrotItem());
				
            if (Utility.RandomDouble() < 0.05)				
                c.DropItem(new ThorvaldsMedallion());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
			
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
			
            int version = reader.ReadInt();
        }
    }
}
