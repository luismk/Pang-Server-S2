using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class RateValue
{
	public uint pang;

	public uint exp;

	public uint clubset;

	public uint rain;

	public uint treasure;

	public byte persist_rain;
}
