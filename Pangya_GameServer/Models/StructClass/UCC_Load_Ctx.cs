using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UCC_Load_Ctx
{
	public uint _typeid;

	public int id;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
	public string ucc_idx;
}
