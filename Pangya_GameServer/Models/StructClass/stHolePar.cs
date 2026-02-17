using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public class stHolePar
{
	public sbyte par;

	public sbyte[] range_score = new sbyte[2];

	public sbyte total_shot;
}
