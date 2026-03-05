using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a crystal elemental corpse")]
    public class CrystalElemental : BaseCreature
    {
        [Constructable]
        public CrystalElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a crystal elemental";
            Body = 300;
            BaseSoundID = 278;

			/* [Crystal Elemental - Fame 15,000 / Normal / Weight 1.25]
			   - 빛의 프리즘 던전 상급 정령
			   - 결정체 구조: 높은 물리/에너지 저항, 부식(독)과 화염에 취약
			   - 원소 생명체: 테이밍 불가 (200 숙련도 고려)
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 정예)

			// [Attributes] (기본 보너스 * 1배 * 1.25) - 기본 보너스
			// Str: 보너스 약 1,400 -> 최종 Set 약 350-450
			this.SetStr(350, 450); 

			// Hits: 보너스 약 37,500 -> 최종 Set 약 9,000-10,500
			this.SetHits(9000, 10500); 

			this.SetDex(140, 160); // 수정 조각처럼 빠른 움직임
			this.SetInt(150, 250); 

			// [Combat Options] 물리와 에너지의 날카로운 복합 타격
			this.SetDamage(40, 65);
			this.SetAttackSpeed(2.2);
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Energy, 50); // 빛의 에너지를 방출

			// [Resistances] 결정체 컨셉 (물리/에너지 방어 특화, 독/화염 취약)
			this.SetResistance(ResistanceType.Physical, 65, 75); // 단단한 결정 피부
			this.SetResistance(ResistanceType.Fire, 25, 35);      // ★ 고열에 의한 균열(약점)
			this.SetResistance(ResistanceType.Cold, 50, 60);    
			this.SetResistance(ResistanceType.Poison, 15, 25);    // ★ 틈새로 스며드는 독에 매우 취약
			this.SetResistance(ResistanceType.Energy, 70, 75);   // ★ 에너지 흡수 및 굴절

			// [Skills] 정령 특유의 정교한 전투 능력
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 115.0, 130.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 115.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 20;

			this.Fame = 15000;
			this.Karma = -15000;

            SetWeaponAbility(WeaponAbility.ParalyzingBlow);
        }

        public CrystalElemental(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
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