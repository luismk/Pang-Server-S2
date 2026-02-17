using System;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ShotDataBase
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public float[] bar_point = new float[2];

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public float[] ball_effect = new float[2];

	public byte acerto_pangya_flag;

	[MarshalAs(UnmanagedType.Struct)]
	public uSpecialShot special_shot = new uSpecialShot();

	public uint time_hole_sync = 0u;

	public float mira;

	public uint time_shot = 0u;

	public float bar_point1;

	public byte club;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public float[] fUnknown = new float[2];

	public float impact_zone_pixel;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	public int[] natural_wind = new int[2];

	public ShotDataBase()
	{
		clear();
	}

	public void clear()
	{
		Array.Clear(bar_point, 0, 2);
		Array.Clear(ball_effect, 0, 2);
		acerto_pangya_flag = 0;
		special_shot = new uSpecialShot();
		time_hole_sync = 0u;
		mira = 0f;
		time_shot = 0u;
		bar_point1 = 0f;
		club = 0;
		Array.Clear(fUnknown, 0, 2);
		impact_zone_pixel = 0f;
		Array.Clear(natural_wind, 0, 2);
	}

	public override string ToString()
	{
		return "Bar Point: Forca: " + bar_point[0] + " Hit PangYa: " + bar_point[1] + Environment.NewLine + "Ball Effect: X: " + ball_effect[0] + " Y: " + ball_effect[1] + Environment.NewLine + "Acerto PangYa Flag: " + acerto_pangya_flag + Environment.NewLine + "Special Shot: " + special_shot.ToString() + Environment.NewLine + "Time Hole SYNC: " + time_hole_sync + Environment.NewLine + "Mira(shot): " + mira + Environment.NewLine + "Time Shot: " + time_shot + Environment.NewLine + "Bar Point: Start: " + bar_point1 + Environment.NewLine + "Club: " + club + Environment.NewLine + "fUnknown: [1]: " + fUnknown[0] + " [2]: " + fUnknown[1] + Environment.NewLine + "Impact Zone Size Pixel: " + impact_zone_pixel + Environment.NewLine + "Natural Wind: X: " + natural_wind[0] + " Y: " + natural_wind[1] + Environment.NewLine;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteFloat(bar_point);
		p.WriteFloat(ball_effect);
		p.Write(acerto_pangya_flag);
		p.Write(special_shot.ulSpecialShot);
		p.Write(time_hole_sync);
		p.Write(mira);
		p.Write(time_shot);
		p.Write(bar_point1);
		p.Write(club);
		p.WriteFloat(fUnknown);
		p.Write(impact_zone_pixel);
		p.WriteInt32(natural_wind);
		return p.GetBytes;
	}
}
