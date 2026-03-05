using System;

namespace Server.Mobiles
{
    [CorpseName("a slimey corpse")]
    public class Slime : BaseCreature
    {
        [Constructable]
        public Slime()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a slime";
            Body = 51;
            BaseSoundID = 456;

            Hue = Utility.RandomSlimeHue();

			// [역산] 명성 300 보너스(Str+523, Hits+711, Skill+0.7) 반영
			// 스크립트 상 수치를 1.0~5.0 사이로 극도로 낮춥니다.
			this.SetStr(1, 5);      // 최종 Str 524~528
			this.SetDex(1, 5);      // 최종 Dex ~100 (느릿함)

			// 최종 Hits 712~720 목표 (보너스 711 제외 시 1~9 필요)
			this.SetHits(1, 9); 
			this.SetStam(1, 5);     // 최종 Stam ~60
			this.SetMana(0);

			SetAttackSpeed(4.0);    // 매우 느린 공격
			SetDamage(1, 2);        // 유저가 죽지 않는 수준

			// 공격 속성: 물리 100% (초보자에게 속성 데미지는 가혹함)
			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항 설정: 초보자가 때려도 데미지가 온전히 박히도록 최소화
			this.SetResistance(ResistanceType.Physical, 0, 5);
			this.SetResistance(ResistanceType.Poison, 5, 10);
			this.SetResistance(ResistanceType.Fire, 0);
			this.SetResistance(ResistanceType.Cold, 0);
			this.SetResistance(ResistanceType.Energy, 0);

			// 최종 Skill 5.0~10.0 목표 (5.0 - 0.7 = 4.3)
			// 초보 유저(스킬 10~20)가 명중률 60~70%를 뽑아낼 수 있는 수치
			this.SetSkill(SkillName.Wrestling, 4.3, 9.3);
			this.SetSkill(SkillName.Tactics, 4.3, 9.3);

			this.Fame = 300;
			this.Karma = -300;
			this.VirtualArmor = 0; // 갑옷 없음

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 1.1; // 누구나 길들일 수 있음
		}

        public Slime(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lesser;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lesser;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat | FoodType.Fish | FoodType.FruitsAndVegies | FoodType.GrainsAndHay | FoodType.Eggs;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Poor);
            AddLoot(LootPack.Gems);
        }

        public override bool CheckMovement(Direction d, out int newZ)
        {
            if (!base.CheckMovement(d, out newZ))
                return false;

            if (Region.IsPartOf("Underworld") && newZ > Location.Z)
                return false;

            return true;
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

            if (version == 0 && (AbilityProfile == null || AbilityProfile.MagicalAbility == MagicalAbility.None))
            {
                SetMagicalAbility(MagicalAbility.Poisoning);
            }
        }
    }
}
