using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class DropItemRet
{
	public List<DropItem> v_drop = new List<DropItem>();

	public DropItemRet()
	{
		clear();
	}

	public void clear()
	{
		if (v_drop.Any())
		{
			v_drop.Clear();
		}
	}
}
