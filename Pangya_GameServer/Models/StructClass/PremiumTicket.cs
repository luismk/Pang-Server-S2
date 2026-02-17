namespace Pangya_GameServer.Models;

public class PremiumTicket
{
	public int id;

	public uint _typeid;

	public int unix_sec_date;

	public int unix_end_date;

	public void clear()
	{
		id = 0;
		_typeid = 0u;
		unix_sec_date = 0;
		unix_end_date = 0;
	}
}
