using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PangyaTime_ItemBuff : SYSTEMTIME
{
	public void setTime(uint time)
	{
		base.Second = (ushort)(time / 65535);
		base.MilliSecond = (ushort)(time % 65535);
	}

	public uint getTime()
	{
		return (uint)((base.Second * 65535) | base.MilliSecond);
	}
}
