using System.Collections.Generic;
using System.Linq;

namespace Pangya_GameServer.Models;

public class AchievementInfoEx : AchievementInfo
{
	public uint quest_base_typeid = 0u;

	public AchievementInfoEx()
	{
		clear();
	}

	public void clear()
	{
		Clear();
		quest_base_typeid = 0u;
	}

	public List<QuestStuffInfo>.Enumerator getQuestBase()
	{
		if (quest_base_typeid == 0)
		{
			return v_qsi.ToList().GetEnumerator();
		}
		return v_qsi.Where((QuestStuffInfo c) => c._typeid == quest_base_typeid).ToList().GetEnumerator();
	}
}
