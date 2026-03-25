using System;
using System.IO;
using System.Text;

namespace System
{
    public class ConsoleHook : TextWriter
    {
#if DEBUG
        private static readonly bool _Enabled = false;
#else
        private static readonly bool _Enabled = true;
#endif

        private static Stream m_OldOutput;
        private static bool m_Newline;

        // [수정] ASCII라는 좁은 바구니를 UTF8이라는 넓은 바구니로 교체합니다.
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8; 
            }
        }

        private string Timestamp
        {
            get
            {
                return String.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2} ", 
                    DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 
                    DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }
        }

        public static void Initialize()
        {
            if (_Enabled)
            {
                // 콘솔 자체의 출력 인코딩도 UTF8로 한 번 더 확실히 잡아줍니다.
                Console.OutputEncoding = Encoding.UTF8;

                m_OldOutput = Console.OpenStandardOutput();
                Console.SetOut(new ConsoleHook());
                m_Newline = true;
            }
        }

        public override void WriteLine(string value)
        {
            if (m_Newline)
            {
                value = this.Timestamp + value;
            }

            // 이제 UTF8로 안전하게 인코딩되어 한글이 보존됩니다.
            byte[] data = this.Encoding.GetBytes(value);
            m_OldOutput.Write(data, 0, data.Length);
            
            // 줄바꿈 문자 처리
            byte[] newline = this.Encoding.GetBytes(Environment.NewLine);
            m_OldOutput.Write(newline, 0, newline.Length);
            
            m_Newline = true;
        }

        public override void Write(string value)
        {
            if (m_Newline)
            {
                value = this.Timestamp + value;
            }

            byte[] data = this.Encoding.GetBytes(value);
            m_OldOutput.Write(data, 0, data.Length);
            m_Newline = false;
        }
    }
}