using System;

namespace Pangya_GameServer.Models;

public class stRangeTime : IDisposable
{
	public TimeSpan m_start = default(TimeSpan);

	public TimeSpan m_end = default(TimeSpan);

	public byte m_channel_id;

	public bool m_sended_message;

	public bool m_room_created;

	public bool m_room_closed;

	public stRangeTime(uint _ul = 0u)
	{
		m_channel_id = byte.MaxValue;
		m_sended_message = false;
		m_room_created = false;
		m_room_closed = false;
	}

	public stRangeTime(ushort _hour_start, ushort _min_start, ushort _sec_start, ushort _hour_end, ushort _min_end, ushort _sec_end, byte _channel_id)
	{
		m_start = new TimeSpan(_hour_start, _min_start, _sec_start, 0);
		m_end = new TimeSpan(_hour_end, _min_end, _sec_end, 0);
		m_channel_id = _channel_id;
		m_sended_message = false;
		m_room_created = false;
		m_room_closed = false;
	}

	public stRangeTime(TimeSpan _start, TimeSpan _end, byte _channel_id)
	{
		m_start = _start;
		m_end = _end;
		m_channel_id = _channel_id;
		m_sended_message = false;
		m_room_created = false;
		m_room_closed = false;
	}

	public void Dispose()
	{
		clear();
	}

	public void clear()
	{
		m_start = default(TimeSpan);
		m_end = default(TimeSpan);
		m_channel_id = byte.MaxValue;
		m_sended_message = false;
		m_room_created = false;
		m_room_closed = false;
	}

	public bool isBetweenTime(TimeSpan _st)
	{
		return intoStartTime(_st) && intoEndTime(_st);
	}

	public bool isBetweenTime(ushort _hour, ushort _min, ushort _sec, ushort _milli = 0)
	{
		TimeSpan st = new TimeSpan(_hour, _min, _sec, _milli);
		return isBetweenTime(st);
	}

	public bool isPastEnd(TimeSpan _st)
	{
		return timeToMilliseconds(_st) > timeToMilliseconds(m_end);
	}

	public uint getDiffInterval()
	{
		return timeToMilliseconds(m_end) - timeToMilliseconds(m_start);
	}

	protected bool intoStartTime(TimeSpan _st)
	{
		return timeToMilliseconds(m_start) <= timeToMilliseconds(_st);
	}

	protected bool intoEndTime(TimeSpan _st)
	{
		return timeToMilliseconds(_st) < timeToMilliseconds(m_end);
	}

	protected uint timeToMilliseconds(TimeSpan _st)
	{
		return (uint)(_st.Hours * 60 * 60 * 1000 + _st.Minutes * 60 * 1000 + _st.Seconds * 1000 + _st.Milliseconds);
	}
}
