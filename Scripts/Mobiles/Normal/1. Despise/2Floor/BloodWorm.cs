using System;
using Server.Items;
using Server.Network;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public interface IBloodCreature
    {
    }

    [CorpseName("a bloodworm corpse")]
    public class BloodWorm : BaseCreature, IBloodCreature
    {
        [Constructable]
        public BloodWorm()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Boss a bloodworm";
            Body = 287;

            SetStr(10010, 12473);
            SetDex(7280, 9290);
            SetInt(1800, 1900);

            SetHits(100000, 123220);
			SetStam(13000, 15000);
			SetMana(1000, 2500);

			SetAttackSpeed( 2.5 );

            SetDamage(460, 900);

            SetDamageType(ResistanceType.Physical, 60);
            SetDamageType(ResistanceType.Poison, 40);
				
            SetResistance(ResistanceType.Physical, 52, 55);
            SetResistance(ResistanceType.Fire, 42, 50);
            SetResistance(ResistanceType.Cold, 29, 31);
            SetResistance(ResistanceType.Poison, 69, 75);
            SetResistance(ResistanceType.Energy, 26, 27);

            SetSkill(SkillName.MagicResist, 205.0);
            SetSkill(SkillName.Tactics, 300.0);
            SetSkill(SkillName.Wrestling, 300.0);

            Fame = 19000;
            Karma = -19000;

			Boss = true;
			
            VirtualArmor = 5;
			
            SetSpecialAbility(SpecialAbility.Anemia);
        }

        public BloodWorm(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
        }

        public override int GetIdleSound()
        {
            return 1503;
        }

        public override int GetAngerSound()
        {
            return 1500;
        }

        public override int GetHurtSound()
        {
            return 1502;
        }

        public override int GetDeathSound()
        {
            return 1501;
        }

        public override void OnAfterMove(Point3D oldLocation)
        {
            base.OnAfterMove(oldLocation);

            if (Hits < HitsMax && 0.25 > Utility.RandomDouble())
            {
                Corpse toAbsorb = null;

                foreach (Item item in Map.GetItemsInRange(Location, 1))
                {
                    if (item is Corpse)
                    {
                        Corpse c = (Corpse)item;

                        if (c.ItemID == 0x2006)
                        {
                            toAbsorb = c;
                            break;
                        }
                    }
                }

                if (toAbsorb != null)
                {
                    toAbsorb.ProcessDelta();
                    toAbsorb.SendRemovePacket();
                    toAbsorb.ItemID = Utility.Random(0xECA, 9); // bone graphic
                    toAbsorb.Hue = 0;
                    toAbsorb.Direction = Direction.North;
                    toAbsorb.ProcessDelta();

                    Hits = HitsMax;

                    // * The creature drains blood from a nearby corpse to heal itself. *
                    PublicOverheadMessage(MessageType.Regular, 0x3B2, 1111699);
                }
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
