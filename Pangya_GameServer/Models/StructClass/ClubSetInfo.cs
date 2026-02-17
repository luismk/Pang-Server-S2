using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ClubSetInfo
{
	public int id;

	public uint _typeid;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public short[] slot_c = new short[5];

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public short[] enchant_c = new short[5];

	public void setValues(int _uid, uint id_type, short[] value)
	{
		slot_c = value;
		_typeid = id_type;
		id = _uid;
	}

	public ClubSetInfo()
	{
		slot_c = new short[5];
		enchant_c = new short[5];
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(id);
		p.WriteUInt32(_typeid);
		p.WriteInt16(slot_c);
		p.WriteInt16(enchant_c);
		return p.GetBytes;
	}
}
