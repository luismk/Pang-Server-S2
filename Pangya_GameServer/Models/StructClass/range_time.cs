using System;

namespace Pangya_GameServer.Models;

public class range_time : ICloneable
{
	public enum eTYPE_MAKE_ROOM : byte
	{
		TMR_MAKE_ALL,
		TMR_MAKE_INTERMEDIARE,
		TMR_MAKE_ADVANCED
	}

	public bool m_room_created;

	public bool m_room_closed;

	public TimeSpan m_start { get; set; } = default(TimeSpan);

	public TimeSpan m_end { get; set; } = default(TimeSpan);

	public eTYPE_MAKE_ROOM m_type { get; set; }

	public ushort RoomID { get; set; } = ushort.MaxValue;

	public bool m_sended_message { get; set; }

	public range_time(uint _ul = 0u)
	{
		m_start = default(TimeSpan);
		m_end = default(TimeSpan);
		m_type = eTYPE_MAKE_ROOM.TMR_MAKE_ALL;
		m_sended_message = false;
		RoomID = ushort.MaxValue;
	}

	public range_time(ushort _hour_start, ushort _min_start, ushort _sec_start, ushort _hour_end, ushort _min_end, ushort _sec_end, eTYPE_MAKE_ROOM _type)
	{
		m_start = new TimeSpan(_hour_start, _min_start, _sec_start, 0);
		m_end = new TimeSpan(_hour_end, _min_end, _sec_end, 0);
		m_type = _type;
		m_sended_message = false;
	}

	public range_time(TimeSpan _start, TimeSpan _end, eTYPE_MAKE_ROOM _type)
	{
		m_start = _start;
		m_end = _end;
		m_type = _type;
		m_sended_message = false;
	}

	public virtual void Dispose()
	{
		clear();
	}

	public void clear()
	{
		m_start = default(TimeSpan);
		m_end = default(TimeSpan);
		m_sended_message = false;
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

	public bool isPastEnd(TimeSpan _st)
	{
		return timeToMilliseconds(_st) > timeToMilliseconds(m_end);
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
