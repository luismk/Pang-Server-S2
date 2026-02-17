using System;
using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ShotDataEx : ShotData
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class PowerShot
	{
		public byte option;

		public int decrease_power_shot = 0;

		public int increase_power_shot = 0;

		public PowerShot()
		{
			clear();
		}

		public override string ToString()
		{
			return "Option: " + Convert.ToString((ushort)option) + Environment.NewLine + "Decrease Power Shot: " + Convert.ToString(decrease_power_shot) + Environment.NewLine + "Increase Power Shot: " + Convert.ToString(increase_power_shot) + Environment.NewLine;
		}

		public void clear()
		{
		}
	}

	public ushort option;

	[MarshalAs(UnmanagedType.Struct)]
	public PowerShot power_shot = new PowerShot();

	public ShotDataEx()
	{
		clear();
	}

	public override string ToString()
	{
		return (option != 0) ? (power_shot.ToString() + base.ToString()) : base.ToString();
	}

	public void setShot(ShotData value)
	{
		if (value != null)
		{
			spend_time_game = value.spend_time_game;
			Array.Copy(value.bar_point, bar_point, 2);
			Array.Copy(value.ball_effect, ball_effect, 2);
			acerto_pangya_flag = value.acerto_pangya_flag;
			special_shot.ulSpecialShot = value.special_shot.ulSpecialShot;
			time_hole_sync = value.time_hole_sync;
			mira = value.mira;
			time_shot = value.time_shot;
			bar_point1 = value.bar_point1;
			club = value.club;
			Array.Copy(value.fUnknown, fUnknown, 2);
			impact_zone_pixel = value.impact_zone_pixel;
			Array.Copy(value.natural_wind, natural_wind, 2);
		}
	}

	public ShotDataEx ToRead(packet _packet)
	{
		option = _packet.ReadUInt16();
		if (option == 1)
		{
			power_shot.option = _packet.ReadByte();
			power_shot.decrease_power_shot = _packet.ReadInt32();
			power_shot.increase_power_shot = _packet.ReadInt32();
		}
		bar_point[0] = _packet.ReadSingle();
		bar_point[1] = _packet.ReadSingle();
		ball_effect[0] = _packet.ReadSingle();
		ball_effect[1] = _packet.ReadSingle();
		acerto_pangya_flag = _packet.ReadByte();
		special_shot.ulSpecialShot = _packet.ReadUInt32();
		time_hole_sync = _packet.ReadUInt32();
		mira = _packet.ReadSingle();
		time_shot = _packet.ReadUInt32();
		bar_point1 = _packet.ReadSingle();
		club = _packet.ReadByte();
		fUnknown[0] = _packet.ReadSingle();
		fUnknown[1] = _packet.ReadSingle();
		impact_zone_pixel = _packet.ReadSingle();
		natural_wind[0] = _packet.ReadInt32();
		natural_wind[1] = _packet.ReadInt32();
		return this;
	}

	public ShotDataEx ToReadEx(packet _packet)
	{
		ToRead(_packet);
		spend_time_game = _packet.ReadSingle();
		return this;
	}
}
