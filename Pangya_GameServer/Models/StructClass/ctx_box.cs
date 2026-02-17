using System.Collections.Generic;

namespace Pangya_GameServer.Models;

public class ctx_box
{
	public BOX_TYPE_OPEN tipo_open;

	public BOX_TYPE tipo;

	public int numero = 0;

	public int id = 0;

	public uint _typeid = 0u;

	public uint opened_typeid = 0u;

	public string msg = "";

	public List<ctx_box_item> item = new List<ctx_box_item>();

	public ctx_box(uint _ul = 0u)
	{
		clear();
	}

	public void clear()
	{
	}
}
