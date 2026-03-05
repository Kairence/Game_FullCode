using System;

namespace Server.Mobiles
{
    [CorpseName("a dire wolf corpse")]
    [TypeAlias("Server.Mobiles.Direwolf")]
    public class DireWolf : BaseCreature
    {
        [Constructable]
        public DireWolf()
            : base(AIType.AI_Melee,FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a dire wolf";
            Body = 23;
            BaseSoundID = 0xE5;

            // [역산] 명성 1,200 보너스(Str+592, Hits+2298, Stam+68, Skill+3.1) 반영
			this.SetStr(108, 128);
			this.SetDex(82, 102); // 최종 Dex ~260
			this.SetInt(10, 20);

			this.SetHits(1702, 1800); // 최종 Hits 4,000~4,098
			this.SetStam(32, 52);    // 최종 Stam 100~120
			this.SetMana(0);

			SetAttackSpeed(2.2); // 늑대의 빠른 공격
			SetDamage(12, 20); // 평균 16.0

			this.SetSkill(SkillName.Wrestling, 6.9, 8.9); // 최종 10.0~12.0

			this.Fame = 1200;
			this.VirtualArmor = 3;
			this.Tamable = true;
			this.MinTameSkill = 83.1;
			SetDamageType(ResistanceType.Physical, 100);
        }

        public DireWolf(Serial serial)
            : base(serial)
        {
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m is BaseCreature && ((BaseCreature)m).IsMonster && m.Karma > 0)
            {
                return true;
            }

            return base.IsEnemy(m);
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int Hides
        {
            get
            {
                return 7;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override PackInstinct PackInstinct
        {
            get
            {
                return PackInstinct.Canine;
            }
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