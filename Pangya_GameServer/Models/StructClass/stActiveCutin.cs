using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public class stActiveCutin
{
	public uint uid { get; set; }

	public uint tipo { get; set; }

	public ushort opt { get; set; }

	public uint char_typeid { get; set; }

	public byte active { get; set; }

	public void clear()
	{
	}
}
