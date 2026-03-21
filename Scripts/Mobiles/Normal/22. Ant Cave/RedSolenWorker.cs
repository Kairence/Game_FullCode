using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a solen worker corpse")]
    public class RedSolenWorker : BaseCreature, IRedSolen
    {
        [Constructable]
        public RedSolenWorker()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a red solen worker";
            this.Body = 781;
            this.BaseSoundID = 959;

			/* [Red Solen Worker - Fame 3,000 / General / Weight 1.15]
			   - 스킬 200 마스터 서버용 '초급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (3,000/1000) - 2 = 1 (경량 갑각)
			   - 흑개미 일꾼보다 높은 체력과 화염 속성 공격 미량 가미
			   -------------------------------------------------- */

			// [Attributes] 명성 3,000 보너스 + 가중치 1.15 반영
			this.SetStr(30, 40); 
			this.SetHits(750, 900); 
			this.SetDex(5, 10);
			this.SetInt(5, 10);

			SetAttackSpeed(2.0);
			SetDamage(14, 20);

			// [Damage Types] 90% 물리 + 10% 화염 속성 (붉은 솔렌의 미열)
			this.SetDamageType(ResistanceType.Physical, 90);
			this.SetDamageType(ResistanceType.Fire, 10);

			// [Resistances] 총합 약 155 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 50, 60);      // 일꾼치고 높은 화염 저항
			this.SetResistance(ResistanceType.Cold, 10, 20);      // 냉기에 매우 취약
			this.SetResistance(ResistanceType.Poison, 30, 40);
			this.SetResistance(ResistanceType.Energy, 15, 25);

			// [Skills] ★ 스킬 200 서버 기준 - 초중반 수련용 (재설계)
			// 유저 스킬 40 ~ 60 구간에서 전투하기 적합한 수치
			this.SetSkill(SkillName.Wrestling, 35.0, 45.0); 
			this.SetSkill(SkillName.Tactics, 35.0, 45.0);
			this.SetSkill(SkillName.Anatomy, 35.0, 45.0); // 흑개미보다 높은 공격 효율
			this.SetSkill(SkillName.MagicResist, 20.0, 30.0);

			// [Misc] 가상 방어력(Virtual Armor): (3,000/1000) - 2 = 1
			this.VirtualArmor = 1;

			this.Fame = 3000;
			this.Karma = -3000;

            this.PackGold(Utility.Random(100, 180));

            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus());
        }

        public RedSolenWorker(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound()
        {
            return 0x269;
        }

        public override int GetIdleSound()
        {
            return 0x269;
        }

        public override int GetAttackSound()
        {
            return 0x186;
        }

        public override int GetHurtSound()
        {
            return 0x1BE;
        }

        public override int GetDeathSound()
        {
            return 0x8E;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Gems, Utility.RandomMinMax(1, 2));
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
