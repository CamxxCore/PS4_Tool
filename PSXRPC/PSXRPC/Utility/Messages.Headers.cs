using System;
using System.Runtime.InteropServices;

namespace PSXRPC
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Data
	{
		public ulong Address;
		public ulong Size;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct NotifyMessage
	{
		[MarshalAs(UnmanagedType.BStr, SizeConst = 255)]
		public string Message;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct CallFunction
	{
		ulong Address;
		ulong RDI;
		ulong RSI;
		ulong RDX;
		ulong RCX;
		ulong R8;
		ulong R9;
	}
}
