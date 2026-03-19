using System;

using Server.Mobiles;

namespace Server.Misc
{
    public class RenameRequests
    {
        public static void Initialize()
        {
            EventSink.RenameRequest += new RenameRequestEventHandler(EventSink_RenameRequest);
        }

        private static void EventSink_RenameRequest(RenameRequestEventArgs e)
        {
            Mobile from = e.From;
            Mobile targ = e.Target;
            string name = e.Name;
			
			// ==========================================
            // [디버그용] 서버가 받은 문자열과 헥스(Hex) 코드 출력
            Console.WriteLine("\n=== [Rename Debug] ===");
            Console.WriteLine($"받은 이름(String): {name}");
            Console.WriteLine($"문자열 길이: {name.Length}");
            Console.Write("글자별 Hex 코드: ");
            foreach (char c in name)
            {
                Console.Write($"{(int)c:X4} ");
            }
            Console.WriteLine("\n======================\n");
            // ==========================================

            if (from.CanSee(targ) && from.InRange(targ, 12) && targ.CanBeRenamedBy(from))
            {
                name = name.Trim();

                var numExceptions = 0;
                var exceptions = NameVerification.Empty;

                if (targ is BaseCreature)
                {
                    exceptions = new char[] { ' ' };
                    numExceptions = 5;
                }

                // [추가] 한글 포함 여부 체크 (가~힣)
				bool containsKorean = false;
				foreach (char c in name) {
					if ((c >= 0xAC00 && c <= 0xD7A3) || (c >= 0x3131 && c <= 0x318E)) {
						containsKorean = true;
						break;
					}
				}

				// [수정] 기존 Validate 결과 혹은 한글이 포함되어 있으면 OK!
				if (containsKorean || NameVerification.Validate(name, 1, 16, true, false, true, numExceptions, exceptions, NameVerification.StartDisallowed, (Core.ML ? NameVerification.Disallowed : new string[] { })))
				{
					// ... 성공 로직 ...
					targ.Name = name;
				}
				else
				{
					from.SendMessage("That name is unacceptable.");
				}
            }
        }
    }
}
