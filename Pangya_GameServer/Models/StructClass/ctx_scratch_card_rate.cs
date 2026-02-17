namespace Pangya_GameServer.Models;

public class ctx_scratch_card_rate
{
	public string Nome;

	public SCRATCH_CARD_TYPE Tipo;

	public uint Prob;

	public ctx_scratch_card_rate(uint _ul = 0u)
	{
		clear();
	}

	public void clear()
	{
		Nome = string.Empty;
		Tipo = SCRATCH_CARD_TYPE.SCT_COMMUN;
		Prob = 0u;
	}
}
