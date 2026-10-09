using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a fetid essence corpse")]
    public class FetidEssence : BaseCreature
    {
        [Constructable]
        public FetidEssence()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a fetid essence";
            Body = 273;

			/* [Fetid Essence - Fame 13,000 / Normal / Weight 1.26]
			   - 빛의 프리즘 던전 오염 구역 정령
			   - 부패 컨셉: 치명적인 독 공격, 높은 독/냉기 저항, 화염에 매우 취약
			   - 비정형 액체: 테이밍 불가 (200 숙련도 고려)
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 정예)

			// [Attributes] (기본 보너스 * 1배 * 1.26) - 기본 보너스
			// Str: 보너스 약 1,170 -> 최종 Set 약 300-350
			this.SetStr(300, 350); 

			// Hits: 보너스 약 31,000 -> 최종 Set 약 7,500-9,000
			this.SetHits(7500, 9000); 

			this.SetDex(140, 160); // 흐물거리는 유연한 움직임
			this.SetInt(250, 350); 

			SetAttackSpeed(10.0);
			SetDamage(18, 28);
			this.SetDamageType(ResistanceType.Poison, 80); // ★ 주 대미지원이 독
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] 오물/액체 컨셉 (독/냉기 특화, 화염/물리 취약)
			this.SetResistance(ResistanceType.Physical, 35, 45); // 액체라 물리 타격엔 약함
			this.SetResistance(ResistanceType.Fire, 10, 20);      // ★ 메탄가스/오염물질로 인해 화염에 매우 취약(약점)
			this.SetResistance(ResistanceType.Cold, 60, 75);    // 얼어붙지 않는 오물
			this.SetResistance(ResistanceType.Poison, 75, 75);  // ★ 독 면역 수준 (75% 캡 적용)
			this.SetResistance(ResistanceType.Energy, 40, 50);

			// [Skills] 독소 방출 및 마법 능력
			this.SetSkill(SkillName.Wrestling, 110.0, 120.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 120.0);
			this.SetSkill(SkillName.Poisoning, 120.0, 140.0);   // 치명적인 독 부여
			this.SetSkill(SkillName.MagicResist, 110.0, 125.0);
			this.SetSkill(SkillName.Magery, 100.0, 115.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 10;

			this.Fame = 13000;
			this.Karma = -13000;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }

            SetAreaEffect(AreaEffect.EssenceOfDisease);
        }

        public FetidEssence(Serial serial)
            : base(serial)
        {
        }

        public override Poison HitPoison
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override void GenerateLoot() // Need to verify
        {
            AddLoot(LootPack.FilthyRich);
        }

        public override int GetAngerSound()
        {
            return 0x56d;
        }

        public override int GetIdleSound()
        {
            return 0x56b;
        }

        public override int GetAttackSound()
        {
            return 0x56c;
        }

        public override int GetHurtSound()
        {
            return 0x56c;
        }

        public override int GetDeathSound()
        {
            return 0x56e;
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
