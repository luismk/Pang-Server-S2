using System;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

public class stPlayerState
{
	public ulong id = 0uL;

	public uint uid = 0u;

	public uint count_days = 0u;

	public uint count_seq = 0u;

	public SYSTEMTIME update_date = new SYSTEMTIME();

	public bool is_clear;

	public stPlayerState(uint _ul = 0u)
	{
		clear();
	}

	public stPlayerState(ulong _id, uint _uid, uint _count_days, uint _count_seq, SYSTEMTIME _upt_date, bool _is_clear = false)
	{
		id = _id;
		uid = _uid;
		count_days = _count_days;
		update_date = _upt_date;
		count_seq = _count_seq;
		is_clear = _is_clear;
	}

	public void clear()
	{
	}

	public string toString()
	{
		return "ID=" + Convert.ToString(id) + ", UID=" + Convert.ToString(uid) + ", COUNT_DAYS=" + Convert.ToString(count_days) + ", COUNT_SEQ=" + Convert.ToString(count_seq) + ", IS_CLEAR=" + (is_clear ? "TRUE" : "FALSE") + ", UPDATE_DATE=" + update_date.ConvertTime();
	}
}
