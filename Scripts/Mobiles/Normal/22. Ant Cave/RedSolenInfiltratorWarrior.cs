using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a solen infiltrator corpse")]
    public class RedSolenInfiltratorWarrior : BaseCreature, IRedSolen
    {
        [Constructable]
        public RedSolenInfiltratorWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a red solen infiltrator";
            this.Body = 782;
            this.BaseSoundID = 959;

			/* [Red Solen Infiltrator Warrior - Fame 9,000 / General / Weight 1.24]
			   - 스킬 200 마스터 서버용 '중상급 정예' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (9,000/1000) - 2 = 7 (공격 특화 경량 갑각)
			   - 흑개미보다 높은 힘(Str)과 데미지, 화염 속성 공격
			   -------------------------------------------------- */

			// [Attributes] 명성 9,000 보너스 + 가중치 1.24 반영
			this.SetStr(190, 230); 
			this.SetHits(4400, 4900); 
			this.SetDex(35, 50);
			this.SetInt(35, 50);

			// [Combat Options]
			this.SetDamage(35, 55); // 흑개미 정예(30, 50)보다 강력함
			this.SetAttackSpeed(2.2);

			// [Damage Types] 60% 물리 + 40% 화염 속성 (붉은 솔렌의 파괴력)
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Fire, 40);

			// [Resistances] 총합 약 215 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 45, 55);
			this.SetResistance(ResistanceType.Fire, 65, 75);      // 화염 저항 특화
			this.SetResistance(ResistanceType.Cold, 15, 25);      // 치명적 냉기 약점
			this.SetResistance(ResistanceType.Poison, 45, 55);
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] ★ 스킬 200 서버 기준 - 그랜드 마스터(100) 유저의 라이벌 (재설계)
			// 유저 스킬 90 ~ 120 구간 수련 및 전투에 적합
			this.SetSkill(SkillName.Wrestling, 95.0, 110.0); 
			this.SetSkill(SkillName.Tactics, 95.0, 110.0);
			this.SetSkill(SkillName.Anatomy, 95.0, 110.0); // 공격적 컨셉으로 아나토미 강화
			this.SetSkill(SkillName.MagicResist, 85.0, 100.0);

			// [Misc] 가상 방어력(Virtual Armor): (9,000/1000) - 2 = 7
			this.VirtualArmor = 7;

			this.Fame = 9000;
			this.Karma = -9000;

            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus((0.05 < Utility.RandomDouble()) ? 3 : 13));
        }

        public RedSolenInfiltratorWarrior(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound()
        {
            return 0xB5;
        }

        public override int GetIdleSound()
        {
            return 0xB5;
        }

        public override int GetAttackSound()
        {
            return 0x289;
        }

        public override int GetHurtSound()
        {
            return 0xBC;
        }

        public override int GetDeathSound()
        {
            return 0xE4;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average, 2);
            this.AddLoot(LootPack.Gems, Utility.RandomMinMax(1, 4));
        }

        public override bool IsEnemy(Mobile m)
        {
            if (SolenHelper.CheckRedFriendship(m))
                return false;
            else
                return base.IsEnemy(m);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            SolenHelper.OnRedDamage(from);

            base.OnDamage(amount, from, willKill);
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
