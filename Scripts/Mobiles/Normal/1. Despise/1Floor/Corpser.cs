using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a corpser corpse")]
    public class Corpser : BaseCreature
    {
        [Constructable]
        public Corpser()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a corpser";
            this.Body = 8;
            this.BaseSoundID = 684;

            this.SetStr(126, 130);
            this.SetDex(116, 125);
            this.SetInt(126, 130);

            this.SetHits(350, 374);
			SetMana(10, 15);
			SetStam(130, 135);
			SetAttackSpeed( 7.5 );

            this.SetDamage(8, 24);

            this.SetDamageType(ResistanceType.Physical, 60);
            this.SetDamageType(ResistanceType.Poison, 40);

            this.SetResistance(ResistanceType.Physical, 15, 20);
            this.SetResistance(ResistanceType.Fire, 15, 25);
            this.SetResistance(ResistanceType.Cold, 10, 20);
            this.SetResistance(ResistanceType.Poison, 20, 30);

            this.SetSkill(SkillName.MagicResist, 12.1, 15.0);
            this.SetSkill(SkillName.Tactics, 12.1, 15.0);
            this.SetSkill(SkillName.Wrestling, 12.1, 15.0);

            this.Fame = 2500;
            this.Karma = -2500;

            this.VirtualArmor = 4;

            this.PackItem(new ParasiticPlant(10));

            this.PackItem(new MandrakeRoot(3));
        }

        public Corpser(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lesser;
            }
        }
        public override bool DisallowAllMoves
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
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