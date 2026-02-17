using System;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

public class stItem
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class item_stat
	{
		public int qntd_ant = 0;

		public int qntd_dep = 0;

		public void clear()
		{
			qntd_ant = 0;
			qntd_dep = 0;
		}

		public byte[] ToArray()
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.Write(qntd_ant);
			p.Write(qntd_dep);
			return p.GetBytes;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class UCC
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
		public string IDX;

		public uint status = 0u;

		public uint seq = 0u;

		public void clear()
		{
			IDX = "";
			status = 0u;
			seq = 0u;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class stDate
	{
		public class stDateSys
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public SYSTEMTIME[] sysDate = new SYSTEMTIME[2]
			{
				new SYSTEMTIME(),
				new SYSTEMTIME()
			};

			public void clear()
			{
				sysDate = new SYSTEMTIME[2]
				{
					new SYSTEMTIME(),
					new SYSTEMTIME()
				};
			}

			public stDateSys()
			{
				clear();
			}

			public stDateSys(SYSTEMTIME start, SYSTEMTIME end)
			{
				clear();
				sysDate[0].SetInfo(start);
				sysDate[1].SetInfo(end);
			}
		}

		public uint active = 1u;

		public stDateSys date = new stDateSys();

		public stDate()
		{
			clear();
		}

		public stDate(int index, stDateSys st)
		{
			clear();
			date.sysDate = st.sysDate;
		}

		public void clear()
		{
			active = 0u;
			date.clear();
		}
	}

	public int id = 0;

	public uint _typeid = 0u;

	public byte type_iff;

	public byte type;

	public byte flag;

	public byte flag_time;

	public int qntd = 0;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string name = new string(new char[64]);

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
	public string icon = new string(new char[41]);

	[MarshalAs(UnmanagedType.Struct)]
	public item_stat stat = new item_stat();

	[MarshalAs(UnmanagedType.Struct)]
	public UCC ucc = new UCC();

	public byte is_cash = 0;

	public uint price = 0u;

	public uint desconto = 0u;

	public stDate date = new stDate();

	public ushort date_reserve;

	public short[] c = new short[5];

	public short STDA_C_ITEM_QNTD
	{
		get
		{
			return c[0];
		}
		set
		{
			c[0] = value;
		}
	}

	public short STDA_C_ITEM_TICKET_REPORT_ID_HIGH
	{
		get
		{
			return c[1];
		}
		set
		{
			c[1] = value;
		}
	}

	public short STDA_C_ITEM_TICKET_REPORT_ID_LOW
	{
		get
		{
			return c[2];
		}
		set
		{
			c[2] = value;
		}
	}

	public short STDA_C_ITEM_TIME
	{
		get
		{
			return c[3];
		}
		set
		{
			c[3] = value;
		}
	}

	public int STDA_C_ITEM_QNTD32 => Convert.ToInt32(STDA_C_ITEM_QNTD);

	public int STDA_C_ITEM_TIME32 => Convert.ToInt32(STDA_C_ITEM_TIME);

	public virtual void clear()
	{
		id = 0;
		_typeid = 0u;
		type_iff = 0;
		type = 0;
		flag = 0;
		flag_time = 0;
		qntd = 0;
		name = "";
		icon = "";
		stat.clear();
		ucc.clear();
		is_cash = 0;
		price = 0u;
		desconto = 0u;
		date.clear();
		date_reserve = 0;
	}

	public bool IsUCC()
	{
		return type_iff == 8 || type_iff == 9;
	}

	public stItem()
	{
	}

	public stItem(stItem _item)
	{
		id = _item.id;
		_typeid = _item._typeid;
		type_iff = _item.type_iff;
		type = _item.type;
		flag = _item.flag;
		flag_time = _item.flag_time;
		qntd = _item.qntd;
		name = _item.name;
		icon = _item.icon;
		stat = new item_stat
		{
			qntd_ant = _item.stat.qntd_ant,
			qntd_dep = _item.stat.qntd_dep
		};
		ucc = new UCC
		{
			IDX = _item.ucc.IDX,
			status = _item.ucc.status,
			seq = _item.ucc.seq
		};
		is_cash = _item.is_cash;
		price = _item.price;
		desconto = _item.desconto;
		date = new stDate
		{
			active = _item.date.active,
			date = new stDate.stDateSys
			{
				sysDate = new SYSTEMTIME[2]
				{
					_item.date.date.sysDate[0],
					_item.date.date.sysDate[1]
				}
			}
		};
		date_reserve = _item.date_reserve;
		c = new short[_item.c.Length];
		Array.Copy(_item.c, c, _item.c.Length);
	}
}
