using System;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

public class ctx_papel_shop
{
	public uint numero = 0u;

	public ulong price_normal = 0uL;

	public ulong price_big = 0uL;

	public byte limitted_per_day = 1;

	public SYSTEMTIME update_date = new SYSTEMTIME();

	public void clear()
	{
	}

	public string toString()
	{
		return "NUMERO=" + Convert.ToString(numero) + ", PRICE_NORMAL=" + Convert.ToString(price_normal) + ", PRICE_BIG=" + Convert.ToString(price_big) + ", LIMITTED_PER_DAY=" + Convert.ToString((ushort)limitted_per_day) + ", UPDATE_DATE=" + UtilTime.FormatDate(update_date);
	}
}
