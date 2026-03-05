using System;

namespace Server.Mobiles
{
    [CorpseName("a phoenix corpse")]
    public class Phoenix : BaseCreature, IAuraCreature
    {
        [Constructable]
        public Phoenix()
            : base(AIType.AI_Mage, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a phoenix";
            Body = 0x340;
            BaseSoundID = 0x8F;

			Boss = true;

			/* [Phoenix - Boss - Fame 29,000 / Weight 1.30]
			   - 정글 던전의 불멸의 지배자 / 월드 보스급 설계
			   - 보스 배수 적용: Attributes(5x) / Skills(2x)
			   - 체력 5만 이상 룰: 민맥 편차 2,000 고정
			   - VirtualArmor: 30 (명성/1000 + 1 보정)
			   -------------------------------------------------- */

			// [Attributes] 보스 공식에 따른 역산값 적용
			this.SetStr(23400, 23600); 
			this.SetHits(521000, 523000); // 총합 약 61만 이상의 보스급 체력
			this.SetDex(4650, 4750);
			this.SetInt(4650, 4750);

			// [Combat Options] 화염 100% (태양의 불꽃)
			this.SetDamage(120, 180);
			this.SetAttackSpeed(1.5);
			this.SetDamageType(ResistanceType.Fire, 100);

			// [Resistances] 최고 저항 75 이하 엄격 준수 / 냉기 약점
			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 75, 75);      // 화염 내성 Max
			this.SetResistance(ResistanceType.Cold, 20, 30);     // ★ 공략 포인트 (냉기 약점)
			this.SetResistance(ResistanceType.Poison, 60, 70); 
			this.SetResistance(ResistanceType.Energy, 60, 75);   

			// [Skills] 기본 120~130에 보스 보정(228.2) 가산
			// 최종 숙련도 약 350대의 초월적 전투 능력
			this.SetSkill(SkillName.Wrestling, 225.0, 230.0); 
			this.SetSkill(SkillName.Tactics, 225.0, 230.0);
			this.SetSkill(SkillName.Anatomy, 225.0, 230.0);
			this.SetSkill(SkillName.Magery, 220.0, 235.0);       
			this.SetSkill(SkillName.EvalInt, 220.0, 235.0);
			this.SetSkill(SkillName.MagicResist, 220.0, 235.0);

			this.Tamable = false;
			this.VirtualArmor = 30;
			this.Fame = 29000;
			this.Karma = 29000; // 성스러운 보스 (공격 시 유저 카르마 감소)

            SetAreaEffect(AreaEffect.AuraDamage);
        }

        public Phoenix(Serial serial)
            : base(serial)
        {
        }

        public override bool CanAngerOnTame { get { return true; } }
        public override int Meat { get { return 1; } }
        public override MeatType MeatType { get { return MeatType.Bird; } }
        public override int Feathers { get { return 36; } }
        public override bool CanFly { get { return true; } }

        public void AuraEffect(Mobile m)
        {
            m.SendLocalizedMessage(1008112); // The intense heat is damaging you!
        }

        public override void OnAfterTame(Mobile tamer)
        {
            base.OnAfterTame(tamer);

            var profile = PetTrainingHelper.GetAbilityProfile(this);

            if (profile != null)
            {
                profile.RemoveAbility(AreaEffect.AuraDamage);
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Rich);
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
