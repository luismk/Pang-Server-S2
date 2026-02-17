using System.Collections.Generic;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public class TicketReportInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public class stTicketReportDados
	{
		public uint uid = 0u;

		public int score = 0;

		[MarshalAs(UnmanagedType.Struct)]
		public uMedalWin medal = new uMedalWin();

		public byte trofel;

		public ulong pang = 0uL;

		public ulong bonus_pang = 0uL;

		public uint exp = 0u;

		public uint mascot_typeid = 0u;

		public uint flag_item_pang = 0u;

		public uint premium = 0u;

		public uint state = 0u;

		[MarshalAs(UnmanagedType.Struct)]
		public SYSTEMTIME finish_time = new SYSTEMTIME();

		public void clear()
		{
		}
	}

	public int id = 0;

	public List<stTicketReportDados> v_dados = new List<stTicketReportDados>();

	public TicketReportInfo()
	{
		clear();
	}

	public void clear()
	{
		id = -1;
		v_dados.Clear();
	}
}
