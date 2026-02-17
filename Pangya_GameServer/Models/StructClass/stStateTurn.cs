using System.Threading;

namespace Pangya_GameServer.Models;

public class stStateTurn
{
	protected STATE_TURN m_state = STATE_TURN.WAIT_HIT_SHOT;

	protected bool m_lock;

	private object m_cs = new object();

	public stStateTurn()
	{
		m_state = STATE_TURN.WAIT_HIT_SHOT;
		m_lock = false;
	}

	public void Dispose()
	{
		m_state = STATE_TURN.WAIT_HIT_SHOT;
		if (m_lock)
		{
			unlock();
		}
	}

	public void @lock()
	{
		Monitor.Enter(m_cs);
		m_lock = true;
	}

	public void unlock()
	{
		if (m_lock)
		{
			m_lock = false;
			Monitor.Exit(m_cs);
		}
	}

	public STATE_TURN getState()
	{
		return m_state;
	}

	public void setState(STATE_TURN _state)
	{
		m_state = _state;
	}

	public void setStateWithLock(STATE_TURN _state)
	{
		@lock();
		m_state = _state;
		unlock();
	}
}
