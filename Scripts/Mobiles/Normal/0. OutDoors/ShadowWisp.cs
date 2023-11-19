using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a wisp corpse")]
    public class ShadowWisp : BaseCreature
    {
        [Constructable]
        public ShadowWisp()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.3, 0.6)
        {
            Name = "a shadow wisp";
            Body = 165;
            BaseSoundID = 466;

            SetStr(16, 40);
            SetDex(16, 45);
            SetInt(11, 25);

            SetHits(40, 45);

            SetDamage(5, 10);

            SetDamageType(ResistanceType.Physical, 100);

            Fame = 500;


            AddItem(new LightSource());

            PackBones();
        }

        public ShadowWisp(Serial serial)
            : base(serial)
        {
        }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
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