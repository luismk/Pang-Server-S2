using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class stLegacyTikiShopExchangeItem
{
	public uint _typeid { get; set; }

	public int id { get; set; }

	public int qntd { get; set; }

	public uint value_tp { get; set; }

	public stLegacyTikiShopExchangeItem ToRead(packet r)
	{
		_typeid = r.ReadUInt32();
		id = r.ReadInt32();
		qntd = r.ReadInt32();
		value_tp = r.ReadUInt32();
		return this;
	}
}
