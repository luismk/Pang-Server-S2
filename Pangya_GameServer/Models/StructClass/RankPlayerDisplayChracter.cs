using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

public class RankPlayerDisplayChracter
{
	public uint uid = 0u;

	public uint rank = 0u;

	public byte default_hair;

	public byte default_shirts;

	public uint[] parts_typeid = new uint[24];

	public uint[] auxparts = new uint[5];

	public uint[] parts_id = new uint[24];

	public RankPlayerDisplayChracter(uint _ul = 0u)
	{
		clear();
	}

	public void clear()
	{
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(uid);
		p.Write(rank);
		p.Write(default_hair);
		p.Write(default_shirts);
		for (int i = 0; i < 24; i++)
		{
			p.Write(parts_typeid[i]);
		}
		for (int j = 0; j < 5; j++)
		{
			p.Write(auxparts[j]);
		}
		for (int k = 0; k < 24; k++)
		{
			p.Write(parts_id[k]);
		}
		return p.GetBytes;
	}
}
