using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ItemBuffEx : ItemBuff
{
	public long index;

	public SYSTEMTIME end_date = new SYSTEMTIME();

	public uint percent;
}
