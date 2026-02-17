using System.Collections.Generic;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

public class Bot
{
	public enum eTYPE_SCORE : byte
	{
		MIN_SCORE,
		MED_SCORE,
		MAX_SCORE
	}

	public class Hole
	{
		public uint m_course = 0u;

		public uint m_hole = 0u;

		public int m_score = 0;

		public uint m_ulUnknown = 0u;

		public ulong m_pang = 0uL;

		public ulong m_bonus_pang = 0uL;

		public ulong m_ullUnknown = 0uL;

		public Hole(uint _ul = 0u)
		{
			clear();
		}

		public Hole(uint _course, uint _hole, int _score, ulong _pang, ulong _bonus_pang)
		{
			m_course = _course;
			m_hole = _hole;
			m_score = _score;
			m_pang = _pang;
			m_bonus_pang = _bonus_pang;
			m_ulUnknown = 0u;
			m_ullUnknown = 0uL;
		}

		public void clear()
		{
			m_course = 0u;
			m_hole = 0u;
			m_score = 0;
			m_ulUnknown = 0u;
			m_pang = 0uL;
			m_bonus_pang = 0uL;
			m_ullUnknown = 0uL;
		}

		public byte[] ToArray()
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.Write(m_course);
			p.Write(m_hole);
			p.Write(m_score);
			p.Write(m_ulUnknown);
			p.Write(m_pang);
			p.Write(m_bonus_pang);
			p.Write(m_ullUnknown);
			return p.GetBytes;
		}
	}

	public uint id = 0u;

	public byte qntd_hole;

	public ulong pang_total = 0uL;

	public ulong bonus_pang_total = 0uL;

	public int record = 0;

	public int max_record = 0;

	public int med_shot_per_hole = 0;

	public eTYPE_SCORE type_score = eTYPE_SCORE.MIN_SCORE;

	public PlayerGameInfo pi = new PlayerGameInfo();

	public List<Hole> hole = new List<Hole>();

	public Bot(uint _ul = 0u)
	{
		clear();
	}

	public void Dispose()
	{
		clear();
	}

	public void clear()
	{
		id = 0u;
		qntd_hole = 0;
		pang_total = 0uL;
		bonus_pang_total = 0uL;
		record = 0;
		max_record = 0;
		med_shot_per_hole = 0;
		type_score = eTYPE_SCORE.MIN_SCORE;
		pi.clear();
		if (!hole.empty())
		{
			hole.Clear();
		}
	}
}
