using System;
using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ShotSyncData
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class Location
	{
		public float x;

		public float y;

		public float z;

		public void clear()
		{
		}

		public override string ToString()
		{
			return "X: " + Convert.ToString(x) + " Y: " + Convert.ToString(y) + " Z: " + Convert.ToString(z);
		}
	}

	public enum SHOT_STATE : byte
	{
		PLAYABLE_AREA = 2,
		OUT_OF_BOUNDS,
		INTO_HOLE,
		UNPLAYABLE_AREA
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class stStateShot
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public class uDisplayState
		{
			public uint ulState;

			public bool over_drive
			{
				get
				{
					return (ulState & 1) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 1u;
					}
					else
					{
						ulState &= 4294967294u;
					}
				}
			}

			public bool _bit2_unknown
			{
				get
				{
					return (ulState & 2) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 2u;
					}
					else
					{
						ulState &= 4294967293u;
					}
				}
			}

			public bool super_pangya
			{
				get
				{
					return (ulState & 4) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 4u;
					}
					else
					{
						ulState &= 4294967291u;
					}
				}
			}

			public bool special_shot
			{
				get
				{
					return (ulState & 8) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 8u;
					}
					else
					{
						ulState &= 4294967287u;
					}
				}
			}

			public bool beam_impact
			{
				get
				{
					return (ulState & 0x10) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 16u;
					}
					else
					{
						ulState &= 4294967279u;
					}
				}
			}

			public bool chip_in_17_a_199
			{
				get
				{
					return (ulState & 0x20) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 32u;
					}
					else
					{
						ulState &= 4294967263u;
					}
				}
			}

			public bool chip_in_200_plus
			{
				get
				{
					return (ulState & 0x40) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 64u;
					}
					else
					{
						ulState &= 4294967231u;
					}
				}
			}

			public bool long_putt
			{
				get
				{
					return (ulState & 0x80) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 128u;
					}
					else
					{
						ulState &= 4294967167u;
					}
				}
			}

			public bool acerto_hole
			{
				get
				{
					return (ulState & 0x100) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 256u;
					}
					else
					{
						ulState &= 4294967039u;
					}
				}
			}

			public bool approach_shot
			{
				get
				{
					return (ulState & 0x200) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 512u;
					}
					else
					{
						ulState &= 4294966783u;
					}
				}
			}

			public bool chip_in_with_special_shot
			{
				get
				{
					return (ulState & 0x400) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 1024u;
					}
					else
					{
						ulState &= 4294966271u;
					}
				}
			}

			public bool _bit12_unknown
			{
				get
				{
					return (ulState & 0x800) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 2048u;
					}
					else
					{
						ulState &= 4294965247u;
					}
				}
			}

			public bool happy_bonus
			{
				get
				{
					return (ulState & 0x1000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 4096u;
					}
					else
					{
						ulState &= 4294963199u;
					}
				}
			}

			public bool clear_bonus
			{
				get
				{
					return (ulState & 0x2000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 8192u;
					}
					else
					{
						ulState &= 4294959103u;
					}
				}
			}

			public bool aztec_bonus
			{
				get
				{
					return (ulState & 0x4000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 16384u;
					}
					else
					{
						ulState &= 4294950911u;
					}
				}
			}

			public bool recovery_bonus
			{
				get
				{
					return (ulState & 0x8000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 32768u;
					}
					else
					{
						ulState &= 4294934527u;
					}
				}
			}

			public bool chip_in_without_special_shot
			{
				get
				{
					return (ulState & 0x10000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 65536u;
					}
					else
					{
						ulState &= 4294901759u;
					}
				}
			}

			public bool bound_bonus
			{
				get
				{
					return (ulState & 0x20000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 131072u;
					}
					else
					{
						ulState &= 4294836223u;
					}
				}
			}

			public bool _bit19_unknown
			{
				get
				{
					return (ulState & 0x40000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 262144u;
					}
					else
					{
						ulState &= 4294705151u;
					}
				}
			}

			public bool _bit20_unknown
			{
				get
				{
					return (ulState & 0x80000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 524288u;
					}
					else
					{
						ulState &= 4294443007u;
					}
				}
			}

			public bool mascot_bonus_with_pangya
			{
				get
				{
					return (ulState & 0x100000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 1048576u;
					}
					else
					{
						ulState &= 4293918719u;
					}
				}
			}

			public bool mascot_bonus_without_pangya
			{
				get
				{
					return (ulState & 0x200000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 2097152u;
					}
					else
					{
						ulState &= 4292870143u;
					}
				}
			}

			public bool special_bonus_with_pangya
			{
				get
				{
					return (ulState & 0x400000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 4194304u;
					}
					else
					{
						ulState &= 4290772991u;
					}
				}
			}

			public bool special_bonus_without_pangya
			{
				get
				{
					return (ulState & 0x800000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 8388608u;
					}
					else
					{
						ulState &= 4286578687u;
					}
				}
			}

			public bool _bit25_unknown
			{
				get
				{
					return (ulState & 0x1000000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 16777216u;
					}
					else
					{
						ulState &= 4278190079u;
					}
				}
			}

			public bool _bit26_unknown
			{
				get
				{
					return (ulState & 0x2000000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 33554432u;
					}
					else
					{
						ulState &= 4261412863u;
					}
				}
			}

			public bool devil_bonus
			{
				get
				{
					return (ulState & 0x4000000) != 0;
				}
				set
				{
					if (value)
					{
						ulState |= 67108864u;
					}
					else
					{
						ulState &= 4227858431u;
					}
				}
			}

			public byte _bit28_a_32_unknown
			{
				get
				{
					return (byte)((ulState >> 27) & 0x1F);
				}
				set
				{
					ulState = (ulState & 0x7FFFFFF) | (uint)((value & 0x1F) << 27);
				}
			}

			public void clear()
			{
				ulState = 0u;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public class uShotState
		{
			public uint ulState;

			public uint _bit1_unknown
			{
				get
				{
					return ((ulState & 1) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 1u;
					}
					else
					{
						ulState &= 4294967294u;
					}
				}
			}

			public uint tomahawk
			{
				get
				{
					return ((ulState & 2) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 2u;
					}
					else
					{
						ulState &= 4294967293u;
					}
				}
			}

			public uint spike
			{
				get
				{
					return ((ulState & 4) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 4u;
					}
					else
					{
						ulState &= 4294967291u;
					}
				}
			}

			public uint cobra
			{
				get
				{
					return ((ulState & 8) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 8u;
					}
					else
					{
						ulState &= 4294967287u;
					}
				}
			}

			public uint spin_front
			{
				get
				{
					return ((ulState & 0x10) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 16u;
					}
					else
					{
						ulState &= 4294967279u;
					}
				}
			}

			public uint spin_back
			{
				get
				{
					return ((ulState & 0x20) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 32u;
					}
					else
					{
						ulState &= 4294967263u;
					}
				}
			}

			public uint curve_left
			{
				get
				{
					return ((ulState & 0x40) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 64u;
					}
					else
					{
						ulState &= 4294967231u;
					}
				}
			}

			public uint curve_right
			{
				get
				{
					return ((ulState & 0x80) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 128u;
					}
					else
					{
						ulState &= 4294967167u;
					}
				}
			}

			public uint _bit9_unknown
			{
				get
				{
					return ((ulState & 0x100) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 256u;
					}
					else
					{
						ulState &= 4294967039u;
					}
				}
			}

			public uint _bit10_unknown
			{
				get
				{
					return ((ulState & 0x200) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 512u;
					}
					else
					{
						ulState &= 4294966783u;
					}
				}
			}

			public uint _bit11_unknown
			{
				get
				{
					return ((ulState & 0x400) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 1024u;
					}
					else
					{
						ulState &= 4294966271u;
					}
				}
			}

			public uint sem_setas
			{
				get
				{
					return ((ulState & 0x800) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 2048u;
					}
					else
					{
						ulState &= 4294965247u;
					}
				}
			}

			public uint power_shot
			{
				get
				{
					return ((ulState & 0x1000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 4096u;
					}
					else
					{
						ulState &= 4294963199u;
					}
				}
			}

			public uint double_power_shot
			{
				get
				{
					return ((ulState & 0x2000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 8192u;
					}
					else
					{
						ulState &= 4294959103u;
					}
				}
			}

			public uint _bit15_unknown
			{
				get
				{
					return ((ulState & 0x4000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 16384u;
					}
					else
					{
						ulState &= 4294950911u;
					}
				}
			}

			public uint _bit16_unknown
			{
				get
				{
					return ((ulState & 0x8000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 32768u;
					}
					else
					{
						ulState &= 4294934527u;
					}
				}
			}

			public uint _bit17_unknown
			{
				get
				{
					return ((ulState & 0x10000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 65536u;
					}
					else
					{
						ulState &= 4294901759u;
					}
				}
			}

			public uint _bit18_unknown
			{
				get
				{
					return ((ulState & 0x20000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 131072u;
					}
					else
					{
						ulState &= 4294836223u;
					}
				}
			}

			public uint _bit19_unknown
			{
				get
				{
					return ((ulState & 0x40000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 262144u;
					}
					else
					{
						ulState &= 4294705151u;
					}
				}
			}

			public uint _bit20_unknown
			{
				get
				{
					return ((ulState & 0x80000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 524288u;
					}
					else
					{
						ulState &= 4294443007u;
					}
				}
			}

			public uint club_wood
			{
				get
				{
					return ((ulState & 0x100000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 1048576u;
					}
					else
					{
						ulState &= 4293918719u;
					}
				}
			}

			public uint club_iron
			{
				get
				{
					return ((ulState & 0x200000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 2097152u;
					}
					else
					{
						ulState &= 4292870143u;
					}
				}
			}

			public uint club_pw_sw
			{
				get
				{
					return ((ulState & 0x400000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 4194304u;
					}
					else
					{
						ulState &= 4290772991u;
					}
				}
			}

			public uint club_putt
			{
				get
				{
					return ((ulState & 0x800000) != 0) ? 1u : 0u;
				}
				set
				{
					if (value != 0)
					{
						ulState |= 8388608u;
					}
					else
					{
						ulState &= 4286578687u;
					}
				}
			}

			public uint _bit25_a_32_unknown
			{
				get
				{
					return (ulState & 0xFF000000u) >> 24;
				}
				set
				{
					ulState = (ulState & 0xFFFFFF) | ((value & 0xFF) << 24);
				}
			}

			public void Clear()
			{
				ulState = 0u;
			}
		}

		[MarshalAs(UnmanagedType.Struct)]
		public uDisplayState display = new uDisplayState();

		[MarshalAs(UnmanagedType.Struct)]
		public uShotState shot = new uShotState();

		public stStateShot()
		{
			clear();
		}

		public void clear()
		{
			shot = new uShotState();
			display = new uDisplayState();
		}

		public override string ToString()
		{
			string s = "Display State.\n\r";
			s = s + "OverDrive: " + Convert.ToString(display.over_drive) + " SuperPangya: " + Convert.ToString(display.super_pangya);
			s = s + " SpecialShot: " + Convert.ToString(display.special_shot) + " BeamImpact: " + Convert.ToString(display.beam_impact);
			s = s + " ChipIn17a199: " + Convert.ToString(display.chip_in_17_a_199) + " ChipIn200+: " + Convert.ToString(display.chip_in_200_plus);
			s = s + " LongPutt: " + Convert.ToString(display.long_putt) + " AcertoHole: " + Convert.ToString(display.acerto_hole);
			s = s + " ApproachShot: " + Convert.ToString(display.approach_shot) + " ChipInWithSpecialShot(BS,FS): " + Convert.ToString(display.chip_in_with_special_shot);
			s = s + " HappyBonus: " + Convert.ToString(display.happy_bonus) + " ClearBonus: " + Convert.ToString(display.clear_bonus) + " AztecBonus: " + Convert.ToString(display.aztec_bonus);
			s = s + " RecoveryBonus: " + Convert.ToString(display.recovery_bonus) + " ChipInWithoutSpecialShot: " + Convert.ToString(display.chip_in_without_special_shot);
			s = s + " BoundBonus: " + Convert.ToString(display.bound_bonus);
			s = s + " MascotBonusWithPangya: " + Convert.ToString(display.mascot_bonus_with_pangya) + " MascotBonusWithoutPangya: " + Convert.ToString(display.mascot_bonus_without_pangya);
			s = s + " SpecialBonusWithPangya: " + Convert.ToString(display.special_bonus_with_pangya);
			s = s + " SpecialBonusWithouPangya: " + Convert.ToString(display.special_bonus_without_pangya);
			s = s + " DevilBonus: " + Convert.ToString(display.devil_bonus) + Environment.NewLine;
			s += "Shot State.\n\r";
			s = s + "Tomahawk: " + Convert.ToString(shot.tomahawk) + " Spike: " + Convert.ToString(shot.spike);
			s = s + " Cobra: " + Convert.ToString(shot.cobra) + " SpinFront: " + Convert.ToString(shot.spin_front);
			s = s + " SpinBack: " + Convert.ToString(shot.spin_back) + " CurveLeft: " + Convert.ToString(shot.curve_left);
			s = s + " CurveRight: " + Convert.ToString(shot.curve_right) + " SemSetas: " + Convert.ToString(shot.sem_setas);
			s = s + " PowerShot: " + Convert.ToString(shot.power_shot) + " DoublePowerShot: " + Convert.ToString(shot.double_power_shot);
			s = s + " ClubWood: " + Convert.ToString(shot.club_wood) + " ClubIron: " + Convert.ToString(shot.club_iron);
			return s + " ClubPWeSW: " + Convert.ToString(shot.club_pw_sw) + " ClubPutt: " + Convert.ToString(shot.club_putt);
		}
	}

	public int oid = -1;

	[MarshalAs(UnmanagedType.Struct)]
	public Location location = new Location();

	public SHOT_STATE state;

	public byte bunker_flag;

	public byte ucUnknown;

	public uint pang;

	public uint bonus_pang;

	[MarshalAs(UnmanagedType.Struct)]
	public stStateShot state_shot = new stStateShot();

	public short tempo_shot;

	public byte grand_prix_penalidade;

	public void clear()
	{
	}

	public override string ToString()
	{
		return "OID: " + Convert.ToString(oid) + Environment.NewLine + "Location: " + location.ToString() + Environment.NewLine + "STATE: " + Convert.ToString((ushort)state) + Environment.NewLine + "Bunker Flag: " + Convert.ToString((ushort)bunker_flag) + Environment.NewLine + "ucUnknown: " + Convert.ToString((ushort)ucUnknown) + Environment.NewLine + "Pang: " + Convert.ToString(pang) + Environment.NewLine + "Pang Bonus: " + Convert.ToString(bonus_pang) + Environment.NewLine + "State Shot: " + state_shot.ToString() + Environment.NewLine + "Tempo Shot: " + Convert.ToString(tempo_shot) + Environment.NewLine + "Grand Prix Penalidade: " + Convert.ToString((ushort)grand_prix_penalidade) + Environment.NewLine;
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.Write(oid);
		p.Write(location.x);
		p.Write(location.y);
		p.Write(location.z);
		p.Write((byte)state);
		p.Write(bunker_flag);
		p.Write(ucUnknown);
		p.Write(pang);
		p.Write(bonus_pang);
		p.Write(state_shot.display.ulState);
		p.Write(state_shot.shot.ulState);
		p.Write(tempo_shot);
		p.Write(grand_prix_penalidade);
		return p.GetBytes;
	}

	public bool isMakeHole()
	{
		return state_shot.display.acerto_hole;
	}
}
