#region References
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
#endregion

namespace Ultima
{
	public sealed class Multis
	{
		private static MultiComponentList[] m_Components = new MultiComponentList[0x2000];
		private static FileIndex m_FileIndex = new FileIndex("Multi.idx", "Multi.mul", 0x2000, 14);

		public enum ImportType
		{
			TXT,
			UOA,
			UOAB,
			WSC,
			MULTICACHE,
			UOADESIGN,
		}

		public static bool PostHSFormat { get; set; }

		/// <summary>
		///     ReReads multi.mul
		/// </summary>
		public static void Reload()
		{
			m_FileIndex = new FileIndex("Multi.idx", "Multi.mul", 0x2000, 14);
			m_Components = new MultiComponentList[0x2000];
		}

		/// <summary>
		///     Gets <see cref="MultiComponentList" /> of multi
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static MultiComponentList GetComponents(int index)
		{
			MultiComponentList mcl;

			index &= 0x1FFF;

			if (index >= 0 && index < m_Components.Length)
			{
				mcl = m_Components[index];

				if (mcl == null)
				{
					m_Components[index] = mcl = Load(index);
				}
			}
			else
			{
				mcl = MultiComponentList.Empty;
			}

			return mcl;
		}

		public static MultiComponentList Load(int index)
		{
			try
			{
				int length,
					extra;
				bool patched;
				Stream stream = m_FileIndex.Seek(index, out length, out extra, out patched);

				if (stream == null)
				{
					return MultiComponentList.Empty;
				}

				if (PostHSFormat || Art.IsUOAHS())
				{
					return new MultiComponentList(new BinaryReader(stream), length / 16);
				}
				else
				{
					return new MultiComponentList(new BinaryReader(stream), length / 12);
				}
			}
			catch
			{
				return MultiComponentList.Empty;
			}
		}

		public static void Remove(int index)
		{
			m_Components[index] = MultiComponentList.Empty;
		}

		public static void Add(int index, MultiComponentList comp)
		{
			m_Components[index] = comp;
		}

		public static MultiComponentList ImportFromFile(int index, string FileName, ImportType type)
		{
			try
			{
				return m_Components[index] = new MultiComponentList(FileName, type);
			}
			catch
			{
				return m_Components[index] = MultiComponentList.Empty;
			}
		}

		public static MultiComponentList LoadFromFile(string FileName, ImportType type)
		{
			try
			{
				return new MultiComponentList(FileName, type);
			}
			catch
			{
				return MultiComponentList.Empty;
			}
		}

		public static List<MultiComponentList> LoadFromCache(string FileName)
		{
			var multilist = new List<MultiComponentList>();
			using (var ip = new StreamReader(FileName))
			{
				string line;
				while ((line = ip.ReadLine()) != null)
				{
					string[] split = Regex.Split(line, @"\s+");
					if (split.Length == 7)
					{
						int count = Convert.ToInt32(split[2], CultureInfo.CurrentCulture);
						multilist.Add(new MultiComponentList(ip, count));
					}
				}
			}
			return multilist;
		}

		public static string ReadUOAString(BinaryReader bin)
		{
			byte flag = bin.ReadByte();

			if (flag == 0)
			{
				return null;
			}
			else
			{
				return bin.ReadString();
			}
		}

		public static List<Object[]> LoadFromDesigner(string FileName)
		{
			var multilist = new List<Object[]>();
			string root = Path.GetFileNameWithoutExtension(FileName);
			string idx = String.Format(CultureInfo.CurrentCulture, "{0}.idx", root);
			string bin = String.Format(CultureInfo.CurrentCulture, "{0}.bin", root);
			if ((!File.Exists(idx)) || (!File.Exists(bin)))
			{
				return multilist;
			}
			using (
				FileStream idxfs = new FileStream(idx, FileMode.Open, FileAccess.Read, FileShare.Read),
					binfs = new FileStream(bin, FileMode.Open, FileAccess.Read, FileShare.Read)
			)
			{
				using (
					BinaryReader idxbin = new BinaryReader(idxfs),
						binbin = new BinaryReader(binfs)
				)
				{
					int count = idxbin.ReadInt32();
					int version = idxbin.ReadInt32();

					for (int i = 0; i < count; ++i)
					{
						var data = new Object[2];
						switch (version)
						{
							case 0:
								data[0] = ReadUOAString(idxbin);
								var arr = new List<MultiComponentList.MultiTileEntry>();
								data[0] += "-" + ReadUOAString(idxbin);
								data[0] += "-" + ReadUOAString(idxbin);
								int width = idxbin.ReadInt32();
								int height = idxbin.ReadInt32();
								int uwidth = idxbin.ReadInt32();
								int uheight = idxbin.ReadInt32();
								long filepos = idxbin.ReadInt64();
								int reccount = idxbin.ReadInt32();

								binbin.BaseStream.Seek(filepos, SeekOrigin.Begin);
								int index,
									x,
									y,
									z,
									level,
									hue;
								for (int j = 0; j < reccount; ++j)
								{
									index = x = y = z = level = hue = 0;
									int compVersion = binbin.ReadInt32();
									switch (compVersion)
									{
										case 0:
											index = binbin.ReadInt32();
											x = binbin.ReadInt32();
											y = binbin.ReadInt32();
											z = binbin.ReadInt32();
											level = binbin.ReadInt32();
											break;

										case 1:
											index = binbin.ReadInt32();
											x = binbin.ReadInt32();
											y = binbin.ReadInt32();
											z = binbin.ReadInt32();
											level = binbin.ReadInt32();
											hue = binbin.ReadInt32();
											break;
									}
									var tempitem = new MultiComponentList.MultiTileEntry();
									tempitem.ItemID = (ushort)index;
									tempitem.Flags = TileFlag.Background;
									tempitem.OffsetX = (short)x;
									tempitem.OffsetY = (short)y;
									tempitem.OffsetZ = (short)z;
									arr.Add(tempitem);
								}
								data[1] = new MultiComponentList(arr);
								break;
						}
						multilist.Add(data);
					}
				}
				return multilist;
			}
		}

		public static List<MultiComponentList.MultiTileEntry> RebuildTiles(MultiComponentList.MultiTileEntry[] tiles)
		{
			var newtiles = new List<MultiComponentList.MultiTileEntry>();
			newtiles.AddRange(tiles);

			if (newtiles[0].OffsetX == 0 && newtiles[0].OffsetY == 0 && newtiles[0].OffsetZ == 0) // found a centeritem
			{
				if (newtiles[0].ItemID != 0x1) // its a "good" one
				{
					for (int j = newtiles.Count - 1; j >= 0; --j) // remove all invis items
					{
						if (newtiles[j].ItemID == 0x1)
						{
							newtiles.RemoveAt(j);
						}
					}
					return newtiles;
				}
				else // a bad one
				{
					for (int i = 1; i < newtiles.Count; ++i) // do we have a better one?
					{
						if (
							newtiles[i].OffsetX == 0
							&& newtiles[i].OffsetY == 0
							&& newtiles[i].ItemID != 0x1
							&& newtiles[i].OffsetZ == 0
						)
						{
							MultiComponentList.MultiTileEntry centeritem = newtiles[i];
							newtiles.RemoveAt(i); // jep so save it
							for (int j = newtiles.Count - 1; j >= 0; --j) // and remove all invis
							{
								if (newtiles[j].ItemID == 0x1)
								{
									newtiles.RemoveAt(j);
								}
							}
							newtiles.Insert(0, centeritem);
							return newtiles;
						}
					}
					for (int j = newtiles.Count - 1; j >= 1; --j) // nothing found so remove all invis exept the first
					{
						if (newtiles[j].ItemID == 0x1)
						{
							newtiles.RemoveAt(j);
						}
					}
					return newtiles;
				}
			}
			for (int i = 0; i < newtiles.Count; ++i) // is there a good one
			{
				if (
					newtiles[i].OffsetX == 0
					&& newtiles[i].OffsetY == 0
					&& newtiles[i].ItemID != 0x1
					&& newtiles[i].OffsetZ == 0
				)
				{
					MultiComponentList.MultiTileEntry centeritem = newtiles[i];
					newtiles.RemoveAt(i); // store it
					for (int j = newtiles.Count - 1; j >= 0; --j) // remove all invis
					{
						if (newtiles[j].ItemID == 0x1)
						{
							newtiles.RemoveAt(j);
						}
					}
					newtiles.Insert(0, centeritem);
					return newtiles;
				}
			}
			for (int j = newtiles.Count - 1; j >= 0; --j) // nothing found so remove all invis
			{
				if (newtiles[j].ItemID == 0x1)
				{
					newtiles.RemoveAt(j);
				}
			}
			var invisitem = new MultiComponentList.MultiTileEntry();
			invisitem.ItemID = 0x1; // and create a new invis
			invisitem.OffsetX = 0;
			invisitem.OffsetY = 0;
			invisitem.OffsetZ = 0;
			invisitem.Flags = 0;
			newtiles.Insert(0, invisitem);
			return newtiles;
		}

		public static void Save(string path)
		{
			bool isUOAHS = PostHSFormat || Art.IsUOAHS();
			string idx = Path.Combine(path, "multi.idx");
			string mul = Path.Combine(path, "multi.mul");
			using (
				FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
					fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write)
			)
			{
				using (
					BinaryWriter binidx = new BinaryWriter(fsidx),
						binmul = new BinaryWriter(fsmul)
				)
				{
					for (int index = 0; index < 0x2000; ++index)
					{
						MultiComponentList comp = GetComponents(index);

						if (comp == MultiComponentList.Empty)
						{
							binidx.Write(-1); // lookup
							binidx.Write(-1); // length
							binidx.Write(-1); // extra
						}
						else
						{
							List<MultiComponentList.MultiTileEntry> tiles = RebuildTiles(comp.SortedTiles);
							binidx.Write((int)fsmul.Position); //lookup
							if (isUOAHS)
							{
								binidx.Write((tiles.Count * 16)); //length
							}
							else
							{
								binidx.Write((tiles.Count * 12)); //length
							}
							binidx.Write(-1); //extra
							for (int i = 0; i < tiles.Count; ++i)
							{
								binmul.Write(tiles[i].ItemID);
								binmul.Write(tiles[i].OffsetX);
								binmul.Write(tiles[i].OffsetY);
								binmul.Write(tiles[i].OffsetZ);

								if (isUOAHS)
								{
									binmul.Write((ulong)tiles[i].Flags);
								}
								else
								{
									binmul.Write((uint)tiles[i].Flags);
								}
							}
						}
					}
				}
			}
		}
	}

	public sealed class MultiComponentList
	{
		private Point m_Min,
			m_Max,
			m_Center;
		private int m_Width,
			m_Height;
		private readonly int m_maxHeight;
		private int m_Surface;
		private MTile[][][] m_Tiles;
		private readonly MultiTileEntry[] m_SortedTiles;

		public static readonly MultiComponentList Empty = new MultiComponentList();

		public Point Min
		{
			get { return m_Min; }
		}
		public Point Max
		{
			get { return m_Max; }
		}
		public Point Center
		{
			get { return m_Center; }
		}
		public int Width
		{
			get { return m_Width; }
		}
		public int Height
		{
			get { return m_Height; }
		}
		public MTile[][][] Tiles
		{
			get { return m_Tiles; }
		}
		public int maxHeight
		{
			get { return m_maxHeight; }
		}
		public MultiTileEntry[] SortedTiles
		{
			get { return m_SortedTiles; }
		}
		public int Surface
		{
			get { return m_Surface; }
		}

		public struct MultiTileEntry
		{
			public ushort ItemID { get; set; }
			public short OffsetX { get; set; }
			public short OffsetY { get; set; }
			public short OffsetZ { get; set; }
			public TileFlag Flags { get; set; }
		}

		/// <summary>
		///     Returns Bitmap of Multi
		/// </summary>
		/// <returns></returns>
		public Bitmap GetImage()
		{
			return GetImage(300);
		}

		/// <summary>
		///     Returns Bitmap of Multi to maxheight
		/// </summary>
		/// <param name="maxheight"></param>
		/// <returns></returns>
		public Bitmap GetImage(int maxheight)
		{
			if (m_Width == 0 || m_Height == 0)
			{
				return null;
			}

			int xMin = 1000,
				yMin = 1000;
			int xMax = -1000,
				yMax = -1000;

			for (int x = 0; x < m_Width; ++x)
			{
				for (int y = 0; y < m_Height; ++y)
				{
					MTile[] tiles = m_Tiles[x][y];

					for (int i = 0; i < tiles.Length; ++i)
					{
						Bitmap bmp = Art.GetStatic(tiles[i].ID);

						if (bmp == null)
						{
							continue;
						}

						int px = (x - y) * 22;
						int py = (x + y) * 22;

						px -= (bmp.Width / 2);
						py -= tiles[i].Z << 2;
						py -= bmp.Height;

						if (px < xMin)
						{
							xMin = px;
						}

						if (py < yMin)
						{
							yMin = py;
						}

						px += bmp.Width;
						py += bmp.Height;

						if (px > xMax)
						{
							xMax = px;
						}

						if (py > yMax)
						{
							yMax = py;
						}
					}
				}
			}

			var canvas = new Bitmap(xMax - xMin, yMax - yMin);
			Graphics gfx = Graphics.FromImage(canvas);
			gfx.Clear(Color.White);
			for (int x = 0; x < m_Width; ++x)
			{
				for (int y = 0; y < m_Height; ++y)
				{
					MTile[] tiles = m_Tiles[x][y];

					for (int i = 0; i < tiles.Length; ++i)
					{
						Bitmap bmp = Art.GetStatic(tiles[i].ID);

						if (bmp == null)
						{
							continue;
						}
						if ((tiles[i].Z) > maxheight)
						{
							continue;
						}
						int px = (x - y) * 22;
						int py = (x + y) * 22;

						px -= (bmp.Width / 2);
						py -= tiles[i].Z << 2;
						py -= bmp.Height;
						px -= xMin;
						py -= yMin;

						gfx.DrawImageUnscaled(bmp, px, py, bmp.Width, bmp.Height);
					}

					int tx = (x - y) * 22;
					int ty = (x + y) * 22;
					tx -= xMin;
					ty -= yMin;
				}
			}

			gfx.Dispose();

			return canvas;
		}

		public MultiComponentList(BinaryReader reader, int count)
		{
			bool useNewMultiFormat = Multis.PostHSFormat || Art.IsUOAHS();
			m_Min = m_Max = Point.Empty;
			m_SortedTiles = new MultiTileEntry[count];
			for (int i = 0; i < count; ++i)
			{
				m_SortedTiles[i].ItemID = Art.GetLegalItemID(reader.ReadUInt16());
				m_SortedTiles[i].OffsetX = reader.ReadInt16();
				m_SortedTiles[i].OffsetY = reader.ReadInt16();
				m_SortedTiles[i].OffsetZ = reader.ReadInt16();

				if (useNewMultiFormat)
				{
					m_SortedTiles[i].Flags = (TileFlag)reader.ReadUInt64();
				}
				else
				{
					m_SortedTiles[i].Flags = (TileFlag)reader.ReadUInt32();
				}

				MultiTileEntry e = m_SortedTiles[i];

				if (e.OffsetX < m_Min.X)
				{
					m_Min.X = e.OffsetX;
				}

				if (e.OffsetY < m_Min.Y)
				{
					m_Min.Y = e.OffsetY;
				}

				if (e.OffsetX > m_Max.X)
				{
					m_Max.X = e.OffsetX;
				}

				if (e.OffsetY > m_Max.Y)
				{
					m_Max.Y = e.OffsetY;
				}

				if (e.OffsetZ > m_maxHeight)
				{
					m_maxHeight = e.OffsetZ;
				}
			}
			ConvertList();
			reader.Close();
		}

		public MultiComponentList(string FileName, Multis.ImportType Type)
		{
			m_Min = m_Max = Point.Empty;
			int itemcount;
			switch (Type)
			{
				case Multis.ImportType.TXT:
					itemcount = 0;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						while ((line = ip.ReadLine()) != null)
						{
							itemcount++;
						}
					}
					m_SortedTiles = new MultiTileEntry[itemcount];
					itemcount = 0;
					m_Min.X = 10000;
					m_Min.Y = 10000;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						while ((line = ip.ReadLine()) != null)
						{
							string[] split = line.Split(' ');

							string tmp = split[0];
							tmp = tmp.Replace("0x", "");

							m_SortedTiles[itemcount].ItemID = ushort.Parse(
								tmp,
								NumberStyles.HexNumber,
								CultureInfo.CurrentCulture
							);
							m_SortedTiles[itemcount].OffsetX = Convert.ToInt16(split[1], CultureInfo.CurrentCulture);
							m_SortedTiles[itemcount].OffsetY = Convert.ToInt16(split[2], CultureInfo.CurrentCulture);
							m_SortedTiles[itemcount].OffsetZ = Convert.ToInt16(split[3], CultureInfo.CurrentCulture);
							m_SortedTiles[itemcount].Flags = (TileFlag)
								Convert.ToUInt64(split[4], CultureInfo.CurrentCulture);

							MultiTileEntry e = m_SortedTiles[itemcount];

							if (e.OffsetX < m_Min.X)
							{
								m_Min.X = e.OffsetX;
							}

							if (e.OffsetY < m_Min.Y)
							{
								m_Min.Y = e.OffsetY;
							}

							if (e.OffsetX > m_Max.X)
							{
								m_Max.X = e.OffsetX;
							}

							if (e.OffsetY > m_Max.Y)
							{
								m_Max.Y = e.OffsetY;
							}

							if (e.OffsetZ > m_maxHeight)
							{
								m_maxHeight = e.OffsetZ;
							}

							itemcount++;
						}
						int centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
						int centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

						m_Min = m_Max = Point.Empty;
						int i = 0;
						for (; i < m_SortedTiles.Length; i++)
						{
							m_SortedTiles[i].OffsetX -= (short)centerx;
							m_SortedTiles[i].OffsetY -= (short)centery;
							if (m_SortedTiles[i].OffsetX < m_Min.X)
							{
								m_Min.X = m_SortedTiles[i].OffsetX;
							}
							if (m_SortedTiles[i].OffsetX > m_Max.X)
							{
								m_Max.X = m_SortedTiles[i].OffsetX;
							}

							if (m_SortedTiles[i].OffsetY < m_Min.Y)
							{
								m_Min.Y = m_SortedTiles[i].OffsetY;
							}
							if (m_SortedTiles[i].OffsetY > m_Max.Y)
							{
								m_Max.Y = m_SortedTiles[i].OffsetY;
							}
						}
					}
					break;
				case Multis.ImportType.UOA:
					itemcount = 0;

					using (var ip = new StreamReader(FileName))
					{
						string line;
						while ((line = ip.ReadLine()) != null)
						{
							++itemcount;
							if (itemcount == 4)
							{
								string[] split = line.Split(' ');
								itemcount = Convert.ToInt32(split[0], CultureInfo.CurrentCulture);
								break;
							}
						}
					}
					m_SortedTiles = new MultiTileEntry[itemcount];
					itemcount = 0;
					m_Min.X = 10000;
					m_Min.Y = 10000;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						int i = -1;
						while ((line = ip.ReadLine()) != null)
						{
							++i;
							if (i < 4)
							{
								continue;
							}
							string[] split = line.Split(' ');

							m_SortedTiles[itemcount].ItemID = Convert.ToUInt16(split[0], CultureInfo.CurrentCulture);
							m_SortedTiles[itemcount].OffsetX = Convert.ToInt16(split[1], CultureInfo.CurrentCulture);
							m_SortedTiles[itemcount].OffsetY = Convert.ToInt16(split[2], CultureInfo.CurrentCulture);
							m_SortedTiles[itemcount].OffsetZ = Convert.ToInt16(split[3], CultureInfo.CurrentCulture);
							m_SortedTiles[itemcount].Flags = (TileFlag)
								Convert.ToUInt64(split[4], CultureInfo.CurrentCulture);

							MultiTileEntry e = m_SortedTiles[itemcount];

							if (e.OffsetX < m_Min.X)
							{
								m_Min.X = e.OffsetX;
							}

							if (e.OffsetY < m_Min.Y)
							{
								m_Min.Y = e.OffsetY;
							}

							if (e.OffsetX > m_Max.X)
							{
								m_Max.X = e.OffsetX;
							}

							if (e.OffsetY > m_Max.Y)
							{
								m_Max.Y = e.OffsetY;
							}

							if (e.OffsetZ > m_maxHeight)
							{
								m_maxHeight = e.OffsetZ;
							}

							++itemcount;
						}
						int centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
						int centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

						m_Min = m_Max = Point.Empty;
						i = 0;
						for (; i < m_SortedTiles.Length; ++i)
						{
							m_SortedTiles[i].OffsetX -= (short)centerx;
							m_SortedTiles[i].OffsetY -= (short)centery;
							if (m_SortedTiles[i].OffsetX < m_Min.X)
							{
								m_Min.X = m_SortedTiles[i].OffsetX;
							}
							if (m_SortedTiles[i].OffsetX > m_Max.X)
							{
								m_Max.X = m_SortedTiles[i].OffsetX;
							}

							if (m_SortedTiles[i].OffsetY < m_Min.Y)
							{
								m_Min.Y = m_SortedTiles[i].OffsetY;
							}
							if (m_SortedTiles[i].OffsetY > m_Max.Y)
							{
								m_Max.Y = m_SortedTiles[i].OffsetY;
							}
						}
					}

					break;
				case Multis.ImportType.UOAB:
					using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						using (var reader = new BinaryReader(fs))
						{
							if (reader.ReadInt16() != 1) //Version check
							{
								return;
							}
							string tmp;
							tmp = Multis.ReadUOAString(reader); //Name
							tmp = Multis.ReadUOAString(reader); //Category
							tmp = Multis.ReadUOAString(reader); //Subsection
							int width = reader.ReadInt32();
							int height = reader.ReadInt32();
							int uwidth = reader.ReadInt32();
							int uheight = reader.ReadInt32();

							int count = reader.ReadInt32();
							itemcount = count;
							m_SortedTiles = new MultiTileEntry[itemcount];
							itemcount = 0;
							m_Min.X = 10000;
							m_Min.Y = 10000;
							for (; itemcount < count; ++itemcount)
							{
								m_SortedTiles[itemcount].ItemID = (ushort)reader.ReadInt16();
								m_SortedTiles[itemcount].OffsetX = reader.ReadInt16();
								m_SortedTiles[itemcount].OffsetY = reader.ReadInt16();
								m_SortedTiles[itemcount].OffsetZ = reader.ReadInt16();
								reader.ReadInt16(); // level
								m_SortedTiles[itemcount].Flags = TileFlag.Background;
								reader.ReadInt16(); // hue

								MultiTileEntry e = m_SortedTiles[itemcount];

								if (e.OffsetX < m_Min.X)
								{
									m_Min.X = e.OffsetX;
								}

								if (e.OffsetY < m_Min.Y)
								{
									m_Min.Y = e.OffsetY;
								}

								if (e.OffsetX > m_Max.X)
								{
									m_Max.X = e.OffsetX;
								}

								if (e.OffsetY > m_Max.Y)
								{
									m_Max.Y = e.OffsetY;
								}

								if (e.OffsetZ > m_maxHeight)
								{
									m_maxHeight = e.OffsetZ;
								}
							}
							int centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
							int centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

							m_Min = m_Max = Point.Empty;
							itemcount = 0;
							for (; itemcount < m_SortedTiles.Length; ++itemcount)
							{
								m_SortedTiles[itemcount].OffsetX -= (short)centerx;
								m_SortedTiles[itemcount].OffsetY -= (short)centery;
								if (m_SortedTiles[itemcount].OffsetX < m_Min.X)
								{
									m_Min.X = m_SortedTiles[itemcount].OffsetX;
								}
								if (m_SortedTiles[itemcount].OffsetX > m_Max.X)
								{
									m_Max.X = m_SortedTiles[itemcount].OffsetX;
								}

								if (m_SortedTiles[itemcount].OffsetY < m_Min.Y)
								{
									m_Min.Y = m_SortedTiles[itemcount].OffsetY;
								}
								if (m_SortedTiles[itemcount].OffsetY > m_Max.Y)
								{
									m_Max.Y = m_SortedTiles[itemcount].OffsetY;
								}
							}
						}
					}
					break;

				case Multis.ImportType.WSC:
					itemcount = 0;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						while ((line = ip.ReadLine()) != null)
						{
							line = line.Trim();
							if (line.StartsWith("SECTION WORLDITEM", StringComparison.CurrentCulture))
							{
								++itemcount;
							}
						}
					}
					m_SortedTiles = new MultiTileEntry[itemcount];
					itemcount = 0;
					m_Min.X = 10000;
					m_Min.Y = 10000;
					using (var ip = new StreamReader(FileName))
					{
						string line;
						var tempitem = new MultiTileEntry();
						tempitem.ItemID = 0xFFFF;
						tempitem.Flags = TileFlag.Background;

						while ((line = ip.ReadLine()) != null)
						{
							line = line.Trim();
							if (line.StartsWith("SECTION WORLDITEM", StringComparison.CurrentCulture))
							{
								if (tempitem.ItemID != 0xFFFF)
								{
									m_SortedTiles[itemcount] = tempitem;
									++itemcount;
								}
								tempitem.ItemID = 0xFFFF;
							}
							else if (line.StartsWith("ID", StringComparison.CurrentCulture))
							{
								line = line.Remove(0, 2);
								line = line.Trim();
								tempitem.ItemID = Convert.ToUInt16(line, CultureInfo.CurrentCulture);
							}
							else if (line.StartsWith("X", StringComparison.CurrentCulture))
							{
								line = line.Remove(0, 1);
								line = line.Trim();
								tempitem.OffsetX = Convert.ToInt16(line, CultureInfo.CurrentCulture);
								if (tempitem.OffsetX < m_Min.X)
								{
									m_Min.X = tempitem.OffsetX;
								}
								if (tempitem.OffsetX > m_Max.X)
								{
									m_Max.X = tempitem.OffsetX;
								}
							}
							else if (line.StartsWith("Y", StringComparison.CurrentCulture))
							{
								line = line.Remove(0, 1);
								line = line.Trim();
								tempitem.OffsetY = Convert.ToInt16(line, CultureInfo.CurrentCulture);
								if (tempitem.OffsetY < m_Min.Y)
								{
									m_Min.Y = tempitem.OffsetY;
								}
								if (tempitem.OffsetY > m_Max.Y)
								{
									m_Max.Y = tempitem.OffsetY;
								}
							}
							else if (line.StartsWith("Z", StringComparison.CurrentCulture))
							{
								line = line.Remove(0, 1);
								line = line.Trim();
								tempitem.OffsetZ = Convert.ToInt16(line, CultureInfo.CurrentCulture);
								if (tempitem.OffsetZ > m_maxHeight)
								{
									m_maxHeight = tempitem.OffsetZ;
								}
							}
						}
						if (tempitem.ItemID != 0xFFFF)
						{
							m_SortedTiles[itemcount] = tempitem;
						}

						int centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
						int centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

						m_Min = m_Max = Point.Empty;
						int i = 0;
						for (; i < m_SortedTiles.Length; i++)
						{
							m_SortedTiles[i].OffsetX -= (short)centerx;
							m_SortedTiles[i].OffsetY -= (short)centery;
							if (m_SortedTiles[i].OffsetX < m_Min.X)
							{
								m_Min.X = m_SortedTiles[i].OffsetX;
							}
							if (m_SortedTiles[i].OffsetX > m_Max.X)
							{
								m_Max.X = m_SortedTiles[i].OffsetX;
							}

							if (m_SortedTiles[i].OffsetY < m_Min.Y)
							{
								m_Min.Y = m_SortedTiles[i].OffsetY;
							}
							if (m_SortedTiles[i].OffsetY > m_Max.Y)
							{
								m_Max.Y = m_SortedTiles[i].OffsetY;
							}
						}
					}
					break;
			}
			ConvertList();
		}

		public MultiComponentList(List<MultiTileEntry> arr)
		{
			m_Min = m_Max = Point.Empty;
			int itemcount = arr.Count;
			m_SortedTiles = new MultiTileEntry[itemcount];
			m_Min.X = 10000;
			m_Min.Y = 10000;
			int i = 0;
			foreach (MultiTileEntry entry in arr)
			{
				if (entry.OffsetX < m_Min.X)
				{
					m_Min.X = entry.OffsetX;
				}

				if (entry.OffsetY < m_Min.Y)
				{
					m_Min.Y = entry.OffsetY;
				}

				if (entry.OffsetX > m_Max.X)
				{
					m_Max.X = entry.OffsetX;
				}

				if (entry.OffsetY > m_Max.Y)
				{
					m_Max.Y = entry.OffsetY;
				}

				if (entry.OffsetZ > m_maxHeight)
				{
					m_maxHeight = entry.OffsetZ;
				}
				m_SortedTiles[i] = entry;

				++i;
			}
			arr.Clear();
			int centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
			int centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

			m_Min = m_Max = Point.Empty;
			for (i = 0; i < m_SortedTiles.Length; ++i)
			{
				m_SortedTiles[i].OffsetX -= (short)centerx;
				m_SortedTiles[i].OffsetY -= (short)centery;
				if (m_SortedTiles[i].OffsetX < m_Min.X)
				{
					m_Min.X = m_SortedTiles[i].OffsetX;
				}
				if (m_SortedTiles[i].OffsetX > m_Max.X)
				{
					m_Max.X = m_SortedTiles[i].OffsetX;
				}

				if (m_SortedTiles[i].OffsetY < m_Min.Y)
				{
					m_Min.Y = m_SortedTiles[i].OffsetY;
				}
				if (m_SortedTiles[i].OffsetY > m_Max.Y)
				{
					m_Max.Y = m_SortedTiles[i].OffsetY;
				}
			}
			ConvertList();
		}

		public MultiComponentList(StreamReader stream, int count)
		{
			string line;
			int itemcount = 0;
			m_Min = m_Max = Point.Empty;
			m_SortedTiles = new MultiTileEntry[count];
			m_Min.X = 10000;
			m_Min.Y = 10000;

			while ((line = stream.ReadLine()) != null)
			{
				string[] split = Regex.Split(line, @"\s+");
				m_SortedTiles[itemcount].ItemID = Convert.ToUInt16(split[0], CultureInfo.CurrentCulture);
				m_SortedTiles[itemcount].Flags = (TileFlag)Convert.ToUInt64(split[1], CultureInfo.CurrentCulture);
				m_SortedTiles[itemcount].OffsetX = Convert.ToInt16(split[2], CultureInfo.CurrentCulture);
				m_SortedTiles[itemcount].OffsetY = Convert.ToInt16(split[3], CultureInfo.CurrentCulture);
				m_SortedTiles[itemcount].OffsetZ = Convert.ToInt16(split[4], CultureInfo.CurrentCulture);

				MultiTileEntry e = m_SortedTiles[itemcount];

				if (e.OffsetX < m_Min.X)
				{
					m_Min.X = e.OffsetX;
				}
				if (e.OffsetY < m_Min.Y)
				{
					m_Min.Y = e.OffsetY;
				}
				if (e.OffsetX > m_Max.X)
				{
					m_Max.X = e.OffsetX;
				}
				if (e.OffsetY > m_Max.Y)
				{
					m_Max.Y = e.OffsetY;
				}
				if (e.OffsetZ > m_maxHeight)
				{
					m_maxHeight = e.OffsetZ;
				}

				++itemcount;
				if (itemcount == count)
				{
					break;
				}
			}
			int centerx = m_Max.X - (int)(Math.Round((m_Max.X - m_Min.X) / 2.0));
			int centery = m_Max.Y - (int)(Math.Round((m_Max.Y - m_Min.Y) / 2.0));

			m_Min = m_Max = Point.Empty;
			int i = 0;
			for (; i < m_SortedTiles.Length; i++)
			{
				m_SortedTiles[i].OffsetX -= (short)centerx;
				m_SortedTiles[i].OffsetY -= (short)centery;
				if (m_SortedTiles[i].OffsetX < m_Min.X)
				{
					m_Min.X = m_SortedTiles[i].OffsetX;
				}
				if (m_SortedTiles[i].OffsetX > m_Max.X)
				{
					m_Max.X = m_SortedTiles[i].OffsetX;
				}

				if (m_SortedTiles[i].OffsetY < m_Min.Y)
				{
					m_Min.Y = m_SortedTiles[i].OffsetY;
				}
				if (m_SortedTiles[i].OffsetY > m_Max.Y)
				{
					m_Max.Y = m_SortedTiles[i].OffsetY;
				}
			}
			ConvertList();
		}

		private void ConvertList()
		{
			m_Center = new Point(-m_Min.X, -m_Min.Y);
			m_Width = (m_Max.X - m_Min.X) + 1;
			m_Height = (m_Max.Y - m_Min.Y) + 1;

			var tiles = new MTileList[m_Width][];
			m_Tiles = new MTile[m_Width][][];

			for (int x = 0; x < m_Width; ++x)
			{
				tiles[x] = new MTileList[m_Height];
				m_Tiles[x] = new MTile[m_Height][];

				for (int y = 0; y < m_Height; ++y)
				{
					tiles[x][y] = new MTileList();
				}
			}

			for (int i = 0; i < m_SortedTiles.Length; ++i)
			{
				int xOffset = m_SortedTiles[i].OffsetX + m_Center.X;
				int yOffset = m_SortedTiles[i].OffsetY + m_Center.Y;

				tiles[xOffset]
					[yOffset]
					.Add((m_SortedTiles[i].ItemID), (sbyte)m_SortedTiles[i].OffsetZ, m_SortedTiles[i].Flags);
			}

			m_Surface = 0;

			for (int x = 0; x < m_Width; ++x)
			{
				for (int y = 0; y < m_Height; ++y)
				{
					m_Tiles[x][y] = tiles[x][y].ToArray();
					for (int i = 0; i < m_Tiles[x][y].Length; ++i)
					{
						m_Tiles[x][y][i].Solver = i;
					}
					if (m_Tiles[x][y].Length > 1)
					{
						Array.Sort(m_Tiles[x][y]);
					}
					if (m_Tiles[x][y].Length > 0)
					{
						++m_Surface;
					}
				}
			}
		}

		public MultiComponentList(MTileList[][] newtiles, int count, int width, int height)
		{
			m_Min = m_Max = Point.Empty;
			m_SortedTiles = new MultiTileEntry[count];
			m_Center = new Point((int)(Math.Round((width / 2.0))) - 1, (int)(Math.Round((height / 2.0))) - 1);
			if (m_Center.X < 0)
			{
				m_Center.X = width / 2;
			}
			if (m_Center.Y < 0)
			{
				m_Center.Y = height / 2;
			}
			m_maxHeight = -128;

			int counter = 0;
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					MTile[] tiles = newtiles[x][y].ToArray();
					for (int i = 0; i < tiles.Length; ++i)
					{
						m_SortedTiles[counter].ItemID = (tiles[i].ID);
						m_SortedTiles[counter].OffsetX = (short)(x - m_Center.X);
						m_SortedTiles[counter].OffsetY = (short)(y - m_Center.Y);
						m_SortedTiles[counter].OffsetZ = (short)(tiles[i].Z);
						m_SortedTiles[counter].Flags = tiles[i].Flag;

						if (m_SortedTiles[counter].OffsetX < m_Min.X)
						{
							m_Min.X = m_SortedTiles[counter].OffsetX;
						}
						if (m_SortedTiles[counter].OffsetX > m_Max.X)
						{
							m_Max.X = m_SortedTiles[counter].OffsetX;
						}
						if (m_SortedTiles[counter].OffsetY < m_Min.Y)
						{
							m_Min.Y = m_SortedTiles[counter].OffsetY;
						}
						if (m_SortedTiles[counter].OffsetY > m_Max.Y)
						{
							m_Max.Y = m_SortedTiles[counter].OffsetY;
						}
						if (m_SortedTiles[counter].OffsetZ > m_maxHeight)
						{
							m_maxHeight = m_SortedTiles[counter].OffsetZ;
						}
						++counter;
					}
				}
			}
			ConvertList();
		}

		private MultiComponentList()
		{
			m_Tiles = Array.Empty<MTile[][]>();
		}

		public void ExportToTextFile(string FileName)
		{
			using (
				var Tex = new StreamWriter(
					new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite),
					Encoding.GetEncoding(1252)
				)
			)
			{
				for (int i = 0; i < m_SortedTiles.Length; ++i)
				{
					Tex.WriteLine(
						String.Format(
							CultureInfo.CurrentCulture,
							"0x{0:X} {1} {2} {3} {4}",
							m_SortedTiles[i].ItemID,
							m_SortedTiles[i].OffsetX,
							m_SortedTiles[i].OffsetY,
							m_SortedTiles[i].OffsetZ,
							m_SortedTiles[i].Flags
						)
					);
				}
			}
		}

		public void ExportToWscFile(string FileName)
		{
			using (
				var Tex = new StreamWriter(
					new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite),
					Encoding.GetEncoding(1252)
				)
			)
			{
				for (int i = 0; i < m_SortedTiles.Length; ++i)
				{
					Tex.WriteLine(String.Format(CultureInfo.CurrentCulture, "SECTION WORLDITEM {0}", i));
					Tex.WriteLine("{");
					Tex.WriteLine(String.Format(CultureInfo.CurrentCulture, "\tID\t{0}", m_SortedTiles[i].ItemID));
					Tex.WriteLine(String.Format(CultureInfo.CurrentCulture, "\tX\t{0}", m_SortedTiles[i].OffsetX));
					Tex.WriteLine(String.Format(CultureInfo.CurrentCulture, "\tY\t{0}", m_SortedTiles[i].OffsetY));
					Tex.WriteLine(String.Format(CultureInfo.CurrentCulture, "\tZ\t{0}", m_SortedTiles[i].OffsetZ));
					Tex.WriteLine("\tColor\t0");
					Tex.WriteLine("}");
				}
			}
		}

		public void ExportToUOAFile(string FileName)
		{
			using (
				var Tex = new StreamWriter(
					new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite),
					Encoding.GetEncoding(1252)
				)
			)
			{
				Tex.WriteLine("6 version");
				Tex.WriteLine("1 template id");
				Tex.WriteLine("-1 item version");
				Tex.WriteLine(String.Format(CultureInfo.CurrentCulture, "{0} num components", m_SortedTiles.Length));
				for (int i = 0; i < m_SortedTiles.Length; ++i)
				{
					Tex.WriteLine(
						String.Format(
							CultureInfo.CurrentCulture,
							"{0} {1} {2} {3} {4}",
							m_SortedTiles[i].ItemID,
							m_SortedTiles[i].OffsetX,
							m_SortedTiles[i].OffsetY,
							m_SortedTiles[i].OffsetZ,
							m_SortedTiles[i].Flags
						)
					);
				}
			}
		}
	}
}
