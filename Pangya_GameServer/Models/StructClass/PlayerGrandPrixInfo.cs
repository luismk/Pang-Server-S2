namespace Pangya_GameServer.Models;

public class PlayerGrandPrixInfo : PlayerGameInfo
{
	public uint _flag;

	public PlayerGrandPrixInfo(uint _ul = 0u)
	{
		base.clear();
		_flag = 0u;
	}
}
