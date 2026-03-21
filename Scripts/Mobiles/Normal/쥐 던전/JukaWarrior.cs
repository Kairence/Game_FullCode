using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a juka corpse")]
    public class JukaWarrior : BaseCreature
    {
        [Constructable]
        public JukaWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a juka warrior";
            Body = 764;

			/* [Juka Warrior - Normal - Fame 9,000 / Weight 1.18]
			   - 정글 던전의 표준 보병 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 10 (명성/1000 + 1 보정)
			   - 특이사항: 균형 잡힌 공방 밸런스
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(150, 165); 
			this.SetHits(3450, 3600); 
			this.SetDex(30, 40);
			this.SetInt(30, 40);

			// [Combat Options] 물리 100% (쥬카 군용 도검)
			this.SetDamage(30, 50);
			this.SetAttackSpeed(2.4); 
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 냉기 약점 설정
			this.SetResistance(ResistanceType.Physical, 45, 60); 
			this.SetResistance(ResistanceType.Fire, 40, 55);      
			this.SetResistance(ResistanceType.Cold, 15, 25);    // ★ 확실한 약점 (냉기에 취약)
			this.SetResistance(ResistanceType.Poison, 40, 55); 
			this.SetResistance(ResistanceType.Energy, 35, 50);   

			// [Skills] 기본 105~115에 역산 보너스(5.2) 가산
			// 최종 숙련도 약 110~120대의 정규군 수준
			this.SetSkill(SkillName.Wrestling, 110.0, 120.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 120.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 95.0, 110.0);
			this.SetSkill(SkillName.Parry, 100.0, 115.0);

			this.Tamable = false;
			this.VirtualArmor = 10;
			this.Fame = 9000;
			this.Karma = -9000;

            if (Utility.RandomDouble() < 0.1)
                PackItem(new ArcaneGem());
        }

        public JukaWarrior(Serial serial)
            : base(serial)
        {
        }

        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.Gems, 1);
        }

        public override int GetIdleSound()
        {
            return 0x1AC;
        }

        public override int GetAngerSound()
        {
            return 0x1CD;
        }

        public override int GetHurtSound()
        {
            return 0x1D0;
        }

        public override int GetDeathSound()
        {
            return 0x28D;
        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (0.2 < Utility.RandomDouble())
                return;

            switch ( Utility.Random(3) )
            {
                case 0:
                    {
                        defender.SendLocalizedMessage(1004014); // You have been stunned!
                        defender.Freeze(TimeSpan.FromSeconds(4.0));
                        break;
                    }
                case 1:
                    {
                        defender.SendAsciiMessage("You have been hit by a paralyzing blow!");
                        defender.Freeze(TimeSpan.FromSeconds(3.0));
                        break;
                    }
                case 2:
                    {
                        AOS.Damage(defender, this, Utility.Random(10, 5), 100, 0, 0, 0, 0);
                        defender.SendAsciiMessage("You have been hit by a critical strike!");
                        break;
                    }
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
    }
}