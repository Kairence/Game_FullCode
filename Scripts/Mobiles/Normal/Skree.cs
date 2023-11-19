using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skree corpse")]
    public class Skree : BaseCreature
    {
        [Constructable]
        public Skree()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a skree";
            Body = 733;

            SetStr(126, 130);
            SetDex(126, 138);
            SetInt(106, 114);

			SetStam(150, 180);
            SetHits(280, 320);
            SetMana(20, 30);

			SetAttackSpeed( 4.0 );

            SetDamage(4, 6);

            SetDamageType(ResistanceType.Physical, 100);

            Tamable = false;
            ControlSlots = 0;
            MinTameSkill = 195.1;
			
            Fame = 2500;
            Karma = -2500;
            VirtualArmor = 8;

		}

        public Skree(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get { return 3; }
        }

        public override MeatType MeatType
        {
            get { return MeatType.Bird; }
        }

        public override int Hides
        {
            get { return 5; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override int GetIdleSound()
        {
            return 1585;
        }

        public override int GetAngerSound()
        {
            return 1582;
        }

        public override int GetHurtSound()
        {
            return 1584;
        }

        public override int GetDeathSound()
        {
            return 1583;
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