using System.Collections.Generic;

namespace Pangya_GameServer.Models;

public class ctx_coin(uint _ul = 0u)
{
	public MEMORIAL_COIN_TYPE tipo;

	public uint _typeid = 0u;

	public uint probabilidade = 0u;

	public List<ctx_coin_item_ex> item = new List<ctx_coin_item_ex>();
}
