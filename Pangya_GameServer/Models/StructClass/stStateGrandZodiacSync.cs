using System.Threading;

namespace Pangya_GameServer.Models;

public class stStateGrandZodiacSync
{
	protected eSTATE_GRAND_ZODIAC_SYNC m_state;

	protected object m_cs = new object();

	public stStateGrandZodiacSync()
	{
		m_state = eSTATE_GRAND_ZODIAC_SYNC.FIRST_HOLE;
	}

	public void @lock()
	{
		Monitor.Enter(m_cs);
	}

	public void unlock()
	{
		Monitor.Exit(m_cs);
	}

	public eSTATE_GRAND_ZODIAC_SYNC getState()
	{
		return m_state;
	}

	public void setState(eSTATE_GRAND_ZODIAC_SYNC _state)
	{
		m_state = _state;
	}

	public void setStateWithLock(eSTATE_GRAND_ZODIAC_SYNC _state)
	{
		@lock();
		m_state = _state;
		unlock();
	}
}
