using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class LoloCardComposeEx : LoloCardCompose
{
	public byte tipo = 0;
}
