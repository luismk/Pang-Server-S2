using Pangya_GameServer.UTIL;

namespace Pangya_GameServer.Models;

public class options3D
{
	public uSpecialShot m_shot = new uSpecialShot();

	public Vector3D m_position;

	public IExtraPower m_extra_power;

	public ePOWER_SHOT_FACTORY m_power_shot = ePOWER_SHOT_FACTORY.NO_POWER_SHOT;

	public float m_distance;

	public float m_power_slot;

	public float m_percent_shot;

	public float m_spin;

	public float m_curve;

	public float m_mira;

	public options3D(uSpecialShot _shot, Vector3D _position, IExtraPower _extra_power, ePOWER_SHOT_FACTORY _power_shot, float _distance, float _power_slot, float _percent_shot, float _spin, float _curve, float _mira)
	{
		m_shot = _shot;
		m_position = _position;
		m_extra_power = _extra_power;
		m_power_shot = _power_shot;
		m_distance = _distance;
		m_power_slot = _power_slot;
		m_percent_shot = _percent_shot;
		m_spin = _spin;
		m_curve = _curve;
		m_mira = _mira;
	}
}
