using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class RemoveDailyQuestUser
{
	public int id;

	public uint _typeid;
}
