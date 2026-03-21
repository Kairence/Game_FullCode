using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a solen worker corpse")]
    public class BlackSolenWorker : BaseCreature, IBlackSolen
    {
        [Constructable]
        public BlackSolenWorker()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a black solen worker";
            this.Body = 805;
            this.BaseSoundID = 959;
            this.Hue = 0x453;

            /* [Black Solen Worker - Fame 3,000 / General / Weight 1.12]
			   - 스킬 200 마스터 서버용 '중하급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (3,000/1000) + 0 = 3
			   - 저항 밸런스: 최대 75 상한 엄격 준수
			   -------------------------------------------------- */

			// [Attributes] 명성 3,000 보너스 + 가중치 1.12 반영
			this.SetStr(25, 35); 
			this.SetHits(600, 750); 
			this.SetDex(5, 10);
			this.SetInt(5, 10);

			SetAttackSpeed(2.2);
			SetDamage(35, 50);

			// [Damage Types] 100% 물리 공격 (노동용 턱)
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 총합 약 160 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, 15, 25);
			this.SetResistance(ResistanceType.Cold, 20, 30);
			this.SetResistance(ResistanceType.Poison, 40, 50);
			this.SetResistance(ResistanceType.Energy, 15, 25);

			// [Skills] ★ 스킬 200 서버 기준 - 초중반 수련용 (재설계)
			// 유저 스킬 40~60 구간에서 전투하기 적합한 수치
			this.SetSkill(SkillName.Wrestling, 35.0, 45.0); 
			this.SetSkill(SkillName.Tactics, 35.0, 45.0);
			this.SetSkill(SkillName.Anatomy, 30.0, 40.0);
			this.SetSkill(SkillName.MagicResist, 25.0, 35.0);

			// [Misc] 가상 방어력(Virtual Armor): (3,000/1000) + 0 = 3
			this.VirtualArmor = 3;

			this.Fame = 3000;
			this.Karma = -3000;

            this.PackGold(Utility.Random(100, 180));

            SolenHelper.PackPicnicBasket(this);

            this.PackItem(new ZoogiFungus());
        }

        public BlackSolenWorker(Serial serial)
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
