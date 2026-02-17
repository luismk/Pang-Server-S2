using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ProbCardExtra
{
	public byte active;

	public byte stat { get; set; }

	public uint prob { get; set; }
}
