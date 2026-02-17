namespace Pangya_GameServer.Models;

public class CubeEx : Cube
{
	public uint rate = 0u;

	public CubeEx(uint _ul = 0u)
		: base(_ul)
	{
		rate = 1u;
	}

	public CubeEx(uint _id, eTYPE _tipo, uint _flag_unknown, eFLAG_LOCATION _flag_location, float _x, float _y, float _z, uint _rate)
		: base(_id, _tipo, _flag_unknown, _flag_location, _x, _y, _z)
	{
		rate = _rate;
	}

	public new void clear()
	{
		base.clear();
		rate = 1u;
	}
}
