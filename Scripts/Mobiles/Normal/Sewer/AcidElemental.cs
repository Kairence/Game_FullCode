using System;
using Server.Items;

namespace Server.Mobiles
{
    public interface IAcidCreature
    {
    }

    [CorpseName("an acid elementals corpse")]
    public class AcidElemental : BaseCreature, IAcidCreature
    {
        [Constructable]
        public AcidElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an acid elemental";
            Body = 158;
            BaseSoundID = 263;

			/* [Acid Elemental - Fame 6,500 / Sewer / Weight 1.22]
			   - 스킬 200 마스터 서버용 '하수구 정예' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (6,500/1000) - 0.5 = 6
			   - 테이밍 난이도: 85.0 ~ 95.0 (하수구 테이머의 최종 목표)
			   -------------------------------------------------- */

			// [Attributes] 명성 6,500 보너스 + 가중치 1.22 반영
			this.SetStr(110, 150); 
			this.SetHits(2500, 3200); 
			this.SetDex(20, 30);
			this.SetInt(20, 30);

			SetAttackSpeed(10.0);
			SetDamage(15, 25);

			// [Damage Types] 40% 물리 + 60% 독 (부식성 대미지)
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Poison, 60);

			// [Resistances] 독 면역 수준의 저항과 불에 취약한 산성 성분
			this.SetResistance(ResistanceType.Physical, 35, 45); 
			this.SetResistance(ResistanceType.Fire, 10, 20);      // 가연성 물질 컨셉
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 50, 75);  // 산 그 자체이므로 독 면역급
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] 유저 스킬 80 ~ 110 구간 (GM 진입로)
			this.SetSkill(SkillName.Wrestling, 85.0, 105.0); 
			this.SetSkill(SkillName.Tactics, 85.0, 105.0);
			this.SetSkill(SkillName.MagicResist, 70.0, 90.0);
			this.SetSkill(SkillName.Poisoning, 100.0, 120.0); // 타격 시 강력한 독 효과

			// [Misc]
			this.VirtualArmor = 6;

			this.Fame = 6500;
			this.Karma = -6500;

            PackItem(new Nightshade(4));
        }

        public AcidElemental(Serial serial)
            : base(serial)
        {
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
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override double HitPoisonChance
        {
            get
            {
                return 0.75;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 2;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    Body = 158;
                    break;
            }
        }
    }
}