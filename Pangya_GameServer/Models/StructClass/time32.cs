using System;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class time32
{
	public ushort high_time;

	public ushort low_time;

	public void setTime(int time)
	{
		high_time = (ushort)(time / 65535);
		low_time = (ushort)(time % 65535);
	}

	public uint getTime()
	{
		return Convert.ToUInt32((high_time * 65535) | low_time);
	}
}
