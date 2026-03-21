using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a cu sidhe corpse")]
    public class CuSidhe : BaseMount
    {
        public override double HealChance { get { return 1.0; } }

        [Constructable]
        public CuSidhe()
            : this("a cu sidhe")
        {
        }

        [Constructable]
        public CuSidhe(string name)
            : base(name, 277, 0x3E91, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            double chance = Utility.RandomDouble() * 23301;

            if (chance <= 1)
                Hue = 0x489;
            else if (chance < 50)
                Hue = Utility.RandomList(0x657, 0x515, 0x4B1, 0x481, 0x482, 0x455);
            else if (chance < 500)
                Hue = Utility.RandomList(0x97A, 0x978, 0x901, 0x8AC, 0x5A7, 0x527);

			/* [Cu Sidhe - Normal - Fame 20,000 / Karma +20,000 / Weight 1.25]
			   - 정글 던전의 성스러운 수호견 / 일반 몬스터 공식 (상급 펫)
			   - 배수: 1x (Normal)
			   - VirtualArmor: 25 (기본 20 + 보정 5)
			   - 테이밍 가능: 4슬롯 (최상급 전투 펫)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(610, 640); 
			this.SetHits(13700, 14000); 
			this.SetDex(120, 130);
			this.SetInt(120, 130);

			// [Combat Options] 물리 40% / 냉기 60% (성스러운 냉기 타격)
			this.SetDamage(50, 80);
			this.SetAttackSpeed(2.0); // 거구임에도 민첩한 공격
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Cold, 60);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 및 화염 약점 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); 
			this.SetResistance(ResistanceType.Fire, 30, 45);      // ★ 확실한 약점 (열기에 취약)
			this.SetResistance(ResistanceType.Cold, 70, 75);    // 냉기 내성 특화
			this.SetResistance(ResistanceType.Poison, 50, 65); 
			this.SetResistance(ResistanceType.Energy, 40, 50);   

			// [Skills] 기본 115~125에 역산 보너스(20.8) 가산
			this.SetSkill(SkillName.Wrestling, 135.0, 145.0); 
			this.SetSkill(SkillName.Tactics, 135.0, 145.0);
			this.SetSkill(SkillName.Anatomy, 135.0, 145.0);
			this.SetSkill(SkillName.MagicResist, 120.0, 135.0);
			this.SetSkill(SkillName.Healing, 120.0, 135.0);    // 자가 및 주인 치유 (붕대)

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 4; // 200 숙련도 시대의 강력한 4슬롯 펫
			this.MinTameSkill = 150.0; // 숙련된 테이머만 가능
			this.VirtualArmor = 25;
			this.Fame = 20000;
			this.Karma = 20000; // 영물 (선 성향)

            SetWeaponAbility(WeaponAbility.BleedAttack);
        }

        public CuSidhe(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get { return 5; }
        }

        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }
        public override bool StatLossAfterTame
        {
            get
            {
                return true;
            }
        }
        public override int Hides
        {
            get
            {
                return 10;
            }
        }
        public override int Meat
        {
            get
            {
                return 3;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.AosFilthyRich, 5);
        }

        public override void OnAfterTame(Mobile tamer)
        {
            if (Owners.Count == 0 && PetTrainingHelper.Enabled)
            {
                if (RawStr > 0)
                    RawStr = (int)Math.Max(1, RawStr * 0.5);

                if (RawDex > 0)
                    RawDex = (int)Math.Max(1, RawDex * 0.5);

                if (HitsMaxSeed > 0)
                    HitsMaxSeed = (int)Math.Max(1, HitsMaxSeed * 0.5);

                Hits = Math.Min(HitsMaxSeed, Hits);
                Stam = Math.Min(RawDex, Stam);
            }
            else
            {
                base.OnAfterTame(tamer);
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Race != Race.Elf && from == ControlMaster && from.IsPlayer())
            {
                Item pads = from.FindItemOnLayer(Layer.Shoes);

                if (pads is PadsOfTheCuSidhe)
                    from.SendLocalizedMessage(1071981); // Your boots allow you to mount the Cu Sidhe.
                else
                {
                    from.SendLocalizedMessage(1072203); // Only Elves may use 
                    return;
                }
            }

            base.OnDoubleClick(from);
        }

        public override int GetIdleSound()
        {
            return 0x577;
        }

        public override int GetAttackSound()
        {
            return 0x576;
        }

        public override int GetAngerSound()
        {
            return 0x578;
        }

        public override int GetHurtSound()
        {
            return 0x576;
        }

        public override int GetDeathSound()
        {
            return 0x579;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 3 && Controlled && RawStr >= 1200 && ControlSlots == ControlSlotsMin)
            {
                //Server.SkillHandlers.AnimalTaming.ScaleStats(this, 0.5);
            }

            if (version < 1 && Name == "a Cu Sidhe")
                Name = "a cu sidhe";

            if (version == 1)
            {
                SetWeaponAbility(WeaponAbility.BleedAttack);
            }
        }
    }
}
