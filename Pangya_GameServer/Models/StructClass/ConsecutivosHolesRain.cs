using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ConsecutivosHolesRain
{
	[MarshalAs(UnmanagedType.Struct)]
	public HolesRain _4_pluss_count = new HolesRain();

	[MarshalAs(UnmanagedType.Struct)]
	public HolesRain _3_count = new HolesRain();

	[MarshalAs(UnmanagedType.Struct)]
	public HolesRain _2_count = new HolesRain();

	public ConsecutivosHolesRain()
	{
		clear();
	}

	public void clear()
	{
	}

	public bool isValid()
	{
		return _4_pluss_count.getCountHolesRain() > 0 || _3_count.getCountHolesRain() > 0 || _2_count.getCountHolesRain() > 0;
	}
}
