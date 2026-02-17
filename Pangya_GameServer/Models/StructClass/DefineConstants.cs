using System;
using Pangya_GameServer.UTIL;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Utilities;

namespace Pangya_GameServer.Models;

public static class DefineConstants
{
	public const int NUMBER_OF_PLAYER_TO_WINNER = 100;

	public const int OPENNED_SPINNING_CUBE_TYPEID = 436207969;

	public const uint CARD_ABBOT_ELEMENTAL_SHARD = 2088763430u;

	public const int KEY_OF_SPINNING_CUBE_TYPEID = 436207964;

	public const int PAPEL_BOX_TYPEID = 436208136;

	public const double PI = Math.PI;

	public const int MAX_REWARD_PER_ROUND = 3;

	public const float SCALE_PANGYA = 3.2f;

	public const float DIVIDE_SCALE_PANGYA = 0.3125f;

	public const float DIVIDE_SLOPE_CURVE_SCALE = 0.00875f;

	public const float SCALE_SLOPE_CURVE = 114.28571f;

	public const double GRAVITY_SCALE_PANGYA = 34.295295715332;

	public const float DESVIO_SCALE_PANGYA_TO_YARD = 5f / 24f;

	public const float STEP_TIME = 0.02f;

	public const float EFECT_MAGNUS = 8E-05f;

	public const float ROTATION_SPIN_FACTOR = 3f;

	public const float ROTATION_CURVE_FACTOR = 0.75f;

	public const float SPIN_DECAI_FACTOR = 0.1f;

	public const float WIND_SPIKE_FACTOR = 0.01f;

	public const uint BASE_POWER_CLUB = 15u;

	public const double POWER_SPIN_PW_FACTORY = 0.0698131695389748;

	public const double POWER_CURVE_PW_FACRORY = 0.349065847694874;

	public const float ACUMULATION_SPIN_FACTOR = (float)Math.PI * 8f;

	public const float ACUMULATION_CURVE_FACTOR = (float)Math.PI * 4f;

	public const float BALL_ROTATION_SPIN_COBRA = 2.5f;

	public const float BALL_ROTATION_SPIN_SPIKE = 3.1f;

	public const double ROUND_ZERO = 1E-05;

	public const int LIMIT_LEVEL_CADDIE = 3;

	public const int LIMIT_LEVEL_MASCOT = 9;

	public const byte LIMIT_DEGREE = byte.MaxValue;

	public const ulong EXPIRES_CACHE_TIME = 3000uL;

	public const uint NUM_OF_EMAIL_PER_PAGE = 20u;

	public const uint LIMIT_OF_UNREAD_EMAIL = 300u;

	public const uint UPDATE_TIME_INTERVALE_HOUR = 24u;

	public const long STDA_10_MICRO_PER_MICRO = 10L;

	public const long STDA_10_MICRO_PER_MILLI = 10000L;

	public const long STDA_10_MICRO_PER_SEC = 10000000L;

	public const long STDA_10_MICRO_PER_MIN = 600000000L;

	public const long STDA_10_MICRO_PER_HOUR = 36000000000L;

	public const long STDA_10_MICRO_PER_DAY = 864000000000L;

	public const sbyte DEFAULT_CHANNEL = -1;

	public const sbyte DEFAULT_ROOM_ID = -1;

	public const uint CLEAR_10_DAILY_QUEST_TYPEID = 2021654529u;

	public const uint ASSIST_ITEM_TYPEID = 467664918u;

	public const uint GRAND_PRIX_TICKET = 436208228u;

	public const uint LIMIT_GRAND_PRIX_TICKET = 50u;

	public const uint MULLIGAN_ROSE_TYPEID = 402653198u;

	public const uint DEFAULT_COMET_TYPEID = 335544320u;

	public const uint AIR_KNIGHT_SET = 268435456u;

	public const uint CLUB_PATCHER_TYPEID = 436208015u;

	public const uint MILAGE_POINT_TYPEID = 436208295u;

	public const uint TIKI_POINT_TYPEID = 436208294u;

	public const uint SPECIAL_SHUFFLE_COURSE_TICKET_TYPEID = 436207863u;

	public const uint PANG_POUCH_TYPEID = 436207632u;

	public const uint EXP_POUCH_TYPEID = 436207965u;

	public const uint CP_POUCH_TYPEID = 436207968u;

	public const uint DECREASE_COMBO_VALUE = 3u;

	public const float MEDIDA_PARA_YARDS = 0.3125f;

	public const float GOOD_PLAYER_ICON = 3f;

	public const float QUITER_ICON_1 = 20f;

	public const float QUITER_ICON_2 = 30f;

	public static readonly uint[] active_item_cant_have_2_inventory = new uint[2] { 402653229u, 402653231u };

	public static readonly uint[] cadie_cauldron_Hermes_item_typeid = new uint[9] { 134283314u, 134537304u, 134799397u, 135061569u, 135307312u, 135585886u, 135831648u, 136110127u, 136372271u };

	public static readonly uint[] cadie_cauldron_Jester_item_typeid = new uint[14]
	{
		134219848u, 134482019u, 134744107u, 135004204u, 135266355u, 135528510u, 135790686u, 136052779u, 136314904u, 136577038u,
		136839197u, 137887748u, 137363468u, 137101316u
	};

	public static readonly uint[] cadie_cauldron_Twilight_item_typeid = new uint[9] { 134326290u, 134547473u, 134809629u, 135088156u, 135340059u, 135669776u, 135880723u, 136161303u, 136423436u };

	public const uint TICKET_BOT_TYPEID = 436208641u;

	public const uint TICKET_BOT_TYPEID2 = 436207927u;

	public const uint TROFEL_GM_EVENT_TYPEID = 755645184u;

	public const byte cadie_cauldron_Hermes_random_id = 2;

	public const byte cadie_cauldron_Jester_random_id = 3;

	public const byte cadie_cauldron_Twilight_random_id = 4;

	public const int MS_NUM_MAPS = 22;

	public const int STDA_INVITE_TIME_MILLISECONDS = 5000;

	public const int TREASURE_HUNTER_TIME_UPDATE = 1800;

	public const int TREASURE_HUNTER_LIMIT_POINT_COURSE = 1000;

	public const int TREASURE_HUNTER_INCREASE_POINT = 50;

	public const int TREASURE_HUNTER_BOX_PER_POINT = 100;

	public const uint PREMIUM_TICKET_TYPEID = 437256194u;

	public const uint PREMIUM_2_TICKET_TYPEID = 437256195u;

	public const uint PREMIUM_BOX_TYPEID = 436208314u;

	public const uint PREMIUM_CLUBSET_TYPEID = 268439553u;

	public const uint PREMIUM_BALL_TYPEID = 335544536u;

	public const uint PREMIUM_MASCOT_TYPEID = 1073741900u;

	public const uint PREMIUM_TITLE_TYPEID = 964690582u;

	public const uint PREMIUM_2_TITLE_TYPEID = 964690583u;

	public const uint PREMIUM_2_BALL_TYPEID = 335544553u;

	public const uint PREMIUM_2_CLUBSET_TYPEID = 268447744u;

	public const uint PREMIUM_3_CLUBSET_TYPEID = 268455939u;

	public const uint PREMIUM_2_AUTO_CALIPER_TYPEID = 436207680u;

	public const uint TICKET_REPORT_SCROLL_TYPEID = 436207682u;

	public const uint TICKET_REPORT_TYPEID = 436207681u;

	public const string STDA_END_LINE = "\r\n";

	public const uint TIME_BOOSTER_TYPEID = 436207633u;

	public const uint COIN_TYPEID = 436207632u;

	public const uint SPINNING_CUBE_TYPEID = 436207963u;

	public const uint KURAFAITO_RING_CLUBMASTERY = 1881210889u;

	public const uint AUTO_COMMAND_TYPEID = 436208031u;

	public const uint AUTO_CALIPER_TYPEID = 436207680u;

	public const uint POWER_MILK_TYPEID = 402653221u;

	public const uint CHIP_IN_PRACTICE_TICKET_TYPEID = 436207998u;

	public static readonly uint[] silent_wind_item = new uint[4] { 402653190u, 402653228u, 402653229u, 402653231u };

	public static readonly uint[] safety_item = new uint[2] { 402653224u, 402653229u };

	public static readonly uint[] passive_item = new uint[34]
	{
		436207626u, 436207627u, 436207629u, 436207630u, 436207631u, 436207635u, 436207636u, 436207663u, 436207669u, 436207748u,
		436207749u, 436207750u, 436207760u, 436207769u, 436207789u, 436207868u, 436207617u, 436207618u, 436207790u, 436207621u,
		436208567u, 436208087u, 436208088u, 436208218u, 436207623u, 436207624u, 436207625u, 436207628u, 436207680u, 436207633u,
		436208031u, 436208032u, 436207926u, 436208440u
	};

	public static readonly uint[] passive_item_exp_1perGame = new uint[2] { 436207631u, 436207636u };

	public static readonly uint[] passive_item_exp = new uint[16]
	{
		436207626u, 436207627u, 436207629u, 436207630u, 436207631u, 436207635u, 436207636u, 436207663u, 436207669u, 436207748u,
		436207749u, 436207750u, 436207760u, 436207769u, 436207789u, 436207868u
	};

	public static readonly uint[] passive_item_pang_x2 = new uint[3] { 436207617u, 436207618u, 436207790u };

	public static readonly uint[] passive_item_pang_x4 = new uint[2] { 436207621u, 436208567u };

	public static readonly uint[] passive_item_pang_x1_5 = new uint[2] { 436208087u, 436208088u };

	public static readonly uint[] passive_item_pang_x1_4 = new uint[1] { 436208218u };

	public static readonly uint[] passive_item_pang_x1_2 = new uint[4] { 436207623u, 436207624u, 436207625u, 436207628u };

	public static readonly uint[] passive_item_pang = new uint[12]
	{
		436207617u, 436207618u, 436207790u, 436207621u, 436208567u, 436208087u, 436208088u, 436208218u, 436207623u, 436207624u,
		436207625u, 436207628u
	};

	public static readonly uint[] passive_item_club_boost = new uint[1] { 436208440u };

	public const int DL_LIMIT_ITEM_PER_PAGE = 20;

	public const uint ART_LUMINESCENT_CORAL = 436208042u;

	public const uint ART_TROPICAL_TREE = 436208044u;

	public const uint ART_TWIN_LUNAR_MIRROR = 436208046u;

	public const uint ART_MACHINA_WRENCH = 436208048u;

	public const uint ART_SILVIA_MANUAL = 436208050u;

	public const uint ART_SCROLL_OF_FOUR_GODS = 436208064u;

	public const uint ART_ZEPHYR_TOTEM = 436208066u;

	public const uint ART_DRAGON_ORB = 436208120u;

	public const uint ART_FROZEN_FLAME = 436208122u;

	public static readonly uint[] devil_wings = new uint[13]
	{
		134309889u, 134580225u, 134842369u, 135120897u, 135366657u, 135661569u, 135858177u, 136194049u, 136398849u, 136660993u,
		137185293u, 137447425u, 138004481u
	};

	public static readonly uint[] obsidian_wings = new uint[14]
	{
		134309900u, 134580236u, 134842380u, 135120908u, 135366668u, 135661580u, 135858188u, 136194060u, 136398860u, 136661004u,
		136923148u, 137185286u, 137447434u, 138004490u
	};

	public static readonly uint[] corrupt_wings = new uint[14]
	{
		134309904u, 134580240u, 134842384u, 135120912u, 135366672u, 135661584u, 135858192u, 136194064u, 136398864u, 136661008u,
		136923154u, 137185283u, 137447437u, 138004493u
	};

	public static readonly uint[] hasegawa_chirain = new uint[2] { 135858185u, 136661000u };

	public static readonly uint[] hat_spooky_halloween = new uint[10] { 134318091u, 134588466u, 134850613u, 135071820u, 135374897u, 135635042u, 135848028u, 136153143u, 136388697u, 136650790u };

	public static readonly uint[] hat_lua_sol = new uint[10] { 134318083u, 134588456u, 134850599u, 135071807u, 135374883u, 135637077u, 135848016u, 136153125u, 136388682u, 136650773u };

	public static readonly uint[] hat_birthday = new uint[12]
	{
		134219909u, 134588444u, 134744114u, 135071798u, 135266360u, 135635015u, 135848008u, 136153118u, 136388668u, 136650771u,
		136912910u, 137191424u
	};

	public const uint ORCHID_BLOSSOM_ART = 436208036u;

	public const uint PENNE_ABACUS_ART = 436208038u;

	public const uint TITAN_WINDMILL_ART = 436208040u;

	public const uint ONLY_1M_RULE = 436208229u;

	public const uint SUPER_WIND_RULE = 436208230u;

	public const uint HOLE_CUP_MAGNET_RULE = 436208233u;

	public const uint NO_TURNING_BACK_RULE = 436208234u;

	public const uint WIND_3M_A_5M_RULE = 436208271u;

	public const uint WIND_7M_A_9M_RULE = 436208272u;

	public const uint ART_RAINBOW_MAGIC_HAT = 436208062u;

	public const uint ART_WICKED_BROOMSTICK = 436208052u;

	public const uint ART_TEORITE_ORE = 436208054u;

	public const uint ART_REDNOSE_WIZBERRY = 436208056u;

	public const uint ART_MAGANI_FLOWER = 436208058u;

	public const uint ART_ROGER_K_STEERING_WHEEL = 436208060u;

	public const uint SSC_TICKET = 436207863u;

	public static readonly uint[] motion_item = new uint[38]
	{
		134375424u, 134375425u, 134375426u, 134629376u, 134629377u, 134629378u, 134629379u, 134883328u, 134883329u, 134883330u,
		135153664u, 135153665u, 135153666u, 135407616u, 135407617u, 135407618u, 135718913u, 135718914u, 135718915u, 135718917u,
		135718918u, 135940096u, 135940097u, 136226816u, 136480768u, 136480769u, 136480770u, 136480771u, 136742912u, 136996864u,
		137250816u, 137250817u, 137496576u, 137496577u, 137496578u, 138037248u, 138037249u, 138037250u
	};

	public const float ALTURA_MIN_TO_CUBE_SPAWN = 60f;

	public const uint LIMIT_LOCATION_COIN_CUBE_PER_HOLE_PAR_3 = 30u;

	public const uint LIMIT_LOCATION_COIN_CUBE_PER_HOLE_PAR_4 = 100u;

	public const uint LIMIT_LOCATION_COIN_CUBE_PER_HOLE_PAR_5 = 150u;

	public const uint UPDATE_TIME_INTERVAL_HOUR = 24u;

	public const string BOT_GM_EVENT_NAME = "Bot GM Event";

	public const string MESSAGE_BOT_GM_EVENT_START_PART1 = "Bot GM Event comecou, sala criada no canal \"";

	public const string MESSAGE_BOT_GM_EVENT_START_PART2 = "\", o jogo comeca em ";

	public const string MESSAGE_BOT_GM_EVENT_START_PART3 = " minutos. Os premios sao ";

	public static uint STDA_MAKE_TROFEL(uint soma, int numPlayer)
	{
		if (numPlayer != 0)
		{
			uint match = (uint)Singleton<IFFHandle>.getInstance().MATCH;
			uint roundSoma = STDA_ROUND_SOMA_LEVEL(soma);
			return (match << 26) | (roundSoma / (uint)numPlayer / 5 << 16);
		}
		return 0u;
	}

	public static bool CHECK_PASSIVE_ITEM(uint _typeid)
	{
		return Singleton<IFFHandle>.getInstance().getItemGroupIdentify(_typeid) == Singleton<IFFHandle>.getInstance().ITEM && Singleton<IFFHandle>.getInstance().getItemSubGroupIdentify24(_typeid) > 1;
	}

	public static uint STDA_ROUND_SOMA_LEVEL(uint _soma)
	{
		return _soma + (5 - _soma % 5) - 5;
	}

	public static float TRANSF_SERVER_RATE_VALUE(int value)
	{
		return (value <= 0) ? 1f : ((float)value / 100f);
	}

	public static ulong enumToBitValue(Enum value)
	{
		return (ulong)(1L << Convert.ToInt32(value));
	}

	public static eTYPE_DISTANCE calculeTypeDistance(float distance)
	{
		eTYPE_DISTANCE type = eTYPE_DISTANCE.BIGGER_OR_EQUAL_58;
		if (distance >= 58f)
		{
			return eTYPE_DISTANCE.BIGGER_OR_EQUAL_58;
		}
		if (distance < 10f)
		{
			return eTYPE_DISTANCE.LESS_10;
		}
		if (distance < 15f)
		{
			return eTYPE_DISTANCE.LESS_15;
		}
		if (distance < 28f)
		{
			return eTYPE_DISTANCE.LESS_28;
		}
		if (distance < 58f)
		{
			return eTYPE_DISTANCE.LESS_58;
		}
		return type;
	}

	public static eTYPE_DISTANCE calculeTypeDistanceByPosition(Vector3D vec1, Vector3D vec2)
	{
		return calculeTypeDistance(vec1.distanceXZTo(vec2) * 0.3125f);
	}

	public static float getPowerShotFactory(byte ps)
	{
		float powerShotFactory = 0f;
		switch ((ePOWER_SHOT_FACTORY)ps)
		{
		case ePOWER_SHOT_FACTORY.ONE_POWER_SHOT:
			powerShotFactory = 10f;
			break;
		case ePOWER_SHOT_FACTORY.TWO_POWER_SHOT:
			powerShotFactory = 20f;
			break;
		case ePOWER_SHOT_FACTORY.ITEM_15_POWER_SHOT:
			powerShotFactory = 15f;
			break;
		}
		return powerShotFactory;
	}

	public static float getPowerByDegreeAndSpin(float degree, float spin)
	{
		return (float)(0.5 + 0.5 * ((double)degree + (double)spin * 0.0698131695389748) / 0.9773844164869327);
	}
}
