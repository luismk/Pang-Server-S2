using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class AddDailyQuestUser
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string name;

	public uint _typeid;

	public uint quest_typeid;

	public uint status;
}
