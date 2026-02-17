using System;

namespace Pangya_GameServer.Models;

public class stReward
{
	public uint _typeid = 0u;

	public uint qntd = 0u;

	public uint qntd_time = 0u;

	public uint rate = 0u;

	public stReward(uint _ul = 0u)
	{
		_typeid = 0u;
		qntd = 0u;
		qntd_time = 0u;
		rate = 100u;
	}

	public stReward(uint __typeid, uint _qntd, uint _qntd_time, uint _rate = 100u)
	{
		_typeid = __typeid;
		qntd = _qntd;
		qntd_time = _qntd_time;
		rate = _rate;
	}

	public override string ToString()
	{
		return "TYPEID=" + Convert.ToString(_typeid) + ", QNTD=" + Convert.ToString(qntd) + ", QNTD_TIME=" + Convert.ToString(qntd_time) + ", RATE=" + Convert.ToString(rate);
	}
}
