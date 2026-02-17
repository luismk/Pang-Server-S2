using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class UserInfoEx : UserInfo
{
	public ulong total_pang_win_game { get; set; }

	public void add(UserInfoEx _ui, ulong _total_pang_win_game)
	{
		add(_ui);
		if (_total_pang_win_game != 0)
		{
			total_pang_win_game += _total_pang_win_game;
		}
	}

	public string[] ToStringDB()
	{
		return new string[63]
		{
			base.best_drive.ToString(),
			base.best_chip_in.ToString(),
			base.best_long_putt.ToString(),
			base.combo.ToString(),
			base.all_combo.ToString(),
			base.tacada.ToString(),
			base.putt.ToString(),
			base.tempo.ToString(),
			base.tempo_tacada.ToString(),
			base.acerto_pangya.ToString(),
			base.timeout.ToString(),
			base.ob.ToString(),
			base.total_distancia.ToString(),
			base.hole.ToString(),
			base.hole_in.ToString(),
			base.hio.ToString(),
			base.bunker.ToString(),
			base.fairway.ToString(),
			base.albatross.ToString(),
			base.mad_conduta.ToString(),
			base.putt_in.ToString(),
			base.media_score.ToString(),
			base.best_score[0].ToString(),
			base.best_score[1].ToString(),
			base.best_score[2].ToString(),
			base.best_score[3].ToString(),
			base.best_score[4].ToString(),
			base.best_pang[0].ToString(),
			base.best_pang[1].ToString(),
			base.best_pang[2].ToString(),
			base.best_pang[3].ToString(),
			base.best_pang[4].ToString(),
			base.sum_pang.ToString(),
			base.event_flag.ToString(),
			base.jogado.ToString(),
			base.team_game.ToString(),
			base.team_win.ToString(),
			base.team_hole.ToString(),
			base.ladder_point.ToString(),
			base.ladder_hole.ToString(),
			base.ladder_win.ToString(),
			base.ladder_lose.ToString(),
			base.ladder_draw.ToString(),
			base.quitado.ToString(),
			base.skin_pang.ToString(),
			base.skin_win.ToString(),
			base.skin_lose.ToString(),
			base.skin_run_hole.ToString(),
			base.skin_all_in_count.ToString(),
			base.disconnect.ToString(),
			base.jogados_disconnect.ToString(),
			base.event_value.ToString(),
			base.skin_strike_point.ToString(),
			base.sys_school_serie.ToString(),
			base.game_count_season.ToString(),
			total_pang_win_game.ToString(),
			base.medal.lucky.ToString(),
			base.medal.fast.ToString(),
			base.medal.best_drive.ToString(),
			base.medal.best_chipin.ToString(),
			base.medal.best_puttin.ToString(),
			base.medal.best_recovery.ToString(),
			base._16bit_nao_sei.ToString()
		};
	}

	public UserInfoEx ToRead(packet p)
	{
		base.tacada = p.ReadInt32();
		base.putt = p.ReadInt32();
		base.tempo = p.ReadInt32();
		base.tempo_tacada = p.ReadInt32();
		base.best_drive = p.ReadSingle();
		base.acerto_pangya = p.ReadInt32();
		base.timeout = p.ReadInt32();
		base.ob = p.ReadInt32();
		base.total_distancia = p.ReadInt32();
		base.hole = p.ReadInt32();
		base.hole_in = p.ReadInt32();
		base.hio = p.ReadInt32();
		base.bunker = p.ReadInt16();
		base.fairway = p.ReadInt32();
		base.albatross = p.ReadInt32();
		base.mad_conduta = p.ReadInt32();
		base.putt_in = p.ReadInt32();
		base.best_long_putt = p.ReadSingle();
		base.best_chip_in = p.ReadSingle();
		base.exp = p.ReadUInt32();
		base.level = p.ReadByte();
		base.pang = p.ReadUInt64();
		base.media_score = p.ReadInt32();
		base.best_score = p.ReadSBytes(5);
		base.event_flag = p.ReadByte();
		base.best_pang = new long[5];
		for (int i = 0; i < 5; i++)
		{
			base.best_pang[i] = p.ReadInt64();
		}
		base.sum_pang = p.ReadInt64();
		base.jogado = p.ReadInt32();
		base.team_hole = p.ReadInt32();
		base.team_win = p.ReadInt32();
		base.team_game = p.ReadInt32();
		base.ladder_point = p.ReadInt32();
		base.ladder_hole = p.ReadInt32();
		base.ladder_win = p.ReadInt32();
		base.ladder_lose = p.ReadInt32();
		base.ladder_draw = p.ReadInt32();
		base.combo = p.ReadInt32();
		base.all_combo = p.ReadInt32();
		base.quitado = p.ReadInt32();
		base.skin_pang = p.ReadInt64();
		base.skin_win = p.ReadInt32();
		base.skin_lose = p.ReadInt32();
		base.skin_all_in_count = p.ReadInt32();
		base.skin_run_hole = p.ReadInt32();
		base.skin_strike_point = p.ReadInt32();
		base.jogados_disconnect = p.ReadInt32();
		base.event_value = p.ReadInt16();
		base.disconnect = p.ReadInt32();
		base.medal = new stMedal
		{
			lucky = p.ReadInt32(),
			fast = p.ReadInt32(),
			best_drive = p.ReadInt32(),
			best_chipin = p.ReadInt32(),
			best_puttin = p.ReadInt32(),
			best_recovery = p.ReadInt32()
		};
		base.sys_school_serie = p.ReadInt32();
		base.game_count_season = p.ReadInt32();
		base._16bit_nao_sei = p.ReadInt16();
		return this;
	}
}
