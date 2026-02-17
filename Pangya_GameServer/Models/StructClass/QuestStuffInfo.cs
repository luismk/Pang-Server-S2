namespace Pangya_GameServer.Models;

public class QuestStuffInfo
{
	public int id = 0;

	public uint _typeid = 0u;

	public int counter_item_id = 0;

	public uint clear_date_unix = 0u;

	public void clear()
	{
		id = 0;
		_typeid = 0u;
		counter_item_id = 0;
		clear_date_unix = 0u;
	}

	public bool isValid()
	{
		return id > 0 && _typeid != 0;
	}
}
