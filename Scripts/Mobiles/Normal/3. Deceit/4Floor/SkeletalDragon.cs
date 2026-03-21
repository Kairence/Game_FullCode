using System;

namespace Server.Mobiles
{
    [CorpseName("a skeletal dragon corpse")]
    public class SkeletalDragon : BaseCreature
    {
        [Constructable]
        public SkeletalDragon()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a skeletal dragon";
            Body = 104;
            BaseSoundID = 0x488;

			Boss = true;

            /* [Skeletal Dragon - Fame 28,000 / Boss / Weight 1.28]
			   - 편차 수정: 체력 민맥 차이를 최소화 (약 2,000 차이)
			   - VirtualArmor: 30 (Max)
			   -------------------------------------------------- */

			// 최종 Str 약 26,000 (편차 1,000 이내)
			this.SetStr(21400, 22400); 

			// 최종 Hits 약 576,000 (민맥 차이를 확 줄임)
			this.SetHits(485000, 487000); 

			// 최종 Dex/Int 약 5,200
			this.SetDex(4280, 4480);
			this.SetInt(4280, 4480);

			// 최종 Stam/Mana 약 5,500
			this.SetStam(4520, 4720);
			this.SetMana(4520, 4720);

			// [Combat Options]
			SetAttackSpeed(2.5);
			SetDamage(90, 130);

			// [Resistances] 최고 저항 75 이하
			this.SetResistance(ResistanceType.Physical, 70, 75);
			this.SetResistance(ResistanceType.Fire, 30, 40);
			this.SetResistance(ResistanceType.Cold, 65, 75);
			this.SetResistance(ResistanceType.Poison, 75);
			this.SetResistance(ResistanceType.Energy, 40, 50);

			// [Skills] 최종 346.3 부근
			this.SetSkill(SkillName.Wrestling, 205.0, 215.0);
			this.SetSkill(SkillName.Tactics, 205.0, 215.0);
			this.SetSkill(SkillName.Anatomy, 205.0, 215.0);
			this.SetSkill(SkillName.MagicResist, 205.0, 215.0);

			// 가방 방어력: (28,000/1000) + 2 = 30
			this.VirtualArmor = 30;

			this.Fame = 28000;
			this.Karma = -28000;

			this.SpecialType2 = 5;
			this.SpecialChance2 = 0.45;	

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public SkeletalDragon(Serial serial)
            : base(serial)
        {
        }

        public override bool AutoDispel { get { return !Controlled; } }
        public override bool BleedImmune { get { return true; } }
        public override bool ReacquireOnMovement { get { return !Controlled; } }
        public override double BonusPetDamageScalar { get { return (Core.SE) ? 3.0 : 1.0; } }
        public override int Hides { get { return 20; } }
        public override int Meat { get { return 19; } } // where's it hiding these? :)
        public override HideType HideType { get { return HideType.Barbed; } }
        public override OppositionGroup OppositionGroup { get { return OppositionGroup.FeyAndUndead; } }
        public override Poison PoisonImmune { get { return Poison.Lethal; } }
        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 4);
            AddLoot(LootPack.Gems, 5);
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
