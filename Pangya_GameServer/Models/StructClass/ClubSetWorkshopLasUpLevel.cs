using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ClubSetWorkshopLasUpLevel
{
	public int clubset_id;

	public uint stat;
}
