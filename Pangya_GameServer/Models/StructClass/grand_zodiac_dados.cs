using System.Collections.Generic;

namespace Pangya_GameServer.Models;

public class grand_zodiac_dados
{
	public uint position { get; set; }

	public uint pontos { get; set; }

	public uint hole_in_one { get; set; }

	public ulong jackpot { get; set; }

	public uint trofeu { get; set; }

	public int total_score { get; set; }

	public List<eGRAND_ZODIAC_TYPE_SHOT> m_score_shot { get; set; } = new List<eGRAND_ZODIAC_TYPE_SHOT>();

	public grand_zodiac_dados(uint _ul = 0u)
	{
		position = 0u;
		pontos = 0u;
		hole_in_one = 0u;
		jackpot = 0uL;
		total_score = 0;
		trofeu = 0u;
		m_score_shot = new List<eGRAND_ZODIAC_TYPE_SHOT>();
	}

	public void clear()
	{
		position = 0u;
		pontos = 0u;
		hole_in_one = 0u;
		jackpot = 0uL;
		trofeu = 0u;
		total_score = 0;
		if (m_score_shot.Count > 0)
		{
			m_score_shot.Clear();
		}
	}
}
