using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class GuildInfoEx : GuildInfo
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
	public string mark_emblem;

	public GuildInfoEx()
	{
		mark_emblem = "guildmark";
		clear();
	}
}
