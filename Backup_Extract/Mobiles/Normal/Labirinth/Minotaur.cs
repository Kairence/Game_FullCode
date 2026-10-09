using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a minotaur corpse")]
    public class Minotaur : BaseCreature
    {
        [Constructable]
        public Minotaur()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)// NEED TO CHECK
        {
            Name = "a minotaur";
            Body = 263;

			/* [Minotaur - Fame 16,000 / General / Weight 1.27]
			   - 스킬 200 마스터 서버용 '최상위 물리 정예' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (16,000/1000) + 4 = 20
			   - 리벤넌트(115~135)를 넘어서는 압도적 근접 파괴력
			   -------------------------------------------------- */

			// [Attributes] 명성 16,000 보너스 + 가중치 1.27 반영
			this.SetStr(450, 550); 
			this.SetHits(10000, 12000); 
			this.SetDex(80, 120);
			this.SetInt(80, 120);

			// [Combat Options] 거대한 도끼 한 방의 위력
			this.SetDamage(55, 80);
			this.SetAttackSpeed(2.8); // 묵직하고 강력한 일격

			// [Damage Types] 100% 물리 공격 (순수한 힘)
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 두꺼운 가죽과 강인한 신체 (면역 제거 및 밸런스 조정)
			this.SetResistance(ResistanceType.Physical, 65, 75); // 물리 전사에게는 매우 단단함
			this.SetResistance(ResistanceType.Fire, 40, 50);
			this.SetResistance(ResistanceType.Cold, 40, 50);
			this.SetResistance(ResistanceType.Poison, 50, 60);
			this.SetResistance(ResistanceType.Energy, 30, 40);   // 마법적 공격에 상대적 약점

			// [Skills] ★ 스킬 200 서버 기준 - 진정한 마스터용 (재설계)
			// 유저 스킬 150 ~ 180 구간에서 정면 승부하는 대상
			this.SetSkill(SkillName.Wrestling, 140.0, 160.0); 
			this.SetSkill(SkillName.Tactics, 140.0, 160.0);
			this.SetSkill(SkillName.Anatomy, 130.0, 150.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 130.0);

			// [Misc] 가상 방어력(Virtual Armor): 20
			this.VirtualArmor = 20;

			this.Fame = 16000;
			this.Karma = -16000;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }

            SetWeaponAbility(WeaponAbility.ParalyzingBlow);
        }

        public Minotaur(Serial serial)
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
