#define DEBUG
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TrofelInfo
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
	public short[,] ama_6_a_1 = new short[6, 3];

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
	public short[,] pro_1_a_7 = new short[7, 3];

	public TrofelInfo()
	{
		clear();
	}

	public void clear()
	{
		Array.Clear(ama_6_a_1, 0, ama_6_a_1.Length);
		Array.Clear(pro_1_a_7, 0, pro_1_a_7.Length);
	}

	public void update(uint _type, byte _rank)
	{
		if (_type > 12)
		{
			throw new exception("[TrofelInfo::update][Error] _type[VALUE=" + _type + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_GAME_ST, 200u, 0u));
		}
		if (_rank == 0 || _rank > 3)
		{
			throw new exception("[TrofelInfo::update][Error] _rank[VALUE=" + _rank + "] is invalid", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_GAME_ST, 201u, 0u));
		}
		if (_type < 6)
		{
			ama_6_a_1[_type, _rank - 1]++;
		}
		else
		{
			pro_1_a_7[_type - 6, _rank - 1]++;
		}
	}

	public uint getSumGold()
	{
		uint gold_sum = 0u;
		for (int i = 0; i < ama_6_a_1.GetLength(0); i++)
		{
			gold_sum += (uint)ama_6_a_1[i, 0];
		}
		for (int j = 0; j < pro_1_a_7.GetLength(0); j++)
		{
			gold_sum += (uint)pro_1_a_7[j, 0];
		}
		return gold_sum;
	}

	public uint getSumSilver()
	{
		uint silver_sum = 0u;
		for (int i = 0; i < ama_6_a_1.GetLength(0); i++)
		{
			silver_sum += (uint)ama_6_a_1[i, 1];
		}
		for (int j = 0; j < pro_1_a_7.GetLength(0); j++)
		{
			silver_sum += (uint)pro_1_a_7[j, 1];
		}
		return silver_sum;
	}

	public uint getSumBronze()
	{
		uint bronze_sum = 0u;
		for (int i = 0; i < ama_6_a_1.GetLength(0); i++)
		{
			bronze_sum += (uint)ama_6_a_1[i, 2];
		}
		for (int j = 0; j < pro_1_a_7.GetLength(0); j++)
		{
			bronze_sum += (uint)pro_1_a_7[j, 2];
		}
		return bronze_sum;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				p.Write(ama_6_a_1[i, j]);
			}
		}
		for (int k = 0; k < 5; k++)
		{
			for (int l = 0; l < 3; l++)
			{
				p.Write(pro_1_a_7[k, l]);
			}
		}
		Debug.Assert(p.BaseStream.Length == 66, "TrofelInfo::ToArray() is error");
		return p.GetBytes;
	}
}
