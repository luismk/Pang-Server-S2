using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class EmailInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class item
	{
		public int id;

		public uint _typeid;

		public byte flag_time;

		public uint qntd;

		public uint tempo_qntd;

		public ulong pang;

		public ulong cookie;

		public int gm_id;

		public uint flag_gift;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
		public string ucc_img_mark;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public byte[] ucUnknown3;

		public short type;

		public item()
		{
			clear();
		}

		public void clear()
		{
			ucc_img_mark = "";
			ucUnknown3 = new byte[3];
		}

		public byte[] ToArray()
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.WriteInt32(id);
			p.WriteUInt32(_typeid);
			p.WriteUInt16(qntd);
			return p.GetBytes;
		}

		public item ToRead(packet r)
		{
			id = r.ReadInt32();
			_typeid = r.ReadUInt32();
			flag_time = r.ReadByte();
			qntd = r.ReadUInt32();
			tempo_qntd = r.ReadUInt32();
			pang = r.ReadUInt64();
			cookie = r.ReadUInt64();
			gm_id = r.ReadInt32();
			flag_gift = r.ReadUInt32();
			ucc_img_mark = r.ReadPStr(9u);
			ucUnknown3 = r.ReadBytes(3);
			type = r.ReadInt16();
			return this;
		}

		public item(int _id, uint typeid, byte _flag_time, uint _qntd, ushort _tempo_qntd, uint _pang, uint _cookie, int _gm_id, uint _flag_gift, string _ucc_img_mark, short _type)
		{
			id = _id;
			_typeid = typeid;
			flag_time = _flag_time;
			qntd = _qntd;
			pang = _pang;
			cookie = _cookie;
			gm_id = _gm_id;
			tempo_qntd = _tempo_qntd;
			ucc_img_mark = _ucc_img_mark;
			flag_gift = _flag_gift;
			type = _type;
		}
	}

	public int id;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
	public string from_id;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
	public string gift_date;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
	public string msg;

	public byte lida_yn;

	public List<item> itens;

	public DateTime RegDate => string.IsNullOrEmpty(gift_date) ? DateTime.Now : DateTime.Parse(gift_date);

	public EmailInfo()
	{
		clear();
	}

	public void clear()
	{
		id = -1;
		lida_yn = 0;
		from_id = "";
		msg = "";
		gift_date = "";
		itens = new List<item>();
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		if (itens.Count > 0)
		{
			for (int i = 0; i < itens.Count; i++)
			{
				p.WriteBytes((itens.Count == 0) ? new byte[10] : itens[i].ToArray());
				p.WriteStr(string.IsNullOrEmpty(from_id) ? "@ADM" : from_id, 22);
				p.WriteStr(msg, 80);
				p.WriteStr(RegDate.ToString("dd/MM/yyyy"), 16);
				p.WriteStr(RegDate.ToString("dd/MM/yyyy"), 16);
			}
		}
		else
		{
			p.WriteBytes(new byte[144]);
		}
		return p.GetBytes;
	}
}
