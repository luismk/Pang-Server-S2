using System;
using System.IO;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PlayerLobbyInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class uStateFlag
	{
		public byte ucByte;

		public byte away
		{
			get
			{
				return (byte)(ucByte & 1);
			}
			set
			{
				ucByte = (byte)((ucByte & -2) | (value & 1));
			}
		}

		public byte sexo
		{
			get
			{
				return (byte)((ucByte >> 1) & 1);
			}
			set
			{
				ucByte = (byte)((ucByte & -3) | ((value & 1) << 1));
			}
		}

		public byte quiter_1
		{
			get
			{
				return (byte)((ucByte >> 2) & 1);
			}
			set
			{
				ucByte = (byte)((ucByte & -5) | ((value & 1) << 2));
			}
		}

		public byte quiter_2
		{
			get
			{
				return (byte)((ucByte >> 3) & 1);
			}
			set
			{
				ucByte = (byte)((ucByte & -9) | ((value & 1) << 3));
			}
		}

		public byte azinha
		{
			get
			{
				return (byte)((ucByte >> 4) & 1);
			}
			set
			{
				ucByte = (byte)((ucByte & -17) | ((value & 1) << 4));
			}
		}

		public byte icon_angel
		{
			get
			{
				return (byte)((ucByte >> 5) & 1);
			}
			set
			{
				ucByte = (byte)((ucByte & -33) | ((value & 1) << 5));
			}
		}

		public byte ucUnknown_bit7
		{
			get
			{
				return (byte)((ucByte >> 6) & 1);
			}
			set
			{
				ucByte = (byte)((ucByte & -65) | ((value & 1) << 6));
			}
		}

		public byte ucUnknown_bit8
		{
			get
			{
				return (byte)((ucByte >> 7) & 1);
			}
			set
			{
				ucByte = (byte)((ucByte & -129) | ((value & 1) << 7));
			}
		}

		public uStateFlag()
		{
			ucByte = 0;
		}

		public uStateFlag(byte op)
		{
			ucByte = op;
		}
	}

	public uint uid;

	public int oid;

	public sbyte sala_numero;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
	private byte[] id_bytes;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
	private byte[] nickname_bytes;

	public byte level;

	public uCapability capability;

	public uint title;

	public uint team_point;

	[MarshalAs(UnmanagedType.Struct)]
	public uStateFlag state_flag;

	public int guild_uid;

	public uint guild_index_mark;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
	public string guild_mark_img;

	public short flag_visible_gm;

	public uint l_unknown;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
	private byte[] sDisplayID_Bytes;

	public string id
	{
		get
		{
			return id_bytes.GetString();
		}
		set
		{
			id_bytes.SetString(value);
		}
	}

	public string nickname
	{
		get
		{
			return nickname_bytes.GetString();
		}
		set
		{
			nickname_bytes.SetString(value);
		}
	}

	public string sDisplayID
	{
		get
		{
			return sDisplayID_Bytes.GetString();
		}
		set
		{
			sDisplayID_Bytes.SetString(value);
		}
	}

	public PlayerLobbyInfo()
	{
		clear();
	}

	public void clear()
	{
		sala_numero = -1;
		capability = new uCapability();
		state_flag = new uStateFlag();
		guild_mark_img = "";
		sDisplayID_Bytes = new byte[128];
		id_bytes = new byte[22];
		nickname_bytes = new byte[22];
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteUInt32(uid);
		p.WriteSByte(sala_numero);
		p.WriteStr(id, 22);
		p.WriteStr(nickname, 22);
		p.WriteByte(level);
		p.WriteInt32(capability.ulCapability);
		p.WriteUInt32(title);//title id
		p.WriteUInt32(team_point);
		p.WriteByte(1);
		p.WriteInt32(1);
		p.WriteUInt32(1); 
		return p.GetBytes;
	}
}
