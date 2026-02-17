using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class UsedItem
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class Passive
	{
		public uint _typeid = 0u;

		public uint count = 0u;

		public void clear()
		{
		}

		public Passive()
		{
		}

		public Passive(uint typeid, uint _count)
		{
			_typeid = typeid;
			count = _count;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class Active
	{
		public uint _typeid = 0u;

		public uint count = 0u;

		public List<byte> v_slot = new List<byte>();

		public void Dispose()
		{
		}

		public void clear()
		{
			_typeid = 0u;
			count = 0u;
			if (v_slot.Any())
			{
				v_slot.Clear();
			}
		}

		public Active()
		{
		}

		public Active(uint typeid, uint _count, List<byte> _slot)
		{
			_typeid = typeid;
			count = _count;
			v_slot = _slot;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class Rate
	{
		public uint pang = 0u;

		public uint exp = 0u;

		public uint club = 0u;

		public uint drop = 0u;

		public void clear()
		{
			pang = 100u;
			exp = 100u;
			club = 100u;
			drop = 100u;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class ClubMastery
	{
		public uint _typeid = 0u;

		public uint count = 0u;

		public float rate;

		public void clear()
		{
		}
	}

	public Dictionary<uint, Passive> v_passive = new Dictionary<uint, Passive>();

	public Dictionary<uint, Active> v_active = new Dictionary<uint, Active>();

	public Rate rate = new Rate();

	public ClubMastery club = new ClubMastery();

	public void Dispose()
	{
	}

	public void clear()
	{
		if (v_passive.Any())
		{
			v_passive.Clear();
		}
		if (v_active.Any())
		{
			v_active.Clear();
		}
	}
}
