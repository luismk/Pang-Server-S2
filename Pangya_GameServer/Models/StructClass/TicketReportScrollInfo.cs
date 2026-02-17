using System.Collections.Generic;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TicketReportScrollInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class stPlayerDados
	{
		public uint uid;

		public ulong pang;

		public ulong bonus_pang;

		public uint trofel_typeid;

		public uint exp;

		public uint mascot_typeid;

		public byte premium_user;

		public byte item_boost;

		public uint level;

		public sbyte score;

		public uMedalWin medalha;

		public byte trofel;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
		public string id;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 22)]
		public string nickname;

		public uint ulUnknown;

		public uint guild_uid;

		public uint mark_index;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
		public string guild_mark_img;

		public uint tipo;

		public byte state;

		public byte ucUnknown_flg;

		public SYSTEMTIME finish_time;

		public stPlayerDados()
		{
			clear();
		}

		public void clear()
		{
			ucUnknown_flg = 2;
		}

		public byte[] ToArray()
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.WriteUInt32(uid);
			p.WriteUInt64(pang);
			p.WriteUInt64(bonus_pang);
			p.WriteUInt32(trofel_typeid);
			p.WriteUInt32(exp);
			p.WriteUInt32(mascot_typeid);
			p.WriteByte(premium_user);
			p.WriteByte(item_boost);
			p.WriteUInt32(level);
			p.WriteSByte(score);
			p.WriteBytes(medalha.ToArray());
			p.WriteByte(trofel);
			p.WriteStr(id, 22);
			p.WriteStr(nickname, 22);
			p.WriteUInt32(ulUnknown);
			p.WriteUInt32(guild_uid);
			p.WriteUInt32(mark_index);
			p.WriteStr(guild_mark_img, 12);
			p.WriteUInt32(tipo);
			p.WriteByte(state);
			p.WriteByte(ucUnknown_flg);
			p.WriteBuffer(finish_time, 16);
			return p.GetBytes;
		}
	}

	public int id;

	public SYSTEMTIME date;

	public List<stPlayerDados> v_players;

	public void clear()
	{
		id = -1;
		date = new SYSTEMTIME();
		v_players = new List<stPlayerDados>();
	}
}
