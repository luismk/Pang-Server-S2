using System;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class WarehouseItemEx : WarehouseItem
{
	public uint apply_date_unix_local;

	public uint end_date_unix_local;

	public short STDA_C_ITEM_QNTD
	{
		get
		{
			return base.c[0];
		}
		set
		{
			base.c[0] = value;
		}
	}

	public short STDA_C_ITEM_TICKET_REPORT_ID_HIGH
	{
		get
		{
			return base.c[1];
		}
		set
		{
			base.c[1] = value;
		}
	}

	public short STDA_C_ITEM_TICKET_REPORT_ID_LOW
	{
		get
		{
			return base.c[2];
		}
		set
		{
			base.c[2] = value;
		}
	}

	public short STDA_C_ITEM_TIME
	{
		get
		{
			return base.c[3];
		}
		set
		{
			base.c[3] = value;
		}
	}

	public int STDA_C_ITEM_QNTD32 => Convert.ToInt32(STDA_C_ITEM_QNTD);

	public WarehouseItemEx()
	{
		clear();
	}

	public WarehouseItemEx(WarehouseItemEx pWi)
	{
		clear();
		base.id = pWi.id;
		base._typeid = pWi._typeid;
		base.ano = pWi.ano;
		if (pWi.c != null)
		{
			Array.Copy(pWi.c, base.c, base.c.Length);
		}
		base.purchase = pWi.purchase;
		base.flag = pWi.flag;
		base.apply_date = pWi.apply_date;
		base.end_date = pWi.end_date;
		base.type = pWi.type;
		if (pWi.ucc != null)
		{
			ucc.name = pWi.ucc.name;
			ucc.trade = pWi.ucc.trade;
			ucc.idx = pWi.ucc.idx;
			ucc.status = pWi.ucc.status;
			ucc.seq = pWi.ucc.seq;
			ucc.copier_nick = pWi.ucc.copier_nick;
			ucc.copier = pWi.ucc.copier;
		}
		if (pWi.card != null)
		{
			Array.Copy(pWi.card.character, card.character, card.character.Length);
			Array.Copy(pWi.card.caddie, card.caddie, card.caddie.Length);
			Array.Copy(pWi.card.NPC, card.NPC, card.NPC.Length);
		}
		if (pWi.clubset_workshop != null)
		{
			clubset_workshop.flag = pWi.clubset_workshop.flag;
			if (pWi.clubset_workshop.c != null)
			{
				Array.Copy(pWi.clubset_workshop.c, clubset_workshop.c, clubset_workshop.c.Length);
			}
			clubset_workshop.mastery = pWi.clubset_workshop.mastery;
			clubset_workshop.recovery_pts = pWi.clubset_workshop.recovery_pts;
			clubset_workshop.level = pWi.clubset_workshop.level;
			clubset_workshop.rank = pWi.clubset_workshop.rank;
		}
		apply_date_unix_local = pWi.apply_date_unix_local;
		end_date_unix_local = pWi.end_date_unix_local;
	}
}
