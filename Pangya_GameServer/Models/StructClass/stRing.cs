using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public class stRing
{
	public uint _typeid { get; set; }

	public uint effect_value { get; set; }

	public byte efeito { get; set; }

	public void clear()
	{
	}
}
