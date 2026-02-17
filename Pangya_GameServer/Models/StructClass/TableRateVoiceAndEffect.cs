using System;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

public class TableRateVoiceAndEffect
{
	public enum eTYPE : byte
	{
		NONE,
		W_BIGBONGDARI,
		R_BIGBONGDARI,
		VOICE_CLUB
	}

	public string name;

	public eTYPE type;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	public byte[] table = new byte[100];

	public TableRateVoiceAndEffect()
	{
		clear();
	}

	public TableRateVoiceAndEffect(string _name, eTYPE _type)
	{
		name = _name;
		type = _type;
		randomTable();
	}

	public void clear()
	{
		name = "";
		type = eTYPE.NONE;
	}

	public void randomTable()
	{
		ushort min_value = 0;
		if (type == eTYPE.VOICE_CLUB)
		{
			min_value = 1;
		}
		Random rnd = new Random();
		for (int i = 0; i < 100; i++)
		{
			int randValue = rnd.Next();
			table[i] = (byte)(min_value + randValue % (4 - min_value));
		}
	}
}
