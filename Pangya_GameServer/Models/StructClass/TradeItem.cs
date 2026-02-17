using System;
using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TradeItem
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class Card
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public uint[] character = new uint[4];

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public uint[] caddie = new uint[4];

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public uint[] NPC = new uint[4];

		public ushort character_slot_count;

		public ushort caddie_slot_count;

		public ushort NPC_slot_count;

		public void clear()
		{
			Array.Clear(character, 0, character.Length);
			Array.Clear(caddie, 0, caddie.Length);
			Array.Clear(NPC, 0, NPC.Length);
			character_slot_count = 0;
			caddie_slot_count = 0;
			NPC_slot_count = 0;
		}
	}

	public uint _typeid;

	public int id;

	public int qntd;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	public byte[] ucUnknown3 = new byte[3];

	public ulong pang;

	public uint upgrade_custo;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public ushort[] c = new ushort[5];

	public ushort usUnknown;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
	public string sd_idx = "";

	public short sd_seq;

	public byte sd_status;

	[MarshalAs(UnmanagedType.Struct)]
	public Card card = new Card();

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
	public string sd_name = "";

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
	public string sd_copier_nick = "";

	public void clear()
	{
		_typeid = 0u;
		id = -1;
		qntd = 0;
		Array.Clear(ucUnknown3, 0, ucUnknown3.Length);
		pang = 0uL;
		upgrade_custo = 0u;
		Array.Clear(c, 0, c.Length);
		usUnknown = 0;
		sd_idx = "";
		sd_seq = 0;
		sd_status = 0;
		card.clear();
		sd_name = "";
		sd_copier_nick = "";
	}

	public TradeItem()
	{
		clear();
	}

	public TradeItem(TradeItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		clear();
		_typeid = item._typeid;
		id = item.id;
		qntd = item.qntd;
		Array.Copy(item.ucUnknown3, ucUnknown3, ucUnknown3.Length);
		pang = item.pang;
		upgrade_custo = item.upgrade_custo;
		Array.Copy(item.c, c, c.Length);
		usUnknown = item.usUnknown;
		sd_idx = string.Copy(item.sd_idx ?? "");
		sd_seq = item.sd_seq;
		sd_status = item.sd_status;
		card = new Card
		{
			character = (uint[])item.card.character.Clone(),
			caddie = (uint[])item.card.caddie.Clone(),
			NPC = (uint[])item.card.NPC.Clone(),
			character_slot_count = item.card.character_slot_count,
			caddie_slot_count = item.card.caddie_slot_count,
			NPC_slot_count = item.card.NPC_slot_count
		};
		sd_name = item.sd_name;
		sd_copier_nick = item.sd_copier_nick;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(_typeid);
		p.WriteInt32(id);
		p.WriteInt32(qntd);
		p.WriteBytes(ucUnknown3, 3);
		p.WriteUInt64(pang);
		p.WriteUInt32(upgrade_custo);
		p.WriteUInt16(c);
		p.WriteUInt16(usUnknown);
		p.WriteStr(sd_idx, 9);
		p.WriteInt16(sd_seq);
		p.WriteByte(sd_status);
		p.WriteUInt32(card.character);
		p.WriteUInt32(card.caddie);
		p.WriteUInt32(card.NPC);
		p.WriteUInt16(card.character_slot_count);
		p.WriteUInt16(card.caddie_slot_count);
		p.WriteUInt16(card.NPC_slot_count);
		p.WriteStr(sd_name, 41);
		p.WriteStr(sd_copier_nick, 22);
		return p.GetBytes;
	}

	public TradeItem ToRead(packet r)
	{
		_typeid = r.ReadUInt32();
		id = r.ReadInt32();
		qntd = r.ReadInt32();
		ucUnknown3 = r.ReadBytes(3);
		pang = r.ReadUInt64();
		upgrade_custo = r.ReadUInt32();
		c = new ushort[5];
		for (int i = 0; i < 5; i++)
		{
			c[i] = r.ReadUInt16();
		}
		usUnknown = r.ReadUInt16();
		sd_idx = r.ReadPStr(9u);
		sd_seq = r.ReadInt16();
		sd_status = r.ReadByte();
		card = new Card();
		card.character = new uint[4];
		for (int j = 0; j < 4; j++)
		{
			card.character[j] = r.ReadUInt32();
		}
		card.caddie = new uint[4];
		for (int k = 0; k < 4; k++)
		{
			card.caddie[k] = r.ReadUInt32();
		}
		card.NPC = new uint[4];
		for (int l = 0; l < 4; l++)
		{
			card.NPC[l] = r.ReadUInt32();
		}
		card.character_slot_count = r.ReadUInt16();
		card.caddie_slot_count = r.ReadUInt16();
		card.NPC_slot_count = r.ReadUInt16();
		sd_name = r.ReadPStr(41u);
		sd_copier_nick = r.ReadPStr(22u);
		return this;
	}
}
