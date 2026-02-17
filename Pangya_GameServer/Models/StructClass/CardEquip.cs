using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CardEquip
{
	public uint char_typeid { get; set; }

	public int char_id { get; set; }

	public uint card_typeid { get; set; }

	public int card_id { get; set; }

	public uint char_card_slot { get; set; }

	public CardEquip ToRead(packet r)
	{
		char_typeid = r.ReadUInt32();
		char_id = r.ReadInt32();
		card_typeid = r.ReadUInt32();
		card_id = r.ReadInt32();
		char_card_slot = r.ReadUInt32();
		return this;
	}
}
