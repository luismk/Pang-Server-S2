namespace Pangya_GameServer.Models;

public class PlayerApproachInfo : PlayerGameInfo
{
	public approach_dados_ex m_app_dados = new approach_dados_ex();

	public PlayerApproachInfo()
	{
		m_app_dados = new approach_dados_ex();
	}

	public override void clear()
	{
		base.clear();
		m_app_dados.clear();
	}
}
