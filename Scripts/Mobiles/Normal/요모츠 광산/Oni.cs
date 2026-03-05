using System;

namespace Server.Mobiles
{
    [CorpseName("an oni corpse")]
    public class Oni : BaseCreature
    {
        //private DateTime m_NextAbilityTime;

        [Constructable]
        public Oni()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an oni";
            Body = 241;

			/* [Oni - World Boss - Fame 28,000 / Weight 1.30]
			   - 성지 던전의 지옥 군주 / 보스급 배수 적용
			   - Attributes: x5 / Skills: x2 (Keep Formula)
			   - VirtualArmor: 30 (기본 28 + 보정 2)
			   -------------------------------------------------- */

			Boss = true;

			// 최종 Str 약 26,390 (보너스 포함)
			this.SetStr(22100, 22500); 

			// 최종 Hits 약 585,300 (민맥 편차 2,000 고정 룰)
			this.SetHits(494000, 496000); 

			// 최종 Dex/Int 약 5,278
			this.SetDex(4400, 4500);
			this.SetInt(4400, 4500);

			// [Combat Options] 물리 70% / 화염 30% (지옥의 일격)
			this.SetDamage(95, 135);
			this.SetAttackSpeed(2.0);
			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Fire, 30);

			// [Resistances] 최고 저항 75 이하 엄격 준수 (형님 지침)
			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 65, 75);      
			this.SetResistance(ResistanceType.Cold, 45, 55);    // ★ 명확한 약점 (빙결에 취약)
			this.SetResistance(ResistanceType.Poison, 60, 70); 
			this.SetResistance(ResistanceType.Energy, 50, 60);   

			// [Skills] 최종 숙련도 약 351 (서버 캡 200에 맞춰 설정)
			this.SetSkill(SkillName.Wrestling, 190.0, 200.0); 
			this.SetSkill(SkillName.Tactics, 190.0, 200.0);
			this.SetSkill(SkillName.Anatomy, 190.0, 200.0);
			this.SetSkill(SkillName.Magery, 180.0, 195.0);
			this.SetSkill(SkillName.EvalInt, 180.0, 195.0);
			this.SetSkill(SkillName.MagicResist, 170.0, 185.0);

			this.Tamable = false;
			this.VirtualArmor = 30; // Max Limit 30 준수
			this.Fame = 28000;
			this.Karma = -28000;

            SetSpecialAbility(SpecialAbility.AngryFire);
        }

        public Oni(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 4;
            }
        }
        public override int GetAngerSound()
        {
            return 0x4E3;
        }

        public override int GetIdleSound()
        {
            return 0x4E2;
        }

        public override int GetAttackSound()
        {
            return 0x4E1;
        }

        public override int GetHurtSound()
        {
            return 0x4E4;
        }

        public override int GetDeathSound()
        {
            return 0x4E0;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
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
