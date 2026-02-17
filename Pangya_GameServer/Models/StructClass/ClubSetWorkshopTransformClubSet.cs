using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ClubSetWorkshopTransformClubSet
{
	public int clubset_id;

	public uint stat;

	public uint transform_typeid;
}
