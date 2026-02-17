namespace Pangya_GameServer.Models;

public class RetFinishShot
{
	public int ret;

	public Player p;

	public RetFinishShot()
	{
		clear();
	}

	public void clear()
	{
		p = new Player();
	}
}
