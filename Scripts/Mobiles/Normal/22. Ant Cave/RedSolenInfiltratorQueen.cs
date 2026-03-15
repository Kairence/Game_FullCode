using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a solen infiltrator corpse")] // TODO: Corpse name?
    public class RedSolenInfiltratorQueen : BaseCreature, IRedSolen
    {
        [Constructable]
        public RedSolenInfiltratorQueen()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a red solen infiltrator";
            this.Body = 783;
            this.BaseSoundID = 959;

			/* [Red Solen Infiltrator Queen - Fame 10,000 / General / Weight 1.26]
			   - 스킬 200 마스터 서버용 '상급 정예' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (10,000/1000) + 0 = 10 (표준 갑피)
			   - 흑개미보다 높은 공격적 가중치(1.26)와 화염 속성 가미
			   -------------------------------------------------- */

			// [Attributes] 명성 10,000 보너스 + 가중치 1.26 반영
			this.SetStr(230, 290); 
			this.SetHits(5400, 6000); 
			this.SetDex(45, 60);
			this.SetInt(45, 60);

			SetAttackSpeed(2.2);
			SetDamage(55, 80);

			// [Damage Types] 물리 60% + 화염 40% (붉은 솔렌의 열기)
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Fire, 40);

			// [Resistances] 총합 약 230 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 65, 75);      // 화염 저항 특화
			this.SetResistance(ResistanceType.Cold, 20, 30);      // 냉기 약점
			this.SetResistance(ResistanceType.Poison, 50, 60);
			this.SetResistance(ResistanceType.Energy, 35, 45);

			// [Skills] ★ 스킬 200 서버 기준 - 그마(100) 초과 유저용 수문장
			// 유저 스킬 110 ~ 140 구간 수련 및 전투에 적합
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.Anatomy, 105.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			// [Misc] 가상 방어력(Virtual Armor): (10,000/1000) + 0 = 10
			this.VirtualArmor = 10;

			this.Fame = 10000;
			this.Karma = -10000;

            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus((0.05 < Utility.RandomDouble()) ? 4 : 16));
        }

        public RedSolenInfiltratorQueen(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound()
        {
            return 0x259;
        }

        public override int GetIdleSound()
        {
            return 0x259;
        }

        public override int GetAttackSound()
        {
            return 0x195;
        }

        public override int GetHurtSound()
        {
            return 0x250;
        }

        public override int GetDeathSound()
        {
            return 0x25B;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
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
