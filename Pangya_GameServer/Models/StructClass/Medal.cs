using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Medal
{
	public int oid = 0;

	public uint item_typeid = 0u;

	public Medal(uint _ul = 0u)
	{
		clear();
	}

	public void clear()
	{
		oid = -1;
		item_typeid = 0u;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(oid);
		p.WriteUInt32(item_typeid);
		return p.GetBytes;
	}
}
