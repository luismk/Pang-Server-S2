using PangyaAPI.Network.Models;

namespace Pangya_GameServer.Models;

public class player_info
{
	public byte m_state_logged;

	public uint uid { get; set; }

	public BlockFlag block_flag { get; set; }

	public short level { get; set; }

	public string id { get; set; }

	public string nickname { get; set; }

	public string pass { get; set; }

	public player_info()
	{
		block_flag = new BlockFlag();
		id = "";
		nickname = "";
		pass = "";
	}

	public void set_info(player_info info)
	{
		uid = info.uid;
		level = info.level;
		block_flag = info.block_flag;
		nickname = info.nickname;
		pass = info.pass;
	}
}
