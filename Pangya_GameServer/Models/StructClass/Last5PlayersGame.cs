using System.Collections.Generic;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

public class Last5PlayersGame
{
	public class LastPlayerGame
	{
		public uint sex;

		public string nick;

		public string id;

		public uint uid;

		public LastPlayerGame()
		{
			nick = "";
			id = "";
		}

		public bool Equals(LastPlayerGame obj)
		{
			return uid == obj.uid && id == obj.id;
		}

		public byte[] ToArray()
		{
			using PangyaBinaryWriter p = new PangyaBinaryWriter();
			p.WriteUInt32(sex);
			p.WriteStr(nick, 22);
			p.WriteStr(id, 22);
			p.WriteUInt32(uid);
			return p.GetBytes;
		}
	}

	public List<LastPlayerGame> players;

	public Last5PlayersGame()
	{
		players = new List<LastPlayerGame>();
		for (int i = 0; i < 5; i++)
		{
			players.Add(new LastPlayerGame());
		}
	}

	public void add(player_info _pi, uint _sex)
	{
		if (players.Count > 0 && players[players.Count - 1].uid == _pi.uid)
		{
			players[players.Count - 1].sex = _sex;
			players[players.Count - 1].nick = _pi.nickname;
			return;
		}
		players.Add(new LastPlayerGame
		{
			id = _pi.id,
			uid = _pi.uid,
			sex = _sex,
			nick = _pi.nickname
		});
		for (int i = players.Count - 2; i >= 0; i--)
		{
			if (players[i].uid == _pi.uid)
			{
				players.RemoveAt(i);
				break;
			}
		}
		while (players.Count > 5)
		{
			players.RemoveAt(0);
		}
	}
}
