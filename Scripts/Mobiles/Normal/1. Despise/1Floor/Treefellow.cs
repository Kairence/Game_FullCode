using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a treefellow corpse")]
    public class Treefellow : BaseCreature
    {
        [Constructable]
        public Treefellow()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a treefellow";
            Body = 301;

            this.SetStr(51, 101);    // 최종 Str 750~800
			this.SetDex(32, 52);     // 느릿함
			this.SetInt(42, 92);     // 최종 Int 150~200

			this.SetHits(51, 551);   // 최종 Hits 5,000~5,500
			this.SetStam(32, 52);

			SetAttackSpeed(7.0);
			SetDamage(45, 65);

			this.SetDamageType(ResistanceType.Physical, 100);

			// 저항: 나무 껍질 방어(30%) / 화염 취약(-20%)
			this.SetResistance(ResistanceType.Physical, 25, 30);
			this.SetResistance(ResistanceType.Fire, -20, -10);
			this.SetResistance(ResistanceType.Poison, 20, 25);

			this.SetSkill(SkillName.Wrestling, 53.2, 63.2);
			this.SetSkill(SkillName.Tactics, 53.2, 63.2);

			this.VirtualArmor = 10;

			this.Fame = 2500;
			this.Karma = 0;

            PackItem(new BarkFragment(6));
        }

        public Treefellow(Serial serial)
            : base(serial)
        {
        }

        public override TribeType Tribe { get { return TribeType.Fey; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }

        public override int GetIdleSound()
        {
            return 443;
        }

        public override int GetDeathSound()
        {
            return 31;
        }

        public override int GetAttackSound()
        {
            return 672;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
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
