using System.Collections.Generic;

namespace Pangya_GameServer.Models;

public class BarSpaceDetector
{
	private const int WindowSize = 6;

	private const float MaxAllowedDiff = 0.03f;

	private const int MinPerfectHits = 4;

	private readonly Queue<float> diffs = new Queue<float>();

	public void Add(float diff)
	{
		diffs.Enqueue(diff);
		if (diffs.Count > 6)
		{
			diffs.Dequeue();
		}
	}

	public bool IsSuspicious(out float avgDiff)
	{
		avgDiff = 0f;
		if (diffs.Count < 6)
		{
			return false;
		}
		int perfectHits = 0;
		float sum = 0f;
		foreach (float d in diffs)
		{
			sum += d;
			if (d <= 0.03f)
			{
				perfectHits++;
			}
		}
		avgDiff = sum / (float)diffs.Count;
		return perfectHits >= 4 && avgDiff <= 0.03f;
	}

	public void Reset()
	{
		diffs.Clear();
	}
}
