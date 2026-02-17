using System;

namespace Pangya_GameServer.Models;

public class ctx_personal_shop
{
	public uint index;

	public string name = string.Empty;

	public uint id;

	public uint price;

	public DateTime reg_date;

	public ctx_personal_shop()
	{
		index = 0u;
		id = 0u;
		price = 0u;
		name = string.Empty;
		reg_date = DateTime.MinValue;
	}

	public void clear()
	{
		index = 0u;
		name = string.Empty;
		id = 0u;
		price = 0u;
		reg_date = DateTime.MinValue;
	}

	public string toString()
	{
		return $"Index={index}, Name={name}, ID={id}, Price={price}, RegDate={reg_date}";
	}
}
