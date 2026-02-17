using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class UserInfo
{
	public int tacada { get; set; }

	public int putt { get; set; }

	public int tempo { get; set; }

	public int tempo_tacada { get; set; }

	public float best_drive { get; set; }

	public int acerto_pangya { get; set; }

	public int timeout { get; set; }

	public int ob { get; set; }

	public int total_distancia { get; set; }

	public int hole { get; set; }

	public int hole_in { get; set; }

	public int hio { get; set; }

	public short bunker { get; set; }

	public int fairway { get; set; }

	public int albatross { get; set; }

	public int mad_conduta { get; set; }

	public int putt_in { get; set; }

	public float best_long_putt { get; set; }

	public float best_chip_in { get; set; }

	public uint exp { get; set; }

	public byte level { get; set; }

	public ulong pang { get; set; }

	public int media_score { get; set; }

	[field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public sbyte[] best_score { get; set; }

	public byte event_flag { get; set; }

	[field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
	public long[] best_pang { get; set; }

	public long sum_pang { get; set; }

	public int jogado { get; set; }

	public int team_hole { get; set; }

	public int team_win { get; set; }

	public int team_game { get; set; }

	public int ladder_point { get; set; }

	public int ladder_hole { get; set; }

	public int ladder_win { get; set; }

	public int ladder_lose { get; set; }

	public int ladder_draw { get; set; }

	public int combo { get; set; }

	public int all_combo { get; set; }

	public int quitado { get; set; }

	public long skin_pang { get; set; }

	public int skin_win { get; set; }

	public int skin_lose { get; set; }

	public int skin_all_in_count { get; set; }

	public int skin_run_hole { get; set; }

	public int skin_strike_point { get; set; }

	public int jogados_disconnect { get; set; }

	public short event_value { get; set; }

	public int disconnect { get; set; }

	[field: MarshalAs(UnmanagedType.Struct)]
	public stMedal medal { get; set; }

	public int sys_school_serie { get; set; }

	public int game_count_season { get; set; }

	public short _16bit_nao_sei { get; set; }

	public int Cookies { get; internal set; }

	public UserInfo()
	{
		clear();
	}

	public float getMediaScore()
	{
		if (hole - hole_in == 0)
		{
			return 0f;
		}
		return 18f / (float)(hole - hole_in) * (float)media_score + 72f;
	}

	public float getPangyaShotRate()
	{
		if (tacada == 0)
		{
			return 0f;
		}
		return (float)acerto_pangya / (float)tacada * 100f;
	}

	public float getFairwayRate()
	{
		if (hole - hole_in == 0)
		{
			return 0f;
		}
		return (float)fairway / (float)(hole - hole_in) * 100f;
	}

	public float getPuttRate()
	{
		if (putt == 0)
		{
			return 0f;
		}
		return (float)putt_in / (float)putt * 100f;
	}

	public float getOBRate()
	{
		if (tacada + putt == 0)
		{
			return 0f;
		}
		return (float)ob / (float)(tacada + putt) * 100f;
	}

	public float getMatchWinRate()
	{
		if (team_game == 0)
		{
			return 0f;
		}
		return (float)team_win / (float)team_game * 100f;
	}

	public float getShotTimeRate()
	{
		if (tacada + putt == 0)
		{
			return 0f;
		}
		return (float)tempo_tacada / (float)(tacada + putt) * 100f;
	}

	public float getQuitRate()
	{
		if (jogado == 0)
		{
			return 0f;
		}
		return quitado * 100 / jogado;
	}

	public void clear()
	{
		best_pang = new long[5];
		best_score = new sbyte[5];
		medal = new stMedal();
	}

	public void add(UserInfo _ui)
	{
		if (_ui.best_drive > best_drive)
		{
			best_drive = _ui.best_drive;
		}
		if (_ui.best_long_putt > best_long_putt)
		{
			best_long_putt = _ui.best_long_putt;
		}
		if (_ui.best_chip_in > best_chip_in)
		{
			best_chip_in = _ui.best_chip_in;
		}
		if (_ui.combo < 0)
		{
			if ((long)combo <= 3L)
			{
				combo = 0;
			}
			else
			{
				combo += _ui.combo;
			}
		}
		else
		{
			combo += _ui.combo;
			if (combo > all_combo)
			{
				all_combo += _ui.combo;
			}
		}
		if (_ui.quitado < 0)
		{
			if (quitado + _ui.quitado <= 0)
			{
				quitado = 0;
			}
			else
			{
				quitado += _ui.quitado;
			}
		}
		else
		{
			quitado += _ui.quitado;
		}
		if (skin_all_in_count + _ui.skin_all_in_count >= 5)
		{
			skin_all_in_count = 0;
			skin_pang += 1000L;
		}
		else
		{
			skin_all_in_count += _ui.skin_all_in_count;
		}
		tacada += _ui.tacada;
		putt += _ui.putt;
		tempo += _ui.tempo;
		tempo_tacada += _ui.tempo_tacada;
		acerto_pangya += _ui.acerto_pangya;
		timeout += _ui.timeout;
		ob += _ui.ob;
		total_distancia += _ui.total_distancia;
		hole += _ui.hole;
		hole_in += _ui.hole - _ui.hole_in;
		hio += _ui.hio;
		bunker += _ui.bunker;
		fairway += _ui.fairway;
		albatross += _ui.albatross;
		putt_in += _ui.putt_in;
		media_score += _ui.media_score;
		best_score[0] += _ui.best_score[0];
		best_score[1] += _ui.best_score[1];
		best_score[2] += _ui.best_score[2];
		best_score[3] += _ui.best_score[3];
		best_score[4] += _ui.best_score[4];
		best_pang[0] += _ui.best_pang[0];
		best_pang[1] += _ui.best_pang[1];
		best_pang[2] += _ui.best_pang[2];
		best_pang[3] += _ui.best_pang[3];
		best_pang[4] += _ui.best_pang[4];
		sum_pang += _ui.sum_pang;
		event_flag += _ui.event_flag;
		jogado += _ui.jogado;
		team_game += _ui.team_game;
		team_win += _ui.team_win;
		team_hole += _ui.team_hole;
		ladder_point += _ui.ladder_point;
		ladder_hole += _ui.ladder_hole;
		ladder_win += _ui.ladder_win;
		ladder_lose += _ui.ladder_lose;
		ladder_draw += _ui.ladder_draw;
		skin_pang += _ui.skin_pang;
		skin_win += _ui.skin_win;
		skin_lose += _ui.skin_lose;
		skin_run_hole += _ui.skin_run_hole;
		skin_strike_point += _ui.skin_strike_point;
		disconnect += _ui.disconnect;
		jogados_disconnect += _ui.jogados_disconnect;
		event_value += _ui.event_value;
		sys_school_serie += _ui.sys_school_serie;
		game_count_season += _ui.game_count_season;
		medal.add(_ui.medal);
	}

	public byte[] ToArray()
	{
		using PangyaBinaryWriter p = new PangyaBinaryWriter();
		p.WriteInt32(100);
		p.WriteInt32(50);
		p.WriteInt32(3600);
		p.WriteInt32(120);
		p.WriteInt32(250);
		p.WriteInt32(850);
		p.WriteInt32(5);
		p.WriteInt32(2);
		p.WriteInt32(150000);
		p.WriteInt32(180);
		p.WriteInt32(90);
		p.WriteUInt16(11);
		p.WriteUInt16(22);
		p.WriteInt32(75);
		p.WriteUInt16(77);
		p.WriteInt32(33);
		p.WriteInt32(44);
		p.WriteSingle(280.5f);
		p.WriteSingle(15.2f);
		p.WriteUInt32(50000);
		p.WriteByte(20);
		p.WriteInt32(777777);
		p.WriteInt32(888888);
		for (int i = 0; i < 4; i++)
		{
			p.WriteSByte((sbyte)(-10 - i));
		}
		for (int j = 0; j < 4; j++)
		{
			p.WriteInt32(10000 + j);
		}
		p.WriteInt32(100);
		p.WriteInt32(5);
		p.WriteInt32(40);
		p.WriteInt32(60);
		p.WriteInt32(1500);
		p.WriteInt32(10);
		p.WriteInt32(50);
		p.WriteInt32(5);
		p.WriteUInt16(50);
		p.WriteUInt16(200);
		p.WriteInt32(1);
		p.WriteInt32(2);
		p.WriteInt32(12347);
		p.WriteInt32(123456);
		p.WriteInt32(30);
		p.WriteInt32(10);
		p.WriteUInt16(5);
		p.WriteInt16(99);
		p.WriteInt16(1);
		p.WriteInt32(100);
		return p.GetBytes;
	}

	public override string ToString()
	{
		return "Tacada: " + tacada + "  Putt: " + putt + "  Tempo: " + tempo + "  Tempo Tacada: " + tempo_tacada + "  Best drive: " + best_drive + "  Acerto pangya: " + acerto_pangya + "  timeout: " + timeout + "  OB: " + ob + "  Total distancia: " + total_distancia + "  hole: " + hole + "  Hole in: " + hole_in + "  HIO: " + hio + "  Bunker: " + bunker + "  Fairway: " + fairway + "  Albratross: " + albatross + "  Mad conduta: " + mad_conduta + "  Putt in: " + putt_in + "  Best long puttin: " + best_long_putt + "  Best chipin: " + best_chip_in + "  Exp: " + exp + "  Level: " + level + "  Pang: " + pang + "  Media score: " + media_score + "  Best score[" + best_score[0] + ", " + best_score[1] + ", " + best_score[2] + ", " + best_score[3] + ", " + best_score[4] + "]  Event type: " + event_flag + "  Best pang[" + best_pang[0] + ", " + best_pang[1] + ", " + best_pang[2] + ", " + best_pang[3] + ", " + best_pang[4] + "]  Soma pang: " + sum_pang + "  Jogado: " + jogado + "  Team Hole: " + team_hole + "  Team win: " + team_win + "  Team game: " + team_game + "  Ladder point: " + ladder_point + "  Ladder hole: " + ladder_hole + "  Ladder win: " + ladder_win + "  Ladder lose: " + ladder_lose + "  Ladder draw: " + ladder_draw + "  Combo: " + combo + "  All combo: " + all_combo + "  Quitado: " + quitado + "  Skin pang: " + skin_pang + "  Skin win: " + skin_win + "  Skin lose: " + skin_lose + "  Skin all in count: " + skin_all_in_count + "  Skin run hole: " + skin_run_hole + "  Disconnect(MY): " + disconnect + "  Jogados Disconnect(MY): " + jogados_disconnect + "  Event value: " + event_value + "  Skin Strike Point: " + skin_strike_point + "  Sistema School Serie: " + sys_school_serie + "  Game count season: " + game_count_season + "  _16bit nao sei: " + _16bit_nao_sei;
	}
}
