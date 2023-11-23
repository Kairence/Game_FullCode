using System;
using Server;

namespace Server.Items
{
    public class WindOfCorruption : Magerybook
	{
		public override bool IsArtifact { get { return true; } }
        public override int LabelNumber { get { return 1150358; } } // Wind of Corruption


        [Constructable]
        public WindOfCorruption()
        {
			//마법 치피 100%, 주피 50%, 체력 기력 마나 -100
			SuffixOption[0] = 5; //옵션 갯수
			SuffixOption[1] = 1; //유물 레벨
			
			PrefixOption[11] = 45; //옵션 종류
			SuffixOption[11] = 10000; //옵션 값
			PrefixOption[12] = 8; //옵션 종류
			SuffixOption[12] = 5000; //옵션 값
			PrefixOption[13] = 4; //옵션 종류
			SuffixOption[13] = -10000; //옵션 값
			PrefixOption[14] = 5; //옵션 종류
			SuffixOption[14] = -10000; //옵션 값
			PrefixOption[15] = 6; //옵션 종류
			SuffixOption[15] = -10000; //옵션 값

        }

        public WindOfCorruption(Serial serial)
            : base(serial)
        {
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

    public class WindOfCorruptionHuman : Bow
    {
        public override int LabelNumber { get { return 1150358; } } // Wind of Corruption

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        [Constructable]
        public WindOfCorruptionHuman()
        {
            WeaponAttributes.HitLeechStam = 40;
            Attributes.WeaponSpeed = 30;
            Attributes.WeaponDamage = 50;
            WeaponAttributes.HitLowerDefend = 40;
            AosElementDamages.Chaos = 100;
            Slayer = SlayerName.Fey;
            Hue = 1171;
        }

        public WindOfCorruptionHuman(Serial serial)
            : base(serial)
        {
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