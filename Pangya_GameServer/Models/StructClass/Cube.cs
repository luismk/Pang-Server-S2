namespace Pangya_GameServer.Models;

public class Cube
{
	public enum eFLAG_LOCATION : uint
	{
		EDGE_GREEN,
		CARPET,
		AIR,
		GROUND
	}

	public enum eTYPE : uint
	{
		COIN,
		CUBE
	}

	public class stLocation
	{
		public float x;

		public float y;

		public float z;

		public stLocation()
		{
		}

		public stLocation(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	public stLocation location = new stLocation();

	public uint id = 0u;

	public eTYPE tipo;

	public uint flag_unknown = 0u;

	public eFLAG_LOCATION flag_location;

	public Cube(uint _ul = 0u)
	{
		clear();
	}

	public Cube(uint _id, eTYPE _tipo, uint _flag_unknown, eFLAG_LOCATION _flag_location, float _x, float _y, float _z)
	{
		id = _id;
		tipo = _tipo;
		flag_unknown = _flag_unknown;
		flag_location = _flag_location;
		location = new stLocation(_x, _y, _z);
	}

	public void clear()
	{
	}
}
