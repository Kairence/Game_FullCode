#region References
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
#endregion

namespace Ultima
{
	public static class NativeMethods
	{
		[DllImport("User32")]
		private static extern int IsWindowNative(ClientWindowHandle window);

		[DllImport("User32")]
		private static extern int GetWindowThreadProcessIdNative(
			ClientWindowHandle window,
			ref ClientProcessHandle processID
		);

		[DllImport("Kernel32", EntryPoint = "_lread")]
		private static extern unsafe int LReadNative(SafeFileHandle hFile, void* lpBuffer, int wBytes);

		[DllImport("Kernel32")]
		private static extern ClientProcessHandle OpenProcessNative(
			int desiredAccess,
			int inheritClientHandle,
			ClientProcessHandle processID
		);

		[DllImport("Kernel32")]
		private static extern int CloseHandleNative(ClientProcessHandle handle);

		[DllImport("Kernel32")]
		private static extern unsafe int ReadProcessMemoryNative(
			ClientProcessHandle process,
			int baseAddress,
			void* buffer,
			int size,
			ref int op
		);

		[DllImport("Kernel32")]
		private static extern unsafe int WriteProcessMemoryNative(
			ClientProcessHandle process,
			int baseAddress,
			void* buffer,
			int size,
			int nullMe
		);

		[DllImport("User32")]
		private static extern int SetForegroundWindowNative(ClientWindowHandle hWnd);

		[DllImport("User32")]
		private static extern int SendMessageNative(ClientWindowHandle hWnd, int wMsg, int wParam, int lParam);

		[DllImport("User32")]
		private static extern bool PostMessageNative(ClientWindowHandle hWnd, int wMsg, int wParam, int lParam);

		[DllImport("User32")]
		private static extern int OemKeyScanNative(int wOemChar);

		[DllImport("user32", EntryPoint = "FindWindowW", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private static extern ClientWindowHandle FindWindowNative(string lpClassName, string lpWindowName);

		public static int IsWindow(ClientWindowHandle window)
		{
			return IsWindowNative(window);
		}

		public static int GetWindowThreadProcessId(ClientWindowHandle window, ref ClientProcessHandle processID)
		{
			return GetWindowThreadProcessIdNative(window, ref processID);
		}

		public static unsafe int LRead(SafeFileHandle fileHandle, void* buffer, int bytes)
		{
			return LReadNative(fileHandle, buffer, bytes);
		}

		public static ClientProcessHandle OpenProcess(
			int desiredAccess,
			int inheritClientHandle,
			ClientProcessHandle processID
		)
		{
			return OpenProcessNative(desiredAccess, inheritClientHandle, processID);
		}

		public static int CloseHandle(ClientProcessHandle handle)
		{
			return CloseHandleNative(handle);
		}

		public static unsafe int ReadProcessMemory(
			ClientProcessHandle process,
			int baseAddress,
			void* buffer,
			int size,
			ref int op
		)
		{
			return ReadProcessMemoryNative(process, baseAddress, buffer, size, ref op);
		}

		public static unsafe int WriteProcessMemory(
			ClientProcessHandle process,
			int baseAddress,
			void* buffer,
			int size,
			int nullMe
		)
		{
			return WriteProcessMemoryNative(process, baseAddress, buffer, size, nullMe);
		}

		public static int SetForegroundWindow(ClientWindowHandle window)
		{
			return SetForegroundWindowNative(window);
		}

		public static int SendMessage(ClientWindowHandle window, int message, int wParam, int lParam)
		{
			return SendMessageNative(window, message, wParam, lParam);
		}

		public static bool PostMessage(ClientWindowHandle window, int message, int wParam, int lParam)
		{
			return PostMessageNative(window, message, wParam, lParam);
		}

		public static int OemKeyScan(int oemChar)
		{
			return OemKeyScanNative(oemChar);
		}

		public static ClientWindowHandle FindWindowA(string className, string windowName)
		{
			return FindWindowNative(className, windowName);
		}

		/// <summary>
		///     Swaps from Big to LittleEndian and vise versa
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static short SwapEndian(short x)
		{
			var y = (ushort)x;
			return (short)((y >> 8) | (y << 8));
		}

		private static byte[] m_StringBuffer;

		public static unsafe string ReadNameString(byte* buffer, int len)
		{
			if ((m_StringBuffer == null) || (m_StringBuffer.Length < len))
			{
				m_StringBuffer = new byte[20];
			}
			int count;
			for (count = 0; count < len && *buffer != 0; ++count)
			{
				m_StringBuffer[count] = *buffer++;
			}

			return Encoding.Default.GetString(m_StringBuffer, 0, count);
		}

		public static string ReadNameString(byte[] buffer, int len)
		{
			int count;
			for (count = 0; count < 20 && buffer[count] != 0; ++count)
			{
				;
			}
			return Encoding.Default.GetString(buffer, 0, count);
		}
	}
}
