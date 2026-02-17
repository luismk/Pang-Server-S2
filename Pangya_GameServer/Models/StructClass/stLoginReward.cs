using System;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

public class stLoginReward : IDisposable
{
	public class stItemReward
	{
		public uint _typeid = 0u;

		public uint qntd = 0u;

		public uint qntd_time = 0u;

		public stItemReward(uint _ul = 0u)
		{
			clear();
		}

		public stItemReward(uint __typeid, uint _qntd, uint _qntd_time)
		{
			_typeid = __typeid;
			qntd = _qntd;
			qntd_time = _qntd_time;
		}

		public void clear()
		{
		}

		public string toString()
		{
			return "TYPEID=" + Convert.ToString(_typeid) + ", QNTD=" + Convert.ToString(qntd) + ", QNTD_TIME=" + Convert.ToString(qntd_time);
		}
	}

	public enum eTYPE : byte
	{
		N_TIME,
		FOREVER
	}

	public ulong id = 0uL;

	public eTYPE type;

	public bool is_end;

	public uint days_to_gift = 0u;

	public uint n_times_gift = 0u;

	public SYSTEMTIME end_date = new SYSTEMTIME();

	public stItemReward item_reward = new stItemReward();

	private string name = "";

	public stLoginReward(uint _ul = 0u)
	{
		id = 0uL;
		type = eTYPE.N_TIME;
		name = "";
		is_end = false;
		end_date = new SYSTEMTIME();
		days_to_gift = 1u;
		n_times_gift = 1u;
		item_reward = new stItemReward();
	}

	public stLoginReward(ulong _id, eTYPE _type, string _name, uint _days_to_gift, uint _n_times_gift, stItemReward _item, SYSTEMTIME _end_date, bool _is_end = false)
	{
		id = _id;
		type = _type;
		end_date = _end_date;
		days_to_gift = _days_to_gift;
		n_times_gift = _n_times_gift;
		item_reward = _item;
		is_end = _is_end;
		setName(_name);
	}

	public virtual void Dispose()
	{
		clear();
	}

	public void clear()
	{
		id = 0uL;
		is_end = false;
		type = eTYPE.N_TIME;
		name = "";
		days_to_gift = 1u;
		n_times_gift = 1u;
		end_date = new SYSTEMTIME();
		item_reward.clear();
	}

	public void setName(string _name)
	{
		if (_name != null)
		{
			name = _name;
		}
	}

	public string getName()
	{
		if (name == null)
		{
			return "";
		}
		return name;
	}

	public string toString()
	{
		return "ID=" + Convert.ToString(id) + ", TYPE=" + Convert.ToString((ushort)type) + ", NAME=" + getName() + ", DAYS_TO_GIFT=" + Convert.ToString(days_to_gift) + ", N_TIMES_GIFT=" + Convert.ToString(n_times_gift) + ", IS_END=" + (is_end ? "TRUE" : "FALSE") + ", ITEM{" + item_reward.toString() + "}, END_DATE=" + (end_date.IsEmpty ? "" : end_date.ConvertTime().ToString());
	}
}
