using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DropItem
{
	public enum eTYPE : ulong
	{
		NONE,
		NORMAL_QNTD,
		QNTD_MULTIPLE_500,
		COIN_EDGE_GREEN,
		COIN_GROUND,
		CUBE
	}

	public uint _typeid = 0u;

	public byte course;

	public byte numero_hole;

	public short qntd;

	public eTYPE type = eTYPE.NONE;

	public void clear()
	{
	}

	public DropItem()
	{
	}

	public DropItem(uint typeid, byte map, byte nhole, short _qntd, eTYPE _eTYPE)
	{
		_typeid = typeid;
		course = map;
		numero_hole = nhole;
		qntd = _qntd;
		type = _eTYPE;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(_typeid);
		p.WriteByte(course);
		p.WriteByte(numero_hole);
		p.WriteInt16(qntd);
		p.WriteUInt64((ulong)type);
		return p.GetBytes;
	}
}
