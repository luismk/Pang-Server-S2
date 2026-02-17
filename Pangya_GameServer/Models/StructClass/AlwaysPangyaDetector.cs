using System.Collections.Generic;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.Models;

public class AlwaysPangyaDetector
{
	private class ShotStats
	{
		public int totalShots;

		public int pangyaHits;

		public int perfectCenterHits;

		public float minImpact = float.MaxValue;

		public float maxImpact = float.MinValue;
	}

	private const float PANGYA_MIN = 135f;

	private const float PANGYA_MAX = 142f;

	private const float PERFECT_CENTER_MIN = 138f;

	private const float PERFECT_CENTER_MAX = 140f;

	private const float MAX_HUMAN_VARIANCE = 1f;

	private const int MIN_SHOTS_FOR_ANALYSIS = 8;

	private readonly Dictionary<uint, ShotStats> _stats = new Dictionary<uint, ShotStats>();

	public void Analyze(Player session, ShotDataEx sd, uEffectFlag uEffect)
	{
		if (session == null || sd == null)
		{
			return;
		}
		uint uid = session.m_pi.uid;
		bool isclubValid = sd.club == 14 || sd.club == 13 || sd.club == 12 || sd.club == 11 || sd.club == 10;
		if (!_stats.TryGetValue(uid, out ShotStats stats))
		{
			stats = new ShotStats();
			_stats[uid] = stats;
		}
		float impact = sd.bar_point[1];
		bool isPangya = sd.acerto_pangya_flag == 4;
		stats.totalShots++;
		if (impact < stats.minImpact)
		{
			stats.minImpact = impact;
		}
		if (impact > stats.maxImpact)
		{
			stats.maxImpact = impact;
		}
		if (!uEffect.PixelEffect() && !isclubValid && isPangya && (impact < 135f || impact > 142f))
		{
			Flag(session, "INVALID_PANGYA_RANGE", $"Impact={impact}");
		}
		else
		{
			if (!isPangya)
			{
				return;
			}
			stats.pangyaHits++;
			if (impact >= 138f && impact <= 140f)
			{
				stats.perfectCenterHits++;
			}
			if (!uEffect.PixelEffect() && !isclubValid && stats.totalShots >= 8 && stats.perfectCenterHits == stats.pangyaHits && stats.pangyaHits >= 8)
			{
				Flag(session, "PERFECT_CENTER_ABUSE", $"Hits={stats.pangyaHits} ImpactRange={stats.minImpact:F2}-{stats.maxImpact:F2}");
				return;
			}
			if (!uEffect.PixelEffect() && !isclubValid && stats.totalShots >= 8)
			{
				float variance = stats.maxImpact - stats.minImpact;
				if (variance < 1f)
				{
					Flag(session, "NO_HUMAN_VARIANCE", $"Variance={variance:F3}");
					return;
				}
			}
			if (!uEffect.PixelEffect() && !isclubValid && sd.time_shot < 10000)
			{
				Flag(session, "FAST_PERFECT_RELEASE", $"TimeShot={sd.time_shot} Impact={impact}");
			}
		}
	}

	private void Flag(Player session, string reason, string detail)
	{
		Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message($"[AlwaysPangyaDetect][{reason}] UID={session.m_pi.uid} {detail}", type_msg.CL_FILE_LOG_AND_CONSOLE));
	}

	public void Clear(uint uid)
	{
		_stats.Remove(uid);
	}

	public void ClearAll()
	{
		_stats.Clear();
	}
}
