namespace Pangya_GameServer.Models;

public class CounterItemInfo
{
	public byte active;

	public uint _typeid = 0u;

	public int id = 0;

	public uint value = 0u;

	public CounterItemInfo(uint _ul = 0u)
	{
		clear();
	}

	public CounterItemInfo(CounterItemInfo _ul)
	{
		active = _ul.active;
		_typeid = _ul._typeid;
		id = _ul.id;
		value = _ul.value;
	}

	public void clear()
	{
		active = 0;
		_typeid = 0u;
		id = 0;
		value = 0u;
	}

	public bool isValid()
	{
		return id > 0 && _typeid != 0;
	}
}
