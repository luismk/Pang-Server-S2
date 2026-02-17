namespace Pangya_GameServer.Models;

public class GuildPoints
{
	public enum eGUILD_WIN : byte
	{
		WIN,
		LOSE,
		DRAW
	}

	public uint uid;

	public ulong point;

	public ulong pang;

	public eGUILD_WIN win;

	public void clear()
	{
	}
}
