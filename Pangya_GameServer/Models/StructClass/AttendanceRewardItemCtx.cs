using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class AttendanceRewardItemCtx
{
	public uint _typeid;

	public uint qntd;

	public byte tipo;
}
