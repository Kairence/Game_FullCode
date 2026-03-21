using System;
using System.IO;
using System.Xml;
using Server;
using Server.Mobiles;
using Server.Commands;

namespace Server.Misc
{
    public class VendorDataExtractor
    {
        private static string BasePath => Path.Combine(Core.BaseDirectory, "Data", "EconomySystem");
        private static string FilePath => Path.Combine(BasePath, "NewVendor.xml");

        public static void Initialize()
        {
            CommandSystem.Register("ExportVendors", AccessLevel.Administrator, new CommandEventHandler(ExportVendors_OnCommand));
        }

        [Usage("ExportVendors")]
        private static void ExportVendors_OnCommand(CommandEventArgs e)
        {
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);

            int count = 0; // [★ 수정] 블록 밖으로 빼서 맨 밑에서도 쓸 수 있게 함

            using (XmlTextWriter xml = new XmlTextWriter(FilePath, System.Text.Encoding.UTF8))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();
                xml.WriteStartElement("Vendors");

                foreach (Mobile m in World.Mobiles.Values)
                {
                    if (m is BaseVendor v && !(m is TownVendor) && !v.Deleted)
                    {
                        xml.WriteStartElement("Vendor");
                        xml.WriteAttributeString("Name", v.Name);
                        xml.WriteAttributeString("Map", v.Map.ToString());
                        
                        Region reg = Region.Find(v.Location, v.Map);
                        xml.WriteAttributeString("ZoneId", reg?.Name ?? "Unknown");

                        xml.WriteStartElement("Position");
                        xml.WriteElementString("X", v.X.ToString());
                        xml.WriteElementString("Y", v.Y.ToString());
                        xml.WriteElementString("Z", v.Z.ToString());
                        xml.WriteEndElement();

                        xml.WriteStartElement("Inventory");
                        var buyInfo = v.GetBuyInfo();
                        if (buyInfo != null)
                        {
                            foreach (var item in buyInfo)
                            {
                                // [★ 수정] IBuyItemInfo는 Type 속성이 없으므로 캐스팅해서 씁니다.
                                if (item is GenericBuyInfo gbi) 
                                {
                                    xml.WriteStartElement("Item");
                                    xml.WriteAttributeString("Type", gbi.Type?.Name ?? "Unknown");
                                    xml.WriteAttributeString("Price", gbi.Price.ToString());
                                    xml.WriteEndElement();
                                }
                            }
                        }
                        xml.WriteEndElement(); // Inventory
                        xml.WriteEndElement(); // Vendor
                        count++;
                    }
                }
                xml.WriteEndElement(); // Vendors
                xml.WriteEndDocument();
            }
            e.Mobile.SendMessage(68, $"총 {count}명의 데이터를 {FilePath}에 추출했습니다.");
        }
    }
}