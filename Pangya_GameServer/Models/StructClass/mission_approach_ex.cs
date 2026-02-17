namespace Pangya_GameServer.Models;

public class mission_approach_ex : mission_approach
{
	public bool is_player_uid;

	public mission_approach_ex(uint _ul = 0u)
		: base(_ul)
	{
		is_player_uid = false;
	}

	public override void clear()
	{
		base.clear();
		is_player_uid = false;
	}
}
