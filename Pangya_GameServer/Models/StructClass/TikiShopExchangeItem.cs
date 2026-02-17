using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TikiShopExchangeItem
{
	public uint _typeid { get; set; }

	public int id { get; set; }

	public uint qntd { get; set; }

	public TikiShopExchangeItem ToRead(packet r)
	{
		_typeid = r.ReadUInt32();
		id = r.ReadInt32();
		qntd = r.ReadUInt32();
		return this;
	}
}
