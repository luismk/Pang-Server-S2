using System.Collections.Generic;
using System.Text;

namespace Pangya_GameServer.Models;

public class CPLog
{
	public enum TYPE : byte
	{
		BUY_SHOP,
		GIFT_SHOP,
		TICKER,
		CP_POUCH
	}

	public struct stItem
	{
		public uint _typeid;

		public int qntd;

		public ulong price;

		public stItem(uint _ul = 0u)
		{
			_typeid = 0u;
			qntd = 0;
			price = 0uL;
			clear();
		}

		public stItem(uint __typeid, int _qntd, ulong _cp)
		{
			_typeid = __typeid;
			qntd = _qntd;
			price = _cp;
		}

		public void clear()
		{
			_typeid = 0u;
			qntd = 0;
			price = 0uL;
		}
	}

	protected TYPE m_type;

	protected int m_mail_id;

	protected ulong m_cookie;

	protected List<stItem> v_item;

	public CPLog(uint _ul = 0u)
	{
		v_item = new List<stItem>();
		clear();
	}

	~CPLog()
	{
	}

	public void clear()
	{
		m_type = TYPE.BUY_SHOP;
		m_mail_id = -1;
		m_cookie = 0uL;
		if (v_item != null && v_item.Count > 0)
		{
			v_item.Clear();
			v_item.TrimExcess();
		}
	}

	public TYPE getType()
	{
		return m_type;
	}

	public void setType(TYPE _type)
	{
		m_type = _type;
	}

	public int getMailId()
	{
		return m_mail_id;
	}

	public void setMailId(int _mail_id)
	{
		m_mail_id = _mail_id;
	}

	public ulong getCookie()
	{
		ulong total = m_cookie;
		v_item.ForEach(delegate(stItem el)
		{
			total += el.price;
		});
		return total;
	}

	public void setCookie(ulong _cp)
	{
		m_cookie = _cp;
	}

	public uint getItemCount()
	{
		return (uint)v_item.Count;
	}

	public List<stItem> getItens()
	{
		return v_item;
	}

	public void putItem(uint _typeid, int _qntd, ulong _cp)
	{
		v_item.Add(new stItem(_typeid, _qntd, _cp));
	}

	public void putItem(stItem _item)
	{
		v_item.Add(_item);
	}

	public string toString()
	{
		StringBuilder str = new StringBuilder();
		str.Append("TYPE=").Append((ushort)m_type).Append(", mail_id=")
			.Append(m_mail_id)
			.Append(", cookie=")
			.Append(getCookie())
			.Append(", item(ns) quantidade=")
			.Append(v_item.Count);
		foreach (stItem el in v_item)
		{
			str.Append(", {TYPEID=").Append(el._typeid).Append(", QNTD=")
				.Append(el.qntd)
				.Append(", PRICE=")
				.Append(el.price)
				.Append("}");
		}
		return str.ToString();
	}
}
