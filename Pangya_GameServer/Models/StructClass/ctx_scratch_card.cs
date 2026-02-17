using System;

namespace Pangya_GameServer.Models;

public class ctx_scratch_card
{
	public uint numero;

	public bool limitted_per_day;

	public DateTime update_date;

	public ctx_scratch_card()
	{
		clear();
	}

	public void clear()
	{
		numero = 0u;
		limitted_per_day = false;
		update_date = DateTime.MinValue;
	}

	public string toString()
	{
		return "NUMERO=" + numero + ", LIMITTED_PER_DAY=" + (limitted_per_day ? 1 : 0) + ", UPDATE_DATE=" + ((update_date == DateTime.MinValue) ? "0" : update_date.ToString("o"));
	}
}
