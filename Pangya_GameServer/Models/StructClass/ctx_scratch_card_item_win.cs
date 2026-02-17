namespace Pangya_GameServer.Models;

public class ctx_scratch_card_item_win
{
	public ctx_scratch_card_item ctx_psi = new ctx_scratch_card_item();

	public uint qntd;

	public object item;

	public ctx_scratch_card_item_win(uint _ul = 0u)
	{
		clear();
	}

	public void clear()
	{
		ctx_psi.clear();
		qntd = 0u;
		item = null;
	}
}
