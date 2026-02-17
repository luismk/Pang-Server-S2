namespace Pangya_GameServer.Models;

public class PangBattleHolePang
{
	public int player_win = 0;

	public uint pang = 0u;

	public uint pang_extra = 0u;

	public byte vezes;

	public PangBattleHolePang(uint _pang)
	{
		pang = _pang;
		pang_extra = 0u;
		player_win = -3;
		vezes = 1;
	}

	public void clear()
	{
		pang = 0u;
		pang_extra = 0u;
		player_win = -3;
		vezes = 1;
	}
}
