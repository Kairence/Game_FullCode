using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a beetle corpse")]
    public class FrostMite : BaseCreature
    {
        [Constructable]
        public FrostMite() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a frost mite";
            Body = 0x590;
            Female = true;

			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1152;

            SetStr(1017, 2222);
            SetDex(164, 234);
            SetInt(283, 358);

            SetHits(800, 1000);

			SetAttackSpeed( 10.0 );
            SetDamage(21, 180);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Cold, 100);

            SetResistance(ResistanceType.Physical, 60, 70);
            SetResistance(ResistanceType.Fire, 15, 25);
            SetResistance(ResistanceType.Cold, 90, 100);
            SetResistance(ResistanceType.Poison, 50, 70);
            SetResistance(ResistanceType.Energy, 40, 45);

            SetSkill(SkillName.MagicResist, 50.0, 85.0);
            SetSkill(SkillName.Tactics, 70.0, 105.0);
            SetSkill(SkillName.Wrestling, 70.0, 110.0);
            SetSkill(SkillName.DetectHidden, 60.0, 80.0);
            SetSkill(SkillName.Focus, 100.0, 115.0);

            Fame = 3500;
            Karma = -3500;

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 122.0;
        }

        public override int GetAngerSound()
        {
            return 0x4E8;
        }

        public override int GetIdleSound()
        {
            return 0x4E7;
        }

        public override int GetAttackSound()
        {
            return 0x4E6;
        }

        public override int GetHurtSound()
        {
            return 0x4E9;
        }

        public override int GetDeathSound()
        {
            return 0x4E5;
        }

        public override int Meat { get { return 5; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }

        public override bool CanAngerOnTame { get { return true; } }
        public override bool StatLossAfterTame { get { return true; } }

        public void AuraEffect(Mobile m)
        {
            m.FixedParticles(0x374A, 10, 30, 5052, Hue, 0, EffectLayer.Waist);
            m.PlaySound(0x5C6);

            m.SendLocalizedMessage(1008111, false, Name); //  : The intense cold is damaging you!
        }

        public FrostMite(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
