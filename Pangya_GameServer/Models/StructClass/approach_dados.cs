namespace Pangya_GameServer.Models;

public class approach_dados
{
	public enum eSTATUS : byte
	{
		IN_GAME,
		LEFT_GAME
	}

	public eSTATUS status = eSTATUS.IN_GAME;

	public int oid = 0;

	public uint uid = 0u;

	public sbyte position;

	public uint box = 0u;

	public uint distance = 0u;

	public uint time = 0u;

	public ushort rank_box;

	public approach_dados(uint _ul = 0u)
	{
		clear();
	}

	public virtual void clear()
	{
		status = eSTATUS.IN_GAME;
		position = -1;
		distance = uint.MaxValue;
		box = 0u;
		rank_box = 0;
		time = 0u;
	}

	public virtual void setLeftGame()
	{
		status = eSTATUS.LEFT_GAME;
		position = -1;
		distance = uint.MaxValue;
		box = 0u;
		rank_box = 0;
		time = 0u;
	}
}
