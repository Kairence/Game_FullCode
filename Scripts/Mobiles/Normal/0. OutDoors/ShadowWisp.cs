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

            this.SetStr(1, 10);
			this.SetDex(141, 191); // 최종 Dex ~400 (빠름)

			this.SetHits(16, 116); // 최종 Hits 1,000~1,100
			this.SetStam(41, 91);
			this.SetMana(500, 800);

			SetAttackSpeed(2.5); // 정령 특유의 빠른 리듬
			SetDamage(14, 20); // 에너지 저항이 낮으면 초보 법사에게 치명적

			this.SetDamageType(ResistanceType.Physical, 0);
			this.SetDamageType(ResistanceType.Energy, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);
			this.SetResistance(ResistanceType.Energy, 45, 50);

			this.Fame = 500;
			this.Karma = -500;
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