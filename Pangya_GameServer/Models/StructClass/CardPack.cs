using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

public class CardPack
{
	public class Rate
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public ushort[] value = new ushort[4];

		public void clear()
		{
			value = new ushort[4];
		}
	}

	public uint _typeid = 0u;

	public uint num = 0u;

	public byte volume;

	public Rate rate = new Rate();

	public List<Card> card = new List<Card>();

	public CardPack(uint _ul = 0u)
	{
		clear();
	}

	public CardPack(uint __typeid, uint _num, byte _volume)
	{
		_typeid = __typeid;
		num = _num;
		volume = _volume;
	}

	public void clear()
	{
		rate = new Rate();
		card = new List<Card>();
		if (card.Count > 0)
		{
			card.Clear();
		}
		_typeid = 0u;
		num = 0u;
		volume = 0;
	}
}
