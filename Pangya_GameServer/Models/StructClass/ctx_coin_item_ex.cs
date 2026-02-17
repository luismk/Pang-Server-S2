namespace Pangya_GameServer.Models;

public class ctx_coin_item_ex : ctx_coin_item
{
	public uint probabilidade = 0u;

	public int gacha_number = 0;

	public ctx_coin_item_ex(uint _ul = 0u)
		: base(_ul)
	{
		clear();
	}

	public ctx_coin_item_ex(int _tipo, uint __typeid, uint _qntd, uint _probabilidade, int _gachar_number)
		: base(_tipo, __typeid, _qntd)
	{
		probabilidade = _probabilidade;
		gacha_number = _gachar_number;
	}
}
