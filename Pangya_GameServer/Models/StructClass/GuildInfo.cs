using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class GuildInfo
{
	public int uid;

	public byte leadder;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
	public byte[] name_Bytes;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
	public byte[] mark_img_Bytes;

	public uint index_mark_emblem;

	public ulong ull_unknown;

	public ulong pang;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] _16unknown;

	public uint point;

	public string name
	{
		get
		{
			return name_Bytes.GetString();
		}
		set
		{
			name_Bytes.SetString(value);
		}
	}

	public string mark_img
	{
		get
		{
			return mark_img_Bytes.GetString();
		}
		set
		{
			mark_img_Bytes.SetString(value);
		}
	}

	public GuildInfo()
	{
		clear();
	}

	public void clear()
	{
		name_Bytes = new byte[20];
		mark_img_Bytes = new byte[12];
		_16unknown = new byte[16];
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(uid);
		p.Write(leadder);
		p.WriteStr(name, 20);
		p.WriteStr(mark_img, 12);
		p.Write(index_mark_emblem);
		p.Write(ull_unknown);
		p.Write(pang);
		p.WriteBytes(_16unknown, 16);
		p.Write(point);
		return p.GetBytes;
	}
}
