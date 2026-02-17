using System;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DailyQuestInfoUser
{
	public uint now_date;

	public uint accept_date;

	public uint current_date;

	public uint count;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	public uint[] _typeid;

	public DailyQuestInfoUser()
	{
		_typeid = new uint[3];
	}

	public DailyQuestInfoUser(DailyQuestInfoUser user)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		now_date = user.now_date;
		accept_date = user.accept_date;
		current_date = user.current_date;
		count = user.count;
		_typeid = (uint[])user._typeid.Clone();
	}

	public DailyQuestInfoUser(uint initialValue)
	{
		now_date = initialValue;
		accept_date = initialValue;
		current_date = initialValue;
		count = initialValue;
		_typeid = new uint[3];
	}
}
