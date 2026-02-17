using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public class stEarcuff
{
	public uint _typeid { get; set; }

	public byte angle { get; set; }

	public float x_point_angle { get; set; }

	public void clear()
	{
	}
}
