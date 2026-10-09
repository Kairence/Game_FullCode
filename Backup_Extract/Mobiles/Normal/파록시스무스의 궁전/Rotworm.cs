using Server;
using System;
using System.Collections.Generic;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    [CorpseName("a rotworm corpse")]
    [TypeAlias("Server.Mobiles.RotWorm")]
    public class Rotworm : BaseCreature
    {
        [Constructable]
        public Rotworm()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.25, 0.5)
        {
            Name = "a rotworm";
            Body = 732;

			/* [Rotworm - Normal - Fame 3,000 / Weight 1.18]
			   - 파록시스무스 던전의 하급 청소부 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 1 (명성/1000 - 2 보정)
			   - 특이사항: 낮은 방어력이지만 끈질긴 체력과 독 저항
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(40, 48); 
			this.SetHits(980, 1000); 
			this.SetDex(50, 60); 
			this.SetInt(15, 25);

			// [Combat Options] 물리 50% / 독 50% (오염된 점액 타격)
			this.SetDamage(12, 22);
			this.SetAttackSpeed(2.5); 
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Poison, 50);

			// [Resistances] 최고 저항 75 이하 준수 / 냉기 약점 설정
			this.SetResistance(ResistanceType.Physical, 15, 25); 
			this.SetResistance(ResistanceType.Fire, 20, 30);      
			this.SetResistance(ResistanceType.Cold, 5, 15);     // ★ 확실한 약점 (냉기에 취약)
			this.SetResistance(ResistanceType.Poison, 70, 75);  // 독성 내성 특화
			this.SetResistance(ResistanceType.Energy, 20, 30);   

			// [Skills] 기본 85~95에 역산 보너스(1.5) 가산
			this.SetSkill(SkillName.Wrestling, 86.5, 96.5); 
			this.SetSkill(SkillName.Tactics, 86.5, 96.5);
			this.SetSkill(SkillName.Anatomy, 86.5, 96.5);
			this.SetSkill(SkillName.MagicResist, 65.0, 80.0);
			this.SetSkill(SkillName.Poisoning, 80.0, 100.0);

			// [Misc]
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 75.0; // 기초 테이밍 펫
			this.VirtualArmor = 1;
			this.Fame = 3000;
			this.Karma = -3000;

            PackBodyPartOrBones();

            SetSpecialAbility(SpecialAbility.BloodDisease);
        }

        public Rotworm(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound() { return 0x62D; }
        public override int GetIdleSound() { return 0x62D; }
        public override int GetAttackSound() { return 0x62A; }
        public override int GetHurtSound() { return 0x62C; }
        public override int GetDeathSound() { return 0x62B; }

        public override int Meat { get { return 2; } }
        public override MeatType MeatType { get { return MeatType.Rotworm; } }
        public override FoodType FavoriteFood { get { return FoodType.Fish; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override void OnKilledBy(Mobile mob)
        {
            base.OnKilledBy(mob);

            if (mob is PlayerMobile && 0.2 > Utility.RandomDouble())
            {
                PlayerMobile pm = mob as PlayerMobile;

                if (QuestHelper.HasQuest<Missing>(pm))
                {
                    // As the rotworm dies, you find and pickup a scroll case. Inside the scroll case is parchment. The scroll case crumbles to dust.
                    pm.SendLocalizedMessage(1095146);

                    pm.AddToBackpack(new ArielHavenWritofMembership());
                }
            }
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            CandlewoodTorch torch = m.FindItemOnLayer(Layer.TwoHanded) as CandlewoodTorch;

            if (torch != null && torch.Burning)
                BeginFlee(TimeSpan.FromSeconds(5.0));
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
