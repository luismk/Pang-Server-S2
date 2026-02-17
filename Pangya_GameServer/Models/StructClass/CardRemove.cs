using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CardRemove
{
	public uint char_typeid { get; set; }

	public int char_id { get; set; }

	public uint removedor_typeid { get; set; }

	public int removedor_id { get; set; }

	public uint card_slot { get; set; }

	public CardRemove ToRead(packet r)
	{
		char_typeid = r.ReadUInt32();
		char_id = r.ReadInt32();
		removedor_typeid = r.ReadUInt32();
		removedor_id = r.ReadInt32();
		card_slot = r.ReadUInt32();
		return this;
	}
}
