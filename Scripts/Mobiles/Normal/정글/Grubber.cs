using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a grubber corpse")]
    public class Grubber : BaseCreature
    {
        [Constructable]
        public Grubber()
            : base(AIType.AI_Animal, FightMode.None, 10, 1, 0.06, 0.1)
        {
            Name = "a grubber";
            Body = 270;

			/* [Grubber - Normal - Fame 6,000 / Weight 1.15]
			   - 정글 던전의 도둑 몬스터 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 1 (명성/1000 보정 -5)
			   - 특이사항: 높은 Dex로 기동성 확보, 아이템 탈취 기믹
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(75, 85); 
			this.SetHits(1750, 1850); 
			this.SetDex(150, 180); // 도둑다운 매우 높은 민첩성
			this.SetInt(10, 20);

			// [Combat Options] 물리 100% (약한 위력)
			this.SetDamage(10, 20);
			this.SetAttackSpeed(1.5); // 아주 빠른 연타로 유저를 당황시킴
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 물리 약점 설정
			this.SetResistance(ResistanceType.Physical, 10, 20); // ★ 매우 연약함
			this.SetResistance(ResistanceType.Fire, 30, 40);      
			this.SetResistance(ResistanceType.Cold, 30, 40);    
			this.SetResistance(ResistanceType.Poison, 60, 75);  // 독 내성 특화
			this.SetResistance(ResistanceType.Energy, 35, 45);   

			// [Skills] 기본 90~100에 역산 보너스(2.7) 가산
			this.SetSkill(SkillName.Wrestling, 92.0, 102.0); 
			this.SetSkill(SkillName.Tactics, 92.0, 102.0);
			this.SetSkill(SkillName.Stealing, 120.0, 150.0); // 전설적 도둑질 실력
			this.SetSkill(SkillName.Hiding, 100.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 80.0, 95.0);

			this.Tamable = false;
			this.VirtualArmor = 1;
			this.Fame = 6000;
			this.Karma = -6000;
        }

        public override IDamageable Combatant
        {
            get { return base.Combatant; }
            set
            {
                base.Combatant = value;

                if (0.10 > Utility.RandomDouble())
                    StopFlee();
                else if (!CheckFlee())
                    BeginFlee(TimeSpan.FromSeconds(10));
            }
        }

        public Grubber(Serial serial)
            : base(serial)
        {
        }

        public override int Meat { get { return 1; } }
        public override int Hides { get { return 1; } }

        public override int GetAttackSound()
        {
            return 0xC9;
        }

        public override int GetHurtSound()
        {
            return 0xCA;
        }

        public override int GetDeathSound()
        {
            return 0xCB;
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