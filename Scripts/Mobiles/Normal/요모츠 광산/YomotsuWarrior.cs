using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a yomotsu corpse")]
    public class YomotsuWarrior : BaseCreature
    {
        [Constructable]
        public YomotsuWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a yomotsu warrior";
            Body = 245;
            BaseSoundID = 0x452;

			/* [Yomotsu Warrior - Normal - Fame 8,500 / Weight 1.25]
			   - 요모츠 광산 정예 보병 / 일반 던전
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 6 (기본 8 + 보정 -2)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(200, 215); 
			this.SetHits(4500, 4600); 
			this.SetDex(35, 45);
			this.SetInt(35, 45);

			// [Combat Options] 100% 물리 대미지 (마비 타격 위협)
			this.SetDamage(30, 50);
			this.SetAttackSpeed(2.0); // 작지만 매우 빠른 공격
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 및 명확한 약점(냉기) 설정
			this.SetResistance(ResistanceType.Physical, 45, 55); 
			this.SetResistance(ResistanceType.Fire, 55, 65);     // 광산 내 열기 적응
			this.SetResistance(ResistanceType.Cold, 15, 25);    // ★ 확실한 약점 (냉기 취약)
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 35, 45);   

			// [Skills] 기본 95~110에 역산 보너스(6.82) 가산
			this.SetSkill(SkillName.Wrestling, 100.0, 115.0); 
			this.SetSkill(SkillName.Tactics, 100.0, 115.0);
			this.SetSkill(SkillName.Anatomy, 100.0, 115.0);
			this.SetSkill(SkillName.MagicResist, 85.0, 100.0);

			this.Tamable = false;
			this.VirtualArmor = 6;
			this.Fame = 8500;
			this.Karma = -8500;

            PackItem(new GreenGourd());
            PackItem(new ExecutionersAxe());

            if (Utility.RandomBool())
                PackItem(new LongPants());
            else
                PackItem(new ShortPants());

            switch ( Utility.Random(4) )
            {
                case 0:
                    PackItem(new Shoes());
                    break;
                case 1:
                    PackItem(new Sandals());
                    break;
                case 2:
                    PackItem(new Boots());
                    break;
                case 3:
                    PackItem(new ThighBoots());
                    break;
            }

            if (Utility.RandomDouble() < .25)
                PackItem(Engines.Plants.Seed.RandomBonsaiSeed());

            SetWeaponAbility(WeaponAbility.DoubleStrike);
        }

        public YomotsuWarrior(Serial serial)
            : base(serial)
        {
        }

        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
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
                return 3;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 2);
            AddLoot(LootPack.Gems, 2);
        }

        // TODO: Throwing Dagger
        public override void OnGaveMeleeAttack(Mobile defender) 
        {
            base.OnGaveMeleeAttack(defender);

            if (0.1 > Utility.RandomDouble())
            {
                /* Maniacal laugh
                * Cliloc: 1070840
                * Effect: Type: "3" From: "0x57D4F5B" To: "0x0" ItemId: "0x37B9" ItemIdName: "glow" FromLocation: "(884 715, 10)" ToLocation: "(884 715, 10)" Speed: "10" Duration: "5" FixedDirection: "True" Explode: "False"
                * Paralyzes for 4 seconds, or until hit
                */
                defender.FixedEffect(0x37B9, 10, 5);
                defender.SendLocalizedMessage(1070840); // You are frozen as the creature laughs maniacally.

                defender.Paralyze(TimeSpan.FromSeconds(4.0));
            }
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

        public override int GetIdleSound()
        {
            return 0x42A;
        }

        public override int GetAttackSound()
        {
            return 0x435;
        }

        public override int GetHurtSound()
        {
            return 0x436;
        }

        public override int GetDeathSound()
        {
            return 0x43A;
        }
    }
}