using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MapStatisticsEx : MapStatistics
{
	public byte tipo;

	public MapStatisticsEx(uint _ul = 0u)
		: base(_ul)
	{
		tipo = 0;
	}

	public MapStatisticsEx(MapStatisticsEx _cpy)
	{
		CopyFrom(_cpy);
	}

	public MapStatisticsEx(MapStatistics _cpy)
	{
		tipo = 0;
		CopyFrom(_cpy);
	}
}
