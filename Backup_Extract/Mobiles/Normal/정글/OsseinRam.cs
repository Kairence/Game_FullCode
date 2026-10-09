using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ossein ram corpse")]
    public class OsseinRam : BaseCreature
    {
        [Constructable]
        public OsseinRam() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Ossein Ram";
            Body = 0x591;
            Female = true;

			/* [Ossein Ram - Normal - Fame 18,000 / Weight 1.25]
			   - 정글 던전의 언데드 돌진수 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 23 (명성/1000 + 5 보정)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 1.2만 대)
			this.SetStr(530, 550); 
			this.SetHits(11800, 12100); 
			this.SetDex(100, 115);
			this.SetInt(100, 115);

			// [Combat Options] 물리 100% (뼈 뿔을 이용한 강력한 들이받기)
			this.SetDamage(45, 75);
			this.SetAttackSpeed(2.6); // 돌진형이라 공속은 다소 느리지만 강력함
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 및 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); // 단단한 뼈의 저항
			this.SetResistance(ResistanceType.Fire, 25, 35);      // ★ 확실한 약점 1
			this.SetResistance(ResistanceType.Cold, 70, 75);     // 냉기에 강함
			this.SetResistance(ResistanceType.Poison, 70, 75);   // 언데드 특효 내성
			this.SetResistance(ResistanceType.Energy, 30, 40);   // ★ 확실한 약점 2 (전격에 바스러짐)

			// [Skills] 기본 115~125에 역산 보너스(18) 가산
			this.SetSkill(SkillName.Wrestling, 133.0, 143.0); 
			this.SetSkill(SkillName.Tactics, 133.0, 143.0);
			this.SetSkill(SkillName.Anatomy, 133.0, 143.0);
			this.SetSkill(SkillName.MagicResist, 115.0, 130.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; // 200 숙련도 시대의 단단한 2슬롯 언데드 펫
			this.MinTameSkill = 155.4; // 정예급 난이도
			this.VirtualArmor = 23;
			this.Fame = 18000;
			this.Karma = -18000;
            SetMagicalAbility(MagicalAbility.BattleDefense);
            SetWeaponAbility(WeaponAbility.MortalStrike);
            SetSpecialAbility(SpecialAbility.LifeLeech);
        }

        public override int Meat { get { return 3; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }
        public override bool CanAngerOnTame { get { return true; } }
        public override bool StatLossAfterTame { get { return true; } }

        public OsseinRam(Serial serial) : base(serial)
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
