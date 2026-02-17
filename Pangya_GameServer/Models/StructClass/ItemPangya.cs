using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ItemPangya : ItemPangyaBase
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
	public string sd_idx;

	public uint sd_status;

	public uint sd_seq;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public byte[] unknown2;
}
