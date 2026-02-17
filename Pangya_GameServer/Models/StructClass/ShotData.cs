using System;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ShotData : ShotDataBase
{
	public float spend_time_game;

	public ShotData()
	{
		clear();
	}

	public override string ToString()
	{
		return base.ToString() + "Spend Time Game: " + Convert.ToString(spend_time_game) + Environment.NewLine;
	}

	public byte[] ToArrayEx()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteBytes(ToArray());
		p.WriteFloat(spend_time_game);
		return p.GetBytes;
	}
}
