#region References
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
#endregion

namespace Ultima
{
	public sealed class SpeechList
	{
		public static List<SpeechEntry> Entries { get; set; }

		private static readonly byte[] m_Buffer = new byte[128];

		static SpeechList()
		{
			Initialize();
		}

		/// <summary>
		///     Loads speech.mul in <see cref="SpeechList.Entries" />
		/// </summary>
		public static void Initialize()
		{
			string path = Files.GetFilePath("speech.mul");
			if (path == null)
			{
				Entries = new List<SpeechEntry>(0);
				return;
			}
			Entries = new List<SpeechEntry>();
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var buffer = new byte[fs.Length];
				unsafe
				{
					int order = 0;
					fs.Read(buffer, 0, buffer.Length);
					fixed (byte* data = buffer)
					{
						byte* bindat = data;
						byte* bindatend = bindat + buffer.Length;

						while (bindat != bindatend)
						{
							var id = (short)((*bindat++ >> 8) | (*bindat++)); //Swapped Endian
							var length = (short)((*bindat++ >> 8) | (*bindat++));
							if (length > 128)
							{
								length = 128;
							}
							for (int i = 0; i < length; ++i)
							{
								m_Buffer[i] = *bindat++;
							}
							string keyword = Encoding.UTF8.GetString(m_Buffer, 0, length);
							Entries.Add(new SpeechEntry(id, keyword, order));
							++order;
						}
					}
				}
			}
		}

		/// <summary>
		///     Saves speech.mul to <see cref="FileName" />
		/// </summary>
		/// <param name="FileName"></param>
		public static void SaveSpeechList(string FileName)
		{
			Entries.Sort(new OrderComparer());
			using (var fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				using (var bin = new BinaryWriter(fs))
				{
					foreach (SpeechEntry entry in Entries)
					{
						bin.Write(NativeMethods.SwapEndian(entry.ID));
						byte[] utf8String = Encoding.UTF8.GetBytes(entry.KeyWord);
						var length = (short)utf8String.Length;
						bin.Write(NativeMethods.SwapEndian(length));
						bin.Write(utf8String);
					}
				}
			}
		}

		public static void ExportToCSV(string FileName)
		{
			using (
				var Tex = new StreamWriter(
					new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite),
					Encoding.Unicode
				)
			)
			{
				Tex.WriteLine("Order;ID;KeyWord");
				foreach (SpeechEntry entry in Entries)
				{
					Tex.WriteLine(
						String.Format(CultureInfo.CurrentCulture, "{0};{1};{2}", entry.Order, entry.ID, entry.KeyWord)
					);
				}
			}
		}

		public static void ImportFromCSV(string FileName)
		{
			Entries = new List<SpeechEntry>(0);
			if (!File.Exists(FileName))
			{
				return;
			}
			using (var sr = new StreamReader(FileName))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					if ((line = line.Trim()).Length == 0 || line.StartsWith("#", StringComparison.CurrentCulture))
					{
						continue;
					}
					if ((line.Contains("Order")) && (line.Contains("KeyWord")))
					{
						continue;
					}
					try
					{
						string[] split = line.Split(';');
						if (split.Length < 3)
						{
							continue;
						}

						int order = ConvertStringToInt(split[0]);
						int id = ConvertStringToInt(split[1]);
						string word = split[2];
						word = word.Replace("\"", "");
						Entries.Add(new SpeechEntry((short)id, word, order));
					}
					catch { }
				}
			}
		}

		public static int ConvertStringToInt(string text)
		{
			int result;
			if (text.Contains("0x"))
			{
				string convert = text.Replace("0x", "");
				int.TryParse(convert, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result);
			}
			else
			{
				int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out result);
			}

			return result;
		}

		#region SortComparer
		public class IDComparer : IComparer<SpeechEntry>
		{
			private readonly bool m_desc;

			public IDComparer(bool desc)
			{
				m_desc = desc;
			}

			public int Compare(SpeechEntry x, SpeechEntry y)
			{
				if (x.ID == y.ID)
				{
					return 0;
				}
				else if (m_desc)
				{
					return (x.ID < y.ID) ? 1 : -1;
				}
				else
				{
					return (x.ID < y.ID) ? -1 : 1;
				}
			}
		}

		public class KeyWordComparer : IComparer<SpeechEntry>
		{
			private readonly bool m_desc;

			public KeyWordComparer(bool desc)
			{
				m_desc = desc;
			}

			public int Compare(SpeechEntry x, SpeechEntry y)
			{
				if (m_desc)
				{
					return StringComparer.CurrentCulture.Compare(y.KeyWord, x.KeyWord);
				}
				else
				{
					return StringComparer.CurrentCulture.Compare(x.KeyWord, y.KeyWord);
				}
			}
		}

		public class OrderComparer : IComparer<SpeechEntry>
		{
			public int Compare(SpeechEntry x, SpeechEntry y)
			{
				if (x.Order == y.Order)
				{
					return 0;
				}
				else
				{
					return (x.Order < y.Order) ? -1 : 1;
				}
			}
		}
		#endregion
	}

	public sealed class SpeechEntry
	{
		public short ID { get; set; }
		public string KeyWord { get; set; }

		[Browsable(false)]
		public int Order { get; private set; }

		public SpeechEntry(short id, string keyword, int order)
		{
			ID = id;
			KeyWord = keyword;
			Order = order;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SpeechMul
	{
		public short id;
		public short length;
		public byte[] keyword;
	}
}
