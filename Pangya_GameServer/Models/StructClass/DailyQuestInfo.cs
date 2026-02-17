using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DailyQuestInfo
{
	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME date;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	public uint[] _typeid;

	public DailyQuestInfo()
	{
		clear();
	}

	public DailyQuestInfo(int _typeid_0, uint _typeid_1, uint _typeid_2, SYSTEMTIME _st)
	{
		date = _st;
		_typeid = new uint[3] { _typeid_1, _typeid_2, _typeid_2 };
	}

	public void clear()
	{
		date = new SYSTEMTIME();
		_typeid = new uint[3];
	}

	public override string ToString()
	{
		return "QUEST_TYPEID_0=" + _typeid[0] + ", QUEST_TYPEID_1=" + _typeid[1] + ", QUEST_TYPEID_2=" + _typeid[2] + ", UPDATE_DATE=" + date.ConvertTime();
	}
}
