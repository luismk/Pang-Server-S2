using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class AttendanceRewardInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class item
	{
		public uint _typeid;

		public uint qntd;

		public void clear()
		{
			_typeid = 0u;
			qntd = 0u;
		}
	}

	public byte login;

	[MarshalAs(UnmanagedType.Struct)]
	public item now;

	[MarshalAs(UnmanagedType.Struct)]
	public item after;

	public uint counter;

	public AttendanceRewardInfo()
	{
		clear();
	}

	public void clear()
	{
		now = new item();
		after = new item();
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteByte(login);
		p.WriteUInt32(now._typeid);
		p.WriteUInt32(now.qntd);
		p.WriteUInt32(after._typeid);
		p.WriteUInt32(after.qntd);
		p.WriteUInt32(counter);
		return p.GetBytes;
	}
}
