using System;
using System.Threading;

namespace Pangya_GameServer.Models;

public class stSyncUpdateDB
{
	public enum eSTATE_UPDATE : byte
	{
		NONE,
		REQUEST_UPDATE,
		UPDATE_CONFIRMED,
		ERROR_UPDATE
	}

	private eSTATE_UPDATE m_state;

	private readonly object m_lock = new object();

	private readonly AutoResetEvent m_cv = new AutoResetEvent(initialState: false);

	public stSyncUpdateDB()
	{
		m_state = eSTATE_UPDATE.NONE;
	}

	public void requestUpdateOnDB()
	{
		int timeoutCount = 3;
		int timeoutMs = 10000;
		lock (m_lock)
		{
			if (m_state == eSTATE_UPDATE.REQUEST_UPDATE)
			{
				while (m_state == eSTATE_UPDATE.REQUEST_UPDATE && timeoutCount > 0)
				{
					if (!m_cv.WaitOne(timeoutMs))
					{
						timeoutCount--;
					}
				}
				if (timeoutCount == 0)
				{
					Console.WriteLine("[SyncUpdateDB::RequestUpdateOnDB][Warning] 30 segundos consumidos, mudança de estado forçada.");
				}
			}
			m_state = eSTATE_UPDATE.REQUEST_UPDATE;
		}
	}

	public void confirmUpdadeOnDB()
	{
		lock (m_lock)
		{
			if (m_state != eSTATE_UPDATE.REQUEST_UPDATE)
			{
				throw new Exception("[SyncUpdateDB::ConfirmUpdateOnDB][Error] m_state está errado, não é REQUEST_UPDATE.");
			}
			m_state = eSTATE_UPDATE.UPDATE_CONFIRMED;
			m_cv.Set();
		}
	}

	public void errorUpdateOnDB()
	{
		lock (m_lock)
		{
			if (m_state != eSTATE_UPDATE.REQUEST_UPDATE)
			{
				throw new Exception("[SyncUpdateDB::ErrorUpdateOnDB][Error] m_state está errado, não é REQUEST_UPDATE.");
			}
			m_state = eSTATE_UPDATE.ERROR_UPDATE;
			m_cv.Set();
		}
	}
}
