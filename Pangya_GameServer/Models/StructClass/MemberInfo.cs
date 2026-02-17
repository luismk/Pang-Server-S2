#define DEBUG
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using PangyaAPI.Network.Models;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 108)]
public class MemberInfo
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
	private byte[] id_bytes;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
	private byte[] nick_name_bytes;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
	private byte[] guild_name_bytes;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
	public string guild_mark_img;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 35)]
	public string sComment;

	public uint school;

	[MarshalAs(UnmanagedType.Struct)]
	public uCapability capability;

	public uint gallery_uid;

	public int oid;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	public uint[] rank;

	public uint guild_uid;

	public uint guild_mark_img_no;

	[MarshalAs(UnmanagedType.Struct)]
	public uMemberInfoStateFlag state_flag;

	public ushort flag_login_time;

	[MarshalAs(UnmanagedType.Struct)]
	public PlayerPapelShopInfo papel_shop;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
	private byte[] nick_NT_bytes;

	public string id
	{
		get
		{
			return id_bytes.GetString();
		}
		set
		{
			id_bytes.SetString(value);
		}
	}

	public string nick_name
	{
		get
		{
			return nick_name_bytes.GetString();
		}
		set
		{
			nick_name_bytes.SetString(value);
		}
	}

	public string guild_name
	{
		get
		{
			return guild_name_bytes.GetString();
		}
		set
		{
			guild_name_bytes.SetString(value);
		}
	}

	public uint point_point_event { get; set; }

	public BlockFlag flag_block { get; set; }

	public uint channeling_flag { get; set; }

	public string sDisplayID
	{
		get
		{
			return nick_NT_bytes.GetString();
		}
		set
		{
			nick_NT_bytes.SetString(value);
		}
	}

	public MemberInfo()
	{
		Clear();
	}

	public void Clear()
	{
		rank = new uint[3];
		id_bytes = new byte[22];
		nick_name_bytes = new byte[22];
		guild_name_bytes = new byte[17];
		guild_mark_img = "";
		sComment = "";
		channeling_flag = 0u;
		point_point_event = 0u;
		gallery_uid = 0u;
		nick_NT_bytes = new byte[128];
		capability = new uCapability();
		state_flag = new uMemberInfoStateFlag();
		papel_shop = new PlayerPapelShopInfo();
		oid = -1;
		flag_block = new BlockFlag();
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteStr(id, 22);
		p.WriteStr(nick_name, 22);
		p.WriteStr(guild_name, 17);
		p.WriteStr(guild_mark_img, 12);
		p.WriteUInt32(school);
		p.WriteInt32(capability.ulCapability);
		p.WriteInt32(oid);
		p.WriteInt32(oid);
		p.WriteInt32(oid);
		p.WriteInt32(oid);
        p.Write(state_flag.ucByte);
        p.WriteUInt16(flag_login_time);     // 1 é primeira vez que logou, 2 já não é mais a primeira vez que fez login no server 
        p.WriteZero(8);
		//Debug.Assert(p.GetBytes.Length == 108, "MemberInfo::Build() is Error");
		File.WriteAllBytes("MemberInfo-Build.hex", p.GetBytes);
		return p.GetBytes;
	}
}
