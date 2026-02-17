using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class stItem216
{
	public byte type;

	public uint _typeid;

	public uint id;

	public uint flag_time;

	public uint qntd_ant;

	public uint qntd_dep;

	public uint qntd;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public short[] c;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
	public string ucc_idx;

	public byte seq;

	public uint card_typeid;

	public byte card_slot;

	public stItem216()
	{
		c = new short[5];
		ucc_idx = "";
	}
}
