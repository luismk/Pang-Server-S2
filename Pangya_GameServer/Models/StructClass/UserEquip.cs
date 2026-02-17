using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 116)]
public class UserEquip
{
	public int caddie_id;

	public int character_id;

	public int clubset_id;

	public uint clubset_typeid;

	public uint ball_typeid;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public uint[] item_slot;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public uint[] skin_id;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public uint[] skin_typeid;

	public int mascot_id;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public uint[] poster;

	public uint m_title => skin_typeid[4];

	public UserEquip()
	{
		clear();
	}

	public void clear()
	{
		item_slot = new uint[8];
		skin_id = new uint[5];
		skin_typeid = new uint[5];
		poster = new uint[2];
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(caddie_id);
		p.WriteInt32(character_id);
		p.WriteUInt32(clubset_typeid);
		p.WriteUInt32(ball_typeid);
		p.WriteUInt32(item_slot);
		p.WriteUInt32(skin_typeid);
		return p.GetBytes;
	}
}
