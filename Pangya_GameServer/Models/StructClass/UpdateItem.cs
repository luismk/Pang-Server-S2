using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class UpdateItem
{
	public enum UI_TYPE : byte
	{
		CADDIE,
		CADDIE_PARTS,
		MASCOT,
		WAREHOUSE
	}

	public UI_TYPE type;

	public uint _typeid;

	public int id;

	public UpdateItem(UI_TYPE _type, uint typeid, int _id)
	{
		type = _type;
		_typeid = typeid;
		id = _id;
	}
}
