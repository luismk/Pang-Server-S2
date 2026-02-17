using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CommandInfo
{
	public uint idx;

	public uint id;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public int[] arg = new int[5];

	public uint target;

	public short flag;

	public byte valid;

	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME reserveDate;

	public CommandInfo()
	{
		arg = new int[5];
		reserveDate = new SYSTEMTIME();
	}
}
