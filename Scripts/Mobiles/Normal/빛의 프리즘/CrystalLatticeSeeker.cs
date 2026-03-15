using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a Crystal Lattice Seeker corpse")]
    public class CrystalLatticeSeeker : BaseCreature
    {
        [Constructable]
        public CrystalLatticeSeeker()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "Crystal Lattice Seeker";
            this.Body = 0x7B;
            this.Hue = 0x47E;

			/* [Crystal Lattice Seeker - Fame 25,000 / Normal / Weight 1.30]
			   - 빛의 프리즘 던전 초정예 추격자 (명성 25,000)
			   - 구조체 특성: 물리/에너지 저항 극대화, 냉기에 의한 결합 파괴 취약
			   - 체력 5만 이상 룰 적용: 민맥 편차 2,000 이내 고정
			   -------------------------------------------------- */
			// Boss = true 삭제 (초정예 일반 몬스터)

			// [Attributes] (기본 보너스 * 1배 * 1.30) - 기본 보너스
			// Str: 보너스 약 3,437 -> 최종 Set 약 1000-1100
			this.SetStr(1000, 1100); 

			// Hits: 보너스 약 76,200 -> 최종 Set 약 23,000-25,000
			// (Hits 5만 이상 룰에 따라 민맥 편차 2,000 이내 엄격 준수)
			this.SetHits(23000, 25000); 

			this.SetDex(250, 300); // ★ 광속 추격: 매우 높은 이동/캐스팅 속도
			this.SetInt(500, 650); // 고차원 격자 구조에 담긴 압도적 마력

			SetAttackSpeed(12.0);
			SetDamage(25, 40);
			this.SetDamageType(ResistanceType.Physical, 0);
			this.SetDamageType(ResistanceType.Energy, 100);

			// [Resistances] 고밀도 결정 격자 (물리/에너지/독 특화, 냉기/화염 약점)
			this.SetResistance(ResistanceType.Physical, 70, 75); // ★ 물리 방어력 최상급
			this.SetResistance(ResistanceType.Fire, 45, 55);      
			this.SetResistance(ResistanceType.Cold, 25, 40);    // ★ 급격한 냉기에 의한 구조적 붕괴(치명적 약점)
			this.SetResistance(ResistanceType.Poison, 70, 75);  // 무기물 구조체로 독 면역 수준
			this.SetResistance(ResistanceType.Energy, 70, 75);  // ★ 에너지 응집/반사 (흡수 수준)

			// [Skills] 마법 및 전투 기술의 극치
			this.SetSkill(SkillName.Wrestling, 125.0, 140.0); 
			this.SetSkill(SkillName.Tactics, 125.0, 140.0);
			this.SetSkill(SkillName.MagicResist, 140.0, 155.0); // 마법 저항 극대화
			this.SetSkill(SkillName.Magery, 130.0, 145.0);      // 초고속 8서클 마법 난사
			this.SetSkill(SkillName.EvalInt, 130.0, 145.0);
			this.SetSkill(SkillName.Meditation, 130.0, 150.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 30; // 명성 기반 최대치 도달

			this.Fame = 25000;
			this.Karma = -25000;

            for (int i = 0; i < Utility.RandomMinMax(0, 2); i++)
            {
                this.PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }
        }

        public CrystalLatticeSeeker(Serial serial)
            : base(serial)
        {
        }

        public override void OnDeath( Container c )
        {
            base.OnDeath( c );

            if ( Utility.RandomDouble() < 0.75 )
            c.DropItem( new CrystallineFragments() );

            if ( Utility.RandomDouble() < 0.07 )
            c.DropItem( new PiecesOfCrystal() );
        }

        public override int Feathers
        {
            get
            {
                return 100;
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
            this.AddLoot(LootPack.FilthyRich, 4);
            AddLoot( LootPack.Parrot );
            this.AddLoot(LootPack.Gems);
            this.AddLoot(LootPack.HighScrolls, 2);
        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (Utility.RandomDouble() < 0.1)
                this.Drain(defender);
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            if (Utility.RandomDouble() < 0.1)
                this.Drain(attacker);
        }

        public virtual void Drain(Mobile m)
        {
            int toDrain;

            switch ( Utility.Random(3) )
            {
                case 0:
                    {
                        this.Say(1042156); // I can grant life, and I can sap it as easily.
                        this.PlaySound(0x1E6);

                        toDrain = Utility.RandomMinMax(3, 6);
                        this.Hits += toDrain;
                        m.Hits -= toDrain;
                        break;
                    }
                case 1:
                    {
                        this.Say(1042157); // You'll go nowhere, unless I deem it should be so.
                        this.PlaySound(0x1DF);

                        toDrain = Utility.RandomMinMax(10, 25);
                        this.Stam += toDrain;
                        m.Stam -= toDrain;
                        break;
                    }
                case 2:
                    {
                        this.Say(1042155); // Your power is mine to use as I will.
                        this.PlaySound(0x1F8);

                        toDrain = Utility.RandomMinMax(15, 25);
                        this.Mana += toDrain;
                        m.Mana -= toDrain;
                        break;
                    }
            }
        }

        public override int GetAttackSound()
        {
            return 0x2F6;
        }

        public override int GetDeathSound()
        {
            return 0x2F7;
        }

        public override int GetAngerSound()
        {
            return 0x2F8;
        }

        public override int GetHurtSound()
        {
            return 0x2F9;
        }

        public override int GetIdleSound()
        {
            return 0x2FA;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}