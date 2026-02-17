using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TreasureHunterInfo
{
	public sbyte course;

	public int point;

	public TreasureHunterInfo()
	{
		clear();
	}

	public void clear()
	{
		course = 0;
		point = 0;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteSByte(course);
		p.WriteInt32(point);
		return p.GetBytes;
	}
}
