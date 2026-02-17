namespace Pangya_GameServer.Models;

public class ctx_papel_shop_item
{
	public uint _typeid = 0u;

	public uint probabilidade = 0u;

	public int numero = -1;

	public PAPEL_SHOP_TYPE tipo;

	public byte active = 1;

	public void clear()
	{
	}
}
