namespace Pangya_GameServer.Models;

public class mission_approach_dados
{
	public class uMissionFlag
	{
		public uint flag;

		private const int PLAYERS_SHIFT = 0;

		private const uint PLAYERS_MASK = 31u;

		private const int COND1_SHIFT = 5;

		private const uint COND1_MASK = 8191u;

		private const int COND2_SHIFT = 18;

		private const uint COND2_MASK = 8191u;

		public uint players
		{
			get
			{
				return flag & 0x1F;
			}
			set
			{
				flag &= 4294967264u;
				flag |= value & 0x1F;
			}
		}

		public uint condition1
		{
			get
			{
				return (flag >> 5) & 0x1FFF;
			}
			set
			{
				flag &= 4294705183u;
				flag |= (value & 0x1FFF) << 5;
			}
		}

		public uint condition2
		{
			get
			{
				return (flag >> 18) & 0x1FFF;
			}
			set
			{
				flag &= 2147745791u;
				flag |= (value & 0x1FFF) << 18;
			}
		}
	}

	public uint numero = 0u;

	public uint box = 0u;

	public eMISSION_TYPE tipo = eMISSION_TYPE.MT_NO_TYPE;

	public uint reward_tipo = 0u;

	public uMissionFlag flag = new uMissionFlag();
}
