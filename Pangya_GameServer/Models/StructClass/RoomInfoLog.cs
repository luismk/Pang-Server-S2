namespace Pangya_GameServer.Models;

public class RoomInfoLog : RoomInfoEx
{
	public uint Is_GM_Event;

	public uint Is_natural;

	public uint Is_short_game;

	public uint Is_GP;

	public byte Is_hole_repeat;

	public bool m_bot_tourney;

	public uint uid;

	public int character;

	public int club;

	public int mascot;

	public int caddie;

	public uint hole;

	public int score;

	public uint exp;

	public ulong pang;

	public ulong bonus_pang;

	public ulong tacada_num;

	public ulong total_tacada_num;

	public ulong specialshot;

	public bool premium;

	public ulong giveup;

	public uint timeout;

	public uint enter_after_started;

	public uint finish_game;

	public uint assist_flag;

	public uint Win_trofeu;

	public uint HitHio;

	public uint HitAlba;

	public uint HitEagle;

	public uint HitBirdie;

	public uint HitPar;

	public uint HitBogey;

	public uint Hit_x2_Bogey;

	public uint Hit_x3_Bogey;

	public RoomInfoLog()
	{
		clear();
	}

	public RoomInfoLog(RoomInfoEx _ul)
		: this()
	{
		SetInfo(_ul);
	}

	public void SetInfo(RoomInfoEx info)
	{
		if (info != null)
		{
			roomId = info.roomId;
			base.nome = info.nome;
			senha = info.senha;
			senha_flag = info.senha_flag;
			state = info.state;
			flag = info.flag;
			flag_gm = info.flag_gm;
			tipo_ex = info.tipo_ex;
			tipo = info.tipo;
			tipo_show = info.tipo_show;
			numero = info.numero;
			modo = info.modo;
			course = info.course;
			qntd_hole = info.qntd_hole;
			gallery_max_list = info.gallery_max_list;
			time_vs = info.time_vs;
			time_30s = info.time_30s;
			trofel = info.trofel;
			state_flag = info.state_flag;
			max_player = info.max_player;
			num_player = info.num_player;
			master = info.master;
			tipo_ex = info.tipo_ex;
			key = ((info.key != null) ? ((byte[])info.key.Clone()) : new byte[16]);
			guilds = new RoomGuildInfo
			{
				guild_1_uid = info.guilds.guild_1_uid,
				guild_2_uid = info.guilds.guild_2_uid,
				guild_1_mark = info.guilds.guild_1_mark,
				guild_2_mark = info.guilds.guild_2_mark,
				guild_1_index_mark = info.guilds.guild_1_index_mark,
				guild_2_index_mark = info.guilds.guild_2_index_mark,
				guild_1_nome = info.guilds.guild_1_nome,
				guild_2_nome = info.guilds.guild_2_nome
			};
			natural = new NaturalAndShortGame(info.natural.ulNaturalAndShortGame);
			grand_prix = new RoomGrandPrixInfo
			{
				dados_typeid = info.grand_prix.dados_typeid,
				rank_typeid = info.grand_prix.rank_typeid,
				tempo = info.grand_prix.tempo,
				active = info.grand_prix.active
			};
			rate_pang = info.rate_pang;
			rate_exp = info.rate_exp;
			artefato = info.artefato;
			Is_GP = info.grand_prix.active;
			Is_GM_Event = info.flag_gm;
			Is_natural = info.natural.natural;
			Is_short_game = info.natural.short_game;
			state_afk = info.state_afk;
			hole_repeat = info.hole_repeat;
			fixed_hole = info.fixed_hole;
			channel_rookie = info.channel_rookie;
			angel_event = info.angel_event;
			gallery_num = info.gallery_num;
			Is_hole_repeat = info.hole_repeat;
		}
	}

	public void UpdateInfo(uint _uid, int _character, int _club, int _mascot, int _caddie, uint _hole = 0u, int _score = 0, uint _exp = 0u, ulong _pang = 0uL, ulong _bonus_pang = 0uL, ulong _tacada_num = 0uL, ulong _total_tacada_num = 0uL, ulong _specialshot = 0uL, bool _premium = false, ulong _giveup = 0uL, uint _timeout = 0u, uint _enter_after_started = 0u, uint _finish_game = 0u, uint _assist_flag = 0u, uint _Win_trofeu = 0u, uint _hithio = 0u, uint _hitalba = 0u, uint _hiteagle = 0u, uint _hitbirdie = 0u, uint _hitpar = 0u, uint _hitbogey = 0u, uint _hit_2_bogey = 0u, uint _hit_3_bogey = 0u, RoomInfoEx _ul = null)
	{
		uid = _uid;
		character = _character;
		club = _club;
		mascot = _mascot;
		caddie = _caddie;
		hole = _hole;
		score = _score;
		exp = _exp;
		pang = _pang;
		bonus_pang = _bonus_pang;
		tacada_num = _tacada_num;
		total_tacada_num = _total_tacada_num;
		specialshot = _specialshot;
		premium = _premium;
		giveup = _giveup;
		timeout = _timeout;
		enter_after_started = _enter_after_started;
		finish_game = _finish_game;
		assist_flag = _assist_flag;
		Win_trofeu = _Win_trofeu;
		HitHio = _hithio;
		HitAlba = _hitalba;
		HitEagle = _hiteagle;
		HitBirdie = _hitbirdie;
		HitPar = _hitpar;
		HitBogey = _hitbogey;
		Hit_x2_Bogey = _hit_2_bogey;
		Hit_x3_Bogey = _hit_3_bogey;
		if (_ul != null)
		{
			SetInfo(_ul);
		}
	}

	public RoomInfoLog UpdateInfo(uint _uid, uint _character, uint _club, uint _mascot, uint _caddie, RoomInfoEx _ul, bool bot_tourney = false)
	{
		UpdateInfo(_uid, (int)_character, (int)_club, (int)_mascot, (int)_caddie, 0u, 0, 0u, 0uL, 0uL, 0uL, 0uL, 0uL, _premium: false, 0uL);
		m_bot_tourney = bot_tourney;
		if (_ul != null)
		{
			SetInfo(_ul);
		}
		return this;
	}

	public string ToString(bool isDb)
	{
		if (!isDb)
		{
			return $"[UID: {uid}, CharID: {character}, Room Type: {tipo},  Room TypeEx: {tipo_ex}, Game Mode: {modo}, Number Holes: {qntd_hole}, Map: {course}, Actual Hole: {hole}, Record: {score}, Exp: {exp}, Pangs: {pang}, P. Bonus: {bonus_pang}, Number Shot: {tacada_num}, Total Shot: {total_tacada_num}, Giveup: {giveup}, Timeout: {timeout}, EnterAfter: {enter_after_started}, FinishGame: {finish_game}, AssistFlag: {assist_flag}, RoomOwner: {master}, GameShort: {Is_short_game}, Natural: {Is_natural}]";
		}
		return $"{base.nome}, {num_player}, {max_player}, {tipo_ex}, {uid}, {roomId}, {character}, {caddie}, {mascot}, {club}, {tipo}, {modo}, {qntd_hole}, {course}, {hole}, {score}, {exp}, {pang}, {bonus_pang}, {tacada_num}, {total_tacada_num}, {giveup}, {timeout}, {enter_after_started}, {finish_game}, {assist_flag}, {Win_trofeu}, {master}, {Is_short_game}, {Is_natural}, {HitHio}, {HitAlba}, {HitEagle}, {HitBirdie}, {HitPar}, {HitBogey}, {Hit_x2_Bogey}, {Hit_x3_Bogey}";
	}

	public override string ToString()
	{
		return ToString(isDb: false);
	}
}
