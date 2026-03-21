using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a revenant lion corpse")]
    public class RevenantLion : BaseCreature
    {
        [Constructable]
        public RevenantLion()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a Revenant Lion";
            Body = 251;

			/* [Revenant Lion - Normal - Fame 17,500 / Weight 1.30]
			   - 파록시스무스 던전의 상급 언데드 야수 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 20 (명성/1000 + 3 보정)
			   - 특이사항: 압도적인 물리 공격력과 빠른 추격 속도
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용 (Hits 약 1.3만 대)
			this.SetStr(615, 635); 
			this.SetHits(13700, 14000); 
			this.SetDex(140, 160); 
			this.SetInt(140, 160);

			SetAttackSpeed(10.0);
			SetDamage(20, 30);
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Cold, 30);
			this.SetDamageType(ResistanceType.Energy, 30);

			// [Resistances] 최고 저항 75 이하 준수 / 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 55, 65); 
			this.SetResistance(ResistanceType.Fire, 20, 30);     // ★ 확실한 약점 (불에 취약)
			this.SetResistance(ResistanceType.Cold, 70, 75);    // 언데드의 한기 (Max 75)
			this.SetResistance(ResistanceType.Poison, 70, 75);  // 부패 면역 (Max 75)
			this.SetResistance(ResistanceType.Energy, 40, 50);   

			// [Skills] 기본 115~125에 역산 보너스(20.8) 가산
			// 최종 숙련도 약 135~145대의 정예 맹수
			this.SetSkill(SkillName.Wrestling, 135.8, 145.8); 
			this.SetSkill(SkillName.Tactics, 135.8, 145.8);
			this.SetSkill(SkillName.Anatomy, 135.8, 145.8);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);
			this.SetSkill(SkillName.Magery, 110.0, 125.0);       // 네크로맨틱 마력 보유

			this.Tamable = false;
			this.VirtualArmor = 20;
			this.Fame = 17500;
			this.Karma = -17500;

            SetWeaponAbility(WeaponAbility.BleedAttack);
        }

        public RevenantLion(Serial serial)
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
                return Poison.Greater;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Greater;
            }
        }

        public override int GetAngerSound()
        {
            return 0x518;
        }

        public override int GetIdleSound()
        {
            return 0x517;
        }

        public override int GetAttackSound()
        {
            return 0x516;
        }

        public override int GetHurtSound()
        {
            return 0x519;
        }

        public override int GetDeathSound()
        {
            return 0x515;
        }
		
		public override int TreasureMapLevel { get { return 3; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 2);
            AddLoot(LootPack.MedScrolls, 2);
            // TODO: Bone Pile
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