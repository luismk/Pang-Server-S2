using System;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

public class stItemEx : stItem
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class ClubSetWorkshop
	{
		public short[] c = new short[5];

		public uint mastery = 0u;

		public byte level;

		public uint rank = 0u;

		public uint recovery = 0u;

		public void clear()
		{
		}

		public byte[] ToArray()
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.WriteInt16(c);
			p.WriteUInt32(mastery);
			p.WriteByte(level);
			p.WriteUInt32(rank);
			p.WriteUInt32(recovery);
			return p.GetBytes;
		}
	}

	public ClubSetWorkshop clubset_workshop = new ClubSetWorkshop();

	public stItemEx(uint _ul = 0u)
	{
		clear();
	}

	public stItemEx(stItemEx _item)
	{
		id = _item.id;
		_typeid = _item._typeid;
		type_iff = _item.type_iff;
		type = _item.type;
		flag = _item.flag;
		flag_time = _item.flag_time;
		qntd = _item.qntd;
		name = string.Copy(_item.name);
		icon = string.Copy(_item.icon);
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
		clubset_workshop = _item.clubset_workshop;
	}

	public stItemEx(stItem _item)
		: base(_item)
	{
	}

	public override void clear()
	{
		clubset_workshop = new ClubSetWorkshop();
	}
}
