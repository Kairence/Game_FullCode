using System;

namespace Server.Items
{
    [Flipable]
    public class LeatherGloves : BaseArmor, IArcaneEquip
    {
        public override int InitMinHits { get { return 100; } }
        public override int InitMaxHits { get { return 100; } }

        public override int AosStrReq { get { return 750; } }
        public override int AosDexReq { get { return 100; } }
        public override int AosIntReq { get { return 100; } }
        public override int OldStrReq { get { return 15; } }

        public override int ArmorBase { get { return 6; } }

        public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Leather; } }
        public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

        public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.All; } }

        [Constructable]
        public LeatherGloves()
            : base(0x13C6)
        {
            Weight = 8.0;
			PrefixOption[50] = 4;
			PrefixOption[61] = 114;
			SuffixOption[61] = 20000;
			PrefixOption[62] = 19;
			SuffixOption[62] = 300000;
			PrefixOption[63] = 20;
			SuffixOption[63] = 300000;
			PrefixOption[64] = 21;
			SuffixOption[64] = 300000;
        }

        public LeatherGloves(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2); // version

            if (IsArcane)
            {                
                writer.Write(true);
                writer.Write(TempHue);
                writer.Write((int)m_CurArcaneCharges);
                writer.Write((int)m_MaxArcaneCharges);
            }
            else
            {
                writer.Write(false);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        if (reader.ReadBool())
                        {
                            TempHue = reader.ReadInt();
                            m_CurArcaneCharges = reader.ReadInt();
                            m_MaxArcaneCharges = reader.ReadInt();
                        }

                        break;
                    }
                case 1:
                    {
                        if (reader.ReadBool())
                        {
                            m_CurArcaneCharges = reader.ReadInt();
                            m_MaxArcaneCharges = reader.ReadInt();
                        }

                        break;
                    }
            }
        }

        #region Arcane Impl
        private int m_MaxArcaneCharges, m_CurArcaneCharges;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxArcaneCharges
        {
            get { return m_MaxArcaneCharges; }
            set
            {
                m_MaxArcaneCharges = value;
                InvalidateProperties();
                Update();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CurArcaneCharges
        {
            get { return m_CurArcaneCharges; }
            set
            {
                m_CurArcaneCharges = value;
                InvalidateProperties();
                Update();
            }
        }

        public int TempHue { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsArcane
        {
            get
            {
                return m_MaxArcaneCharges > 0 && m_CurArcaneCharges >= 0;
            }
        }

        public void Update()
        {
            if (IsArcane)
                ItemID = 0x26B0;
            else if (ItemID == 0x26B0)
                ItemID = 0x13C6;

            if (IsArcane && CurArcaneCharges == 0)
            {
                TempHue = Hue;
                Hue = 0;
            }                
        }

        public override void AddCraftedProperties(ObjectPropertyList list)
        {
            base.AddCraftedProperties(list);

            if (IsArcane)
                list.Add(1061837, "{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges); // arcane charges: ~1_val~ / ~2_val~
        }

        public void Flip()
        {
            if (ItemID == 0x13C6)
                ItemID = 0x13CE;
            else if (ItemID == 0x13CE)
                ItemID = 0x13C6;
        }
        #endregion
    }
}
