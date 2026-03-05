using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a crystal sea serpent corpse")]
    public class CrystalSeaSerpent : SeaSerpent
    {
        [Constructable]
        public CrystalSeaSerpent()
        {
            Name = "a crystal sea serpent";
            Hue = 0x47E;

			/* [Crystal Sea Serpent - Fame 12,000 / Normal / Weight 1.22]
			   - 빛의 프리즘 던전 수중 정예 야수
			   - 수정 비늘: 높은 냉기/에너지 저항, 화염/독에 매우 취약
			   - 야수형 괴수: 테이밍 불가 (200 숙련도 고려)
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 정예)

			// [Attributes] (기본 보너스 * 1배 * 1.22) - 기본 보너스
			// Str: 보너스 약 1,130 -> 최종 Set 약 250-350
			this.SetStr(250, 350); 

			// Hits: 보너스 약 27,950 -> 최종 Set 약 6,500-7,500
			this.SetHits(6500, 7500); 

			this.SetDex(150, 180); // 수중에서의 유연하고 빠른 움직임
			this.SetInt(100, 150); 

			// [Combat Options] 냉기와 물리 중심의 날카로운 타격
			this.SetDamage(35, 55);
			this.SetAttackSpeed(2.3);
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] 수중 결정체 컨셉 (냉기/에너지 특화, 화염/독 취약)
			this.SetResistance(ResistanceType.Physical, 50, 60); 
			this.SetResistance(ResistanceType.Fire, 10, 25);      // ★ 급격한 온도차에 의한 비늘 균열 (치명적 약점)
			this.SetResistance(ResistanceType.Cold, 70, 75);    // ★ 수중 생물 + 결정체로 냉기에 매우 강함
			this.SetResistance(ResistanceType.Poison, 25, 35);    // 유기체 성분이 섞여 있어 독에 취약
			this.SetResistance(ResistanceType.Energy, 60, 75);   // ★ 빛의 굴절로 에너지 마법 감쇄

			// [Skills] 야수다운 높은 근접 전투 능력
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 115.0, 130.0); // 수정 비늘의 마법 저항 효과

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 12;

			this.Fame = 12000;
			this.Karma = -12000;
        }

        public override void OnDeath( Container c )
        {
        base.OnDeath( c );

            if ( Utility.RandomDouble() < 0.05 )
            c.DropItem( new CrushedCrystals() );

            if ( Utility.RandomDouble() < 0.1 )
            c.DropItem( new IcyHeart() );

            if ( Utility.RandomDouble() < 0.1 )
            c.DropItem( new LuckyDagger() );
        }

        public override int TreasureMapLevel { get { return 3; } }
        public override int Meat{ get{ return 10; } }
        public override int Hides{ get{ return 11; } }
        public override HideType HideType{ get{ return HideType.Horned; } }
        public override int Scales{ get{ return 8; } }
        public override ScaleType ScaleType{ get{ return ScaleType.Blue; } } 

        public CrystalSeaSerpent(Serial serial)
            : base(serial)
        {
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
