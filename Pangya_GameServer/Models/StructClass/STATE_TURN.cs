namespace Pangya_GameServer.Models;

public enum STATE_TURN : byte
{
	WAIT_HIT_SHOT,
	SHOTING,
	END_SHOT,
	LOAD_HOLE,
	WAIT_END_GAME
}
