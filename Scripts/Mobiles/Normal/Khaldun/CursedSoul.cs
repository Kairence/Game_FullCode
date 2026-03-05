using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Samurai
{
    [CorpseName("a cursed soul corpse")]
    public class CursedSoul : BaseCreature
    {
        [Constructable]
        public CursedSoul()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a cursed soul";
            Body = 3;
            BaseSoundID = 471;

			/* [Khaldun Cursed Soul - Fame 6,000 / Khaldun / Weight 1.17]
			   - 스킬 200 마스터 서버용 '중급 영체' 밸런스 적용
			   - 카르마 보정: 명성(6,000) + 1,800 보정 = -7,800
			   - 가상 방어력(VirtualArmor): (6,000/1000) - 1.0 = 5 (희미한 실체)
			   -------------------------------------------------- */

			// [Attributes] 명성 6,000 보너스 + 가중치 1.17 반영
			this.SetStr(85, 100); 
			this.SetHits(1900, 2150); 
			this.SetDex(15, 25);
			this.SetInt(15, 25);

			// [Combat Options]
			this.SetDamage(20, 35);
			this.SetAttackSpeed(2.0);

			// [Damage Types] 100% 에너지 (순수한 원한의 파동)
			this.SetDamageType(ResistanceType.Physical, 0);
			this.SetDamageType(ResistanceType.Energy, 100);

			// [Resistances] 영체 특성 (물리 무시, 마법 저항 특화)
			this.SetResistance(ResistanceType.Physical, 15, 25); // 실체가 없어 물리 방어는 취약
			this.SetResistance(ResistanceType.Fire, 20, 30);
			this.SetResistance(ResistanceType.Cold, 40, 50);
			this.SetResistance(ResistanceType.Poison, 75);     // 독 면역
			this.SetResistance(ResistanceType.Energy, 60, 75);  // 에너지 흡수

			// [Skills] ★ 스킬 200 서버 기준 - 중급 유저용 수련 대상 (재설계)
			// 유저 스킬 70 ~ 90 구간 사냥에 최적화
			this.SetSkill(SkillName.Wrestling, 60.0, 75.0); 
			this.SetSkill(SkillName.Tactics, 60.0, 75.0);
			this.SetSkill(SkillName.Anatomy, 50.0, 65.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 120.0); // 칼둔 원혼답게 마법은 안 통함

			// [Misc]
			this.VirtualArmor = 5;

			this.Fame = 6000;
			this.Karma = -7800; // 칼둔 보정 적용 (-6,000 - 1,800)

            PackBodyPartOrBones();
        }

        public CursedSoul(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}