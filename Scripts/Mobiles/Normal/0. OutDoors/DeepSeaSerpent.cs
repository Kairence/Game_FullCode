using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a deep sea serpents corpse")]
    public class DeepSeaSerpent : BaseCreature
    {
        [Constructable]
        public DeepSeaSerpent()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a deep sea serpent";
            Body = 150;
            BaseSoundID = 447;

            Hue = Utility.Random(0x8A0, 5);

            // [역산] 명성 12,000 보너스(Str+1760, Hits+28141, Stam+365, Skill+42.0) 반영
			this.SetStr(740, 840); 
			this.SetDex(35, 85); // 최종 Dex ~500
			this.SetInt(740, 840);

			this.SetHits(31859, 33000); // 최종 Hits 60,000~61,141
			this.SetStam(135, 185);
			this.SetMana(500, 1000);

			this.SetAttackSpeed(5.0);  // 5.5초에서 5.0초로 소폭 조정 (조금 더 리드미컬하게)

			this.SetDamage(60, 95);    // [조정] 120-180에서 대폭 하향
									   // 평균 데미지: 77.5

			this.SetSkill(SkillName.Wrestling, 18.0, 28.0); // 최종 60.0~70.0
			this.SetSkill(SkillName.Tactics, 18.0, 28.0);

			this.Fame = 12000;
			this.VirtualArmor = 9; // 풀플레이트급 방어력
			
            CanSwim = true;
            CantWalk = true;

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 30, 40);
            SetResistance(ResistanceType.Fire, 70, 80);
            SetResistance(ResistanceType.Cold, 40, 50);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 15, 20);

            if (Utility.RandomBool())
                PackItem(new SulfurousAsh(20));
            else
                PackItem(new BlackPearl(20));

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public DeepSeaSerpent(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel { get { return 2; } }
        public override int Meat { get { return 10; } }
		public override int Hides { get { return 10; } }
        public override HideType HideType { get { return HideType.Horned; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
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
