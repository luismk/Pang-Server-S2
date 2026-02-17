using System;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class CaddieInfoEx : CaddieInfo
{
	public SYSTEMTIME end_date;

	public SYSTEMTIME end_parts_date;

	public bool need_update;

	public CaddieInfoEx()
	{
		end_date = new SYSTEMTIME();
		end_parts_date = new SYSTEMTIME();
	}

	public void updatePartsEndDate()
	{
		if (end_parts_date.IsEmpty)
		{
			parts_end_date_unix = 0;
			if (parts_typeid != 0)
			{
				parts_typeid = 0u;
				need_update = true;
			}
			return;
		}
		DateTime now = DateTime.Now.Date;
		DateTime end = end_parts_date.ConvertTime().Date;
		int diffDays = (end - now).Days;
		if (diffDays <= 0)
		{
			if (parts_typeid != 0)
			{
				parts_typeid = 0u;
				need_update = true;
			}
		}
		else
		{
			parts_end_date_unix = (short)diffDays;
		}
	}

	public void updateEndDate()
	{
		if (end_date.IsEmpty)
		{
			end_date_unix = 0;
			return;
		}
		DateTime now = DateTime.Now.Date;
		DateTime end = end_date.ConvertTime().Date;
		int diffDays = (end - now).Days;
		if (diffDays <= 0)
		{
			end_date_unix = 0;
		}
		else
		{
			end_date_unix = (ushort)diffDays;
		}
	}

	public void Check()
	{
		updateEndDate();
		updatePartsEndDate();
	}

	public override void clear()
	{
		base.clear();
		end_parts_date = new SYSTEMTIME();
	}

	public CaddieInfoEx getInfo()
	{
		Check();
		return this;
	}
}
