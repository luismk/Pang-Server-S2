using System.Collections.Generic;

namespace Pangya_GameServer.Models;

public class ctx_coin_set_item
{
	public int flag = 0;

	public uint _typeid = 0u;

	public byte tipo;

	public List<ctx_coin_item_ex> item = new List<ctx_coin_item_ex>();

	public ctx_coin_set_item(uint _ul = 0u)
	{
		clear();
	}

	public void clear()
	{
		_typeid = 0u;
		tipo = 0;
		flag = -100;
		if (item.Count > 0)
		{
			item.Clear();
		}
	}
}
