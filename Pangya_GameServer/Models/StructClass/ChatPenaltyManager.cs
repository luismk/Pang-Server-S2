using System;
using System.Collections.Generic;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.Models;

public class ChatPenaltyManager
{
	private const int ResetIntervalMs = 600000;

	private static readonly Dictionary<int, int> OffenseToMinutes = new Dictionary<int, int>
	{
		[1] = 1,
		[2] = 5,
		[3] = 15,
		[4] = 60
	};

	private int _offenseCount = 0;

	private int _lastOffenseTick = 0;

	private int _resetOffenseTick = 0;

	public bool IsBlocked { get; private set; } = false;

	public int BlockExpireTick { get; private set; } = 0;

	public void RegisterOffense(uint uid, string reason)
	{
		int now = Environment.TickCount;
		if (now > _resetOffenseTick)
		{
			_offenseCount = 0;
		}
		_offenseCount++;
		_lastOffenseTick = now;
		int min;
		int blockMinutes = (OffenseToMinutes.TryGetValue(_offenseCount, out min) ? min : 60);
		BlockExpireTick = now + blockMinutes * 60000;
		IsBlocked = true;
		_resetOffenseTick = now + 600000;
		Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message($"[ChatBlock]: UID={uid} bloqueado por {blockMinutes} minuto(s) (motivo: {reason}, reincidências: {_offenseCount})", type_msg.CL_FILE_LOG_AND_CONSOLE));
	}

	public void ClearBlock()
	{
		IsBlocked = false;
		BlockExpireTick = 0;
		_offenseCount = 0;
	}

	public bool IsStillBlocked()
	{
		if (!IsBlocked)
		{
			return false;
		}
		if (Environment.TickCount >= BlockExpireTick)
		{
			ClearBlock();
			return false;
		}
		return true;
	}

	public int GetOffenseCount()
	{
		return _offenseCount;
	}
}
