using System.Collections.Generic;
using System.Linq;

namespace Pangya_GameServer.Models;

public class TreasureHunterVersusInfo
{
	public class _stTreasureHunterItem
	{
		public TreasureHunterItem thi = new TreasureHunterItem();

		public uint uid { get; set; } = 0u;

		public _stTreasureHunterItem()
		{
		}

		public _stTreasureHunterItem(uint _uid, TreasureHunterItem _thi)
		{
			AddItem(_uid, _thi);
		}

		public void AddItem(uint _uid, TreasureHunterItem item)
		{
			uid = _uid;
			thi = item;
		}

		public void AddItem(TreasureHunterItem item)
		{
			thi = item;
		}
	}

	public uint treasure_point = 0u;

	public byte all_score;

	public byte par_score;

	public byte birdie_score;

	public byte eagle_score;

	public List<_stTreasureHunterItem> v_item { get; set; } = new List<_stTreasureHunterItem>();

	public TreasureHunterVersusInfo()
	{
		clear();
	}

	public void clear()
	{
		all_score = 0;
		par_score = 0;
		birdie_score = 0;
		eagle_score = 0;
		treasure_point = 0u;
		if (v_item.Any())
		{
			v_item.Clear();
		}
	}

	public void increment(TreasureHunterVersusInfo other)
	{
		all_score += other.all_score;
		par_score += other.par_score;
		birdie_score += other.birdie_score;
		eagle_score += other.eagle_score;
	}

	public void increment(PlayerGameInfo.stTreasureHunterInfo _thi)
	{
		all_score += _thi.all_score;
		par_score += _thi.par_score;
		birdie_score += _thi.birdie_score;
		eagle_score += _thi.eagle_score;
	}

	public uint getPoint(uint _tacada, byte _par_hole)
	{
		byte point = all_score;
		if (_tacada == 1)
		{
			return point;
		}
		switch ((sbyte)(_tacada - _par_hole))
		{
		case 0:
			point += par_score;
			break;
		case -1:
			point += birdie_score;
			break;
		case -2:
			point += eagle_score;
			break;
		}
		return point;
	}
}
