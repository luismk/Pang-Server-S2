using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class BarSpace
{
	protected long startTick;

	protected byte state;

	protected float[] point = new float[4];

	public BarSpace()
	{
		clear();
	}

	public void clear()
	{
		state = 0;
		point = new float[4];
	}

	public bool setState(byte _state)
	{
		if (_state > 3)
		{
			return false;
		}
		if (_state == 1)
		{
			startTick = UtilTime.GetTickCount();
		}
		state = _state;
		return true;
	}

	public void setServerPoint(byte _state, float _point)
	{
		if (_state > 4)
		{
			throw new exception("[BarSpace] Estado inválido no server");
		}
		point[_state] = _point;
	}

	public bool setStateAndPoint(byte _state, float _point)
	{
		if (_state > 4)
		{
			return false;
		}
		state = (byte)((_state == 4) ? 3 : _state);
		if (_state == 4 && point[state] != _point)
		{
			return false;
		}
		point[_state] = _point;
		return true;
	}

	public float CalculateServerPoint()
	{
		if (startTick == 0)
		{
			return 0f;
		}
		long now = UtilTime.GetTickCount();
		float elapsed = (float)(now - startTick) / 1000f;
		float speed = 1.35f;
		float cycle = elapsed * speed;
		float pos = cycle % 2f;
		if (pos > 1f)
		{
			pos = 2f - pos;
		}
		return Tools.Clamp(pos, 0f, 1f);
	}

	public byte getState()
	{
		return state;
	}

	public float[] getPoint()
	{
		return point;
	}

	public override string ToString()
	{
		return $"Start={point[0]} Power={point[1]} ImpactZone={point[2]}, Hit PangYa={point[3]}";
	}
}
