using System;
using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a fire ant corpse")]
    public class FireAnt : BaseCreature
    {
        [Constructable]
        public FireAnt() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a fire ant";
            Body = 738;

            /* [Fire Ant - Fame 500 / General / Weight 1.11]
			   - 스킬 200 마스터 서버의 '입문용' 밸런스 적용
			   - 가상 방어력(VirtualArmor): 0
			   -------------------------------------------------- */

			// [Attributes] 명성 500 보너스(Hits +845) + 가중치 1.11 반영
			this.SetStr(4, 6); 
			this.SetHits(80, 100); 
			this.SetDex(1, 2);
			this.SetInt(1, 2);

			SetAttackSpeed(1.8);
			SetDamage(6, 10);

			// [Damage Types] 80% 물리 + 20% 화염
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Fire, 20);

			// [Resistances] 저레벨용 저항 (총합 약 100 내외)
			this.SetResistance(ResistanceType.Physical, 10, 15);
			this.SetResistance(ResistanceType.Fire, 40, 50);      // 불개미 정체성 유지
			this.SetResistance(ResistanceType.Cold, 0);          // 냉기에 매우 취약
			this.SetResistance(ResistanceType.Poison, 10, 15);
			this.SetResistance(ResistanceType.Energy, 10, 15);

			// [Skills] ★ 스킬 200 서버 기준 - 초보자 사냥용 (대폭 하향)
			// 유저 스킬 20~40 단계에서 수련하기 적합한 수치
			this.SetSkill(SkillName.Wrestling, 15.0, 25.0); 
			this.SetSkill(SkillName.Tactics, 15.0, 25.0);
			this.SetSkill(SkillName.Anatomy, 10.0, 20.0);
			this.SetSkill(SkillName.MagicResist, 10.0, 20.0);

			// [Misc]
			this.VirtualArmor = 0;
			this.Fame = 500;
			this.Karma = -500;

            SetAreaEffect(AreaEffect.ExplosiveGoo);
        }

        public FireAnt(Serial serial) : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
        }
		
		public override int TreasureMapLevel { get { return 3; } }

        public override int GetIdleSound()
        {
            return 846;
        }

        public override int GetAngerSound()
        {
            return 849;
        }

        public override int GetHurtSound()
        {
            return 852;
        }

        public override int GetDeathSound()
        {
            return 850;
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.25)
            {
                c.DropItem(new SearedFireAntGoo());
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}