using System;

namespace Server.Items
{
    public class StormCaller : Spear
	{
		public override bool IsArtifact { get { return true; } }
		public override int LabelNumber { get { return 1113530; } } // Storm Caller
		
        [Constructable]
        public StormCaller()
            : base()
        {	
            Hue = 0x8A5;
            //WeaponAttributes.BattleLust = 1;
            Attributes.BonusStr = Utility.RandomMinMax(5, 15);
            AosElementDamages.Physical = 20;
            AosElementDamages.Fire = 20;
            AosElementDamages.Cold = 20;
            AosElementDamages.Poison = 20;
            AosElementDamages.Energy = 20;
        }

        public StormCaller(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 50;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 50;
            }
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