using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

public class HolesRain
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	protected byte[] rain = new byte[18];

	public HolesRain()
	{
		clear();
	}

	public void clear()
	{
	}

	public byte getCountHolesRainBySeq(uint _seq)
	{
		if (_seq < 1 || _seq > 18)
		{
			return 0;
		}
		byte sum = 0;
		for (uint i = 0u; i < _seq; i++)
		{
			sum += rain[i];
		}
		return sum;
	}

	public byte getCountHolesRain()
	{
		byte sum = 0;
		for (uint i = 0u; i < rain.Length; i++)
		{
			sum += rain[i];
		}
		return sum;
	}

	public void setRain(uint _index, byte _value)
	{
		if ((int)_index >= 0 && _index < 18)
		{
			rain[_index] = _value;
		}
	}
}
