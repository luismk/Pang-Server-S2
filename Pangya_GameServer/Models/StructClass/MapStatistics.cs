#define DEBUG
using System.Diagnostics;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MapStatistics
{
	public sbyte course;

	public uint tacada;

	public uint putt;

	public uint hole;

	public uint fairway;

	public uint hole_in;

	public uint putt_in;

	public int total_score;

	public sbyte best_score;

	public ulong best_pang;

	public uint character_typeid;

	public byte event_score;

	public MapStatistics(uint _ul = 0u)
	{
		clear(-1);
	}

	public void clear(sbyte _course = -1)
	{
		best_score = sbyte.MaxValue;
		course = _course;
	}

	public bool isRecorded()
	{
		return best_score != sbyte.MaxValue;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(character_typeid);
		p.WriteSByte(course);
		p.WriteUInt32(0);
		p.WriteUInt32(0);
		p.WriteUInt32(0);
		p.WriteUInt32(0);
		p.WriteUInt32(0);
		p.WriteUInt32(0);
		p.WriteInt32(0);
		p.WriteSByte(best_score);
		p.WriteUInt32((uint)best_pang);
		Debug.Assert(p.GetSize == 38, "MapStatistics::ToArray() is Error");
		return p.GetBytes;
	}

	public void CopyFrom(MapStatistics _cpy)
	{
		course = _cpy.course;
		tacada = _cpy.tacada;
		putt = _cpy.putt;
		hole = _cpy.hole;
		fairway = _cpy.fairway;
		hole_in = _cpy.hole_in;
		putt_in = _cpy.putt_in;
		total_score = _cpy.total_score;
		best_score = _cpy.best_score;
		best_pang = _cpy.best_pang;
		character_typeid = _cpy.character_typeid;
		event_score = _cpy.event_score;
	}
}
