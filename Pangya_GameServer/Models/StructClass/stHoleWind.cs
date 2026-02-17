using System;

namespace Pangya_GameServer.Models;

public class stHoleWind
{
	public class stDegree
	{
		protected ushort degree;

		protected ushort min_degree;

		protected const byte LIMIT_RANGE = 40;

		public stDegree()
			: this(0)
		{
		}

		public stDegree(ushort _degree)
		{
			degree = _degree;
			min_degree = 0;
			min_degree = (ushort)((degree - 20 < 0) ? ((ushort)(255 + degree - 20)) : (degree - 20));
		}

		public void setDegree(ushort _degree)
		{
			degree = _degree;
			min_degree = (ushort)((degree - 20 < 0) ? ((ushort)(255 + degree - 20)) : (degree - 20));
		}

		public ushort getDegree()
		{
			return degree;
		}

		public ushort getShuffleDegree()
		{
			degree = (ushort)((min_degree + new Random().Next() % 40) % 255);
			return degree;
		}
	}

	public byte wind;

	public stDegree degree = new stDegree();

	public stHoleWind(uint _ul = 0u)
	{
		degree = new stDegree(0);
		clear();
	}

	public stHoleWind(byte _wind, ushort _degree)
	{
		wind = _wind;
		degree = new stDegree(_degree);
	}

	public void clear()
	{
	}
}
