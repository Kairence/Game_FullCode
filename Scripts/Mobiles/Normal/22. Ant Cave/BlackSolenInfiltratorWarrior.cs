using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a solen infiltrator corpse")]
    public class BlackSolenInfiltratorWarrior : BaseCreature, IBlackSolen
    {
        [Constructable]
        public BlackSolenInfiltratorWarrior()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a black solen infiltrator";
            this.Body = 806;
            this.BaseSoundID = 959;
            this.Hue = 0x453;

            /* [Black Solen Infiltrator Warrior - Fame 9,000 / General / Weight 1.21]
			   - 가상 방어력(VirtualArmor): (9,000/1000) + 4 = 13 (정예 보병 갑피 +4)
			   - 저항 밸런스: 최대 75 상한 엄격 준수
			   - 스킬 200 서버용 실전 숙련도 반영
			   -------------------------------------------------- */

			// [Attributes] 명성 9,000 보너스 + 가중치 1.21 반영
			this.SetStr(160, 200); 
			this.SetHits(3800, 4200); 
			this.SetDex(30, 45);
			this.SetInt(30, 45);

			SetAttackSpeed(2.0);
			SetDamage(40, 55);

			// [Damage Types] 70% 물리 + 30% 독 속성
			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Poison, 30);

			// [Resistances] 총합 약 220 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 25, 35);      // 화염 약점 유지
			this.SetResistance(ResistanceType.Cold, 30, 40);
			this.SetResistance(ResistanceType.Poison, 65, 75);   // 독 저항 특화
			this.SetResistance(ResistanceType.Energy, 25, 35);

			// [Skills] 스킬 200 서버용 (명성 9,000 기준 실전 숙련도)
			this.SetSkill(SkillName.Wrestling, 110.0, 120.0);
			this.SetSkill(SkillName.Tactics, 110.0, 120.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 105.0, 115.0);

			// [Misc] 가상 방어력(Virtual Armor): (9,000/1000) + 4 = 13
			this.VirtualArmor = 13;

			this.Fame = 9000;
			this.Karma = -9000;

            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus((0.05 > Utility.RandomDouble()) ? 13 : 3));		
        }

        public BlackSolenInfiltratorWarrior(Serial serial)
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
