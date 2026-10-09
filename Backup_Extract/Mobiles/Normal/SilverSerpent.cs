using System;
using Server.Factions;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a silver serpent corpse")]
    [TypeAlias("Server.Mobiles.Silverserpant")]
    public class SilverSerpent : BaseCreature
    {
        [Constructable]
        public SilverSerpent()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 92;
            Name = "a silver serpent";
            BaseSoundID = 219;
            Hue = 1150;

			/* [Silver Serpent - Fame 20,000 / General / Weight 1.30]
			   - 스킬 200 마스터 서버용 '레전더리' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (20,000/1000) + 5 = 25 (전설적인 은빛 비늘)
			   - 검은 솔렌 여왕(125~140)을 능가하는 필드 최강자 급
			   -------------------------------------------------- */

			// [Attributes] 명성 20,000 보너스 + 가중치 1.30 반영
			this.SetStr(700, 850); 
			this.SetHits(15000, 18000); 
			this.SetDex(120, 160);
			this.SetInt(120, 160);

			// [Combat Options]
			this.SetDamage(60, 90); // 한 방 한 방이 치명적
			this.SetAttackSpeed(1.4); // 전광석화보다 빠른 초고속 연타

			// [Damage Types] 40% 물리 + 60% 독 속성 (독기가 비늘을 뚫고 나옴)
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Poison, 60);

			// [Resistances] 총합 약 280 (최상위 정예 저항)
			this.SetResistance(ResistanceType.Physical, 70, 75); // 물리 완전 방어에 가까움
			this.SetResistance(ResistanceType.Fire, 40, 50);
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 75);      // 독 면역 (Max 75)
			this.SetResistance(ResistanceType.Energy, 40, 50);

			// [Skills] ★ 스킬 200 서버 기준 - 진정한 마스터(200)를 위한 도전 (재설계)
			// 유저 스킬 160 ~ 200 구간의 최종 사냥 타겟
			this.SetSkill(SkillName.Wrestling, 150.0, 180.0); 
			this.SetSkill(SkillName.Tactics, 150.0, 180.0);
			this.SetSkill(SkillName.Anatomy, 160.0, 190.0); // 급소 공격의 정점
			this.SetSkill(SkillName.MagicResist, 140.0, 160.0);
			this.SetSkill(SkillName.Poisoning, 150.0, 200.0); // 스킬 200 서버의 정점 독

			// [Misc] 가상 방어력(Virtual Armor): (20,000/1000) + 5 = 25
			this.VirtualArmor = 25;

			this.Fame = 20000;
			this.Karma = -20000;

        }

        public SilverSerpent(Serial serial)
            : base(serial)
        {
        }

        public override Faction FactionAllegiance { get { return TrueBritannians.Instance; } }
        public override Ethics.Ethic EthicAllegiance { get { return Ethics.Ethic.Hero; } }
        public override bool DeathAdderCharmable { get { return true; } }
        public override int Meat { get { return 1; } }
        public override Poison PoisonImmune { get { return Poison.Lethal; } }
        public override Poison HitPoison { get { return Poison.Lethal; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Gems, 2);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.1)
                c.DropItem(new SilverSerpentVenom());
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
