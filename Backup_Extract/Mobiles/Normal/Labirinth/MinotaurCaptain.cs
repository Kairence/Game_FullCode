using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a minotaur corpse")]
    public class MinotaurCaptain : BaseCreature
    {
        [Constructable]
        public MinotaurCaptain()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)// NEED TO CHECK
        {
            Name = "a minotaur captain";
            Body = 280;

			/* [Minotaur Captain - Fame 21,000 / General / Weight 1.30]
			   - 스킬 200 마스터 서버용 '준 보스급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (21,000/1000) + 5 = 26
			   - 미노타우르스(140~160)를 지휘하는 최상위 포식자
			   -------------------------------------------------- */

			// [Attributes] 명성 21,000 보너스 + 가중치 1.30 반영
			this.SetStr(750, 850); 
			this.SetHits(16000, 18500); 
			this.SetDex(150, 180);
			this.SetInt(150, 180);

			SetAttackSpeed(2.6);
			SetDamage(75, 110);

			// [Damage Types] 90% 물리 + 10% 에너지 (지휘관의 기세)
			this.SetDamageType(ResistanceType.Physical, 90);
			this.SetDamageType(ResistanceType.Energy, 10);

			// [Resistances] 견고한 사령관의 갑주 (밸런스 조정)
			this.SetResistance(ResistanceType.Physical, 70, 75); 
			this.SetResistance(ResistanceType.Fire, 45, 55);
			this.SetResistance(ResistanceType.Cold, 45, 55);
			this.SetResistance(ResistanceType.Poison, 55, 65);
			this.SetResistance(ResistanceType.Energy, 40, 50);

			// [Skills] ★ 스킬 200 서버 기준 - 마스터들의 한계 도전 (재설계)
			// 유저 스킬 170 ~ 200 구간 최상급 유저용
			this.SetSkill(SkillName.Wrestling, 160.0, 185.0); 
			this.SetSkill(SkillName.Tactics, 160.0, 185.0);
			this.SetSkill(SkillName.Anatomy, 150.0, 175.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 150.0);
			this.SetSkill(SkillName.Parry, 120.0, 140.0); // 지휘관다운 방어 숙련도

			// [Misc] 가상 방어력(Virtual Armor): 26
			this.VirtualArmor = 26;

			this.Fame = 21000;
			this.Karma = -21000;

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }

            SetWeaponAbility(WeaponAbility.ParalyzingBlow);
        }

        public MinotaurCaptain(Serial serial)
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
