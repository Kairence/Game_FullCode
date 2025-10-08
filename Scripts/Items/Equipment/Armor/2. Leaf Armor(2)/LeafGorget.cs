using System;

namespace Server.Items
{
    public class LeafGorget : BaseArmor
    {
        [Constructable]
        public LeafGorget()
            : base(0x2FC7)
        {
            Weight = 4.0;
            PrefixOption[50] = 1;    //세트 옵션 번호
            PrefixOption[61] = 41;   //시전 속도%
            SuffixOption[61] = 50000; //5%
            PrefixOption[62] = 6;    //마나
            SuffixOption[62] = 1000000; //100
        }

        public LeafGorget(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 750;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 100;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 100;
            }
        }
        public override int ArmorBase
        {
            get
            {
                return 1;
            }
        }
        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Cloth;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
            }
        }
        public override ArmorMeditationAllowance DefMedAllowance
        {
            get
            {
                return ArmorMeditationAllowance.All;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}