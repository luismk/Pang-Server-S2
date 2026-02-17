using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CadieExchangeItem
{
	public uint QtyPerExchange;

	public uint _typeid { get; set; }

	public int id { get; set; }

	public CadieExchangeItem ToRead(packet r)
	{
		_typeid = r.ReadUInt32();
		id = r.ReadInt32();
		return this;
	}
}
