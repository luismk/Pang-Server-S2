using System.Collections.Generic;

namespace Pangya_GameServer.Models;

public class PangBattleData
{
	public short m_hole;

	public bool m_hole_extra_flag;

	public short m_hole_extra;

	public uint m_count_finish_hole = 0u;

	public int m_player_win_pb = 0;

	public List<PangBattleHolePang> v_player_win = new List<PangBattleHolePang>();

	public PangBattleData(uint _ul = 0u)
	{
		clear();
	}

	public void clear()
	{
		m_hole = -1;
		m_hole_extra = -1;
		m_hole_extra_flag = false;
		m_count_finish_hole = 0u;
		m_player_win_pb = -1;
		if (v_player_win.Count > 0)
		{
			v_player_win.Clear();
		}
	}
}
