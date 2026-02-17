namespace Pangya_GameServer.Models;

public class ctx_coin_item
{
	public int tipo = 0;

	public uint _typeid = 0u;

	public uint qntd = 0u;

	public ctx_coin_item(uint _ul = 0u)
	{
		clear();
	}

	public ctx_coin_item(int _tipo, uint __typeid, uint _qntd)
	{
		tipo = _tipo;
		_typeid = __typeid;
		qntd = _qntd;
	}

	public void clear()
	{
		tipo = 0;
		_typeid = 0u;
		qntd = 0u;
	}
}
