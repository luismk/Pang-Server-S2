namespace Pangya_GameServer.Models;

public class ctx_scratch_card_item
{
	public uint _typeid;

	public uint probabilidade;

	public uint qntd;

	public int numero;

	public SCRATCH_CARD_TYPE tipo;

	public bool active;

	public ctx_scratch_card_item()
	{
		clear();
	}

	public void clear()
	{
		_typeid = 0u;
		probabilidade = 0u;
		qntd = 0u;
		numero = 0;
		tipo = SCRATCH_CARD_TYPE.SCT_COMMUN;
		active = false;
	}
}
