using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class AttendanceRewardInfoEx : AttendanceRewardInfo
{
	[MarshalAs(UnmanagedType.Struct)]
	public SYSTEMTIME last_login;

	public AttendanceRewardInfoEx()
	{
		last_login = new SYSTEMTIME();
		clear();
	}
}
