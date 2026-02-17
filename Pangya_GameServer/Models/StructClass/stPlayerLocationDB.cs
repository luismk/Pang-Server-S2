namespace Pangya_GameServer.Models;

public class stPlayerLocationDB : stSyncUpdateDB
{
	public sbyte channel;

	public sbyte lobby;

	public sbyte room;

	public sbyte place;

	public stPlayerLocationDB(uint _ul = 0u)
	{
		clear();
	}

	~stPlayerLocationDB()
	{
		clear();
	}

	private void clear()
	{
		channel = -1;
		lobby = -1;
		room = -1;
		place = 1;
	}
}
