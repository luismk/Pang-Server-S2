namespace Pangya_GameServer.Models;

public class PlayerGrandZodiacInfo : PlayerGameInfo
{
	public byte init_first_hole_gz = 0;

	public byte end_game = 0;

	public grand_zodiac_dados m_gz { get; set; } = new grand_zodiac_dados();

	public SyncShotGrandZodiac m_sync_shot_gz { get; set; } = new SyncShotGrandZodiac();

	public PlayerGrandZodiacInfo(uint _ul = 0u)
	{
		base.clear();
		m_gz = new grand_zodiac_dados();
		init_first_hole_gz = 0;
		end_game = 0;
		m_sync_shot_gz = new SyncShotGrandZodiac();
		base.clear();
		m_gz.clear();
		init_first_hole_gz = 0;
		end_game = 0;
		m_sync_shot_gz.clearAllState();
	}
}
