using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

public class approach_dados_ex : approach_dados
{
	public class uState
	{
		public sbyte ucState;

		public byte chip_in
		{
			get
			{
				return (byte)(((ucState & 1) != 0) ? 1u : 0u);
			}
			set
			{
				if (value != 0)
				{
					ucState |= 1;
				}
				else
				{
					ucState &= -2;
				}
			}
		}

		public byte giveup
		{
			get
			{
				return (byte)(((ucState & 2) != 0) ? 1u : 0u);
			}
			set
			{
				if (value != 0)
				{
					ucState |= 2;
				}
				else
				{
					ucState &= -3;
				}
			}
		}

		public byte ob_or_water_hazard
		{
			get
			{
				return (byte)(((ucState & 4) != 0) ? 1u : 0u);
			}
			set
			{
				if (value != 0)
				{
					ucState |= 4;
				}
				else
				{
					ucState &= -5;
				}
			}
		}

		public byte timeout
		{
			get
			{
				return (byte)(((ucState & 8) != 0) ? 1u : 0u);
			}
			set
			{
				if (value != 0)
				{
					ucState |= 8;
				}
				else
				{
					ucState &= -9;
				}
			}
		}

		public uState(sbyte _uc = 0)
		{
			ucState = _uc;
		}
	}

	public enum eSTATE_QUIT : byte
	{
		SQ_IN_GAME,
		SQ_QUIT_START,
		SQ_QUIT_ENDED
	}

	public uint total_distance = 0u;

	public uint total_time = 0u;

	public uint total_box = 0u;

	public uState state = new uState(0);

	public eSTATE_QUIT state_quit = eSTATE_QUIT.SQ_IN_GAME;

	public approach_dados_ex(uint _ul = 0u)
		: base(_ul)
	{
		total_distance = 0u;
		total_time = 0u;
		total_box = 0u;
		state = new uState(0);
		state_quit = eSTATE_QUIT.SQ_IN_GAME;
	}

	public override void clear()
	{
		base.clear();
		total_distance = 0u;
		total_time = 0u;
		total_box = 0u;
		state.ucState = 0;
		state_quit = eSTATE_QUIT.SQ_IN_GAME;
	}

	public void toPacket(PangyaBinaryWriter _packet)
	{
		_packet.WriteByte(status);
		_packet.WriteInt32(oid);
		_packet.WriteUInt32(uid);
		_packet.WriteSByte(position);
		_packet.WriteUInt32(box);
		if (state.ucState != 0)
		{
			_packet.WriteUInt32(uint.MaxValue);
			_packet.WriteUInt32(0);
		}
		else
		{
			_packet.WriteUInt32(distance);
			_packet.WriteUInt32(time);
		}
		_packet.WriteUInt16(rank_box);
	}

	public override void setLeftGame()
	{
		base.setLeftGame();
		state_quit = eSTATE_QUIT.SQ_QUIT_START;
	}
}
