using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a kappa corpse")]
    public class Kappa : BaseCreature
    {
        [Constructable]
        public Kappa()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a kappa";
            Body = 240;

			/* [Kappa - Fame 2,000 / General / Weight 1.12]
			   - 스킬 200 마스터 서버용 '초중급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (2,000/1000) + 3 = 5 (등각 보정)
			   - 그렘린(25~35)과 솔렌 일꾼(35~45) 사이의 징검다리
			   -------------------------------------------------- */

			// [Attributes] 명성 2,000 보너스 + 가중치 1.12 반영
			this.SetStr(15, 25); 
			this.SetHits(380, 450); 
			this.SetDex(3, 5);
			this.SetInt(3, 5);

			// [Combat Options]
			this.SetDamage(8, 15);
			this.SetAttackSpeed(2.0);

			// [Damage Types] 80% 물리 + 20% 냉기 (습기 가득한 공격)
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Cold, 20);

			// [Resistances] 총합 약 130 (초보자용 저항)
			this.SetResistance(ResistanceType.Physical, 35, 45); // 등껍질로 인한 물리 저항
			this.SetResistance(ResistanceType.Fire, 0, 10);      // 불에 매우 취약
			this.SetResistance(ResistanceType.Cold, 40, 50);     // 냉기에 강함
			this.SetResistance(ResistanceType.Poison, 15, 25);
			this.SetResistance(ResistanceType.Energy, 10, 20);

			// [Skills] ★ 스킬 200 서버 기준 - 입문을 막 벗어난 유저용 (재설계)
			// 유저 스킬 35 ~ 55 구간 사냥에 최적화
			this.SetSkill(SkillName.Wrestling, 30.0, 40.0); 
			this.SetSkill(SkillName.Tactics, 30.0, 40.0);
			this.SetSkill(SkillName.Anatomy, 25.0, 35.0);
			this.SetSkill(SkillName.MagicResist, 25.0, 35.0);

			// [Misc] 가상 방어력(Virtual Armor): (2,000/1000) + 3 = 5
			this.VirtualArmor = 5;

			this.Fame = 2000;
			this.Karma = -2000;

            PackItem(new RawFishSteak(3));
            for (int i = 0; i < 2; i++)
            {
                switch ( Utility.Random(6) )
                {
                    case 0:
                        PackItem(new Gears());
                        break;
                    case 1:
                        PackItem(new Hinge());
                        break;
                    case 2:
                        PackItem(new Axle());
                        break;
                }
            }

            if (Core.ML && Utility.RandomDouble() < .33)
                PackItem(Engines.Plants.Seed.RandomPeculiarSeed(2));

            SetSpecialAbility(SpecialAbility.LifeLeech);
        }

        public Kappa(Serial serial)
            : base(serial)
        {
        }
		
		public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.Average);
        }

        public override int GetAngerSound()
        {
            return 0x50B;
        }

        public override int GetIdleSound()
        {
            return 0x50A;
        }

        public override int GetAttackSound()
        {
            return 0x509;
        }

        public override int GetHurtSound()
        {
            return 0x50C;
        }

        public override int GetDeathSound()
        {
            return 0x508;
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if (from != null && from.Map != null)
            {
                int amt = 0;
                Mobile target = this; 
                int rand = Utility.Random(1, 100);
                if (willKill)
                {
                    amt = (((rand % 5) >> 2) + 3);
                }
                if ((Hits < 100) && (rand < 21)) 
                {
                    target = (rand % 2) < 1 ? this : from;
                    amt++;
                }
                if (amt > 0)
                {
                    SpillAcid(target, amt);
                    from.SendLocalizedMessage(1070820); 
                    if (Mana > 14)
                        Mana -= 15;
                    amt ^= amt;
                }
            }
            base.OnDamage(amount, from, willKill);
        }

        public override Item NewHarmfulItem()
        {
            return new AcidSlime(TimeSpan.FromSeconds(10), 5, 10);
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