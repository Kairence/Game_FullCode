using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a wrinkly yomotsu corpse")]
    public class YomotsuElder : BaseCreature
    {
        [Constructable]
        public YomotsuElder()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a yomotsu elder";
            Body = 255;
            BaseSoundID = 0x452;

			/* [Yomotsu Elder - Normal - Fame 15,000 / Weight 1.30]
			   - 요모츠 광산의 지배자 / 일반 던전 최상위
			   - 배수: 1x (일반 몬스터)
			   - VirtualArmor: 20 (기본 15 + 노련함 보정 5)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(490, 520); 
			this.SetHits(11000, 11500); 
			this.SetDex(95, 110);
			this.SetInt(95, 110);

			// [Combat Options] 물리 50% / 화염 50% (강력한 근접전 및 마법)
			this.SetDamage(50, 85);
			this.SetAttackSpeed(2.2); 
			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			// [Resistances] 최고 저항 75 이하 준수 및 명확한 약점(냉기) 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); // 75% 캡 준수
			this.SetResistance(ResistanceType.Fire, 60, 70);      
			this.SetResistance(ResistanceType.Cold, 25, 35);    // ★ 종족적 취약점
			this.SetResistance(ResistanceType.Poison, 50, 65); 
			this.SetResistance(ResistanceType.Energy, 50, 65);   

			// [Skills] 기본 110~120에 역산 보너스(16.8) 가산
			this.SetSkill(SkillName.Wrestling, 125.0, 140.0); 
			this.SetSkill(SkillName.Tactics, 125.0, 140.0);
			this.SetSkill(SkillName.Anatomy, 125.0, 140.0);
			this.SetSkill(SkillName.Magery, 115.0, 130.0);       // 노련한 주술 구사
			this.SetSkill(SkillName.EvalInt, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);

			this.Tamable = false;
			this.VirtualArmor = 20;
			this.Fame = 15000;
			this.Karma = -15000;

            PackItem(new GreenGourd());
            PackItem(new ExecutionersAxe());

            switch ( Utility.Random(3) )
            {
                case 0:
                    PackItem(new LongPants());
                    break;
                case 1:
                    PackItem(new ShortPants());
                    break;
            }

            switch ( Utility.Random(6) )
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

        public YomotsuElder(Serial serial)
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
                return 5;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
            AddLoot(LootPack.Gems, 2);
        }

        // TODO: Axe Throw
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