using System;
using System.Xml;
using System.Text;

namespace Server.Accounting
{
    public class AccountComment
    {
        private readonly string m_AddedBy;
        private string m_Content;
        private DateTime m_LastModified;

        public AccountComment(string addedBy, string content)
        {
            this.m_AddedBy = addedBy;
            this.m_Content = content;
            this.m_LastModified = DateTime.UtcNow;
        }

        // XmlElement 에러 해결: 파라미터 타입을 명확히 인지하도록 유지
        public AccountComment(XmlElement node)
        {
            this.m_AddedBy = Utility.GetAttribute(node, "addedBy", "empty");
            this.m_LastModified = Utility.GetXMLDateTime(Utility.GetAttribute(node, "lastModified"), DateTime.UtcNow);
            this.m_Content = Utility.GetText(node, "");
        }

        public string AddedBy => this.m_AddedBy;

        public string Content
        {
            get => this.m_Content;
            set
            {
                this.m_Content = value;
                this.m_LastModified = DateTime.UtcNow;
            }
        }

        public DateTime LastModified => this.m_LastModified;

        /// <summary>
        /// .NET 8.0 호환성을 위해 XmlTextWriter 대신 XmlWriter를 사용합니다.
        /// </summary>
        public void Save(XmlWriter xml) // XmlTextWriter -> XmlWriter 로 변경
        {
            xml.WriteStartElement("comment");

            xml.WriteAttributeString("addedBy", this.m_AddedBy);
            xml.WriteAttributeString("lastModified", XmlConvert.ToString(this.m_LastModified, XmlDateTimeSerializationMode.Utc));

            xml.WriteString(this.m_Content);

            xml.WriteEndElement();
        }
    }
}