using System.Runtime.InteropServices;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 196)]
public class WarehouseItem
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class UCC
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
		private byte[] name_bytes;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
		private byte[] idx_bytes;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
		private byte[] copier_nick_bytes;

		public string name
		{
			get
			{
				return name_bytes.GetString();
			}
			set
			{
				name_bytes.SetString(value);
			}
		}

		public sbyte trade { get; set; }

		public string idx
		{
			get
			{
				return idx_bytes.GetString();
			}
			set
			{
				idx_bytes.SetString(value);
			}
		}

		public byte status { get; set; }

		public short seq { get; set; }

		public string copier_nick
		{
			get
			{
				return copier_nick_bytes.GetString();
			}
			set
			{
				copier_nick_bytes.SetString(value);
			}
		}

		public uint copier { get; set; }

		public UCC()
		{
			name_bytes = new byte[40];
			idx_bytes = new byte[9];
			copier_nick_bytes = new byte[22];
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class Card
	{
		[field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public uint[] character { get; set; } = new uint[4];

		[field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public uint[] caddie { get; set; } = new uint[4];

		[field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public uint[] NPC { get; set; } = new uint[4];
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class ClubsetWorkshop
	{
		public short flag { get; set; }

		[field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public short[] c { get; set; } = new short[5];

		public uint mastery { get; set; }

		public uint recovery_pts { get; set; }

		public int level { get; set; }

		public int rank { get; set; }

		public int calcRank(ushort[] _c)
		{
			int total = c[0] + _c[0] + c[1] + _c[1] + c[2] + _c[2] + c[3] + _c[3] + c[4] + _c[4];
			if (total >= 30 && total < 60)
			{
				return (total - 30) / 5;
			}
			return int.MaxValue;
		}

		public int calcLevel(ushort[] _c)
		{
			int total = c[0] + _c[0] + c[1] + _c[1] + c[2] + _c[2] + c[3] + _c[3] + c[4] + _c[4];
			if (total >= 30 && total < 60)
			{
				return (total - 30) % 5;
			}
			return int.MaxValue;
		}

		public static int s_calcRank(ushort[] _c)
		{
			uint total = (uint)(_c[0] + _c[1] + _c[2] + _c[3] + _c[4]);
			if (total >= 30 && total < 60)
			{
				return (int)(total - 30) / 5;
			}
			return -1;
		}

		public static int s_calcLevel(ushort[] _c)
		{
			uint total = (uint)(_c[0] + _c[1] + _c[2] + _c[3] + _c[4]);
			if (total >= 30 && total < 60)
			{
				return (int)(total - 30) % 5;
			}
			return -1;
		}
	}

	[MarshalAs(UnmanagedType.Struct)]
	public UCC ucc;

	[MarshalAs(UnmanagedType.Struct)]
	public Card card;

	[MarshalAs(UnmanagedType.Struct)]
	public ClubsetWorkshop clubset_workshop;

	public int id { get; set; }

	public uint _typeid { get; set; }

	public int ano { get; set; }

	[field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public short[] c { get; set; } = new short[5];

	public byte purchase { get; set; }

	public sbyte flag { get; set; }

	public long apply_date { get; set; }

	public long end_date { get; set; }

	public sbyte type { get; set; }

	public WarehouseItem()
	{
		clear();
	}

	public void clear()
	{
		c = new short[5];
		card = new Card
		{
			caddie = new uint[4],
			character = new uint[4],
			NPC = new uint[4]
		};
		clubset_workshop = new ClubsetWorkshop
		{
			c = new short[5]
		};
		ucc = new UCC();
	}

	public bool IsUCC()
	{
		return Singleton<IFFHandle>.getInstance().getItemGroupIdentify(_typeid) == Singleton<IFFHandle>.getInstance().PART && !string.IsNullOrEmpty(ucc.idx);
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(id);
		p.WriteUInt32(_typeid);
		p.WriteInt32(ano);
		p.WriteInt16(c);
		p.WriteByte(purchase);
		p.WriteSByte(flag);
		return p.GetBytes;
	}
}
