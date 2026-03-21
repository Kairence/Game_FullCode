using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a crystal vortex corpse")]
    public class CrystalVortex : BaseCreature
    {
        [Constructable]
        public CrystalVortex()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a crystal vortex";
            this.Body = 0xD;
            this.Hue = 0x2B2;
            this.BaseSoundID = 0x107;

			/* [Crystal Vortex - Fame 17,000 / Normal / Weight 1.28]
			   - 빛의 프리즘 던전 초정예 비정형 정령 (명성 17,000)
			   - 보텍스 컨셉: 초고속 공격(1.4), 물리/독 저항 극대화, 냉기 취약
			   - 비정형체: 테이밍 불가 (200 숙련도 고려)
			   -------------------------------------------------- */
			// Boss = true 삭제 (초정예 일반 몬스터)

			// [Attributes] (기본 보너스 * 1배 * 1.28) - 기본 보너스
			// Str: 보너스 약 2,000 -> 최종 Set 약 550-600
			this.SetStr(550, 600); 

			// Hits: 보너스 약 44,200 -> 최종 Set 약 12,000-13,500
			this.SetHits(12000, 13500); 

			this.SetDex(250, 300); // ★ 광풍 수준의 회전 속도
			this.SetInt(400, 500); // 폭풍 중심부에 응집된 거대 마력

			// [Combat Options] 쉴 새 없이 쏟아지는 날카로운 파편 타격
			this.SetDamage(45, 65);
			this.SetAttackSpeed(1.4); // ★ 일반 몬스터 중 최상급 공격 속도
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			// [Resistances] 비정형 수정체 컨셉 (물리/독/에너지 특화, 냉기 약점)
			this.SetResistance(ResistanceType.Physical, 70, 75); // 실체가 없어 물리 타격 무효화 수준
			this.SetResistance(ResistanceType.Fire, 45, 55);      
			this.SetResistance(ResistanceType.Cold, 20, 35);    // ★ 냉기에 의해 회전이 멈추고 구조가 붕괴됨 (치명적 약점)
			this.SetResistance(ResistanceType.Poison, 70, 75);  // 무기물 폭풍으로 독 완벽 면역 수준
			this.SetResistance(ResistanceType.Energy, 70, 75);  // 에너지를 흡수하여 회전 동력으로 사용

			// [Skills] 소용돌이치는 파괴적 기술 및 높은 저항
			this.SetSkill(SkillName.Wrestling, 115.0, 130.0); 
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 145.0);
			this.SetSkill(SkillName.Magery, 110.0, 125.0);
			this.SetSkill(SkillName.EvalInt, 110.0, 125.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 17;

			this.Fame = 17000;
			this.Karma = -17000;

            for (int i = 0; i < Utility.RandomMinMax(0, 2); i++)
            {
                this.PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }
        }

        public CrystalVortex(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich, 2);
            AddLoot( LootPack.Parrot );
            AddLoot(LootPack.MedScrolls);
            AddLoot(LootPack.HighScrolls);
        }

        public override void OnDeath( Container c )
        {
            base.OnDeath( c );

            if ( Utility.RandomDouble() < 0.75 )
            c.DropItem( new CrystallineFragments() );

            if ( Utility.RandomDouble() < 0.06 )
            c.DropItem( new JaggedCrystals() );
        }

        public override int GetAngerSound()
        {
            return 0x15;
        }

        public override int GetAttackSound()
        {
            return 0x28;
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