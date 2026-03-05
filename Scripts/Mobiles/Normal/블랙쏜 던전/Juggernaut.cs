using System;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a juggernaut corpse")]
    public class Juggernaut : BaseCreature
    {
        [Constructable]
        public Juggernaut()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.3, 0.6)
        {
            Name = "a blackthorn juggernaut";
            Body = 768;

			/* [Juggernaut - Fame 15,000 / Normal / Weight 1.30]
			   - 파괴 불가능한 거대 중장갑 병기 (일반 정예)
			   - 기계/구조물: 테이밍 불가 (200 숙련도 고려)
			   - 특징: 압도적인 물리 저항, 느린 공속/이속, 강력한 한방
			   -------------------------------------------------- */
			// Boss = true 삭제

			// [Attributes] (기본 보너스 * 1배 * 1.30) - 기본 보너스
			// Str: 보너칭 약 1,400 -> 최종 Set 약 450-550
			this.SetStr(450, 550); 

			// Hits: 보너스 약 37,500 -> 최종 Set 약 11,000-12,500
			this.SetHits(11000, 12500); 

			this.SetDex(50, 70);   // 중장갑으로 인해 매우 느림
			this.SetInt(10, 30);   // 지능 낮음

			// [Combat Options] 묵직한 물리 타격
			this.SetDamage(55, 85);
			this.SetAttackSpeed(3.0); // 매우 느린 공격 속도
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 금속 장갑 컨셉 (물리 극대화, 에너지/부식 취약)
			this.SetResistance(ResistanceType.Physical, 70, 75); // ★ 물리 저항 극대화 (캡 준수)
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 60, 70);   // 금속이라 독에 강함
			this.SetResistance(ResistanceType.Energy, 20, 30);   // ★ 기계 장치 특성상 전기에 매우 취약

			// [Skills] 파괴적인 근접 기술
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 120.0, 130.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			// [Misc]
			this.Tamable = false; 
			this.VirtualArmor = 20;

			this.Fame = 15000;
			this.Karma = -15000;

            if (0.1 > Utility.RandomDouble())
                PackItem(new PowerCrystal());

            if (0.4 > Utility.RandomDouble())
                PackItem(new ClockworkAssembly());

            SetSpecialAbility(SpecialAbility.ColossalBlow);
        }

        public Juggernaut(Serial serial)
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
        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (0.05 > Utility.RandomDouble())
            {
                if (!IsParagon)
                {
                    if (0.75 > Utility.RandomDouble())
                        c.DropItem(DawnsMusicGear.RandomCommon);
                    else
                        c.DropItem(DawnsMusicGear.RandomUncommon);
                }
                else
                    c.DropItem(DawnsMusicGear.RandomRare);
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Gems, 1);
        }

        public override int GetDeathSound()
        {
            return 0x423;
        }

        public override int GetAttackSound()
        {
            return 0x23B;
        }

        public override int GetHurtSound()
        {
            return 0x140;
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
