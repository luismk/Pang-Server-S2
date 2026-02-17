using System;
using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ShotEndLocationData
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class stLocation
	{
		public float x;

		public float y;

		public float z;

		public void clear()
		{
			x = 0f;
			y = 0f;
			z = 0f;
		}

		public override string ToString()
		{
			return "X: " + Convert.ToString(x) + " Y: " + Convert.ToString(y) + " Z: " + Convert.ToString(z);
		}

		public byte[] ToArray()
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.WriteFloat(x);
			p.WriteFloat(y);
			p.WriteFloat(z);
			return p.GetBytes;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class BallPoint
	{
		public float x;

		public float y;

		public void clear()
		{
			x = 0f;
			y = 0f;
		}

		public override string ToString()
		{
			return "X: " + Convert.ToString(x) + " Y: " + Convert.ToString(y);
		}

		public byte[] ToArray()
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.WriteFloat(x);
			p.WriteFloat(y);
			return p.GetBytes;
		}
	}

	public float porcentagem;

	[MarshalAs(UnmanagedType.Struct)]
	public stLocation ball_velocity = new stLocation();

	public byte option;

	[MarshalAs(UnmanagedType.Struct)]
	public stLocation location = new stLocation();

	[MarshalAs(UnmanagedType.Struct)]
	public stLocation wind_influence = new stLocation();

	[MarshalAs(UnmanagedType.Struct)]
	public BallPoint ball_point = new BallPoint();

	[MarshalAs(UnmanagedType.Struct)]
	public uSpecialShot special_shot = new uSpecialShot();

	public float ball_rotation_spin;

	public float ball_rotation_curve;

	public byte stUnknown;

	public byte taco;

	public float power_factor;

	public float power_club;

	public float rotation_spin_factor;

	public float rotation_curve_factor;

	public float power_factor_shot;

	public uint time_hole_sync = 0u;

	public ShotEndLocationData()
	{
	}

	public ShotEndLocationData(packet r)
	{
		porcentagem = r.ReadSingle();
		ball_velocity.x = r.ReadSingle();
		ball_velocity.y = r.ReadSingle();
		ball_velocity.z = r.ReadSingle();
		option = r.ReadByte();
		location.x = r.ReadSingle();
		location.y = r.ReadSingle();
		location.z = r.ReadSingle();
		wind_influence.x = r.ReadSingle();
		wind_influence.y = r.ReadSingle();
		wind_influence.z = r.ReadSingle();
		ball_point.x = r.ReadSingle();
		ball_point.y = r.ReadSingle();
		special_shot.ulSpecialShot = r.ReadUInt32();
		ball_rotation_spin = r.ReadSingle();
		ball_rotation_curve = r.ReadSingle();
		stUnknown = r.ReadByte();
		taco = r.ReadByte();
		power_factor = r.ReadSingle();
		power_club = r.ReadSingle();
		rotation_spin_factor = r.ReadSingle();
		rotation_curve_factor = r.ReadSingle();
		power_factor_shot = r.ReadSingle();
		time_hole_sync = r.ReadUInt32();
	}

	public void clear()
	{
		porcentagem = 0f;
		ball_velocity.clear();
		option = 0;
		location.clear();
		wind_influence.clear();
		ball_point.clear();
		special_shot.ulSpecialShot = 0u;
		ball_rotation_spin = 0f;
		ball_rotation_curve = 0f;
		stUnknown = 0;
		taco = 0;
		power_factor = 0f;
		power_club = 0f;
		rotation_spin_factor = 0f;
		rotation_curve_factor = 0f;
		power_factor_shot = 0f;
		time_hole_sync = 0u;
	}

	public override string ToString()
	{
		return "Porcentagem: " + Convert.ToString(porcentagem) + Environment.NewLine + "Option: " + Convert.ToString((ushort)option) + Environment.NewLine + "Ball Velocity (Initial): " + ball_velocity.ToString() + Environment.NewLine + "Location (Begin Shot): " + location.ToString() + Environment.NewLine + "Wind Influence: " + wind_influence.ToString() + Environment.NewLine + "Ball Point: " + ball_point.ToString() + Environment.NewLine + "Special Shot(Tipo da tacada): " + special_shot.ToString() + Environment.NewLine + "Ball Rotation (Spin): " + Convert.ToString(ball_rotation_spin) + Environment.NewLine + "Ball Rotation (Curva): " + Convert.ToString(ball_rotation_curve) + Environment.NewLine + "ucUnknown: " + Convert.ToString(stUnknown) + Environment.NewLine + "Taco: " + Convert.ToString((ushort)taco) + Environment.NewLine + "Power Factor (Full): " + Convert.ToString(power_factor) + Environment.NewLine + "Power Club(Range): " + Convert.ToString(power_club) + Environment.NewLine + "Rotation Spin Factor: " + Convert.ToString(rotation_spin_factor) + Environment.NewLine + "Rotation Curve Factor: " + Convert.ToString(rotation_curve_factor) + Environment.NewLine + "Power Factor (Shot): " + Convert.ToString(power_factor_shot) + Environment.NewLine + "Time Hole SYNC: " + Convert.ToString(time_hole_sync) + Environment.NewLine;
	}

	internal byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(porcentagem);
		p.WriteBytes(ball_velocity.ToArray());
		p.Write(option);
		p.WriteBytes(location.ToArray());
		p.WriteBytes(wind_influence.ToArray());
		p.WriteBytes(ball_point.ToArray());
		p.WriteUInt32(special_shot.ulSpecialShot);
		p.Write(ball_rotation_spin);
		p.Write(ball_rotation_curve);
		p.Write(stUnknown);
		p.Write(taco);
		p.Write(power_factor);
		p.Write(power_club);
		p.Write(rotation_spin_factor);
		p.Write(rotation_curve_factor);
		p.Write(power_factor_shot);
		p.Write(time_hole_sync);
		return p.GetBytes;
	}
}
