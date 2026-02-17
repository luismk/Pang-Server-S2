using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

public class LockManager
{
	public class lock_ctx
	{
		public Player m_player;

		protected bool m_lock;

		protected object m_cs = new object();

		public lock_ctx()
		{
			m_player = null;
			m_lock = false;
		}

		public lock_ctx(Player _player)
		{
			m_player = _player;
			m_lock = false;
		}

		public void Dispose()
		{
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
	}

	protected List<lock_ctx> m_lockers = new List<lock_ctx>();

	protected bool m_lock;

	private object m_cs = new object();

	public LockManager()
	{
		m_lockers = new List<lock_ctx>();
		m_lock = false;
	}

	public void Dispose()
	{
		clear();
		if (m_lock)
		{
			unlock();
		}
	}

	public void clear()
	{
		@lock();
		if (!m_lockers.empty())
		{
			m_lockers.Clear();
		}
		unlock();
	}

	public void @lock(Player _player)
	{
		@lock();
		lock_ctx it = findLocker(_player);
		if (it != m_lockers.end())
		{
			it.@lock();
		}
		else
		{
			insertLock(_player)?.@lock();
		}
		unlock();
	}

	public void unlock(Player _player)
	{
		@lock();
		lock_ctx it = findLocker(_player);
		if (it != m_lockers.end())
		{
			it.unlock();
		}
		unlock();
	}

	protected lock_ctx findLocker(Player _player)
	{
		if (_player == null)
		{
			return m_lockers.Last();
		}
		return m_lockers.FirstOrDefault((lock_ctx _el) => _el.m_player == _player);
	}

	protected lock_ctx insertLock(Player _player)
	{
		lock_ctx it = new lock_ctx(_player);
		m_lockers.Add(it);
		return (it != m_lockers.end()) ? it : null;
	}

	protected void @lock()
	{
		Monitor.Enter(m_cs);
		m_lock = true;
	}

	protected void unlock()
	{
		if (m_lock)
		{
			m_lock = false;
			Monitor.Exit(m_cs);
		}
	}
}
