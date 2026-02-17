using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class EmailInfoEx : EmailInfo
{
	public uint visit_count;

	public EmailInfoEx()
	{
		clear();
		visit_count = 0u;
	}

	public EmailInfoEx(uint v)
	{
	}
}
