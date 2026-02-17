using System;

namespace Pangya_GameServer.Models;

public class Location
{
	public float x;

	public float y;

	public float z;

	public float r;

	public void clear()
	{
		x = 0f;
		y = 0f;
		z = 0f;
		r = 0f;
	}

	public double diffXZ(Location _l)
	{
		return Math.Sqrt(Math.Pow(x - _l.x, 2.0) + Math.Pow(z - _l.z, 2.0));
	}

	public double diffXZ(ShotEndLocationData.stLocation _l)
	{
		return Math.Sqrt(Math.Pow(x - _l.x, 2.0) + Math.Pow(z - _l.z, 2.0));
	}

	public static double diffXZ(Location _l1, Location _l2)
	{
		return Math.Sqrt(Math.Pow(_l1.x - _l2.x, 2.0) + Math.Pow(_l1.z - _l2.z, 2.0));
	}

	public double diff(Cube.stLocation _l)
	{
		return Math.Sqrt(Math.Pow(x - _l.x, 2.0) + Math.Pow(y - _l.y, 2.0) + Math.Pow(z - _l.z, 2.0));
	}

	public double diff(ShotEndLocationData.stLocation _l)
	{
		return Math.Sqrt(Math.Pow(x - _l.x, 2.0) + Math.Pow(y - _l.y, 2.0) + Math.Pow(z - _l.z, 2.0));
	}

	public double diff(Location _l)
	{
		return Math.Sqrt(Math.Pow(x - _l.x, 2.0) + Math.Pow(y - _l.y, 2.0) + Math.Pow(z - _l.z, 2.0));
	}

	public static double diff(Location _l1, Location _l2)
	{
		return Math.Sqrt(Math.Pow(_l1.x - _l2.x, 2.0) + Math.Pow(_l1.y - _l2.y, 2.0) + Math.Pow(_l1.z - _l2.z, 2.0));
	}

	public static double diff(Location _l1, Cube.stLocation _l2)
	{
		return Math.Sqrt(Math.Pow(_l1.x - _l2.x, 2.0) + Math.Pow(_l1.y - _l2.y, 2.0) + Math.Pow(_l1.z - _l2.z, 2.0));
	}

	public string toString()
	{
		return "X: " + Convert.ToString(x) + " Y: " + Convert.ToString(y) + " Z: " + Convert.ToString(z) + " R: " + Convert.ToString(r);
	}

	public double diffXZ(ShotSyncData.Location _l)
	{
		return Math.Sqrt(Math.Pow(x - _l.x, 2.0) + Math.Pow(y - _l.y, 2.0) + Math.Pow(z - _l.z, 2.0));
	}

	public Location(float _x, float _y, float _z, float _r)
	{
		x = _x;
		y = _y;
		z = _z;
		r = _r;
	}

	public Location()
	{
	}

	public static implicit operator Location(ShotSyncData.Location loc)
	{
		return new Location
		{
			x = loc.x,
			y = loc.y,
			z = loc.z
		};
	}
}
