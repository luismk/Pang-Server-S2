using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class GrandPrixClear
{
	public uint _typeid;

	public uint position;

	public GrandPrixClear()
	{
	}

	public GrandPrixClear(uint typeid, int _position)
	{
		_typeid = typeid;
		position = (uint)_position;
	}
}
