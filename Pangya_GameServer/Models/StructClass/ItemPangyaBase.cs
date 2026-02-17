using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ItemPangyaBase
{
	public byte tipo;

	public uint _typeid;

	public uint id;

	public uint tipo_unidade_add;

	public uint qntd_ant;

	public uint qntd_dep;

	public uint qntd;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public byte[] unknown;

	public short qntd_time;
}
