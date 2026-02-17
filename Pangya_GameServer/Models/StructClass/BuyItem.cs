using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class BuyItem
{
	public int id;

	public uint _typeid;

	public short time;

	public short ItemType;

	public uint qntd;

	public uint pang;

	public uint cookie;

	public void clear()
	{
		id = 0;
		_typeid = 0u;
		time = 0;
		ItemType = 0;
		qntd = 0u;
		pang = 0u;
		cookie = 0u;
	}

	public BuyItem ToRead(packet r)
	{
		_typeid = r.ReadUInt32();
		time = r.ReadInt16();
		return this;
	}
}
