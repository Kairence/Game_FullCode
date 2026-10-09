using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a tormented minotaur corpse")]
    public class TormentedMinotaur : BaseCreature
    {
        [Constructable]
        public TormentedMinotaur()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Tormented Minotaur";
            Body = 262;

			/* [Tormented Minotaur - Fame 19,000 / General / Weight 1.29]
			   - 스킬 200 마스터 서버용 '최상급 광전사' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (19,000/1000) + 5 = 24
			   - 고통으로 인해 방어보다는 공격과 저항에 치중된 개체
			   -------------------------------------------------- */

			// [Attributes] 명성 19,000 보너스 + 가중치 1.29 반영
			this.SetStr(600, 720); 
			this.SetHits(14000, 16000); 
			this.SetDex(120, 150);
			this.SetInt(120, 150);

			// [Combat Options] 고통을 유저에게 전이시키는 타격
			this.SetDamage(65, 95);
			this.SetAttackSpeed(2.2); // 광기 어린 속도

			// [Damage Types] 50% 물리 + 25% 화염 + 25% 에너지 (고통의 낙인)
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 25);
			this.SetDamageType(ResistanceType.Energy, 25);

			// [Resistances] 고통에 익숙해진 신체 (중상급 저항)
			this.SetResistance(ResistanceType.Physical, 60, 70); 
			this.SetResistance(ResistanceType.Fire, 50, 60);
			this.SetResistance(ResistanceType.Cold, 30, 40);     // 냉기에는 여전히 취약
			this.SetResistance(ResistanceType.Poison, 70, 75);  // 극심한 독 저항
			this.SetResistance(ResistanceType.Energy, 50, 60);

			// [Skills] ★ 스킬 200 서버 기준 - 마스터로 가는 최종 관문
			// 유저 스킬 165 ~ 185 구간의 핵심 사냥감
			this.SetSkill(SkillName.Wrestling, 155.0, 175.0); 
			this.SetSkill(SkillName.Tactics, 155.0, 175.0);
			this.SetSkill(SkillName.Anatomy, 160.0, 180.0);
			this.SetSkill(SkillName.MagicResist, 140.0, 160.0);
			this.SetSkill(SkillName.Healing, 100.0, 120.0);    // 고통을 견디며 스스로 치유

			// [Misc] 가상 방어력(Virtual Armor): 24
			this.VirtualArmor = 24;

			this.Fame = 19000;
			this.Karma = -19000;

            SetWeaponAbility(WeaponAbility.Dismount);
        }

        public TormentedMinotaur(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Deadly;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 3;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 10);
        }

        public override int GetDeathSound()
        {
            return 0x596;
        }

        public override int GetAttackSound()
        {
            return 0x597;
        }

        public override int GetIdleSound()
        {
            return 0x598;
        }

        public override int GetAngerSound()
        {
            return 0x599;
        }

        public override int GetHurtSound()
        {
            return 0x59A;
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
