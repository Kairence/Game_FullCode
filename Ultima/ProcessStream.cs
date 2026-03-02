#region References
using System;
using System.IO;
#endregion

namespace Ultima
{
	public abstract unsafe class ProcessStream : Stream
	{
		private const int ProcessAllAccess = 0x1F0FFF;

		protected bool IsOpen { get; set; }
		protected ClientProcessHandle ProcessHandle { get; set; }

		protected int ProcessPosition { get; set; }

		public abstract ClientProcessHandle ProcessID { get; }

		public virtual bool BeginAccess()
		{
			if (IsOpen)
			{
				return false;
			}

			ProcessHandle = NativeMethods.OpenProcess(ProcessAllAccess, 0, ProcessID);
			IsOpen = true;

			return true;
		}

		public virtual void EndAccess()
		{
			if (!IsOpen)
			{
				return;
			}

			ProcessHandle.Close();
			IsOpen = false;
		}

		public override void Flush() { }

		public override int Read(byte[] buffer, int offset, int count)
		{
			bool end = !BeginAccess();

			int res = 0;

			fixed (byte* p = buffer)
			{
				int readProcessMemoryResult = NativeMethods.ReadProcessMemory(
					ProcessHandle,
					ProcessPosition,
					p + offset,
					count,
					ref res
				);
				_ = readProcessMemoryResult;
			}

			ProcessPosition += count;

			if (end)
			{
				EndAccess();
			}

			return res;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			bool end = !BeginAccess();

			fixed (byte* p = buffer)
			{
				int writeProcessMemoryResult = NativeMethods.WriteProcessMemory(
					ProcessHandle,
					ProcessPosition,
					p + offset,
					count,
					0
				);
				_ = writeProcessMemoryResult;
			}

			ProcessPosition += count;

			if (end)
			{
				EndAccess();
			}
		}

		public override bool CanRead
		{
			get { return true; }
		}
		public override bool CanWrite
		{
			get { return true; }
		}
		public override bool CanSeek
		{
			get { return true; }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}
		public override long Position
		{
			get { return ProcessPosition; }
			set { ProcessPosition = (int)value; }
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					ProcessPosition = (int)offset;
					break;
				case SeekOrigin.Current:
					ProcessPosition += (int)offset;
					break;
				case SeekOrigin.End:
					throw new NotSupportedException();
			}

			return ProcessPosition;
		}
	}
}
