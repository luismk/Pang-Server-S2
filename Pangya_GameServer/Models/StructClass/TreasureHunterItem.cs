using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TreasureHunterItem
{
	public uint _typeid;

	public uint qntd;

	public uint probabilidade;

	public byte flag;

	public byte active = 0;

	public TreasureHunterItem()
	{
	}

	public TreasureHunterItem(TreasureHunterItem item)
	{
		_typeid = item._typeid;
		qntd = item.qntd;
		probabilidade = item.probabilidade;
		flag = item.flag;
		active = item.active;
	}
}
