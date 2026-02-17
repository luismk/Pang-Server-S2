using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class stLegacyTikiShopExchangeTP
{
	public uint _typeid { get; set; }

	public int qntd { get; set; }

	public uint tp { get; set; }

	public stLegacyTikiShopExchangeTP ToRead(packet r)
	{
		_typeid = r.ReadUInt32();
		qntd = r.ReadInt32();
		tp = r.ReadUInt32();
		return this;
	}
}
