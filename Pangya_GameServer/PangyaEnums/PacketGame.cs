namespace Pangya_GameServer.PangyaEnums
{
    /// <summary>
    /// Names of Packets Server
    /// </summary>
    public enum PacketIDServer
    {
        CLIENT_REQ_CONNECT_TO_SERVER = 0x01,
        SERVER_CHATMSG = 0x40,
        SERVER_ANNOUNCEMENT = 0x41,
        SERVER_NOTIFY = 0x42,
        SERVER_NEW_USER = 0x43,
        SERVER_LOGIN_ACK = 0x44,
        SERVER_MY_STATISTICS = 0x45,
    }

    /// <summary>
    /// IDs dos Pacotes Client -> Server (Pangya S1)
    /// </summary>
    public enum PacketIDClient : byte
    {
        CLIENT_NONE = 0x00,
        CLIENT_KEEP_ALIVE = 0x01,          // KeepAlive
        CLIENT_REQ_LOGIN = 0x02,
        CLIENT_CHAT_MESSAGE = 0x05,        // ChatMessage
        CLIENT_REQ_ENTER_CHANNEL = 0x07,
        CLIENT_LEAVE_LOBBY = 0x08,         // LeaveLobby
        CLIENT_STATISTICS = 0x0A,          // FUNC_STATISTICS
        CLIENT_CREATE_ROOM = 0x0E,         // CreateRoom
        CLIENT_JOIN_ROOM = 0x0F,           // JoinRoom
        CLIENT_CHANGE_ROOM_INFO = 0x11,    // ChangeRoomInfo
        CLIENT_USER_INFO_CHANGE = 0x13,    // UserInfoChange
        CLIENT_USER_READY = 0x17,          // FUNC_READY
        CLIENT_START_GAME = 0x1B,          // StartGame
        CLIENT_EXIT_ROOM = 0x1D,           // ExitRoom
        CLIENT_CHANGE_TEAM = 0x1E,         // FUNC_CHANGE_TEAM
        CLIENT_USER_LIST = 0x1F,           // UserList
        CLIENT_GCP_LOAD_OK = 0x22,         // FUNC_GCP_LOAD_OK
        CLIENT_GCP_SHOT = 0x23,            // FUNC_GCP_SHOT
        CLIENT_GAME_CAMERA = 0x25,         // FUNC_GAME_CAMERA
        CLIENT_CLICK = 0x26,               // FUNC_CLICK
        CLIENT_POWER_SHOT = 0x27,          // FUNC_POWER_SHOT
        CLIENT_CLUB = 0x28,                // FUNC_CLUB
        CLIENT_USE_ITEM = 0x29,            // FUNC_USE_ITEM
        CLIENT_EMOTICON = 0x2D,            // FUNC_EMOTICON
        CLIENT_CADDIE_ABILITY = 0x2E,      // CaddieAbility
        CLIENT_DROP = 0x30,                // FUNC_DROP
        CLIENT_HOLE_INFO = 0x32,           // FUNC_HOLE_INFO
        CLIENT_SHOT_RESULT = 0x33,         // FUNC_SHOT_RESULT
        CLIENT_SHOT_ACK = 0x34,            // FUNC_SHOT_ACK
        CLIENT_REQ_BUY_SHOP = 0x37,            // FUNC_SHOT_ACK
        CLIENT_UPDATE_EQUIP = 0x3A,        // UpdateEquip
        CLIENT_TIME_CHECK = 0x42,          // FUNC_TIME_CHECK
        CLIENT_MAIL_BOX = 0x43,              // Reintegrado da lista Antiga
        CLIENT_SKIP = 0x46,                // FUNC_SKIP
        CLIENT_REQUEST_VOTE = 0x47,        // FUNC_REQUEST_VOTE
        CLIENT_VOTE_FOR_BANISH = 0x48,     // FUNC_VOTE_FOR_BANISH
        CLIENT_INVITE = 0x49,              // Invite
        CLIENT_QUICK_INVITE = 0x4A,        // QuickInvite
        CLIENT_CHAT_WHISPER = 0x4B,               // PLAYER_WHISPER - Reintegrado da lista Antiga
        CLIENT_REQUEST_USER_LIST = 0x4C,   // RequestUserLis;t
        CLIENT_REQUEST_ROOM_LIST = 0x4D,   // RequestRoomList
        CLIENT_REQUEST_HELP_DESC = 0x4E,   // RequestHelpDesc
        CLIENT_REQUEST_DETAIL_ROOM = 0x50, // RequestDetailRoomInfo
        CLIENT_REQUEST_USER_STATE = 0x52,  // RequestUserState
        CLIENT_REQUEST_USER_INFO = 0x53,   // RequestUserInfo
        CLIENT_PAUSE = 0x55,               // FUNC_PAUSE
        CLIENT_HOLE_STAT = 0x56,           // FUNC_HOLE_STAT
        CLIENT_SLEEP = 0x57,               // Sleep
        CLIENT_REPORT_ERROR = 0x58,        // ReportError
        CLIENT_TEESHOT_READY = 0x59,       // FUNC_TEESHOT_READY
        CLIENT_TEAM_HOLEIN_PANG = 0x5A,    // FUNC_TEAM_HOLEIN_PANG
        CLIENT_ANSWER_GOSTOP = 0x5B,       // FUNC_ANSWER_GOSTOP
        CLIENT_END_STROKE_GAME = 0x5C,     // FUNC_END_STROKE_GAME
        CLIENT_REQ_CHANGE_NICKNAME = 0x5D, // FUNC_END_STROKE_GAME
        CLIENT_REPORT = 0x62,              // FUNC_REPORT
        CLIENT_REPORT_HACK = 0x66,         // ReportHack
        CLIENT_JOIN_GALLERY = 0x67,        // JoinGallery
        CLIENT_CHANGE_TARGET = 0x68,       // FUNC_CHANGE_TARGET
        CLIENT_PLAYINFO = 0x69,            // FUNC_PLAYINFO
        CLIENT_REQ_IDENTITY = 0x6A,        // Custom/Padrão
        CLIENT_GAME_SHOT_COMMAND = 0x6B,   // GameShotCommand
        CLIENT_REQ_SERVER_LIST = 0x6C,     // Custom/Padrão
        CLIENT_REQ_TITLE_LIST = 0x6D,     // Custom/Padrão
        CLIENT_SET_JJANG = 0x70,           // SetJJang
        CLIENT_LOADING_INFO = 0x72,        // FUNC_LOADING_INFO
        CLIENT_REPLAY = 0x74,              // FUNC_REPLAY
        CLIENT_ENCHANT = 0x75,             // Enchant
        CLIENT_BANISH_ALL = 0x76,          // FUNC_BANISH_ALL
        CLIENT_CHAT_PENALTY = 0x79,        // FUNC_CHAT_PENALTY
        CLIENT_SET_TITLE = 0x7B,           // SetTitle
        CLIENT_MATCH_HOLEIN_PANG = 0x7C,   // FUNC_MATCH_HOLEIN_PANG
        CLIENT_TEAM_CHAT = 0x7E,           // FUNC_30S_TEAMCHAT
        CLIENT_CHECK_PCBANG = 0x80,        // Custom/Padrão
        CLIENT_GAMEPLAY_INFO = 0x85,       // FUNC_GAMEPLAY_INFO
        CLIENT_BROADCASTM = 0x96,            // PLAYER_BROADCASTM - Reintegrado da lista Antiga
        CLIENT_BROADCAST = 0x97              // PLAYER_BROADCAST - Reintegrado da lista Antiga
    }
}
