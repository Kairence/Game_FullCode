using System;
using Server.Spells;

namespace Server.Mobiles
{
    [CorpseName("a meer corpse")]
    public class MeerWarrior : BaseCreature
    {
        [Constructable]
        public MeerWarrior()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a meer warrior";
            this.Body = 771;

			/* [MeerWarrior - Fame 5,000 / Normal / Weight 1.15]
			   - 미어 종족의 근접 보병 전사 (선족 설정)
			   - 지능형 아인종: 테이밍 불가 (200 숙련도 고려)
			   - 종족 특성: 에너지/화염 저항 취약
			   -------------------------------------------------- */
			// Boss = true 삭제 (일반 몬스터)

			// [Attributes] (기본 보너스 * 1배 * 1.15) - 기본 보너스
			// Str: 보너스 약 437 -> 최종 Set 약 150-180
			this.SetStr(150, 180); 

			// Hits: 보너스 약 9,700 -> 최종 Set 약 1400-1600
			this.SetHits(1400, 1600); 

			this.SetDex(100, 120); 
			this.SetInt(50, 70); // 전사 계급으로 마법 능력은 낮음

			// [Combat Options] 물리 위주의 강력한 타격
			this.SetDamage(25, 40);
			this.SetAttackSpeed(2.4);
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 종족 약점 반영 및 전사다운 물리 저항
			this.SetResistance(ResistanceType.Physical, 45, 55); 
			this.SetResistance(ResistanceType.Fire, 15, 25);      // ★ 화염 취약점
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 20, 30);   // 에너지 저항 낮음

			// [Skills] 전사다운 높은 근접 전투 숙련도
			this.SetSkill(SkillName.Wrestling, 95.0, 110.0); 
			this.SetSkill(SkillName.Tactics, 95.0, 110.0);
			this.SetSkill(SkillName.Anatomy, 90.0, 105.0);
			this.SetSkill(SkillName.MagicResist, 80.0, 95.0);

			// [Misc]
			this.Tamable = false; // 지능형 아인종
			this.VirtualArmor = 5;

			this.Fame = 5000;
			this.Karma = 5000; // 선족 설정
        }

        public MeerWarrior(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override bool InitialInnocent
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if (from != null && !willKill && amount > 3 && !this.InRange(from, 7))
            {
                this.MovingEffect(from, 0xF51, 10, 0, false, false);
                SpellHelper.Damage(TimeSpan.FromSeconds(1.0), from, this, Utility.RandomMinMax(30, 40) - (Core.AOS ? 0 : 10), 100, 0, 0, 0, 0);
            }

            base.OnDamage(amount, from, willKill);
        }

        public override int GetHurtSound()
        {
            return 0x156;
        }

        public override int GetDeathSound()
        {
            return 0x15C;
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