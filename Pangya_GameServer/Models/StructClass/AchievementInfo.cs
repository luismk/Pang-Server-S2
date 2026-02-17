using System;
using System.Collections.Generic;

namespace Pangya_GameServer.Models;

public class AchievementInfo
{
	public enum AchievementStatus : byte
	{
		Pending = 1,
		Excluded,
		Active,
		Concluded
	}

	public byte active;

	public uint _typeid = 0u;

	public int id = 0;

	public uint status = 0u;

	public Dictionary<uint, CounterItemInfo> map_counter_item = new Dictionary<uint, CounterItemInfo>();

	public List<QuestStuffInfo> v_qsi = new List<QuestStuffInfo>();

	public void Clear()
	{
		active = 0;
		_typeid = 0u;
		id = 0;
		status = 0u;
		v_qsi.Clear();
		map_counter_item.Clear();
	}

	public CounterItemInfo FindCounterItemById(uint id)
	{
		if (id == 0)
		{
			throw new Exception("[AchievementInfo::FindCounterItemById][Error] id is invalid");
		}
		CounterItemInfo counterItem;
		return map_counter_item.TryGetValue(id, out counterItem) ? counterItem : null;
	}

	public CounterItemInfo FindCounterItemByTypeId(uint typeId)
	{
		if (typeId == 0)
		{
			throw new Exception("[AchievementInfo::FindCounterItemByTypeId][Error] typeId is invalid");
		}
		foreach (CounterItemInfo item in map_counter_item.Values)
		{
			if (item._typeid == typeId)
			{
				return item;
			}
		}
		return null;
	}

	public QuestStuffInfo FindQuestStuffById(uint id)
	{
		if (id == 0)
		{
			throw new Exception("[AchievementInfo::FindQuestStuffById][Error] id is invalid");
		}
		foreach (QuestStuffInfo quest in v_qsi)
		{
			if (quest.id == id)
			{
				return quest;
			}
		}
		return null;
	}

	public QuestStuffInfo FindQuestStuffByTypeId(uint typeId)
	{
		if (typeId == 0)
		{
			throw new Exception("[AchievementInfo::FindQuestStuffByTypeId][Error] typeId is invalid");
		}
		foreach (QuestStuffInfo quest in v_qsi)
		{
			if (quest._typeid == typeId)
			{
				return quest;
			}
		}
		return null;
	}

	public uint AddCounterByTypeId(uint typeId, uint value)
	{
		if (typeId == 0)
		{
			throw new Exception("[AchievementInfo::AddCounterByTypeId][Error] typeId is invalid");
		}
		uint count = 0u;
		foreach (QuestStuffInfo quest in v_qsi)
		{
			if (quest.clear_date_unix == 0)
			{
				CounterItemInfo counterItem = FindCounterItemById((uint)quest.counter_item_id);
				if (counterItem != null && counterItem._typeid == typeId)
				{
					counterItem.value += value;
					count++;
				}
			}
		}
		return count;
	}

	public bool CheckAllQuestClear()
	{
		foreach (QuestStuffInfo quest in v_qsi)
		{
			if (quest.clear_date_unix == 0)
			{
				return false;
			}
		}
		return true;
	}
}
