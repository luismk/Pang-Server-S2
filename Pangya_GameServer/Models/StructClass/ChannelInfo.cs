using System;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ChannelInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class UFlag
	{
		public uint ulFlag { get; set; }

		public bool vs
		{
			get
			{
				return (ulFlag & 1) != 0;
			}
			set
			{
				ulFlag = Convert.ToUInt32(value ? (ulFlag | 1) : (ulFlag & -2));
			}
		}

		public bool tourney
		{
			get
			{
				return (ulFlag & 2) != 0;
			}
			set
			{
				ulFlag = (value ? (ulFlag | 2) : (ulFlag & 0xFFFFFFFDu));
			}
		}

		public bool Ongamenet
		{
			get
			{
				return (ulFlag & 4) != 0;
			}
			set
			{
				ulFlag = (value ? (ulFlag | 4) : (ulFlag & 0xFFFFFFFBu));
			}
		}

		public bool NoItem
		{
			get
			{
				return (ulFlag & 8) != 0;
			}
			set
			{
				ulFlag = (value ? (ulFlag | 8) : (ulFlag & 0xFFFFFFF7u));
			}
		}

		public bool random
		{
			get
			{
				return (ulFlag & 0x10) != 0;
			}
			set
			{
				ulFlag = (value ? (ulFlag | 0x10) : (ulFlag & 0xFFFFFFEFu));
			}
		}

		public bool adult
		{
			get
			{
				return (ulFlag & 0x20) != 0;
			}
			set
			{
				ulFlag = (value ? (ulFlag | 0x20) : (ulFlag & 0xFFFFFFDFu));
			}
		}

		public bool ladder
		{
			get
			{
				return (ulFlag & 0x80) != 0;
			}
			set
			{
				ulFlag = (value ? (ulFlag | 0x80) : (ulFlag & 0xFFFFFF7Fu));
			}
		}

		public bool Channel30IN9
		{
			get
			{
				return (ulFlag & 0x100) != 0;
			}
			set
			{
				ulFlag = (value ? (ulFlag | 0x100) : (ulFlag & 0xFFFFFEFFu));
			}
		}

		public bool LowLevel
		{
			get
			{
				return (ulFlag & 0x200) != 0;
			}
			set
			{
				ulFlag = (uint)(value ? (ulFlag | 0x200) : (ulFlag & -513));
			}
		}

		public bool HighLevel
		{
			get
			{
				return (ulFlag & 0x400) != 0;
			}
			set
			{
				ulFlag = (uint)(value ? (ulFlag | 0x400) : (ulFlag & -1025));
			}
		}

		public bool only_rookie
		{
			get
			{
				return (ulFlag & 0x800) != 0;
			}
			set
			{
				ulFlag = (uint)(value ? (ulFlag | 0x800) : (ulFlag & -2049));
			}
		}

		public bool beginner
		{
			get
			{
				return (ulFlag & 0x1000) != 0;
			}
			set
			{
				ulFlag = (uint)(value ? (ulFlag | 0x1000) : (ulFlag & -4097));
			}
		}

		public bool senior
		{
			get
			{
				return (ulFlag & 0x2000) != 0;
			}
			set
			{
				ulFlag = (uint)(value ? (ulFlag | 0x2000) : (ulFlag & -8193));
			}
		}

		public bool skins
		{
			get
			{
				return (ulFlag & 0x4000) != 0;
			}
			set
			{
				ulFlag = (value ? (ulFlag | 0x4000) : (ulFlag & 0xFFFFBFFFu));
			}
		}

		public UFlag(uint flag = 0u)
		{
			ulFlag = flag;
		}
	}

	[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string name { get; set; }

	public short max_user { get; set; }

	public short curr_user { get; set; }

	public sbyte id { get; set; }

	[field: MarshalAs(UnmanagedType.Struct)]
	public UFlag type { get; set; }

	public uint property { get; set; }

	public uint min_level_allow { get; set; }

	public uint max_level_allow { get; set; }

	public ChannelInfo()
	{
		clear();
	}

	public void clear()
	{
		type = new UFlag();
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteStr(name, 64);
		p.WriteInt16(max_user);
		p.WriteInt16(curr_user);
		p.WriteByte(id);
		p.WriteUInt32(type.ulFlag);
		return p.GetBytes;
	}
}
