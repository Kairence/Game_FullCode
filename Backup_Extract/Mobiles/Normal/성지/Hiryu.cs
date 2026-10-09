using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a hiryu corpse")]
    public class Hiryu : BaseMount
    {
        [Constructable]
        public Hiryu()
            : base("a hiryu", 243, 0x3E94, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Hue = GetHue();

			/* [Hiryu - Google Keep Formula: Balanced Resistance]
			   - 명성: 12,000 / 카르마: -12,000
			   - 저항: 유저의 대미지 체감을 고려하여 과도한 뎀감 억제
			   -------------------------------------------------- */

			// [Attributes] 가중치 1.25 적용
			this.SetStr(550, 650); 
			this.SetHits(9000, 11000); // 맷집은 체력으로 승부 (저항 대신)
			this.SetDex(130, 170); 
			this.SetInt(150, 250);

			// [Combat Options] 
			this.SetDamage(45, 75); 
			this.SetAttackSpeed(2.2);
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] ★ 형님 말씀대로 저항 하향 조정 (사냥하는 맛 강조)
			this.SetResistance(ResistanceType.Physical, 45, 60); // 뎀감 50% 내외 유지
			this.SetResistance(ResistanceType.Fire, 15, 25);      // 화염엔 살살 녹음
			this.SetResistance(ResistanceType.Cold, 35, 45);    
			this.SetResistance(ResistanceType.Poison, 35, 45); 
			this.SetResistance(ResistanceType.Energy, 40, 55);   

			// [Skills]
			this.SetSkill(SkillName.Wrestling, 115.0, 130.0); 
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);
			this.SetSkill(SkillName.Anatomy, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 3; 
			this.MinTameSkill = 155.1; 
			this.VirtualArmor = 15;

			this.Fame = 12000;
			this.Karma = -12000;
            if (Utility.RandomDouble() < .33)
                PackItem(Engines.Plants.Seed.RandomBonsaiSeed());

            if (Core.ML && Utility.RandomDouble() < .33)
                PackItem(Engines.Plants.Seed.RandomPeculiarSeed(4));

            SetWeaponAbility(WeaponAbility.Dismount);
            SetSpecialAbility(SpecialAbility.GraspingClaw);
        }

        public Hiryu(Serial serial)
            : base(serial)
        {
        }

        public override bool StatLossAfterTame
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
        public override int Meat
        {
            get
            {
                return 16;
            }
        }
        public override int Hides
        {
            get
            {
                return 60;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override bool CanAngerOnTame
        {
            get
            {
                return true;
            }
        }
        public override WeaponAbility GetWeaponAbility()
        {
            return WeaponAbility.Dismount;
        }

        public override int GetAngerSound()
        {
            return 0x4FE;
        }

        public override int GetIdleSound()
        {
            return 0x4FD;
        }

        public override int GetAttackSound()
        {
            return 0x4FC;
        }

        public override int GetHurtSound()
        {
            return 0x4FF;
        }

        public override int GetDeathSound()
        {
            return 0x4FB;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
            AddLoot(LootPack.Gems, 4);
        }

        public override void OnAfterTame(Mobile tamer)
        {
            if (Owners.Count == 0 && PetTrainingHelper.Enabled)
            {
                RawStr = (int)Math.Max(1, RawStr * 0.5);
                RawDex = (int)Math.Max(1, RawDex * 0.5);

                HitsMaxSeed = RawStr;
                Hits = RawStr;

                StamMaxSeed = RawDex;
                Stam = RawDex;
            }
            else
            {
                base.OnAfterTame(tamer);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)3);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
                Timer.DelayCall(TimeSpan.Zero, delegate { Hue = GetHue(); });

            if (version <= 1)
                Timer.DelayCall(TimeSpan.Zero, delegate
                {
                    if (InternalItem != null)
                    {
                        InternalItem.Hue = Hue;
                    }
                });

            if (version < 2)
            {
                for (int i = 0; i < Skills.Length; ++i)
                {
                    Skills[i].Cap = Math.Max(100.0, Skills[i].Cap * 0.9);

                    if (Skills[i].Base > Skills[i].Cap)
                    {
                        Skills[i].Base = Skills[i].Cap;
                    }
                }
            }

            if (version < 3)
            {
                SetWeaponAbility(WeaponAbility.Dismount);
                SetSpecialAbility(SpecialAbility.GraspingClaw);
            }
        }

        private static int GetHue()
        {
            int rand = Utility.Random(1075);

            /*
            1000	1075	No Hue Color	93.02%	0x0
            * 
            10	1075	Ice Green    	0.93%	0x847F
            10	1075	Light Blue    	0.93%	0x848D
            10	1075	Strong Cyan		0.93%	0x8495
            10	1075	Agapite			0.93%	0x8899
            10	1075	Gold			0.93%	0x8032
            * 
            8	1075	Blue and Yellow	0.74%	0x8487
            * 
            5	1075	Ice Blue       	0.47%	0x8482
            * 
            3	1075	Cyan			0.28%	0x8123
            3	1075	Light Green		0.28%	0x8295
            * 
            2	1075	Strong Yellow	0.19%	0x8037
            2	1075	Green			0.19%	0x8030	//this one is an approximation
            * 
            1	1075	Strong Purple	0.09%	0x8490
            1	1075	Strong Green	0.09%	0x855C
            * */

            if (rand <= 0)
                return 0x855C;
            else if (rand <= 1)
                return 0x8490;
            else if (rand <= 3)
                return 0x8030;
            else if (rand <= 5)
                return 0x8037;
            else if (rand <= 8)
                return 0x8295;
            else if (rand <= 11)
                return 0x8123;
            else if (rand <= 16)
                return 0x8482;
            else if (rand <= 24)
                return 0x8487;
            else if (rand <= 34)
                return 0x8032;
            else if (rand <= 44)
                return 0x8899;
            else if (rand <= 54)
                return 0x8495;
            else if (rand <= 64)
                return 0x848D;
            else if (rand <= 74)
                return 0x847F;
			
            return 0;
        }
    }
}
