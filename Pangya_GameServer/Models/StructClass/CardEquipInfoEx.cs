using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CardEquipInfoEx : CardEquipInfo
{
	public int index = 0;

	public CardEquipInfoEx()
	{
		use_date = new SYSTEMTIME();
		end_date = new SYSTEMTIME();
	}
}
