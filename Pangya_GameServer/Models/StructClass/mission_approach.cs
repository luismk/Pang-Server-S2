using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

public class mission_approach
{
	public byte numero;

	public byte box_qntd;

	public eMISSION_TYPE tipo = eMISSION_TYPE.MT_NO_TYPE;

	public int[] condition = new int[2];

	public string nick = "";

	public mission_approach(uint _ul = 0u)
	{
		nick = "";
		clear();
	}

	public mission_approach(byte _numero, byte _box_qntd, eMISSION_TYPE _tipo, int _condition1, int _condition2, string _nick)
	{
		numero = _numero;
		box_qntd = _box_qntd;
		tipo = _tipo;
		condition = new int[2] { _condition1, _condition2 };
		nick = _nick;
	}

	public virtual void clear()
	{
		numero = 0;
		box_qntd = 0;
		tipo = eMISSION_TYPE.MT_NO_TYPE;
		condition[0] = 0;
		condition[1] = 0;
		if (!nick.empty())
		{
			nick = "";
		}
	}

	public void toPacket(PangyaBinaryWriter _packet)
	{
		_packet.WriteByte(numero);
		_packet.WriteByte(box_qntd);
		_packet.WriteByte((int)tipo);
		_packet.WriteInt32(condition);
		_packet.WritePStr(nick);
	}
}
