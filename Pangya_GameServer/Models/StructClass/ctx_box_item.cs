namespace Pangya_GameServer.Models;

public class ctx_box_item
{
	public uint _typeid = 0u;

	public int numero = 0;

	public int qntd = 0;

	public uint probabilidade = 0u;

	public BOX_TYPE_RARETY raridade;

	public byte duplicar = 1;

	public byte active = 1;

	public void clear()
	{
	}
}
