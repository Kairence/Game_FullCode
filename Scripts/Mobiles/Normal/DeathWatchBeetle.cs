using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a deathwatchbeetle corpse")]
    [TypeAlias("Server.Mobiles.DeathWatchBeetle")]
    public class DeathwatchBeetle : BaseCreature
    {
        [Constructable]
        public DeathwatchBeetle()
            : base(AIType.AI_Melee, Core.ML ? FightMode.Aggressor : FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a deathwatch beetle";
            Body = 242;

			/* [Deathwatch Beetle - Fame 6,500 / General / Weight 1.18]
			   - 스킬 200 마스터 서버용 '중급 정예' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (6,500/1000) + 2.5 = 9 (단단한 외골격)
			   - 일반 비틀보다 높은 독 저항과 치명적인 독 공격
			   -------------------------------------------------- */

			// [Attributes] 명성 6,500 보너스 + 가중치 1.18 반영
			this.SetStr(95, 115); 
			this.SetHits(2200, 2500); 
			this.SetDex(15, 25);
			this.SetInt(15, 25);

			// [Combat Options]
			this.SetDamage(20, 35);
			this.SetAttackSpeed(2.2);

			// [Damage Types] 70% 물리 + 30% 독 속성 (죽음의 감시자)
			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Poison, 30);

			// [Resistances] 총합 약 210 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 50, 60);
			this.SetResistance(ResistanceType.Fire, 20, 30);
			this.SetResistance(ResistanceType.Cold, 25, 35);
			this.SetResistance(ResistanceType.Poison, 70, 75);      // 독 내성 매우 높음
			this.SetResistance(ResistanceType.Energy, 25, 35);

			// [Skills] ★ 스킬 200 서버 기준 - 중급자용 상위 타겟 (재설계)
			// 유저 스킬 70 ~ 90 구간 수련 및 전투에 적합
			this.SetSkill(SkillName.Wrestling, 60.0, 75.0); 
			this.SetSkill(SkillName.Tactics, 60.0, 75.0);
			this.SetSkill(SkillName.Anatomy, 65.0, 80.0); // 치명적인 급소 공격 컨셉
			this.SetSkill(SkillName.MagicResist, 50.0, 65.0);
			this.SetSkill(SkillName.Poisoning, 80.0, 100.0); // 독 특화

			// [Taming Code] 
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 110.0; // 서버 특성상 고급 테이밍 실력 요구

			// [Misc] 가상 방어력(Virtual Armor): (6,500/1000) + 2.5 = 9
			this.VirtualArmor = 9;

			this.Fame = 6500;
			this.Karma = -6500;
            if (Utility.RandomDouble() < .5)
                PackItem(Engines.Plants.Seed.RandomBonsaiSeed());

            SetWeaponAbility(WeaponAbility.CrushingBlow);
            SetSpecialAbility(SpecialAbility.PoisonSpit);
        }

        public DeathwatchBeetle(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get
            {
                return 8;
            }
        }

        public override int GetAngerSound()
        {
            return 0x4F3;
        }

        public override int GetIdleSound()
        {
            return 0x4F2;
        }

        public override int GetAttackSound()
        {
            return 0x4F1;
        }

        public override int GetHurtSound()
        {
            return 0x4F4;
        }

        public override int GetDeathSound()
        {
            return 0x4F0;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LowScrolls, 1);
            AddLoot(LootPack.Potions, 1);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
            {
                SetWeaponAbility(WeaponAbility.CrushingBlow);
            }
        }
    }
}
