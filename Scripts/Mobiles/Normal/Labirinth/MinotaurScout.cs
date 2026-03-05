using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a minotaur corpse")]
    public class MinotaurScout : BaseCreature
    {
        [Constructable]
        public MinotaurScout()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)// NEED TO CHECK
        {
            Name = "a minotaur scout";
            Body = 281;
		   
			/* [Minotaur Scout - Fame 12,500 / General / Weight 1.22]
			   - 스킬 200 마스터 서버용 '상급 기민형' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (12,500/1000) + 1.5 = 14
			   - 빠른 공격 속도와 높은 회피로 유저의 진입을 방해
			   -------------------------------------------------- */

			// [Attributes] 명성 12,500 보너스 + 가중치 1.22 반영
			this.SetStr(260, 320); 
			this.SetHits(6000, 7000); 
			this.SetDex(50, 75);
			this.SetInt(50, 75);

			// [Combat Options]
			this.SetDamage(35, 50);
			this.SetAttackSpeed(1.8); // 정찰병다운 매우 빠른 연타 속도

			// [Damage Types] 70% 물리 + 30% 독 (독 바른 단검/화살 컨셉)
			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Poison, 30);

			// [Resistances] 가벼운 무장 (에너지와 독에 강점)
			this.SetResistance(ResistanceType.Physical, 45, 55); 
			this.SetResistance(ResistanceType.Fire, 30, 40);
			this.SetResistance(ResistanceType.Cold, 30, 40);
			this.SetResistance(ResistanceType.Poison, 60, 70);
			this.SetResistance(ResistanceType.Energy, 50, 60);

			// [Skills] ★ 스킬 200 서버 기준 - 상급 유저를 위한 추격자 (재설계)
			// 유저 스킬 115 ~ 140 구간 전투에 최적화
			this.SetSkill(SkillName.Wrestling, 110.0, 130.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 130.0);
			this.SetSkill(SkillName.Anatomy, 120.0, 140.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 120.0);
			this.SetSkill(SkillName.Poisoning, 80.0, 100.0); // 치명적이지는 않지만 성가신 독
			this.SetSkill(SkillName.DetectHidden, 150.0);   // 정찰병답게 은신 유저를 잘 찾아냄

			// [Misc] 가상 방어력(Virtual Armor): 14
			this.VirtualArmor = 14;

			this.Fame = 12500;
			this.Karma = -12500;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }

            SetWeaponAbility(WeaponAbility.ParalyzingBlow);
        }

        public MinotaurScout(Serial serial)
            : base(serial)
        {
        }
		
		public override int TreasureMapLevel { get { return 3; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);  // Need to verify
        }

        // Using Tormented Minotaur sounds - Need to veryfy
        public override int GetAngerSound()
        {
            return 0x597;
        }

        public override int GetIdleSound()
        {
            return 0x596;
        }

        public override int GetAttackSound()
        {
            return 0x599;
        }

        public override int GetHurtSound()
        {
            return 0x59a;
        }

        public override int GetDeathSound()
        {
            return 0x59c;
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