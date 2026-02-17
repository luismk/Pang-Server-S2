using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PersonalShopItem
{
	public uint index;

	[MarshalAs(UnmanagedType.Struct)]
	public TradeItem item;

	public PersonalShopItem()
	{
		clear();
	}

	public PersonalShopItem(PersonalShopItem psi)
	{
		clear();
		index = psi.index;
		item = new TradeItem(psi.item);
	}

	public void clear()
	{
		item = new TradeItem();
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(index);
		p.WriteBytes(item.ToArray());
		return p.GetBytes;
	}

	public PersonalShopItem ToRead(packet r)
	{
		index = r.ReadUInt32();
		item = new TradeItem().ToRead(r);
		return this;
	}
}
