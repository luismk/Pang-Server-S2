using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class GameData
{
	public uint tacada_num = 0u;

	public uint total_tacada_num = 0u;

	public int score = 0;

	public byte giveup = 0;

	public uint bad_condute = 0u;

	public uint penalidade = 0u;

	public ulong pang = 0uL;

	public ulong bonus_pang = 0uL;

	public long pang_pang_battle = 0L;

	public int pang_battle_run_hole = 0;

	public uint time_out = 0u;

	public uint exp = 0u;

	public bool _giveup => giveup > 0;

	public GameData()
	{
		clear();
	}

	public void clear()
	{
		tacada_num = 0u;
		total_tacada_num = 0u;
		score = 0;
		giveup = 0;
		bad_condute = 0u;
		penalidade = 0u;
		pang = 0uL;
		bonus_pang = 0uL;
		pang_pang_battle = 0L;
		pang_battle_run_hole = 0;
		time_out = 0u;
		exp = 0u;
	}
}
