using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a crystal hydra corpse")]
    public class CrystalHydra : BaseCreature
    {
        [Constructable]
        public CrystalHydra()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a crystal hydra";
            Body = 0x109;
            Hue = 0x47E;
            BaseSoundID = 0x16A;

			/* [Crystal Hydra - Fame 22,000 / Normal / Weight 1.30]
			   - 빛의 프리즘 던전 최상위 정예 괴수
			   - 다두룡 컨셉: 매우 높은 체력과 복합 속성 공격
			   - 수정 피부: 물리/에너지 저항 극대화, 화염/독 취약
			   -------------------------------------------------- */
			// Boss = true 삭제 (최상위 일반 정예)

			// [Attributes] (기본 보너스 * 1배 * 1.30) - 기본 보너스
			// Str: 보너스 약 2,860 -> 최종 Set 약 750-850
			this.SetStr(750, 850); 

			// Hits: 보너스 약 63,400 -> 최종 Set 약 18,000-20,000
			// (Hits 5만 이상 룰에 따라 민맥 편차 2,000 이내 준수)
			this.SetHits(18000, 20000); 

			this.SetDex(100, 130); // 덩치가 커서 민첩성은 보통
			this.SetInt(100, 150); 

			// [Combat Options] 5속성 복합 브레스 및 강력한 물기
			this.SetDamage(60, 95);
			this.SetAttackSpeed(2.5);
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Fire, 20);
			this.SetDamageType(ResistanceType.Cold, 20);
			this.SetDamageType(ResistanceType.Poison, 20);
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] 수정 결정체 비늘 (물리/냉기/에너지 특화, 화염/독 약점)
			this.SetResistance(ResistanceType.Physical, 70, 75); // ★ 물리 방어력 최상급
			this.SetResistance(ResistanceType.Fire, 30, 45);      // ★ 고열 브레스에 의한 내부 균열(약점)
			this.SetResistance(ResistanceType.Cold, 65, 75);    
			this.SetResistance(ResistanceType.Poison, 35, 45);    // 수정 틈새로 침투하는 독에 취약
			this.SetResistance(ResistanceType.Energy, 70, 75);   // ★ 빛의 에너지 굴절/흡수

			// [Skills] 고대 야수다운 파괴적인 근접 능력
			this.SetSkill(SkillName.Wrestling, 120.0, 135.0); 
			this.SetSkill(SkillName.Tactics, 120.0, 135.0);
			this.SetSkill(SkillName.Anatomy, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 25;

			this.Fame = 22000;
			this.Karma = -22000;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }
		
        public CrystalHydra(Serial serial)
            : base(serial)
        {
        }
		
        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 2);
            AddLoot(LootPack.HighScrolls);
            AddLoot(LootPack.Parrot);
        }
		
        public override void OnDeath(Container c)
        {
            base.OnDeath(c);		
			
            if (Utility.RandomDouble() < 0.25)
                c.DropItem(new ShatteredCrystals());
				
            c.DropItem(new CrystallineFragments());
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
