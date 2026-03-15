using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a solen infiltrator corpse")] // TODO: Corpse name?
    public class BlackSolenInfiltratorQueen : BaseCreature, IBlackSolen
    {
        [Constructable]
        public BlackSolenInfiltratorQueen()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a black solen infiltrator";
            this.Body = 807;
            this.BaseSoundID = 959;
            this.Hue = 0x453;

			/* [Black Solen Infiltrator Queen - Fame 10,000 / General / Weight 1.23]
			   - 스킬 200 마스터 서버용 '상급 정예' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (10,000/1000) + 5 = 15
			   - 저항 밸런스: 최대 75 상한 엄격 준수
			   -------------------------------------------------- */

			// [Attributes] 명성 10,000 보너스 + 가중치 1.23 반영
			this.SetStr(200, 260); 
			this.SetHits(4800, 5400); 
			this.SetDex(40, 55);
			this.SetInt(40, 55);

			SetAttackSpeed(2.2);
			SetDamage(50, 75);

			// [Damage Types] 70% 물리 + 30% 독
			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Poison, 30);

			// [Resistances] 총합 약 235 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 60, 70);
			this.SetResistance(ResistanceType.Fire, 30, 40);
			this.SetResistance(ResistanceType.Cold, 35, 45);
			this.SetResistance(ResistanceType.Poison, 70, 75);
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] ★ 스킬 200 서버 기준 - 100(그마) 초과 유저 전용 (재설계)
			// 유저 스킬 110 ~ 140 구간 수련 및 전투에 적합
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.Anatomy, 105.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			// [Misc] 가상 방어력(Virtual Armor): (10,000/1000) + 5 = 15
			this.VirtualArmor = 15;

			this.Fame = 10000;
			this.Karma = -10000;
            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus((0.05 > Utility.RandomDouble()) ? 16 : 4));
        }

        public BlackSolenInfiltratorQueen(Serial serial)
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
            if (SolenHelper.CheckBlackFriendship(m))
                return false;
            else
                return base.IsEnemy(m);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            SolenHelper.OnBlackDamage(from);

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
