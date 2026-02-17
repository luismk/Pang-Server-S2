using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CWUpRank
{
	public uint item_typeid { get; set; }

	public ushort qntd { get; set; }

	public int clubset_id { get; set; }

	public CWUpRank ToRead(packet r)
	{
		item_typeid = r.ReadUInt32();
		qntd = r.ReadUInt16();
		clubset_id = r.ReadInt32();
		return this;
	}
}
