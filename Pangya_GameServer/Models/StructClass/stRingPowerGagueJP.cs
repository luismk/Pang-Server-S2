using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public class stRingPowerGagueJP
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	public uint[] ring = new uint[2];

	public uint efeito { get; set; }

	public uint option { get; set; }

	public void clear()
	{
	}

	public bool isValid()
	{
		return ring[0] != 0 && ring[1] != 0;
	}
}
