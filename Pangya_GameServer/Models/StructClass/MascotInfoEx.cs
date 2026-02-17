using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class MascotInfoEx : MascotInfo
{
	public byte is_cash;

	public uint price;

	public byte need_update;

	public byte PCBang = 1;

	public MascotInfoEx()
	{
		clear();
		PCBang = 0;
	}

	public bool checkUpdate()
	{
		if (data.IsEmpty)
		{
			need_update = 1;
		}
		return need_update == 1;
	}
}
