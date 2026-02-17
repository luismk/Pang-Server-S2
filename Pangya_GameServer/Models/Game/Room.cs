using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.Models;
using Pangya_GameServer.Models.Game;
using Pangya_GameServer.Models.Game.Base;
using Pangya_GameServer.Models.Manager;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.PangyaEnums;
using Pangya_GameServer.Repository; 
using Pangya_GameServer.UTIL;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.Repository;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Pangya_GameServer.Models.DefineConstants;
using int64_t = System.Int64;
using uint32_t = System.UInt32;
namespace Pangya_GameServer.Game
{
    /// <summary>
    /// class room, finalization 5/7/25
    /// </summary> 
    public partial class Room : IDisposable
    {
        protected List<Player> v_sessions { get; set; } = new List<Player>();
        protected List<Player> m_listGallery = new List<Player>();
        protected Dictionary<Player, PlayerRoomInfoEx> m_player_info = new Dictionary<Player, PlayerRoomInfoEx>();
        protected Dictionary<uint, bool> m_player_kicked = new Dictionary<uint, bool>();
        public object m_lock_cs = new object();        // Bloquea a sala 
        public object m_cs = new object();       // Bloquea a sala  
        protected List<Team> m_teans = new List<Team>();

        protected GuildRoomManager m_guild_manager = new GuildRoomManager();

        protected List<InviteChannelInfo> v_invite = new List<InviteChannelInfo>();

        protected RoomInfoEx m_ri = new RoomInfoEx();

        protected sbyte m_channel_owner; // Id do Canal dono da sala

        protected bool m_bot_tourney; // Bot para começa o Modo tourney só com 1 jogador
        private int m_lock_spin_state;
        protected bool m_destroying;

        protected GameBase m_pGame;
        // Room Tipo Lounge
        protected byte m_weather_lounge;
        public RoomInfoLog m_room_log;
        private bool disposedValue;

        public Room(sbyte _channel_owner, RoomInfoEx _ri)
        {
            this.m_ri = _ri;
            this.m_pGame = null;
            this.m_channel_owner = _channel_owner;
            this.m_teans = new List<Team>();
            this.m_weather_lounge = 0;
            this.m_destroying = false;
            this.m_bot_tourney = false;
            this.m_lock_spin_state = 01;
            this.m_room_log = new RoomInfoLog(m_ri);

            geraSecurityKey();

            // Calcula chuva(weather) se o tipo da sala for lounge
            calcRainLounge();

            // Atualiza tipo da sala
            setTipo(m_ri.tipo);

            // Att Exp rate, e Pang rate, que criou a sala, att ele também quando começa o jogo

            m_ri.rate_exp = (uint)sgs.gs.getInstance().getInfo().rate.exp;
            m_ri.rate_pang = (uint)sgs.gs.getInstance().getInfo().rate.pang;
            m_ri.angel_event = sgs.gs.getInstance().getInfo().rate.angel_event == 1 ? true : false;
        }

        private void geraSecurityKey()
        {
            new Random().NextBytes(m_ri.key);
        }

        private void clear_Player_kicked()
        {
            if (!m_player_kicked.empty())
                m_player_kicked.Clear();
        }

        public void enter(Player _session)
        {

            if (!_session.getState())
            {
                throw new exception("[room::enter] [Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    4, 0));
            }



            if (isFull())
            {
                throw new exception("[room::enter] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou entrar na a sala[NUMERO=" + m_ri.numero + "], mas a sala ja esta cheia.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2, 0));
            }

            if (_session.m_pi.mi.sala_numero != -1)
            {
                throw new exception("[room::enter] [Error] PLAYER[UID=" + _session.m_pi.uid + "] sala[NUMERO=" + m_ri.numero + "], ja esta em outra sala[NUMERO=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "], nao pode entrar em outra. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    120, 0));
            }

            if (m_ri.getTipo() == RoomInfo.TIPO.GUILD_BATTLE
                && m_ri.guilds.guild_1_uid != 0
                && m_ri.guilds.guild_2_uid != 0
                && m_ri.guilds.guild_1_uid != _session.m_pi.gi.uid
                && m_ri.guilds.guild_2_uid != _session.m_pi.gi.uid)
            {
                throw new exception("[room::enter] [Error] PLAYER[UID=" + _session.m_pi.uid + "] sala[NUMERO=" + m_ri.numero + "], ja tem duas guild e o Player que quer entrar nao é de nenhum delas. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    11000, 0));
            }

            try
            {

                _session.m_pi.mi.sala_numero = m_ri.numero;
 
                _session.m_pi.place = 0;
                v_sessions.Add(_session);

                m_ri.num_player = (byte)v_sessions.Count;

                // Update Trofel
                if (m_ri.trofel > 0)
                {
                    updateTrofel();
                }

                 // Add o Player ao jogo
                if (m_pGame != null)
                {
                    m_pGame.addPlayer(_session);

                    if (m_ri.trofel > 0)
                    {
                        updateTrofel();
                    }
                }

                try
                {
                    // Make Info Room Player
                    makePlayerInfo(_session);

                }
                catch (exception e)
                {
                    _smp.message_pool.getInstance().push(new message("[room::enter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                if (m_ri.getTipo() == RoomInfo.TIPO.GUILD_BATTLE)
                {
                    updateGuild(_session);
                }

                //room is player
               // _session.m_room = this;
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[room::enter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        protected virtual PlayerRoomInfoEx makePlayerInfo(Player _session)
        {
            PlayerRoomInfoEx pri = new PlayerRoomInfoEx();

            // Player Room Info Init 
            pri.oid = _session.m_oid;
            pri.uid = _session.m_pi.uid;
            pri.id = _session.m_pi.id;
            pri.nickname = _session.m_pi.nickname;
            pri.guild_name = _session.m_pi.gi.name;
            pri.position = (byte)(getPosition(_session) + 1);
            pri.capability = _session.m_pi.m_cap;
            pri.title = _session.m_pi.ue.m_title; 
            if (_session.m_pi.ei.char_info != null)
                pri.char_typeid = _session.m_pi.ei.char_info._typeid;

            if (_session.m_pi.ei.cad_info != null)
                pri.cad_id = _session.m_pi.ei.cad_info.id;

            if (_session.m_pi.ei.clubset != null)
                pri.club_typeid = _session.m_pi.ei.clubset._typeid;

            if (_session.m_pi.ei.comet != null)
                pri.comet_typeid = _session.m_pi.ei.comet._typeid; 



            //pri.skin[4] = 0;

            if (getMaster() == _session.m_pi.uid)
            {
                pri.state_flag.master = 1;
                pri.state_flag.ready = 1;// Sempre está pronto(ready) o master
            }
            pri.state_flag.sexo = _session.m_pi.mi.sexo;
             
            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
            {
                float rate = _session.m_pi.ui.getQuitRate();

                if (rate < GOOD_PLAYER_ICON)
                {
                    pri.state_flag.azinha = 1;
                } 
            }

            pri.level = _session.m_pi.mi.level;

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                pri.icon_angel = _session.m_pi.ei.char_info.AngelEquiped();
            else
                pri.icon_angel = 0;
             
            if (_session.m_pi.ei.char_info != null)
                pri.ci = _session.m_pi.ei.char_info;
            if (!m_player_info.TryAdd(_session, pri))
            {
                if (m_player_info.TryGetValue(_session, out var existingPri))
                {
                    if (existingPri.uid != _session.m_pi.uid)
                    {
                        try
                        {
                            var pri_ant = m_player_info[_session];
                            m_player_info[_session] = pri;
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            _smp.message_pool.getInstance().push(new message($"[room::makePlayerInfo] [Error][Warning] PLAYER[UID={_session.m_pi.uid}], nao conseguiu atualizar o PlayerRoomInfo da session para o novo PlayerRoomInfo do player atual da session. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            throw e;
                        }
                    }
                    else
                    {
                        _smp.message_pool.getInstance().push(new message($"[room::makePlayerInfo][Info] PLAYER[UID={_session.m_pi.uid}] nao conseguiu adicionar o PlayerRoomInfo da session, por que ja tem o mesmo PlayerRoomInfo no map.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
                else
                {
                    _smp.message_pool.getInstance().push(new message($"[room::makePlayerInfo] [Error] nao conseguiu inserir o pair de PlayerInfo do PLAYER[UID={_session.m_pi.uid}] no map de player info do room. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            return pri;
        }

        protected PlayerRoomInfoEx makePlayerInvitedInfo(Player _session)
        {

            PlayerRoomInfoEx pri = new PlayerRoomInfoEx();

            // Player Room Info Init
            pri.oid = _session.m_oid;
            pri.position = (byte)(getPosition(_session) + 1); // posição na sala

            pri.place = 10; // 0x0A dec"10" _session.m_pi.place, pode ser lugar[place]

            pri.uid = _session.m_pi.uid;

            pri.convidado = 1; // Flag Convidado, [Não sei bem por que os que entra na sala normal tem valor igual aqui, já que é type de convidado waiting], Valor constante da sala para os players(ACHO)

            // Check inset pair in map of room player info
            if (m_player_info.ContainsKey(_session))
            {
                try
                {

                    // pega o antigo PlayerRoomInfo para usar no Log
                    var pri_ant = m_player_info[_session];

                    // Novo PlayerRoomInfo
                    m_player_info[_session] = pri;

                }
                catch (IndexOutOfRangeException e)
                {
                    _smp.message_pool.getInstance().push(new message("[room::makePlayerInfo] [Error][Warning] PLAYER[UID=" + _session.m_pi.uid + "], nao conseguiu atualizar o PlayerRoomInfo da session para o novo PlayerRoomInfo do player atual da session. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    throw e;
                }
            }
            else
                m_player_info.Add(_session, pri);

            return pri;
        }


        protected void updatePosition()
        {
            lock (m_cs)
            {
                for (int i = 0; i < v_sessions.Count; ++i)//255 e o limite
                {
                    m_player_info[v_sessions[i]].position = (byte)Math.Min(i + 1, byte.MaxValue);
                }
            }
        }


        private void updateTrofel()
        {

            if (v_sessions.Count() > 0 && (m_ri.trofel != TROFEL_GM_EVENT_TYPEID || m_ri.max_player <= 30) && (m_ri.time_30s > 0 && m_ri.tipo != (byte)RoomInfo.TIPO.GUILD_BATTLE)
                && m_ri.master != -2)
            {

                if (m_pGame != null)
                    m_pGame.requestUpdateTrofel();
                else
                {

                    uint32_t soma = 0;

                    foreach (var _el in v_sessions)
                    {
                        if (_el != null)
                            soma += (uint)((_el.m_pi.level > 60) ? 60 : (_el.m_pi.level > 0 ? _el.m_pi.level - 1 : 0));

                    }
                    uint32_t new_trofel = STDA_MAKE_TROFEL(soma, v_sessions.Count());

                    if (new_trofel > 0 && new_trofel != m_ri.trofel)
                    {

                        // Check se o trofeu anterior era o GM e se o novo não é mais, aí tira a type de GM da sala
                        if (m_ri.trofel == TROFEL_GM_EVENT_TYPEID && new_trofel != TROFEL_GM_EVENT_TYPEID)
                            m_ri.flag_gm = 0;

                        if (m_ri.trofel > 0)
                        {

                            m_ri.trofel = new_trofel;

                            var p = new PangyaBinaryWriter(0x97);

                            p.WriteUInt32(m_ri.trofel);

                            packet_func.room_broadcast(this, p, 1);

                        }
                        else
                            m_ri.trofel = new_trofel;
                    }
                }
            }
        }

        public int leave(Player _session, int _option)
        {
            try
            {
                int index = findIndexSession(_session);

                if (index == -1)
                {
                    throw new exception("[room::leave] [Error] session[UID=" + _session.m_pi.uid + "] nao existe no vector de sessions da sala[NUMERO=" + m_ri.numero + "].", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        5, 0));
                }

                if (_option != 0
                    && _option != 1
                    && _option != 0x800
                    && _option != 10)
                {
                    addPlayerKicked(_session.m_pi.uid);
                }

                // Verifica se ele está em um jogo e tira ele
                try
                {

                    if (m_pGame != null)
                    {
                        if (m_pGame.deletePlayer(_session, _option) && m_pGame.finish_game(_session, 2))
                        {
                            finish_game();
                        }
                    }

                }
                catch (exception e)
                {
                    _smp.message_pool.getInstance().push(new message("[room::leave][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                if (v_sessions != null && index >= 0 && index < v_sessions.Count)
                {
                    v_sessions.RemoveAt(index);
                }



                if ((m_ri.num_player - 1) > 0 || v_sessions.Count == 0)
                {
                    --m_ri.num_player;
                }

                // Sai do Team se for Match
                if (m_ri.getTipo() == RoomInfo.TIPO.MATCH)
                {

                    if (m_teans.Count < 2)
                    {
                        throw new exception("[room::leave] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou sair da sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem os 2 teans(times). Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1502, 0));
                    }

                    var pPri = getPlayerInfo(_session);

                    if (pPri == null)
                    {
                        throw new exception("[room::leave] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou sair da sala[NUMERO=" + m_ri.numero + "], mas a sala nao encontrou o info do Player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1503, 0));
                    }

                  //  m_teans[pPri.state_flag.team].deletePlayer(_session, _option);
                }
                else if (m_ri.getTipo() == RoomInfo.TIPO.GUILD_BATTLE)
                {

                    var pPri = getPlayerInfo(_session);

                    if (pPri == null)
                    {
                        throw new exception("[room::leave] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou sair da sala[NUMERO=" + m_ri.numero + "], mas a sala nao encontrou o info do Player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1503, 0));
                    }

                    var guild = m_guild_manager.findGuildByPlayer(_session);

                    if (guild == null)
                    {
                        throw new exception("[room::leave] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou sair da sala[NUMERO=" + m_ri.numero + "], mas o Player nao esta em nenhuma guild da sala. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1504, 0));
                    }

                    // Deleta o Player da guild  da sala
                    guild.deletePlayer(_session);

                    //// Deleta Player do team
                    //m_teans[pPri.state_flag.team].deletePlayer(_session, _option);

                    //// Limpa o team do Player
                    //pPri.state_flag.team = 0;

                    // Limpa guild
                    if (guild.numPlayers() == 0)
                    {

                        if (guild.getTeam() == Guild.eTEAM.RED)
                        {
                            // Red
                            m_ri.guilds.guild_1_uid = 0;
                            m_ri.guilds.guild_1_index_mark = 0;
                        }
                        else
                        {

                            // Blue
                            m_ri.guilds.guild_2_uid = 0;
                            m_ri.guilds.guild_2_index_mark = 0;

                        }

                        //delete Guild
                        m_guild_manager.deleteGuild(guild);
                    }
                }

                // Delete Player Info 
                m_player_info.Remove(_session);
                // reseta(default) o número da sala no info do Player
                _session.m_pi.mi.sala_numero = -1;
                _session.m_pi.place = 0;
                 
                updatePosition();

                updateTrofel();

                // Isso é para o cliente saber que ele foi kickado pelo server sem ação de outro Player
                if (_option == 0x800 || (_option != 0 && _option != 1 && _option != 3))
                {

                    uint opt_kick = 0x800;

                    switch (_option)
                    {
                        case 1:
                            opt_kick = 4;
                            break;
                        case 2:
                            opt_kick = 2;
                            break;
                        default:
                            opt_kick = (uint)_option;
                            break;
                    }

                    var p = new PangyaBinaryWriter((ushort)0x7E);

                    p.WriteUInt32(opt_kick);

                    packet_func.session_send(p,
                        _session, 1);
                }
                 
                // Update Players State On Room
                if (v_sessions.Count > 0)
                {
                    sendUpdate();

                    sendCharacter(_session, 2);
                }
                // Fim Update Players State

                if ((m_pGame == null && _session.m_pi.uid == m_ri.master) || (_session.m_pi.m_cap.game_master && m_ri.master == _session.m_pi.uid && m_ri.trofel == TROFEL_GM_EVENT_TYPEID))
                {
                    return 0x801; // deleta todos da sala

                }
                else if (m_pGame == null)
                {
                    updateMaster(null); // update Master
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::leave][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }



            return v_sessions.Count > 0 || m_ri.master == -2 && !isDropRoom() ? 0 : 1;
        }



        public void sendMake(Player _session)
        {
            var p = packet_func.pacote0D(this, 0);
            packet_func.session_send(p, _session, 0);
        }

        public void sendUpdate()
        {
            var p = packet_func.pacote0E(m_ri, -1/*valor constante*/);
            packet_func.room_broadcast(this, p, 0);
        }

        private void addPlayerKicked(uint _uid)
        {
            if (isKickedPlayer(_uid))
                _smp.message_pool.getInstance().push(new message("[room::addPlayerKicked] [Error][Warning] PLAYER[UID=" + (_uid) + "] ja foi chutado da sala[NUMERO="
                    + (m_ri.numero) + "]", type_msg.CL_FILE_TIME_LOG_AND_CONSOLE));
            else
                m_player_kicked[_uid] = true;
        }

        public static void SQLDBResponse(int _msg_id,
                Pangya_DB _pangya_db,
                object _arg)
        {

            if (_arg == null)
            {
                _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Warning] _arg is null com msg_id = " + Convert.ToString(_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse] [Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }
            Channel _channel = null;
            Room _room = null;

            if (_arg is Channel ch)
                _channel = ch;

            if (_arg is Room r)
                _room = r;

            switch (_msg_id)
            {
                case 7: // Update Character PCL
                    {
                        var cmd_ucp = (CmdUpdateCharacterPCL)(_pangya_db);
                        break;
                    }
                case 8: // Update ClubSet Stats
                    {
                        var cmd_ucss = (CmdUpdateClubSetStats)(_pangya_db);

                        break;
                    }
                case 9: // Update Character Mastery
                    {
                        var cmd_ucm = (CmdUpdateCharacterMastery)(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Info] Atualizou Character[TYPEID=" + Convert.ToString(cmd_ucm.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucm.getInfo().id) + "] Mastery[value=" + Convert.ToString(cmd_ucm.getInfo().mastery) + "] do PLAYER[UID=" + Convert.ToString(cmd_ucm.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 12: // Update ClubSet Workshop
                    {
                        var cmd_ucw = (CmdUpdateClubSetWorkshop)(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Info] PLAYER[UID=" + Convert.ToString(cmd_ucw.getUID()) + "] Atualizou ClubSet[TYPEID=" + Convert.ToString(cmd_ucw.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucw.getInfo().id) + "] Workshop[C0=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[0]) + ", C1=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[1]) + ", C2=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[2]) + ", C3=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[3]) + ", C4=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[4]) + ", Level=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.level) + ", Mastery=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.mastery) + ", Rank=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.rank) + ", Recovery=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.recovery_pts) + "] Flag=" + Convert.ToString(cmd_ucw.getFlag()) + "", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 26: // Update Mascot Info
                    {

                        var cmd_umi = (CmdUpdateMascotInfo)(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Info] PLAYER[UID=" + Convert.ToString(cmd_umi.getUID()) + "] Atualizar Mascot Info[TYPEID=" + Convert.ToString(cmd_umi.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_umi.getInfo().id) + ", LEVEL=" + Convert.ToString((ushort)cmd_umi.getInfo().level) + ", EXP=" + Convert.ToString(cmd_umi.getInfo().exp) + ", FLAG=" + Convert.ToString((ushort)cmd_umi.getInfo().flag) + ", TIPO=" + Convert.ToString(cmd_umi.getInfo().tipo) + ", IS_CASH=" + Convert.ToString((ushort)cmd_umi.getInfo().is_cash) + ", PRICE=" + Convert.ToString(cmd_umi.getInfo().price) + ", MESSAGE=" + cmd_umi.getInfo().message + ", END_DT=" + UtilTime.FormatDate(cmd_umi.getInfo().data.ConvertTime()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 0:
                default: // 25 é update item equipado slot
                    break;
            }
        }


        protected void calcRainLounge()
        {
             
        }


        protected void clear_teans()
        {
            if (m_teans.Any())
            {
                m_teans.Clear();
            }
        }

        // Add Bot Tourney Visual to Room
        protected void addBotVisual(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "addBotVisual] [Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            // Add Bot
            List<PlayerRoomInfoEx> v_element = new List<PlayerRoomInfoEx>();
            PlayerRoomInfoEx pri = new PlayerRoomInfoEx();
            PlayerRoomInfoEx tmp_pri = null;

            try
            {


                v_sessions.ForEach(_el =>
                {
                    tmp_pri = getPlayerInfo(_el);
                    if (tmp_pri != null)
                    {
                        v_element.Add(tmp_pri);
                    }
                });


                if (v_element.Count == 0)
                {
                    throw new exception("[room::makeBot] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou criar Bot na sala[NUMERO=" + m_ri.numero + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao nenhum Player na sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 5000));
                }

                // Inicializa os dados do Bot
                // Player Room Info Init
                pri.oid = _session.m_oid;
                pri.uid = _session.m_pi.uid;
                pri.id = "bot_id";
                pri.nickname = "Bot(orgulhoso)";
                pri.guild_name = "";
                pri.position = 0; // 0 Que é para ele ficar em primeiro e parece que tbm não deixa kick(ACHO)
                pri.state_flag.ready = 1;
                pri.capability = new uCapability();
                pri.title = 1;
                if (_session.m_pi.ei.char_info != null)
                    pri.char_typeid = _session.m_pi.ei.char_info._typeid;

                if (_session.m_pi.ei.cad_info != null)
                    pri.cad_id = _session.m_pi.ei.cad_info.id;

                if (_session.m_pi.ei.clubset != null)
                    pri.club_typeid = _session.m_pi.ei.clubset._typeid;

                if (_session.m_pi.ei.comet != null)
                    pri.comet_typeid = _session.m_pi.ei.comet._typeid;

                pri.state_flag.sexo = _session.m_pi.mi.sexo;

                if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
                {
                    float rate = _session.m_pi.ui.getQuitRate();

                    if (rate < GOOD_PLAYER_ICON)
                    {
                        pri.state_flag.azinha = 1;
                    }
                }

                pri.level = _session.m_pi.mi.level;

                if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                    pri.icon_angel = _session.m_pi.ei.char_info.AngelEquiped();
                else
                    pri.icon_angel = 0;

                // Add o Bot a sala, só no visual
                v_element.Add(pri);

                // Packet
                var p = new PangyaBinaryWriter();

                // Option 0, passa todos que estão na sala
                if (packet_func.pacote0A(ref p, _session, v_element, 0x100))
                    packet_func.room_broadcast(this, p, 1);

                // Option 1, passa só o Player que entrou na sala, nesse caso foi o Bot
                if (packet_func.pacote0A(ref p, _session, new List<PlayerRoomInfoEx> { pri }, 0x101))
                    packet_func.room_broadcast(this, p, 1);

                // Criou Bot com sucesso
                m_bot_tourney = true;

                // Log
                _smp.message_pool.getInstance().push(new message("[room::addBotVisual][Info] PLAYER[UID=" + _session.m_pi.uid + "] Room[NUMBER=" + m_ri.numero + ", MASTER=" + Convert.ToString(m_ri.master) + "] Bot criado com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));



            }
            catch (exception e)
            {

                // Relança
                throw e;
            }
        }

        // Para as classes filhas, empedir que exclua a sala depLastLasto do se tem Player ou não na sala
        protected virtual bool isDropRoom()
        {
            return true; // class room normal é sempre true
        }

        // protected porque é um método inseguro (sem thread safety)
        protected uint _getRealNumPlayersWithoutInvited()
        {
            return (uint)v_sessions.Count(_el =>
            {
                if (_el == null)
                    return false;

                return m_player_info.TryGetValue(_el, out var playerInfo) && !(playerInfo.convidado == 1);
            });
        }


        // protected por que é o método unsave(inseguro), sem thread safe
        protected bool _haveInvited()
        {
            return v_sessions.Any(_el =>
            {
                if (_el == null)
                    return false;

                return m_player_info.TryGetValue(_el, out var playerInfo) && playerInfo.convidado == 1;
            });
        }


        // Game
        public virtual void finish_game()
        {

            if (m_pGame != null)
            {

                var toAdd = new List<(Player player, PlayerRoomInfoEx info)>();
                // Zera Player Flags
                var player_info = m_player_info.ToList();
                foreach (var el in player_info)
                {
                    // Update Place Player
                    el.Value.place = 0;

                    el.Value.state_flag.away = 0;

                    // Aqui só zera quem não é Master da sala, o master deixa sempre ready
                    if (m_ri.master == el.Key.m_pi.uid)
                    {
                        el.Value.state_flag.ready = 1;
                    }
                    else
                    {
                        el.Value.state_flag.ready = 0;
                    }

                    // Update Player info
                    updatePlayerInfo(el.Key);

                    // SLast update on room
                    sendCharacter(el.Key, 3);
                }

                // Atualiza type da sala, só não atualiza se for GM evento ou GZ Event e SSC
                if (!(m_ri.trofel == TROFEL_GM_EVENT_TYPEID || m_ri.master == -2))
                    m_ri.state = 1; //em espera

                // Att Exp rate, e Pang rate, que criou a sala, att ele também quando começa o jogo

                m_ri.rate_exp = (uint)sgs.gs.getInstance().getInfo().rate.exp;
                m_ri.rate_pang = (uint)sgs.gs.getInstance().getInfo().rate.pang;
                m_ri.angel_event = sgs.gs.getInstance().getInfo().rate.angel_event.IsTrue();


                // Update Course of Hole
                if (m_ri.getMap() >= 0x7F) // Random Course With Course already draw
                    m_ri.course = RoomInfo.eCOURSE.RANDOM; // Random Course standard


                // Update Master da sala
                updateMaster(null);

                if (m_ri.master == -2)
                    m_ri.master = -1; // pode deletar a sala quando sair todos


                if (v_sessions.Count > 0)
                {
                    // Atualiza info da sala para quem está na sala 
                    packet_func.room_broadcast(this,
                         packet_func.pacote0E(
                        m_ri, -1), 1);
                }

                // limpa lista de Player kikados
                clear_Player_kicked();

                // Verifica se o Bot Tourney está ativo, kika bot e limpa a type
                if (m_bot_tourney)
                {

                    var pMaster = findMaster();

                    if (pMaster != null)
                    {

                        try
                        {
                            // Kick Bot
                            // Atualiza os Player que estão na sala que o Bot sai por que ele é só visual
                            sendCharacter(pMaster, 0);

                        }
                        catch (exception e)
                        {

                            _smp.message_pool.getInstance().push(new message("[room::finish_game::KickBotTourney][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }

                    m_bot_tourney = false;
                }
                if (m_pGame != null)
                {
                    m_pGame.stopTime();//desliga o relogio...
                }

                m_pGame = null;      // elimina a referência
            }
        }

        // Invite
        protected void clear_invite()
        {
            if (v_invite.Any())
            {
                v_invite.Clear();
            }
        }

        // Team
        protected void init_teans()
        {

            // Limpa teans, se tiver teans inicilizados já
            clear_teans();

            // Init Teans
            m_teans.Add(new Team(0));
            m_teans.Add(new Team(1));

            PlayerRoomInfo pPri = null;

            // Add Players All Seus Respectivos teans
            foreach (var el in v_sessions)
            {

                if ((pPri = getPlayerInfo(el)) == null)
                {
                    throw new exception("[room::init_teans] [Error] nao encontrou o info do PLAYER[UID=" + Convert.ToString(el.m_pi.uid) + "] na sala. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1504, 0));
                }

              //  m_teans[pPri.state_flag.team].addPlayer(el);
            }

        }


        public int leaveAll(int _option)
        {

            while (!v_sessions.empty())
            {

                try
                {
                    leave(v_sessions.First(), _option);
                }
                catch (exception e)
                {

                    _smp.message_pool.getInstance().push(new message("[room::leaveAll] [Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            return 0;
        }

        public bool isInvited(Player _session)
        {

            var it = m_player_info.FirstOrDefault(c => c.Key.getUID() == _session.getUID());

            return (it.Value != null && it.Value.convidado == 1);
        }

        public InviteChannelInfo addInvited(uint _uid_has_invite, Player _session)
        {

            if (!_session.getState())
            {
                throw new exception("[room::addInvited] [Error] Player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    4, 0));
            }

            if (isFull())
            {
                throw new exception("[room::addInvited] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou entrar na a sala[NUMERO=" + m_ri.numero + "], mas a sala ja esta cheia.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2, 0));
            }

            if (findIndexSession(_uid_has_invite) == (int)~0)
            {
                throw new exception("[room::addInvited] [Error] quem convidou[UID=" + Convert.ToString(_uid_has_invite) + "] o PLAYER[UID=" + _session.m_pi.uid + "] para a sala nao esta na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2010, 0));
            }

            var s = findSessionByUID(_session.m_pi.uid);

            if (s != null)
            {
                throw new exception("[room::addInvited] [Error] PLAYER[UID=" + Convert.ToString(_uid_has_invite) + "] tentou adicionar o convidado[UID=" + _session.m_pi.uid + "] a sala, mas ele ja esta na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2001, 0));
            }



            _session.m_pi.mi.sala_numero = m_ri.numero;

            _session.m_pi.place = 70; // Está sendo convidado

            v_sessions.Add(_session);

            ++m_ri.num_player;

            PlayerRoomInfoEx pri = null;

            try
            {

                // Make Info Room Player Invited
                pri = makePlayerInvitedInfo(_session);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::addInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            if (pri == null)
            {

                // Pop_back
                v_sessions.Remove(v_sessions.Last());



                throw new exception("[[room::addInvited] [Error] PLAYER[UID=" + Convert.ToString(_uid_has_invite) + "] tentou adicionar o convidado[UID=" + _session.m_pi.uid + "] a sala, nao conseguiu criar o Player Room Info Invited do Player. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2002, 0));
            }

            // Add Invite Channel Info
            InviteChannelInfo ici = new InviteChannelInfo
            {
                room_number = m_ri.numero,
                invite_uid = _uid_has_invite,
                invited_uid = _session.m_pi.uid,
                time = new SYSTEMTIME(DateTime.Now)
            };

            v_invite.Add(ici);
            // End Add Invite Channel Info

            // Update Char Invited ON ROOM
            var p = new PangyaBinaryWriter((ushort)0x48);

            p.WriteByte(1);
            p.WriteInt16(-1);

            p.WriteBytes(pri.ToArrayEx());

            p.WriteByte(0); // Final Packet

            packet_func.room_broadcast(this,
                p, 1);



            return ici;
        }
        public InviteChannelInfo getInvited(Player _session)
        {
            return v_invite.FirstOrDefault(_el =>
            {
                return (_el.room_number == m_ri.numero && _el.invited_uid == _session.m_pi.uid);
            });
        }

        public InviteChannelInfo getInvited(uint _uid)
        {
            return v_invite.FirstOrDefault(_el => _el.room_number == m_ri.numero && _el.invited_uid == _uid);
        }

        public InviteChannelInfo deleteInvited(Player _session)
        {

            var it = m_player_info.FirstOrDefault(c => c.Key.m_pi.uid == _session.m_pi.uid);

            if (it.Key == null && getInvited(_session) == null)
                throw new exception("[room::deleteInvited] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou deletar convidado,"
                    + " mas nao tem o info do convidado na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM, 2003, 0));


            int index = findIndexSession(_session);

            if (index == -1 && getInvited(_session) == null)
            {
                throw new exception("[room::deleteInvited] [Error] session[UID=" + _session.m_pi.uid + "] nao existe no vector de sessions da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    5, 0));
            }

            _session.m_pi.mi.sala_numero = -1;

            _session.m_pi.place = 0; // Limpa Está sendo convidado

            v_sessions.RemoveAt(index);

            --m_ri.num_player;

            m_player_info.Remove(_session);

            // Update Position all Players
            updatePosition();

            // Delete Invite Channel Info
            InviteChannelInfo ici = new InviteChannelInfo();


            var itt = getInvited(_session);

            if (itt != null)
            {

                ici = itt;

                v_invite.Remove(itt);

            }
            else
            {
                _smp.message_pool.getInstance().push(new message("[room::deleteInvited][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao tem um convite.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            // End Delete Invite Channel Info

            // Resposta Delete Convidado
            var p = new PangyaBinaryWriter((ushort)0x130);

            p.WriteUInt32(_session.m_pi.uid);

            packet_func.room_broadcast(this,
                p, 1);

            _smp.message_pool.getInstance().push(new message("[room::deleteInvited][Info] Deleteou um convite[Convidado=" + _session.m_pi.uid + "] na Sala[NUMERO=" + m_ri.numero + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


            return ici;
        }


        public InviteChannelInfo _deleteInvited(Player _session)
        {


            // Delete Invite Channel Info
            InviteChannelInfo ici = new InviteChannelInfo();


            var itt = getInvited(_session);

            if (itt != null)
            {

                ici = itt;

                v_invite.Remove(itt);

            }
            else
            {
                _smp.message_pool.getInstance().push(new message("[room::deleteInvited][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao tem um convite.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            // End Delete Invite Channel Info

            // Resposta Delete Convidado
            var p = new PangyaBinaryWriter((ushort)0x130);

            p.WriteUInt32(_session.m_pi.uid);

            packet_func.room_broadcast(this,
                p, 1);

            _smp.message_pool.getInstance().push(new message("[room::deleteInvited][Info] Deleteou um convite[Convidado=" + _session.m_pi.uid + "] na Sala[NUMERO=" + m_ri.numero + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


            return ici;
        }
        public InviteChannelInfo deleteInvited(uint _uid)
        {

            if (_uid == 0)
            {
                throw new exception("[room::deleteInvited] [Error] _uid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    2005, 0));
            }

            //var it = m_player_info.FirstOrDefault(_el =>
            //{
            //    return (_el.Value.convidado == 1 && _el.Value.uid == _uid);
            //});

            //if (it.Key == m_player_info.Last().Key)
            //{
            //    throw new exception("[room::deleteInvited] [Error] PLAYER[UID=" + Convert.ToString(_uid) + "] tentou deletar convidado," + " mas nao tem o info do convidado na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
            //        2003, 0));
            //}



            //int index = findIndexSession(_uid);

            //if (index == (int)~0)
            //{


            //    throw new exception("[room::deleteInvited] [Error] session[UID=" + Convert.ToString(_uid) + "] nao existe no vector de sessions da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
            //        5, 0));
            //}

            //v_sessions.RemoveAt(index);

            //--m_ri.num_player;

            //m_player_info.Remove(it.Key);

            //// Update Position all Players
            //updatePosition();

            // Delete Invite Channel Info
            InviteChannelInfo ici = new InviteChannelInfo();

            //var itt = v_invite.FirstOrDefault(_el => _el.room_number == m_ri.numero && _el.invited_uid == _uid);


            //if (itt != null)
            //{

            //    ici = itt;

            //    v_invite.Remove(itt);

            //}
            //else
            //{
            //    _smp.message_pool.getInstance().push(new message("[room::deleteInvited][Warning] PLAYER[UID=" + Convert.ToString(_uid) + "] nao tem um convite.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            //}

            //// End Delete Invite Channel Info

            //// Resposta Delete Convidado
            //var p = new PangyaBinaryWriter((ushort)0x130);

            //p.WriteUInt32(_uid);

            //packet_func.room_broadcast(this,
            //    p, 1);

            //_smp.message_pool.getInstance().push(new message("[room::deleteInvited][Info] Deleteou um convite[Convidado=" + Convert.ToString(_uid) + "] na Sala[NUMERO=" + m_ri.numero + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            return ici;
        }

        public RoomInfoEx getInfo()
        {
            return m_ri;
        }

        public byte[] getBuild()
        {
            return m_ri.ToArray();
        }

        // Gets
        public sbyte getChannelOwenerId()
        {
            return m_channel_owner;
        }

        public sbyte getNumero()
        {
            return m_ri.numero;
        }

        public uint getMaster()
        {
            return (uint)m_ri.master;
        }

        public uint getNumPlayers()
        {
            return m_ri.num_player;
        }

        public byte getPosition(Player _session)
        {
            byte position = 255;

            for (byte i = 0; i < v_sessions.Count; ++i)
            {
                if (v_sessions[i] == _session)
                {
                    position = i;
                    break;
                }
            }
            return position;
        }

        public PlayerRoomInfoEx getPlayerInfo(Player _session)
        {

            if (_session == null)
            {
                throw new exception("Error _session is null. Em room::getPlayerInfo()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    7, 0));
            }

            PlayerRoomInfoEx pri = m_player_info.FirstOrDefault(c => c.Key == _session).Value;

            if (pri == null)
                return null;

            return pri;
        }

        public List<Player> getSessions(Player _session = null, bool _with_invited = true)
        {
            List<Player> v_session = new List<Player>();
            HashSet<uint> addedUids = new HashSet<uint>();  // evita duplicatas por UID

            lock (m_cs)
            {
                foreach (var el in v_sessions)
                {
                    if (el != null
                        && el.getState()
                        && el.m_pi.mi.sala_numero != -1
                        && (_session == null || _session != el)
                        && (_with_invited || !isInvited(el))
                        && addedUids.Add(el.m_pi.uid)) // só adiciona se UID ainda não estiver no HashSet
                    {
                        v_session.Add(el);
                    }
                }
            }
            return v_session;
        }


        public uint getRealNumPlayersWithoutInvited()
        {

            uint num = 0;



            try
            {

                num = _getRealNumPlayersWithoutInvited();

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::getRealNumPlayerWithoutInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }



            return (num);
        }

        public bool haveInvited()
        {

            bool question = false;
            try
            {
                question = _haveInvited();
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::haveInvited][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return question;
        }

        // Sets
        public void setNome(string _nome)
        {

            if (_nome.Length == 0)
            {
                throw new exception("Error _nome esta vazio. Em room::setNome()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    6, 0));
            }
            m_ri.nome = _nome;
        }

        public void setSenha(string _senha)
        {

            if (_senha.Length == 0)
            {
                if (!(m_ri.senha_flag == 1))
                {
                    m_ri.senha = "";
                    m_ri.senha_flag = 1;
                }
            }
            else
            {
                m_ri.senha = _senha;
                m_ri.senha_flag = 0;
            }
        }

        public void setTipo(byte _tipo)
        {

            if (_tipo == (byte)RoomInfo.TIPO.MATCH || _tipo == (byte)RoomInfo.TIPO.GUILD_BATTLE)
                init_teans();
            else if (_tipo != (byte)RoomInfo.TIPO.MATCH && m_ri.getTipo() == RoomInfo.TIPO.MATCH)
                clear_teans();

            m_ri.tipo = _tipo;

            // Atualizar tipo da sala 
                m_ri.tipo_show = m_ri.tipo;

             
                m_ri.tipo_ex = 255;

            m_ri.trofel = 0;
        }

        public void setCourse(byte _course)
        {
            m_ri.course = (RoomInfo.eCOURSE)_course;
        }

        public void setQntdHole(byte _qntd_hole)
        {
            m_ri.qntd_hole = _qntd_hole;
        }

        public void setModo(byte _modo)
        {
            m_ri.modo = _modo;
        }

        public void setTempoVS(uint _tempo)
        {
            m_ri.time_vs = _tempo;
        }

        public void setMaxPlayer(byte _max_Player)
        {

            if (v_sessions.Count > _max_Player)
            {
                throw new exception("[room::setMaxPlayer] [Error] MASTER[UID=" + Convert.ToString(m_ri.master) + "] _max_PLAYER[VALUE=" + Convert.ToString(_max_Player) + "] é menor que o numero de jogadores[VALUE=" + Convert.ToString(v_sessions.Count) + "] na sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    250, 0x588000));
            }

            // New Max Player room
            m_ri.max_player = _max_Player;
             
        }

        public void setTempo30S(uint _tempo)
        {
            m_ri.time_30s = _tempo;
        }

        public void setHoleRepeat(byte _hole_repeat)
        {
            m_ri.hole_repeat = _hole_repeat;
        }

        public void setFixedHole(uint _fixed_hole)
        {
            m_ri.fixed_hole = _fixed_hole;
        }

        public void setArtefato(uint _artefato)
        {
            m_ri.artefato = _artefato;
        }

        public void setNatural(uint _natural)
        {
            m_ri.natural.ulNaturalAndShortGame = _natural;
        }

        public void setState(byte _state)
        {
            m_ri.state = _state;
        }

        public void setFlag(byte _flag)
        {
            m_ri.flag = _flag;
        }

        public void setStateAFK(byte _state_afk)
        {
            m_ri.state_afk = _state_afk;
        }

        ///new methods! 
        public void SetAllReady()
        {
            byte ready = 0;
            foreach (var _el in v_sessions)
            {
                var pri = getPlayerInfo(_el);
                pri.state_flag.ready = (byte)(ready == 0 ? 1 : 0);//invertido
                if (pri.state_flag.ready == 1 && !pri.capability.game_master)
                {
                    updatePlayerInfo(_el);
                    var p = new PangyaBinaryWriter();
                    p.init_plain(0x78);
                    p.WriteInt32(pri.oid);
                    p.WriteByte(ready);
                    packet_func.room_broadcast(this, p, 1);
                }
            }
        }

        public bool IsStarted()
        {
            return m_pGame != null && m_pGame.m_game_init_state == 1;
        }

        public bool IsWithBot()
        {
            return m_bot_tourney;
        } 
         
        public void setMatchHole(uint _hole)
        {
            if (m_ri.state == 1 && (m_ri.getTipo() == RoomInfo.TIPO.MATCH))
            {
                m_ri.qntd_hole = (byte)_hole;
            }
        }

        public void setMatchMap(uint _map)
        {
            if (m_ri.state == 1 && (m_ri.getTipo() == RoomInfo.TIPO.MATCH))
            {
                m_ri.course = (RoomInfo.eCOURSE)_map;
            }
        }

        public bool CheckSecurityKey(byte[] senhaEncriptSala)
        {
            if (senhaEncriptSala == null || senhaEncriptSala.Length == 0)
                return false;

            byte[] storedKey = m_ri.key;

            if (storedKey == null || storedKey.Length != senhaEncriptSala.Length)
                return false;

            // Comparação segura (evita timing attack)
            bool equals = true;
            for (int i = 0; i < storedKey.Length; i++)
                equals &= storedKey[i] == senhaEncriptSala[i];

            return equals;
        }


        // Checks
        public bool checkPass(string _pass)
        {

            if (!isLocked())
            {
                throw new exception("[Room::checkPass] [Error] sala nao tem senha", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    1, 0));
            }

            return string.Compare(m_ri.senha, _pass) == 0;
        } 

        // States
        public bool isLocked()
        {
            return !(m_ri.senha_flag == 1);
        }

        public bool isFull()
        {
            m_ri.num_player = (byte)v_sessions.Count;
            return m_ri.num_player >= m_ri.max_player;
        }

        public bool isGaming()
        {
            if (m_pGame != null/* && (getInfo().getTipo() != RoomInfo.TIPO.STROKE || getInfo().getTipo() != RoomInfo.TIPO.MATCH)*/)
            {
                return true;
            }
            return false;
        }

        public bool isGamingBefore(uint _uid)
        {

            if (_uid == 0)
            {
                throw new exception("[room::isGamingBefore] [Error] _uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    1000, 0));
            }

            if (m_pGame == null)
            {
                throw new exception("[room::isGamingBefore] [Error] a sala[NUMERO=" + m_ri.numero + "] nao tem um jogo inicializado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    1001, 0));
            }

            return m_pGame.isGamingBefore(_uid);
        }

        public bool isKickedPlayer(uint _uid)
        {
            return m_player_kicked.Any(el => el.Key == _uid);
        }

        public virtual bool isAllReady()
        {

            var master = findMaster();

            if (master == null)
            {
                return false;
            }
             

            // Se o master for GM então não precisar todos está ready(prontos)
            if (master.m_pi.m_cap.game_master && !_haveInvited())
            {
                return true;
            }


            var count = v_sessions.Count(_el =>
            {
                var pri = getPlayerInfo(_el);
                return (pri != null && pri.state_flag.ready == 1);
            });

            // Conta com o master por que o master sempre está pronto(ready)
            return count == v_sessions.Count;
        }

        public void CreateRoomLogSql(Player _session)
        {
            try
            {
                /////erros aqui, descobrir o que é primerio!

                //if (m_room_log.roomId == Guid.Empty)
                //    generateRoomLogGuid();


                //var char_info_typeid = _session.m_pi.ei.char_info != null ? _session.m_pi.ei.char_info._typeid : 0;
                //var clubset_typeid = _session.m_pi.ei.clubset != null ? _session.m_pi.ei.clubset._typeid : 0;
                //var mascot_info_typeid = _session.m_pi.ei.mascot_info != null ? _session.m_pi.ei.mascot_info._typeid : 0;
                //var cad_info_typeid = _session.m_pi.ei.cad_info != null ? _session.m_pi.ei.cad_info._typeid : 0;

                //var m_log = m_room_log.UpdateInfo(_session.m_pi.uid, char_info_typeid, clubset_typeid, mascot_info_typeid, cad_info_typeid, m_ri, m_bot_tourney);

                //m_pGame.CreateRoomLogSql(m_log);
            }

            catch (Exception e)
            {
                _smp.message_pool.getInstance().push(new message("[Game.CreateRoomLogSql][ErrorSystem] Exceção capturada: " + (e.Message), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }


        void generateRoomLogGuid()
        {
            m_room_log.roomId = Guid.NewGuid();
        }

        // Updates
        public void updatePlayerInfo(Player _session)
        {
            PlayerRoomInfoEx pri = new PlayerRoomInfoEx();
            PlayerRoomInfoEx _pri = null;
            try
            {

                if ((_pri = getPlayerInfo(_session)) == null)
                {
                    throw new exception("[room::updatePlayerInfo] [Error] nao tem o PLAYER[UID=" + _session.m_pi.uid + "] info dessa session na sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        8, 0));
                }

                // Copia do que esta no map
                pri = _pri;

                // Player Room Info Update
                pri.oid = _session.m_oid;

                pri.position = (byte)(getPosition(_session) + 1); // posição na sala
                pri.capability = _session.m_pi.m_cap;
                pri.title = _session.m_pi.ue.m_title;

                if (_session.m_pi.ei.char_info != null)
                    pri.char_typeid = _session.m_pi.ei.char_info._typeid;


               // pri.skin[4] = 0; // Aqui tem que ser zero, se for outro valor não mostra a imagem do character equipado

                if (getMaster() == _session.m_pi.uid)
                {
                    pri.state_flag.master = 1;
                    pri.state_flag.ready = 1; // Sempre está pronto(ready) o master
                }
                else
                {

                    // Só troca o estado de pronto dele na sala, se anterior mente ele era Master da sala ou não estiver pronto
                    if (pri.state_flag.master == 1 || !(pri.state_flag.ready == 1))
                    {
                        pri.state_flag.ready = 0;
                    }

                    pri.state_flag.master = 0;
                }

                pri.state_flag.sexo = _session.m_pi.mi.sexo;
                 
                // Só faz calculo de Quita rate depois que o Player
                // estiver no level Beginner E e jogado 50 games
                if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
                {
                    float rate = _session.m_pi.ui.getQuitRate();

                    if (rate < GOOD_PLAYER_ICON)
                    {
                        pri.state_flag.azinha = 1;
                    } 
                }

                pri.level = _session.m_pi.mi.level;

                if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                    pri.icon_angel = _session.m_pi.ei.char_info.AngelEquiped();
                else
                    pri.icon_angel = 0;
                 
               
                if (_session.m_pi.ei.char_info != null)
                    pri.ci = _session.m_pi.ei.char_info;

                // Salva novamente
                m_player_info[_session] = pri;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        // Finds
        public Player findSessionByOID(uint _oid)
        {
            var i = v_sessions.FirstOrDefault(_el =>
                _el.m_oid == _oid);



            if (i != v_sessions.Last())
            {
                return i;
            }

            return null;
        }

        public Player findSessionByUID(uint _uid)
        {

            var i = v_sessions.FirstOrDefault(_el =>
                _el.m_pi.uid == _uid);



            if (i != null)
            {
                return i;
            }
            return null;
        }

        public Player findMaster()
        {
            Player master = null;

            lock (m_cs)
            {
                var pMaster = v_sessions.Find(_el => _el.m_pi.uid == m_ri.master);

                if (pMaster != null)
                {
                    master = pMaster;
                }
            }

            return master;
        }

        // Bot Tourney, Short Game and Special Shuffle Course
        public void makeBot(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "makeBot] [Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            var p = new PangyaBinaryWriter();

            try
            {

                // Bot Ticket TypeId
                // Add Bot Tourney Visual para a sala
                addBotVisual(_session);


                packet_func.session_send(packet_func.pacote02("SYSTEM", "[ \\2S2_SYS ] \\c0xff00ff00\\cBot was created.", eChatMsg.SPECIAL_NOTICE), _session);
                 
                // Premium User Não precisa de ticket não
                //if (_session.m_pi.m_cap.premium_user || _session.m_pi.m_cap.game_master)
                //{



                //}
                //else
                //{

                //    // Verifica se ele tem o ticket para criar o Bot se não manda mensagem dizenho que ele não tem ticket para criar o bot
                //    var pWi = _session.m_pi.findWarehouseItemByTypeid(TICKET_BOT_TYPEID) != null ? _session.m_pi.findWarehouseItemByTypeid(TICKET_BOT_TYPEID) : _session.m_pi.findWarehouseItemByTypeid(TICKET_BOT_TYPEID2);

                //    if (pWi == null)
                //    {

                //        // Não tem ticket bot suficiente, manda mensagem
                //        // SLast Message
                //        p.init_plain(0x40); // Msg to Chat of Player

                //        p.WriteByte(7); // Notice

                //        p.WritePStr("@SuperSS");
                //        p.WritePStr("\\c0xffff0000\\cYou do not have enough ticket to create the Bot.");

                //        packet_func.session_send(p,
                //            _session, 1);
                //    }
                //    else
                //    {

                //        // Add Bot Tourney Visual para a sala
                //        addBotVisual(_session);

                //        // SLast Message
                //        p.init_plain(0x40); // Msg to Chat of Player

                //        p.WriteByte(7); // Notice

                //        p.WritePStr("@SuperSS");
                //        p.WritePStr("[ \\2Premium ] \\c0xff00ff00\\cBot was created.");

                //        packet_func.session_send(p,
                //            _session, 1);
                //    }
                //}
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::makeBot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // SLast Message
                p.init_plain(0x40); // Msg to Chat of Player

                p.WriteByte(7); // Notice

                p.WritePStr("@SuperSS");
                p.WritePStr("\\c0xffff0000\\cError creating Bot.");

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        // Info Room
        public bool requestChangeInfoRoom(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestChangeInfoRoom] [Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestChangeInfoRoom] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            bool ret = false;

            try
            {

                byte num_info;
                short roomId;

                if (m_ri.master != _session.m_pi.uid)
                {
                    if (!_session.m_pi.m_cap.game_master)
                        throw new exception("[room::requestChangeInfoRoom] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o info da sala[NUMERO=" + m_ri.numero + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao pode trocar o info da sala sem ser master.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        11, 0));
                }

                roomId = _packet.ReadInt16();

                num_info = _packet.ReadByte();

                if (num_info <= 0)
                {
                    throw new exception("[room::requestChangeInfoRoom] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o info da sala[NUMERO=" + m_ri.numero + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nao tem nenhum info para trocar do buffer do cliente.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        8, 0));
                }
                _smp.message_pool.getInstance().push(new message("[Channel::requestChangeInfoRoom][Debug] Packet Hex: " + _packet.Log(), type_msg.CL_FILE_LOG_AND_CONSOLE));


                for (var i = 0; i < num_info; ++i)
                {
                    var type = (RoomInfo.INFO_CHANGE)_packet.ReadByte();

                    switch (type)
                    {
                        case RoomInfo.INFO_CHANGE.NAME:
                            {
                                var title = _packet.ReadPStr();
                                 
                                setNome(title);
                            }
                            break;
                        case RoomInfo.INFO_CHANGE.SENHA:
                            {
                                var pwd = _packet.ReadPStr();
                                 
                                setSenha(pwd);
                            }
                            break;
                        case RoomInfo.INFO_CHANGE.TIPO:
                            {
                                var T8 = _packet.ReadByte();
                                if (Enum.IsDefined(typeof(RoomInfo.TIPO), T8))
                                {
                                    setTipo(T8);
                                }
                                else
                                    ThrowHackException(_session, "falha ao setar o tipo: " + getInfo().getTipo());
                            }
                            break;
                        case RoomInfo.INFO_CHANGE.COURSE:
                            {
                                var T8 = _packet.ReadByte();
                                if (Enum.IsDefined(typeof(RoomInfo.eCOURSE), T8))
                                {
                                    setCourse(T8);
                                }
                                else
                                    ThrowHackException(_session, "falha ao setar o course: " + getInfo().getTipo());
                            }
                            break;
                        case RoomInfo.INFO_CHANGE.QNTD_HOLE:
                            setQntdHole(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.MODO:
                            {
                                var T8 = _packet.ReadByte();
                                if (Enum.IsDefined(typeof(RoomInfo.eMODO), T8))
                                {
                                    setModo(T8);
                                }
                                else
                                    ThrowHackException(_session, "falha ao setar o Modo: " + getInfo().getTipo());
                            }
                            break;
                        case RoomInfo.INFO_CHANGE.TEMPO_VS: // Passa em Segundos
                            {
                                var timevs = (uint)_packet.ReadUInt16();
                                if (timevs > 0)
                                    setTempoVS(timevs * 1000);
                                else
                                    ThrowHackException(_session, "falha ao setar o tempo vs: " + getInfo().getTipo());
                            }
                            break;
                        case RoomInfo.INFO_CHANGE.MAX_PLAYER:
                            {
                                var T8 = _packet.ReadByte();
                                if (!(T8 <= this.v_sessions.Count))
                                    setMaxPlayer(T8);
                                else
                                    ThrowHackException(_session, "falha ao setar o numero de maximo de players: " + getInfo().getTipo());
                            }
                            break;
                        case RoomInfo.INFO_CHANGE.TEMPO_30S: // Passa em Minutos
                            {
                                var time30s = (uint)_packet.ReadByte();
                                if (time30s > 0)
                                    setTempo30S(time30s * 60000);
                                else
                                    ThrowHackException(_session, "falha ao setar o tempo minutos: " + getInfo().getTipo());
                            }
                            break;
                        case RoomInfo.INFO_CHANGE.STATE_FLAG:
                            setStateAFK(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.GALLERY_LIMIT:
                            m_ri.gallery_max_list = _packet.ReadByte();
                            break;
                        case RoomInfo.INFO_CHANGE.HOLE_REPEAT:
                            setHoleRepeat(_packet.ReadByte());
                            break;
                        case RoomInfo.INFO_CHANGE.FIXED_HOLE:
                            setFixedHole(_packet.ReadUInt32());
                            break;
                        case RoomInfo.INFO_CHANGE.ARTEFATO:
                            setArtefato(_packet.ReadUInt32());
                            break;
                        case RoomInfo.INFO_CHANGE.NATURAL:
                            {
                                var value = _packet.ReadUInt32();
                                var natural = new NaturalAndShortGame(value);

                                if (sgs.gs.getInstance().getInfo().propriedade.natural) // Natural não deixa desabilitar o Natural da sala, por que o server é natural
                                {
                                    natural.natural = 1;
                                }

                                setNatural(natural.ulNaturalAndShortGame);

                                break;
                            }
                        default:
                            throw new exception("[room::requestChangeInfoRoom] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar info da sala[NUMERO=" + m_ri.numero + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas info change é desconhecido.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                                9, 0));
                    }
                }
                var _channel = _session.m_channel;
                //check room
                //_channel.FilterRoom(_session, m_ri);
                // send to clients update room info
                SendUpdate();

                ret = true; // Trocou o info da sala com sucesso

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChangeInfoRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta para o cliente 
                packet_func.session_send(packet_func.pacote0E(
                    m_ri, 25),
                    _session, 1);
            }

            return ret;
        }

        private void SendUpdate()
        {
            var p = packet_func.pacote0E(m_ri, -1/*valor constante*/);
            packet_func.room_broadcast(this, p, 0);
        }

        public void sendCharacter(Player _session, int _option)
        {

            int option = !(m_ri.getTipo() == RoomInfo.TIPO.STROKE ||
                           m_ri.getTipo() == RoomInfo.TIPO.MATCH || 
                           m_ri.getTipo() == RoomInfo.TIPO.PANG_BATTLE) ? 0x100 : 0;

            option += _option;
             

            List<PlayerRoomInfoEx> v_element = new List<PlayerRoomInfoEx>();
            PlayerRoomInfoEx pri = null;

            try
            {
                lock (m_cs)
                {

                    foreach (var sess in v_sessions)
                    {
                        pri = getPlayerInfo(sess);
                        if (pri != null)
                            v_element.Add(pri);
                    }

                    pri = getPlayerInfo(_session);

                    if (pri == null && _option != 2)
                        return;

                    var p = new PangyaBinaryWriter();

                    if (packet_func.pacote0A(ref p, _session, ((_option == 1 || _option == 4 || _option == 0x103) ? new List<PlayerRoomInfoEx>() { pri } : v_element), option))
                        packet_func.room_broadcast(this, p, 1);
                }
            }
            catch (exception e)
            {
                // O equivalente ao UNREFERENCED_PARAMETER é simplesmente ignorar a variável.
                throw e;
            }
        }
         
        public void sendWeatherLounge(Player _session)
        {
             
        }
        // Chat Team
        public void requestChatTeam(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestChatTeam] [Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestChatTeam] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            var p = new PangyaBinaryWriter();

            try
            {

                var msg = _packet.ReadPStr();

                // Verifica a mensagem com palavras proibida e manda para o log e bloquea o chat dele
                _smp.message_pool.getInstance().push(new message("[room::requestChatTeam][Info] PLAYER[UID=" + _session.m_pi.uid + ", MESSAGE=" + msg + "]", type_msg.CL_ONLY_FILE_LOG));

                if (msg.empty())
                {
                    throw new exception("[room::requestChatTeam] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + m_ri.numero + "], mas a msg esta vazia. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2000, 0));
                }

                if (m_ri.tipo != (byte)RoomInfo.TIPO.MATCH && m_ri.tipo != (byte)RoomInfo.TIPO.GUILD_BATTLE)
                {
                    throw new exception("[room::requestChatTeam] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + m_ri.numero + "], mas a sala nao é MATCH ou GUILD_BATTLE. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2001, 0));
                }

                if (m_teans.empty())
                {
                    throw new exception("[room::requestChatTeam] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum team. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2002, 0));
                }

                var pri = getPlayerInfo(_session);

                if (pri == null)
                {
                    throw new exception("[room::requetChatTeam] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem o info dele. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2003, 0));
                }

                //var team = m_teans[pri.state_flag.team];

                //if (team.findPlayerByUID(_session.m_pi.uid) == null)
                //{
                //    throw new exception("[room::requestChatTeam] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou mandar messsage[MSG=" + msg + "] no chat do team na sala[NUMERO=" + m_ri.numero + "], mas ele nao esta no team que a type de team dele diz. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                //        2004, 0));
                //}

                // LOG GM
                // Envia para todo os GM do server essa message
                var c = sgs.gs.getInstance().findChannel(_session.m_pi.channel);

                //if (c != null)
                //{

                //    var gm = sgs.gs.getInstance().FindAllGM();

                //    if (!gm.empty())
                //    {

                //        string msg_gm = "\\5" + (_session.m_pi.nickname) + ": '" + msg + "'";
                //        string from = "\\1[Channel=" + (c.getInfo().name) + ", \\1ROOM=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "][Team" + (!(pri.state_flag.team == 1) ? "R" : "B") + "]";

                //        var index = from.IndexOf(' ');

                //        if (index != -1)
                //        {
                //            from = from.Remove(index, 1).Insert(index, " \\1");
                //        }

                //        foreach (Player el in gm)
                //        {
                //            if (((el.m_gi.channel && el.m_pi.channel == c.getInfo().id) || el.m_gi.whisper || el.m_gi.isOpenPlayerWhisper(_session.m_pi.uid)) && (el.m_pi.channel != _session.m_pi.channel || el.m_pi.mi.sala_numero != _session.m_pi.mi.sala_numero || team.findPlayerByUID(el.m_pi.uid) == null))
                //            {

                //                // Responde no chat do Player
                //                p.init_plain(0x40);

                //                p.WriteByte(0);

                //                p.WritePStr(from); // Nickname

                //                p.WritePStr(msg_gm); // Message

                //                packet_func.session_send(p,
                //                    el, 1);
                //            }
                //        }
                //    }
                //}
                //else
                //{

                //}
                //{
                //    _smp.message_pool.getInstance().push(new message("[room::requestChatTeam][Warning] Log GM nao encontrou o Channel[ID=" + Convert.ToString((ushort)_session.m_pi.channel) + "] no server. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                //}

                // Manda message para o team da sala
                p.init_plain(0xB0);

                p.WritePStr(_session.m_pi.nickname);
                p.WritePStr(msg);

                //foreach (var el in team.getPlayers())
                //{
                //    packet_func.session_send(p, _session);
                //}

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChatTeam][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Change Item Equiped of Player
        public virtual void requestChangePlayerItemRoom(Player _session, ChangePlayerItemRoom _cpir)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "ChangePlayerItemRoom] [Error] Player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            var p = new PangyaBinaryWriter();
            var result = new byte[10];
            try
            {

                int error = 0/*SUCCESS*/;
                switch (_cpir.type)
                {
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CADDIE:
                        {
                            CaddieInfoEx pCi = null;

                            // Caddie
                            if (_cpir.caddie != 0 && (pCi = _session.m_pi.findCaddieById(_cpir.caddie)) != null
                                    && sIff.getInstance().getItemGroupIdentify(pCi._typeid) == sIff.getInstance().CADDIE)
                            {

                                // Check if item is in map of update item
                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(pCi._typeid, pCi.id);

                                if (!v_it.empty())
                                {

                                    foreach (var el in v_it)
                                    {

                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE)
                                        {

                                            // Desequipa o caddie
                                            _session.m_pi.ei.cad_info = null;
                                            _session.m_pi.ue.caddie_id = 0;

                                            _cpir.caddie = 0;

                                        }
                                        else if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
                                        {

                                            // Limpa o caddie Parts
                                            pCi.parts_typeid = 0u;
                                            pCi.parts_end_date_unix = 0;
                                            pCi.end_parts_date = new SYSTEMTIME();

                                            _session.m_pi.ei.cad_info = pCi;
                                            _session.m_pi.ue.caddie_id = _cpir.caddie;
                                        }

                                        // Tira esse Update Item do map
                                        _session.m_pi.mp_ui.Remove(el.Key);
                                    }

                                }
                                else
                                {

                                    // Caddie is Good, Update caddie equiped ON SERVER AND DB
                                    _session.m_pi.ei.cad_info = pCi;
                                    _session.m_pi.ue.caddie_id = _cpir.caddie;

                                    // Verifica se o Caddie pode ser equipado
                                    if (_session.checkCaddieEquiped(_session.m_pi.ue))
                                        _cpir.caddie = _session.m_pi.ue.caddie_id;

                                }

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateCaddieEquiped(_session.m_pi.uid, _cpir.caddie), SQLDBResponse, this);

                            }
                            else if (_session.m_pi.ue.caddie_id > 0 && _session.m_pi.ei.cad_info != null)
                            {   // Desequipa Caddie

                                error = (_cpir.caddie == 0) ? 1/*client give invalid item id*/ : (pCi == null ? 2/*Item Not Found*/ : 3/*Erro item typeid invalid*/);

                                if (error > 1)
                                {
                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar o Caddie[ID=" + (_cpir.caddie) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], desequipando o caddie. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }

                                // Check if item is in map of update item
                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(_session.m_pi.ei.cad_info._typeid, _session.m_pi.ei.cad_info.id);

                                if (!v_it.empty())
                                {

                                    foreach (var el in v_it)
                                    {

                                        // Caddie já vai se desequipar, só verifica o parts
                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
                                        {

                                            // Limpa o caddie Parts
                                            _session.m_pi.ei.cad_info.parts_typeid = 0u;
                                            _session.m_pi.ei.cad_info.parts_end_date_unix = 0;
                                            _session.m_pi.ei.cad_info.end_parts_date = new SYSTEMTIME();
                                        }

                                        // Tira esse Update Item do map
                                        _session.m_pi.mp_ui.Remove(el.Key);
                                    }

                                }

                                _session.m_pi.ei.cad_info = null;
                                _session.m_pi.ue.caddie_id = 0;

                                _cpir.caddie = 0;

                                // Zera o Error para o cliente desequipar o caddie que o server desequipou
                                error = 0;

                                // Att No DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateCaddieEquiped(_session.m_pi.uid, _cpir.caddie), SQLDBResponse, this);
                            }


                            //packet_func.room_broadcast(this, packet_func.pacote04B(_session, (byte)_cpir.type, error));
                        }
                        break;
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_BALL:
                        {
                            WarehouseItemEx pWi = null;

                            if (_cpir.ball != 0 && (pWi = _session.m_pi.findWarehouseItemByTypeid(_cpir.ball)) != null
                                    && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().BALL)
                            {

                                _session.m_pi.ei.comet = pWi;
                                _session.m_pi.ue.ball_typeid = _cpir.ball;      // Ball(Comet) é o typeid que o cliente passa

                                // Verifica se a bola pode ser equipada
                                if (_session.checkBallEquiped(_session.m_pi.ue))
                                    _cpir.ball = _session.m_pi.ue.ball_typeid;

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball), SQLDBResponse, this);

                            }
                            else if (_cpir.ball == 0)
                            { // Bola 0 coloca a bola padrão para ele, se for premium user coloca a bola de premium user

                                // Zera para equipar a bola padrão
                                _session.m_pi.ei.comet = null;
                                _session.m_pi.ue.ball_typeid = 0;

                                // Verifica se a Bola pode ser equipada (Coloca para equipar a bola padrão
                                if (_session.checkBallEquiped(_session.m_pi.ue))
                                    _cpir.ball = _session.m_pi.ue.ball_typeid;

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball), SQLDBResponse, this);

                            }
                            else
                            {

                                error = (pWi == null ? 2 : 3);

                                pWi = _session.m_pi.findWarehouseItemByTypeid(DEFAULT_COMET_TYPEID);

                                if (pWi != null)
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar a Ball[TYPEID=" + (_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], colocando a Ball Padrao do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    _session.m_pi.ei.comet = pWi;
                                    _cpir.ball = _session.m_pi.ue.ball_typeid = pWi._typeid;

                                    // Zera o Error para o cliente equipar a Ball Padrão que o server equipou
                                    error = 0;

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball), SQLDBResponse, this);

                                }
                                else
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar a Ball[TYPEID=" + (_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], ele nao tem a Ball Padrao, adiciona a Ball pardrao para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    BuyItem bi = new BuyItem();
                                    stItem item = new stItem();

                                    bi.id = -1;
                                    bi._typeid = DEFAULT_COMET_TYPEID;
                                    bi.qntd = 1;

                                    ItemManager.initItemFromBuyItem(_session.m_pi, item, bi, false, 0, 0, 1);

                                    if (item._typeid != 0)
                                    {

                                        if ((_cpir.ball = (uint)ItemManager.addItem(item, _session, 2/*Padrão Item*/, 0)) != Convert.ToUInt32(ItemManager.RetAddItem.T_ERROR))
                                        {

                                            // Equipa a Ball padrao
                                            pWi = _session.m_pi.findWarehouseItemById((int)_cpir.ball);

                                            if (pWi != null)
                                            {

                                                _session.m_pi.ei.comet = pWi;
                                                _session.m_pi.ue.ball_typeid = pWi._typeid;

                                                // Zera o Error para o cliente equipar a Ball Padrão que o server equipou
                                                error = 0;

                                                // Update ON DB
                                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball), SQLDBResponse, this);

                                                // Update ON GAME
                                                //p.init_plain(0x216);

                                                //p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                //p.WriteUInt32(1);   // Count

                                                //p.WriteByte(item.type);
                                                //p.WriteUInt32(item._typeid);
                                                //p.WriteInt32(item.id);
                                                //p.WriteUInt32(item.flag_time);
                                                //p.WriteBytes(item.stat.ToArray());
                                                //p.WriteInt32((item.STDA_C_ITEM_TIME > 0) ? item.STDA_C_ITEM_TIME32 : item.STDA_C_ITEM_QNTD32);
                                                //p.WriteZero(25);

                                                //packet_func.session_send(p, _session, 1);

                                            }
                                            else
                                                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                        + "] nao conseguiu achar a Ball[ID="
                                                        + (item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        }
                                        else
                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                    + "] nao conseguiu adicionar a Ball[TYPEID="
                                                    + (item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    }
                                    else
                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                + "] nao conseguiu inicializar a Ball[TYPEID="
                                                + (bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }
                            }
                            //p = packet_func.pacote04B(_session,
                            //   (byte)_cpir.type, error);
                            //packet_func.room_broadcast(this,
                            //    p, 1);
                        }
                        break;
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CLUBSET: // ClubSet
                        {
                            WarehouseItemEx pWi = null;

                            // ClubSet
                            if (_cpir.clubset != 0
                                && (pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset)) != null
                                && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().CLUBSET)
                            {

                                var c_it = _session.m_pi.findUpdateItemByTypeidAndType((uint)_cpir.clubset, UpdateItem.UI_TYPE.WAREHOUSE);

                                if (c_it.FirstOrDefault().Key == _session.m_pi.mp_ui.LastOrDefault().Key)
                                {

                                    _session.m_pi.ei.clubset = pWi;

                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                    if (cs != null)
                                    {

                                        for (var j = 0u; j < (_session.m_pi.ei.csi.enchant_c.Length); ++j)
                                        {
                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                        }

                                        _session.m_pi.ue.clubset_id = _cpir.clubset;

                                        // Verifica se o ClubSet pode ser equipado
                                        if (_session.checkClubSetEquiped(_session.m_pi.ue))
                                        {
                                            _cpir.clubset = _session.m_pi.ue.clubset_id;
                                        }

                                        // Update ON DB
                                        snmdb.NormalManagerDB.getInstance().add(0,
                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                            Room.SQLDBResponse, this);

                                    }
                                    else
                                    {

                                        error = 5;

                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou Atualizar Clubset[TYPEID=" + Convert.ToString(pWi._typeid) + ", ID=" + Convert.ToString(pWi.id) + "] equipado, mas ClubSet Not exists on IFF structure. Equipa o ClubSet padrao. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
                                        pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                        if (pWi != null)
                                        {

                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                            // que no original fica no warehouse msm, eu só confundi quando fiz
                                            _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                            cs = sIff.getInstance().findClubSet(pWi._typeid);

                                            if (cs != null)
                                            {
                                                for (var j = 0u; j < (_session.m_pi.ei.csi.enchant_c.Length); ++j)
                                                {
                                                    _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                }
                                            }

                                            _session.m_pi.ei.clubset = pWi;
                                            _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                            // Zera o Error para o cliente equipar a "CV1" que o server equipou
                                            error = 0;

                                            // Update ON DB
                                            snmdb.NormalManagerDB.getInstance().add(0,
                                                new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                Room.SQLDBResponse, this);

                                        }
                                        else
                                        {

                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            BuyItem bi = new BuyItem();
                                            stItem item = new stItem();

                                            bi.id = -1;
                                            bi._typeid = AIR_KNIGHT_SET;
                                            bi.qntd = 1;

                                            ItemManager.initItemFromBuyItem(_session.m_pi,
                                                item, bi, false, 0, 0, 1);

                                            if (item._typeid != 0)
                                            {
                                                _cpir.clubset = ItemManager.addItem(item,
                                                    _session, 2, 0);
                                                if (_cpir.clubset != ItemManager.RetAddItem.T_ERROR)
                                                {

                                                    // Equipa o ClubSet CV1
                                                    pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                                    if (pWi != null)
                                                    {

                                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                                        // que no original fica no warehouse msm, eu só confundi quando fiz
                                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                        cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                        if (cs != null)
                                                        {
                                                            for (var j = 0u; j < (_session.m_pi.ei.csi.enchant_c.Length); ++j)
                                                            {
                                                                _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                            }
                                                        }

                                                        _session.m_pi.ei.clubset = pWi;
                                                        _session.m_pi.ue.clubset_id = pWi.id;

                                                        // Zera o Error para o cliente equipar a "CV1" que o server equipou
                                                        error = 0;

                                                        // Update ON DB
                                                        snmdb.NormalManagerDB.getInstance().add(0,
                                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                            Room.SQLDBResponse, this);

                                                        // Update ON GAME
                                                        //p.init_plain(0x216);

                                                        //p.WriteUInt32((uint32_t)UtilTime.GetSystemTimeAsUnix());
                                                        //p.WriteUInt32(1); // Count

                                                        //p.WriteByte(item.type);
                                                        //p.WriteUInt32(item._typeid);
                                                        //p.WriteInt32(item.id);
                                                        //p.WriteUInt32(item.flag_time);
                                                        //p.WriteBytes(item.stat.ToArray());
                                                        //p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                                        //p.WriteZeroByte(25);

                                                        //packet_func.session_send(p,
                                                        //    _session, 1);

                                                    }
                                                    else
                                                    {
                                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                    }

                                                }
                                                else
                                                {
                                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                }

                                            }
                                            else
                                            {
                                                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                            }
                                        }
                                    }

                                }
                                else
                                { // ClubSet Acabou o tempo

                                    error = 6; // Acabou o tempo do item

                                    // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
                                    pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                    if (pWi != null)
                                    {

                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                        // que no original fica no warehouse msm, eu só confundi quando fiz
                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                        var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                        if (cs != null)
                                        {
                                            for (var j = 0u; j < (_session.m_pi.ei.csi.enchant_c.Length); ++j)
                                            {
                                                _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                            }
                                        }

                                        _session.m_pi.ei.clubset = pWi;
                                        _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                        // Zera o Error para o cliente equipar a "CV1" que o server equipou
                                        error = 0;

                                        // Update ON DB
                                        snmdb.NormalManagerDB.getInstance().add(0,
                                            new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                            Room.SQLDBResponse, this);

                                    }
                                    else
                                    {

                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        BuyItem bi = new BuyItem();
                                        stItem item = new stItem();

                                        bi.id = -1;
                                        bi._typeid = AIR_KNIGHT_SET;
                                        bi.qntd = 1;

                                        ItemManager.initItemFromBuyItem(_session.m_pi,
                                          item, bi, false, 0, 0, 1);

                                        if (item._typeid != 0)
                                        {

                                            if ((_cpir.clubset = (int)ItemManager.addItem(item,
                                                        _session, 2, 0)) != Convert.ToUInt32(ItemManager.RetAddItem.T_ERROR))
                                            {

                                                // Equipa o ClubSet CV1
                                                pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                                if (pWi != null)
                                                {

                                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                    if (cs != null)
                                                    {
                                                        for (var j = 0u; j < (_session.m_pi.ei.csi.enchant_c.Length); ++j)
                                                        {
                                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                        }
                                                    }
                                                    _session.m_pi.ei.clubset = pWi;
                                                    _session.m_pi.ue.clubset_id = pWi.id;

                                                    // Zera o Error para o cliente equipar a "CV1" que o server equipou
                                                    error = 0;

                                                    // Update ON DB
                                                    snmdb.NormalManagerDB.getInstance().add(0,
                                                        new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                        Room.SQLDBResponse, this);

                                                    // Update ON GAME
                                                    //p.init_plain(0x216);

                                                    //p.WriteUInt32((uint32_t)UtilTime.GetSystemTimeAsUnix());
                                                    //p.WriteUInt32(1); // Count

                                                    //p.WriteByte(item.type);
                                                    //p.WriteUInt32(item._typeid);
                                                    //p.WriteInt32(item.id);
                                                    //p.WriteUInt32(item.flag_time);
                                                    //p.WriteBytes(item.stat.ToArray());
                                                    //p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                                    //p.WriteZeroByte(25);

                                                    //packet_func.session_send(p,
                                                    //    _session, 1);

                                                }
                                                else
                                                {
                                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                                }

                                            }
                                            else
                                            {
                                                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                            }

                                        }
                                        else
                                        {
                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                        }
                                    }
                                }

                            }
                            else
                            {

                                error = (_cpir.clubset == 0) ? 1 : (pWi == null ? 2 : 3);

                                pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                if (pWi != null)
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando o ClubSet Padrao\"CV1\" do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                    if (cs != null)
                                    {
                                        for (var j = 0u; j < (_session.m_pi.ei.csi.enchant_c.Length); ++j)
                                        {
                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                        }
                                    }

                                    _session.m_pi.ei.clubset = pWi;
                                    _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                    // Zera o Error para o cliente equipar a "CV1" que o server equipou
                                    error = 0;

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0,
                                        new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                        Room.SQLDBResponse, this);

                                }
                                else
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o ClubSet[ID=" + Convert.ToString(_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    BuyItem bi = new BuyItem();
                                    stItem item = new stItem();

                                    bi.id = -1;
                                    bi._typeid = AIR_KNIGHT_SET;
                                    bi.qntd = 1;

                                    ItemManager.initItemFromBuyItem(_session.m_pi,
                                       item, bi, false, 0, 0, 1);

                                    if (item._typeid != 0)
                                    {

                                        if ((_cpir.clubset = (int)ItemManager.addItem(item,
                                                         _session, 2, 0)) != Convert.ToUInt32(ItemManager.RetAddItem.T_ERROR))
                                        {

                                            // Equipa o ClubSet CV1
                                            pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                            if (pWi != null)
                                            {

                                                // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant,
                                                // que no original fica no warehouse msm, eu só confundi quando fiz
                                                _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                if (cs != null)
                                                {
                                                    for (var j = 0u; j < (_session.m_pi.ei.csi.enchant_c.Length); ++j)
                                                    {
                                                        _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                                    }
                                                }

                                                _session.m_pi.ei.clubset = pWi;
                                                _session.m_pi.ue.clubset_id = pWi.id;

                                                // Zera o Error para o cliente equipar a "CV1" que o server equipou
                                                error = 0;

                                                // Update ON DB
                                                snmdb.NormalManagerDB.getInstance().add(0,
                                                    new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset),
                                                    Room.SQLDBResponse, this);

                                                //// Update ON GAME
                                                //p.init_plain(0x216);

                                                //p.WriteUInt32((uint32_t)UtilTime.GetSystemTimeAsUnix());
                                                //p.WriteUInt32(1); // Count

                                                //p.WriteByte(item.type);
                                                //p.WriteUInt32(item._typeid);
                                                //p.WriteInt32(item.id);
                                                //p.WriteUInt32(item.flag_time);
                                                //p.WriteBytes(item.stat.ToArray());
                                                //p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                                //p.WriteZeroByte(25);

                                                //packet_func.session_send(p,
                                                //    _session, 1);

                                            }
                                            else
                                            {
                                                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu achar o ClubSet\"CV1\"[ID=" + Convert.ToString(item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                            }

                                        }
                                        else
                                        {
                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu adicionar o ClubSet[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                        }

                                    }
                                    else
                                    {
                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu inicializar o ClubSet[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                    }
                                }
                            }

                            //p = packet_func.pacote04B(_session,
                            //   (byte)_cpir.type, error);
                            //packet_func.room_broadcast(this,
                            //    p, 1);
                            break;
                        }
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_CHARACTER:
                        {
                            CharacterInfoEx pCe = null;

                            if (_cpir.character != 0
                                && (pCe = _session.m_pi.findCharacterById(_cpir.character)) != null
                                && sIff.getInstance().getItemGroupIdentify(pCe._typeid) == sIff.getInstance().CHARACTER)
                            {

                                _session.m_pi.ei.char_info = pCe;
                                _session.m_pi.ue.character_id = _cpir.character;

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0,
                                    new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)_cpir.character),
                                    Room.SQLDBResponse, this);

                                // Update Player Info Channel and Room
                                updatePlayerInfo(_session); 

                            }
                            else
                            {

                                error = (_cpir.character == 0) ? 1 : (pCe == null ? 2 : 3);

                                if (_session.m_pi.mp_ce.Count() > 0)
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o Character[ID=" + Convert.ToString(_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], colocando o primeiro character do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    _session.m_pi.ei.char_info = _session.m_pi.mp_ce.FirstOrDefault().Value;
                                    _cpir.character = _session.m_pi.ue.character_id = _session.m_pi.ei.char_info.id;

                                    // Zera o Error para o cliente equipar o Primeiro Character do map de character do player, que o server equipou
                                    error = 0;

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0,
                                        new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)_cpir.character),
                                        Room.SQLDBResponse, this);

                                    // Update Player Info Channel and Room
                                    updatePlayerInfo(_session);

                                    PlayerRoomInfoEx pri = getPlayerInfo(_session); 

                                }
                                else
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o Character[ID=" + Convert.ToString(_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE=" + Convert.ToString(error) + "], ele nao tem nenhum character, adiciona o Nuri para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    BuyItem bi = new BuyItem();
                                    stItem item = new stItem();

                                    bi.id = -1;
                                    bi._typeid = (uint)(sIff.getInstance().CHARACTER << 26); // Nuri
                                    bi.qntd = 1;

                                    ItemManager.initItemFromBuyItem(_session.m_pi,
                                       item, bi, false, 0, 0, 1);

                                    if (item._typeid != 0)
                                    {
                                        _cpir.character = (int)ItemManager.addItem(item,
                                            _session, 2, 0);
                                        // Add Item já atualiza o Character equipado
                                        if ((int)_cpir.character != (int)ItemManager.RetAddItem.T_ERROR)
                                        {

                                            // Zera o Error para o cliente equipar o Nuri que o server equipou
                                            error = 0;

                                            //// Update ON GAME
                                            //p.init_plain(0x216);

                                            //p.WriteUInt32((uint32_t)UtilTime.GetSystemTimeAsUnix());
                                            //p.WriteUInt32(1); // Count

                                            //p.WriteByte(item.type);
                                            //p.WriteUInt32(item._typeid);
                                            //p.WriteInt32(item.id);
                                            //p.WriteUInt32(item.flag_time);
                                            //p.WriteBytes(item.stat.ToArray());
                                            //p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                            //p.WriteZeroByte(25);

                                            //packet_func.session_send(p,
                                            //    _session, 1);

                                            // Update Player Info Channel and Room
                                            updatePlayerInfo(_session);

                                            PlayerRoomInfoEx pri = getPlayerInfo(_session); 

                                        }
                                        else
                                        {
                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu adicionar o Character[TYPEID=" + Convert.ToString(item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                        }

                                    }
                                    else
                                    {
                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu inicializar o Character[TYPEID=" + Convert.ToString(bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                    }
                                }
                            }
                            //p = packet_func.pacote04B(
                            //_session, (byte)_cpir.type, error);
                            //packet_func.room_broadcast(this, p);

                            break;
                        } 
                    case ChangePlayerItemRoom.TYPE_CHANGE.TC_ALL://envia isso quando é pra inicia a sala
                        {

                            // Aqui se não tiver os itens, algum hacker, gera Log, e coloca item padrão ou nenhum
                            CharacterInfoEx pCe = null;
                            CaddieInfoEx pCi = null;
                            WarehouseItemEx pWi = null;
                            error = 0;

                            // Character
                            if (_cpir.character != 0 && (pCe = _session.m_pi.findCharacterById(_cpir.character)) != null
                                    && sIff.getInstance().getItemGroupIdentify(pCe._typeid) == sIff.getInstance().CHARACTER)
                            {

                                _session.m_pi.ei.char_info = pCe;
                                _session.m_pi.ue.character_id = _cpir.character;

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)_cpir.character), SQLDBResponse, this);

                            }
                            else
                            {

                                error = (_cpir.character == 0) ? 1/*client give invalid item id*/ : (pCe == null ? 2/*Item Not Found*/ : 3/*Erro item typeid invalid*/);

                                if (_session.m_pi.mp_ce.Count() > 0)
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar o Character[ID=" + (_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], colocando o primeiro character do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    _session.m_pi.ei.char_info = _session.m_pi.mp_ce.First().Value;
                                    _cpir.character = _session.m_pi.ue.character_id = _session.m_pi.ei.char_info.id;

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)_cpir.character), SQLDBResponse, this);

                                }
                                else
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar o Character[ID=" + (_cpir.character) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], ele nao tem nenhum character, adiciona o Nuri para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    BuyItem bi = new BuyItem();
                                    stItem item = new stItem();

                                    bi.id = -1;
                                    bi._typeid = (uint)(sIff.getInstance().CHARACTER << 26);    // Nuri
                                    bi.qntd = 1;

                                    ItemManager.initItemFromBuyItem(_session.m_pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                    if (item._typeid != 0)
                                    {

                                        // Add Item já atualiza o Character equipado
                                        if ((_cpir.character = (int)ItemManager.addItem(item, _session, 2/*Padrão Item*/, 0)) != Convert.ToUInt32(ItemManager.RetAddItem.T_ERROR))
                                        {

                                            //// Update ON GAME
                                            //p.init_plain(0x216);

                                            //p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                            //p.WriteUInt32(1);   // Count

                                            //p.WriteByte(item.type);
                                            //p.WriteUInt32(item._typeid);
                                            //p.WriteInt32(item.id);
                                            //p.WriteUInt32(item.flag_time);
                                            //p.WriteBytes(item.stat.ToArray());
                                            //p.WriteInt32((item.STDA_C_ITEM_TIME > 0) ? item.STDA_C_ITEM_TIME32 : item.STDA_C_ITEM_QNTD32);
                                            //p.WriteZeroByte(25);

                                            //packet_func.session_send(p, _session, 1);

                                        }
                                        else
                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                    + "] nao conseguiu adicionar o Character[TYPEID="
                                                    + (item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    }
                                    else
                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                + "] nao conseguiu inicializar o Character[TYPEID="
                                                + (bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }
                            }

                            // Caddie
                            if (_cpir.caddie != 0 && (pCi = _session.m_pi.findCaddieById(_cpir.caddie)) != null
                                    && sIff.getInstance().getItemGroupIdentify(pCi._typeid) == sIff.getInstance().CADDIE)
                            {

                                // Check if item is in map of update item
                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(pCi._typeid, pCi.id);

                                if (!v_it.empty())
                                {

                                    foreach (var el in v_it)
                                    {

                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE)
                                        {

                                            // Desequipa o caddie
                                            _session.m_pi.ei.cad_info = null;
                                            _session.m_pi.ue.caddie_id = 0;

                                            _cpir.caddie = 0;

                                        }
                                        else if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
                                        {

                                            // Limpa o caddie Parts
                                            pCi.parts_typeid = 0u;
                                            pCi.parts_end_date_unix = 0;
                                            pCi.end_parts_date = new SYSTEMTIME();

                                            _session.m_pi.ei.cad_info = pCi;
                                            _session.m_pi.ue.caddie_id = _cpir.caddie;
                                        }

                                        // Tira esse Update Item do map
                                        _session.m_pi.mp_ui.Remove(el.Key);
                                    }

                                }
                                else
                                {

                                    // Caddie is Good, Update caddie equiped ON SERVER AND DB
                                    _session.m_pi.ei.cad_info = pCi;
                                    _session.m_pi.ue.caddie_id = _cpir.caddie;

                                    // Verifica se o Caddie pode equipar
                                    if (_session.checkCaddieEquiped(_session.m_pi.ue))
                                        _cpir.caddie = _session.m_pi.ue.caddie_id;

                                }

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)_cpir.caddie), SQLDBResponse, this);

                            }
                            else if (_session.m_pi.ue.caddie_id > 0 && _session.m_pi.ei.cad_info != null)
                            {   // Desequipa Caddie

                                error = (_cpir.caddie == 0) ? 1/*client give invalid item id*/ : (pCi == null ? 2/*Item Not Found*/ : 3/*Erro item typeid invalid*/);

                                if (error > 1)
                                {
                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar o Caddie[ID=" + (_cpir.caddie)
                                            + "] para comecar o jogo, mas deu Error[VALUE=" + (error) + "], desequipando o caddie. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }

                                // Check if item is in map of update item
                                var v_it = _session.m_pi.findUpdateItemByTypeidAndId(_session.m_pi.ei.cad_info._typeid, _session.m_pi.ei.cad_info.id);

                                if (!v_it.empty())
                                {

                                    foreach (var el in v_it)
                                    {

                                        // Caddie já vai se desequipar, só verifica o parts
                                        if (el.Value.type == UpdateItem.UI_TYPE.CADDIE_PARTS)
                                        {

                                            // Limpa o caddie Parts
                                            _session.m_pi.ei.cad_info.parts_typeid = 0u;
                                            _session.m_pi.ei.cad_info.parts_end_date_unix = 0;
                                            _session.m_pi.ei.cad_info.end_parts_date = new SYSTEMTIME();
                                        }

                                        // Tira esse Update Item do map
                                        _session.m_pi.mp_ui.Remove(el.Key);
                                    }

                                }

                                _session.m_pi.ei.cad_info = null;
                                _session.m_pi.ue.caddie_id = 0;

                                _cpir.caddie = 0;

                                // Att No DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)_cpir.caddie), SQLDBResponse, this);
                            }

                            // ClubSet
                            if (_cpir.clubset != 0 && (pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset)) != null
                                    && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().CLUBSET)
                            {

                                var c_it = _session.m_pi.findUpdateItemByTypeidAndType((uint)_cpir.clubset, UpdateItem.UI_TYPE.WAREHOUSE);

                                if (c_it.Count <= 0)
                                {

                                    _session.m_pi.ei.clubset = pWi;

                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                    if (cs != null)
                                    {

                                        for (var j = 0; j < 5; ++j)
                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);

                                        _session.m_pi.ue.clubset_id = _cpir.clubset;

                                        // Verifica se o ClubSet pode equipar
                                        if (_session.checkClubSetEquiped(_session.m_pi.ue))
                                            _cpir.clubset = _session.m_pi.ue.clubset_id;

                                        // Update ON DB
                                        snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset), SQLDBResponse, this);

                                    }
                                    else
                                    {

                                        error = 5/*Item Not Found ON IFF_STRUCT SERVER*/;

                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom] [Error] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou Atualizar Clubset[TYPEID="
                                                + (pWi._typeid) + ", ID=" + (pWi.id)
                                                + "] equipado, mas ClubSet Not exists on IFF structure. Equipa o ClubSet padrao. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
                                        pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                        if (pWi != null)
                                        {

                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                    + "] tentou trocar o ClubSet[ID=" + (_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID="
                                                    + (_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                            // que no original fica no warehouse msm, eu só confundi quando fiz
                                            _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                            cs = sIff.getInstance().findClubSet(pWi._typeid);

                                            if (cs != null)
                                                for (var j = 0; j < 5; ++j)
                                                    _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);

                                            _session.m_pi.ei.clubset = pWi;
                                            _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                            // Update ON DB
                                            snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset), SQLDBResponse, this);

                                        }
                                        else
                                        {

                                            _smp.message_pool.getInstance().push(new message("[channel::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                    + "] tentou trocar o ClubSet[ID=" + (_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID="
                                                    + (_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            BuyItem bi = new BuyItem();
                                            stItem item = new stItem();

                                            bi.id = -1;
                                            bi._typeid = AIR_KNIGHT_SET;
                                            bi.qntd = 1;

                                            ItemManager.initItemFromBuyItem(_session.m_pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                            if (item._typeid != 0)
                                            {

                                                if ((_cpir.clubset = (int)ItemManager.addItem(item, _session, 2/*Padrão Item*/, 0)) != Convert.ToUInt32(ItemManager.RetAddItem.T_ERROR))
                                                {

                                                    // Equipa o ClubSet CV1
                                                    pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                                    if (pWi != null)
                                                    {

                                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                                        // que no original fica no warehouse msm, eu só confundi quando fiz
                                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                        cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                        if (cs != null)
                                                            for (var j = 0; j < 5; ++j)
                                                                _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);

                                                        _session.m_pi.ei.clubset = pWi;
                                                        _session.m_pi.ue.clubset_id = pWi.id;

                                                        // Update ON DB
                                                        snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset), SQLDBResponse, this);

                                                        //// Update ON GAME
                                                        //p.init_plain(0x216);

                                                        //p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                        //p.WriteUInt32(1);   // Count

                                                        //p.WriteByte(item.type);
                                                        //p.WriteUInt32(item._typeid);
                                                        //p.WriteInt32(item.id);
                                                        //p.WriteUInt32(item.flag_time);
                                                        //p.WriteBytes(item.stat.ToArray());
                                                        //p.WriteInt32((item.STDA_C_ITEM_TIME > 0) ? item.STDA_C_ITEM_TIME32 : item.STDA_C_ITEM_QNTD32);
                                                        //p.WriteZeroByte(25);

                                                        //packet_func.session_send(p, _session, 1);

                                                    }
                                                    else
                                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                                + "] nao conseguiu achar o ClubSet\"CV1\"[ID="
                                                                + (item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                                }
                                                else
                                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                            + "] nao conseguiu adicionar o ClubSet[TYPEID="
                                                            + (item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            }
                                            else
                                                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                        + "] nao conseguiu inicializar o ClubSet[TYPEID="
                                                        + (bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                        }
                                    }

                                }
                                else
                                {   // ClubSet Acabou o tempo

                                    // Coloca o ClubSet CV1 no lugar do ClubSet que acabou o tempo
                                    pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                    if (pWi != null)
                                    {

                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                + "] tentou trocar o ClubSet[ID=" + (_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID="
                                                + (_cpir.clubset) + "], colocando o ClubSet Padrao\"CV1\" do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                        // que no original fica no warehouse msm, eu só confundi quando fiz
                                        _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                        var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                        if (cs != null)
                                            for (var j = 0; j < 5; ++j)
                                                _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);

                                        _session.m_pi.ei.clubset = pWi;
                                        _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                        // Update ON DB
                                        snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset), SQLDBResponse, this);

                                    }
                                    else
                                    {

                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                + "] tentou trocar o ClubSet[ID=" + (_cpir.clubset) + "] para comecar o jogo, mas acabou o tempo do ClubSet[ID="
                                                + (_cpir.clubset) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        BuyItem bi = new BuyItem();
                                        stItem item = new stItem();

                                        bi.id = -1;
                                        bi._typeid = AIR_KNIGHT_SET;
                                        bi.qntd = 1;

                                        ItemManager.initItemFromBuyItem(_session.m_pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                        if (item._typeid != 0)
                                        {

                                            if ((_cpir.clubset = (int)ItemManager.addItem(item, _session, 2/*Padrão Item*/, 0)) != Convert.ToUInt32(ItemManager.RetAddItem.T_ERROR))
                                            {

                                                // Equipa o ClubSet CV1
                                                pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                                if (pWi != null)
                                                {

                                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                    if (cs != null)
                                                        for (var j = 0; j < 5; ++j)
                                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);

                                                    _session.m_pi.ei.clubset = pWi;
                                                    _session.m_pi.ue.clubset_id = pWi.id;

                                                    // Update ON DB
                                                    snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset), SQLDBResponse, this);

                                                    //// Update ON GAME
                                                    //p.init_plain(0x216);

                                                    //p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                    //p.WriteUInt32(1);   // Count

                                                    //p.WriteByte(item.type);
                                                    //p.WriteUInt32(item._typeid);
                                                    //p.WriteInt32(item.id);
                                                    //p.WriteUInt32(item.flag_time);
                                                    //p.WriteBytes(item.stat.ToArray());
                                                    //p.WriteInt32((item.STDA_C_ITEM_TIME > 0) ? item.STDA_C_ITEM_TIME32 : item.STDA_C_ITEM_QNTD32);
                                                    //p.WriteZeroByte(25);

                                                    //packet_func.session_send(p, _session, 1);

                                                }
                                                else
                                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                            + "] nao conseguiu achar o ClubSet\"CV1\"[ID="
                                                            + (item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                            }
                                            else
                                                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                        + "] nao conseguiu adicionar o ClubSet[TYPEID="
                                                        + (item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        }
                                        else
                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                    + "] nao conseguiu inicializar o ClubSet[TYPEID="
                                                    + (bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                    }
                                }

                            }
                            else
                            {

                                error = (_cpir.clubset == 0) ? 1/*client give invalid item id*/ : (pWi == null ? 2/*Item Not Found*/ : 3/*Erro item typeid invalid*/);

                                pWi = _session.m_pi.findWarehouseItemByTypeid(AIR_KNIGHT_SET);

                                if (pWi != null)
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar o ClubSet[ID=" + (_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], colocando o ClubSet Padrao\"CV1\" do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                    // que no original fica no warehouse msm, eu só confundi quando fiz
                                    _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                    var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                    if (cs != null)
                                        for (var j = 0; j < 5; ++j)
                                            _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);

                                    _session.m_pi.ei.clubset = pWi;
                                    _cpir.clubset = _session.m_pi.ue.clubset_id = pWi.id;

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset), SQLDBResponse, this);

                                }
                                else
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar o ClubSet[ID=" + (_cpir.clubset) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], ele nao tem o ClubSet Padrao\"CV1\", adiciona o ClubSet pardrao\"CV1\" para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    BuyItem bi = new BuyItem();
                                    stItem item = new stItem();

                                    bi.id = -1;
                                    bi._typeid = AIR_KNIGHT_SET;
                                    bi.qntd = 1;

                                    ItemManager.initItemFromBuyItem(_session.m_pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                    if (item._typeid != 0)
                                    {

                                        if ((_cpir.clubset = (int)ItemManager.addItem(item, _session, 2/*Padrão Item*/, 0)) != Convert.ToUInt32(ItemManager.RetAddItem.T_ERROR))
                                        {

                                            // Equipa o ClubSet CV1
                                            pWi = _session.m_pi.findWarehouseItemById(_cpir.clubset);

                                            if (pWi != null)
                                            {

                                                // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                                // que no original fica no warehouse msm, eu só confundi quando fiz
                                                _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                                var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                                if (cs != null)
                                                    for (var j = 0; j < 5; ++j)
                                                        _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);

                                                _session.m_pi.ei.clubset = pWi;
                                                _session.m_pi.ue.clubset_id = pWi.id;

                                                // Update ON DB
                                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)_cpir.clubset), SQLDBResponse, this);

                                                //// Update ON GAME
                                                //p.init_plain(0x216);

                                                //p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                //p.WriteUInt32(1);   // Count

                                                //p.WriteByte(item.type);
                                                //p.WriteUInt32(item._typeid);
                                                //p.WriteInt32(item.id);
                                                //p.WriteUInt32(item.flag_time);
                                                //p.WriteBytes(item.stat.ToArray());
                                                //p.WriteInt32((item.STDA_C_ITEM_TIME > 0) ? item.STDA_C_ITEM_TIME32 : item.STDA_C_ITEM_QNTD32);
                                                //p.WriteZeroByte(25);

                                                //packet_func.session_send(p, _session, 1);

                                            }
                                            else
                                                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                        + "] nao conseguiu achar o ClubSet\"CV1\"[ID="
                                                        + (item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        }
                                        else
                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                    + "] nao conseguiu adicionar o ClubSet[TYPEID="
                                                    + (item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    }
                                    else
                                        _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                + "] nao conseguiu inicializar o ClubSet[TYPEID="
                                                + (bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }
                            }

                            // Ball(Comet)
                            if (_cpir.ball != 0 && (pWi = _session.m_pi.findWarehouseItemByTypeid(_cpir.ball)) != null
                                    && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().BALL)
                            {

                                _session.m_pi.ei.comet = pWi;
                                _session.m_pi.ue.ball_typeid = _cpir.ball;      // Ball(Comet) é o typeid que o cliente passa

                                // Verifica se a Bola pode ser equipada
                                if (_session.checkBallEquiped(_session.m_pi.ue))
                                    _cpir.ball = _session.m_pi.ue.ball_typeid;

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball), SQLDBResponse, this);

                            }
                            else if (_cpir.ball == 0)
                            { // Bola 0 coloca a bola padrão para ele, se for premium user coloca a bola de premium user

                                // Zera para equipar a bola padrão
                                _session.m_pi.ei.comet = null;
                                _session.m_pi.ue.ball_typeid = 0;

                                // Verifica se a Bola pode ser equipada (Coloca para equipar a bola padrão
                                if (_session.checkBallEquiped(_session.m_pi.ue))
                                    _cpir.ball = _session.m_pi.ue.ball_typeid;

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball), SQLDBResponse, this);

                            }
                            else
                            {

                                error = (pWi == null ? 2/*Item Not Found*/ : 3/*Erro item typeid invalid*/);

                                pWi = _session.m_pi.findWarehouseItemByTypeid(DEFAULT_COMET_TYPEID);

                                if (pWi != null)
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar a Ball[TYPEID=" + (_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], colocando a Ball Padrao do player. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    _session.m_pi.ei.comet = pWi;
                                    _cpir.ball = _session.m_pi.ue.ball_typeid = pWi._typeid;

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball), SQLDBResponse, this);

                                }
                                else
                                {

                                    _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                            + "] tentou trocar a Ball[TYPEID=" + (_cpir.ball) + "] para comecar o jogo, mas deu Error[VALUE="
                                            + (error) + "], ele nao tem a Ball Padrao, adiciona a Ball pardrao para ele. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    BuyItem bi = new BuyItem();
                                    stItem item = new stItem();

                                    bi.id = -1;
                                    bi._typeid = DEFAULT_COMET_TYPEID;
                                    bi.qntd = 1;

                                    ItemManager.initItemFromBuyItem(_session.m_pi, item, bi, false, 0, 0, 1/*Não verifica o Level*/);

                                    if (item._typeid != 0)
                                    {

                                        if ((_cpir.ball = (uint)ItemManager.addItem(item, _session, 2/*Padrão Item*/, 0)) != Convert.ToUInt32(ItemManager.RetAddItem.T_ERROR))
                                        {

                                            // Equipa a Ball padrao
                                            pWi = _session.m_pi.findWarehouseItemById((int)_cpir.ball);

                                            if (pWi != null)
                                            {

                                                _session.m_pi.ei.comet = pWi;
                                                _session.m_pi.ue.ball_typeid = pWi._typeid;

                                                // Update ON DB
                                                snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateBallEquiped(_session.m_pi.uid, _cpir.ball), SQLDBResponse, this);

                                                //// Update ON GAME
                                                //p.init_plain(0x216);

                                                //p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                                //p.WriteUInt32(1);   // Count

                                                //p.WriteByte(item.type);
                                                //p.WriteUInt32(item._typeid);
                                                //p.WriteInt32(item.id);
                                                //p.WriteUInt32(item.flag_time);
                                                //p.WriteBytes(item.stat.ToArray());
                                                //p.WriteInt32((item.STDA_C_ITEM_TIME > 0) ? item.STDA_C_ITEM_TIME32 : item.STDA_C_ITEM_QNTD32);
                                                //p.WriteZeroByte(25);

                                                //packet_func.session_send(p, _session, 1);

                                            }
                                            else
                                                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                        + "] nao conseguiu achar a Ball[ID="
                                                        + (item.id) + "] que acabou de adicionar para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                        }
                                        else
                                            _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                    + "] nao conseguiu adicionar a Ball[TYPEID="
                                                    + (item._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                    }
                                    else
                                        _smp.message_pool.getInstance().push(new message("[channel::requestChangePlayerItemRoom][Info][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                                + "] nao conseguiu inicializar a Ball[TYPEID="
                                                + (bi._typeid) + "] para ele. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }
                            }

                            // Verifica se o Mascot Equipado acabou o tempo
                            if (_session.m_pi.ue.mascot_id != 0 && _session.m_pi.ei.mascot_info != null)
                            {
                                //FindUpdateItemByIdAndType
                                var m_it = _session.m_pi.findUpdateItemByTypeidAndType((uint)_session.m_pi.ue.mascot_id, UpdateItem.UI_TYPE.MASCOT);

                                if (m_it.Count > 0)
                                {

                                    // Desequipa o Mascot que acabou o tempo dele
                                    _session.m_pi.ei.mascot_info = null;
                                    _session.m_pi.ue.mascot_id = 0;

                                    snmdb.NormalManagerDB.getInstance().add(0, new CmdUpdateMascotEquiped(_session.m_pi.uid, 0/*Mascot_id == 0 not equiped*/), SQLDBResponse, this);

                                    // Update on GAME se não o cliente continua com o mascot equipado
                                   // packet_func.pacote04B(_session, (byte)ChangePlayerItemRoom.TYPE_CHANGE.TC_MASCOT, 0);
                                    //packet_func.session_send(p, _session, 0);

                                }
                            }

                            // Começa jogo
                            startGame(_session);
                        }
                        break;
                    default:
                        throw new exception("[room::requestChangePlayerItemRoom] [Error] sala[NUMERO=" + Convert.ToString(getNumero()) + "] type desconhecido.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                           13, 1));
                }
                updatePlayerInfo(_session);
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChangePlayerItemRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                //packet_func.pacote04B(_session, (byte)_cpir.type,
                //    (int)(ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.ROOM ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 1));
                //packet_func.session_send(p,
                //    _session, 0);
            }
        }


        void startGame(Player _session)
        {

            try
            {

                if (m_pGame == null)
                    throw new exception("[room::startGame] [Error] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou comecar o jogo na sala[NUMERO="
                        + (m_ri.numero) + "], mas a sala nao tem nenhum jogo iniciado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM, 1, 0x5200101));

                if (m_ri.flag == 0)
                {

                    m_pGame.sendInitialData(_session);//aqui

                    if (m_ri.getTipo() == RoomInfo.TIPO.STROKE || m_ri.getTipo() == RoomInfo.TIPO.MATCH)
                        sendCharacter(_session, 0x103);
                }
                else
                {   // Entra depois

                    try
                    {
                        List<PlayerRoomInfoEx> v_element = new List<PlayerRoomInfoEx>();
                        PlayerRoomInfoEx pri = null;

                        foreach (var _el in v_sessions)
                        {
                            if ((pri = getPlayerInfo(_el)) != null)
                                v_element.Add(pri);
                        }


                        // Send Make Room
                        var p = new PangyaBinaryWriter(0x113);

                        p.WriteByte(4);  // Cria sala
                        p.WriteByte(0);

                        p.WriteBytes(m_ri.ToArray());

                        packet_func.session_send(p, _session, 1);

                        //// Send All Player Of Room
                        //p.init_plain(0x113);

                        //p.WriteByte(4);
                        //p.WriteByte(1);

                        //p.WriteByte((int)v_element.Count);

                        //for (var i = 0; i < v_element.Count; i++)
                        //    p.WriteBytes(v_element[i].ToArray());

                        //packet_func.session_send(p, _session, 1);

                        //// Rate Pang
                        //p.init_plain(0x113);

                        //p.WriteByte(4);
                        //p.WriteByte(2);

                        //p.WriteUInt32(m_ri.rate_pang);

                        //packet_func.session_send(p, _session, 1);

                        //// Send Initial of Game
                        //m_pGame.sendInitialDataAfter(_session);

                        //// Add Player ON GAME to ALL players
                        //p.init_plain(0x113);//start packet

                        //p.WriteByte(7);
                        //p.WriteByte(0);

                        //p.WriteString(_session.m_pi.nickname);

                        //p.WriteBytes(m_ri.ToArray());

                        //p.WriteByte(v_element.Count);

                        //p.WriteUInt32((uint32_t)v_sessions.Count);

                        //for (var i = 0; i < v_element.Count; i++)
                        //    p.WriteBytes(v_element[i].ToArray());

                        //// Send ALL players of room exceto ele
                        //packet_func.vector_send(p, getSessions(_session), 1);

                    }
                    catch (exception e)
                    {
                        throw e;
                    }
                }

            }

            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::startGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }


        public void updateMaster(Player _session)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                Player master = findSessionByUID((uint)m_ri.master);

                if (_session != null && _session.m_pi.m_cap.game_master && m_ri.master != -2)
                {
                    // Só troca o master se ele saiu da sala ou se ele não for GM
                    if (master == null || !(master.m_pi.m_cap.game_master/* & 4*/))
                    {
                        m_ri.master = (int)_session.m_pi.uid;
                        m_ri.state_flag = 0x100; // GM

                        if (master != null)
                        {
                            updatePlayerInfo(master);
                            p.init_plain(0x78);
                            p.WriteInt32(master.m_oid);
                            p.WriteByte((byte)~getPlayerInfo(master).state_flag.ready);

                            packet_func.room_broadcast(this, p, 1);
                        }

                        p = new PangyaBinaryWriter();
                        p.init_plain(0x7C);
                        p.WriteInt32(_session.m_oid);
                        p.WriteInt16(0);

                        packet_func.room_broadcast(this, p, 0);
                    }
                } 
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public void updateGuild(Player _session)
        {
            if (_session.m_pi.gi.uid == -1)
                throw new Exception($"[channel::UpdateGuild] [Error] PLAYER[UID={_session.m_pi.uid}] player nao esta em uma guild.");

            PlayerRoomInfoEx pri = getPlayerInfo(_session);

            if (pri == null)
                throw new Exception($"[channel::UpdateGuild] [Error] PLAYER[UID={_session.m_pi.uid}] nao tem o info do player na sala[NUMERO={m_ri.numero}]. Hacker ou Bug.");

            Guild guild = null;

            if (m_ri.guilds.guild_1_uid == 0 && m_ri.guilds.guild_2_uid != _session.m_pi.gi.uid)
            {
                m_ri.guilds.guild_1_uid = _session.m_pi.gi.uid;
                m_ri.guilds.guild_1_nome = _session.m_pi.gi.name;
                m_ri.guilds.guild_1_mark = _session.m_pi.gi.mark_emblem;
                m_ri.guilds.guild_1_index_mark = (ushort)_session.m_pi.gi.index_mark_emblem;

              //  pri.state_flag.team = 0;
                guild = m_guild_manager.addGuild(Guild.eTEAM.RED, m_ri.guilds.guild_1_uid);
            }
            else if (m_ri.guilds.guild_1_uid == _session.m_pi.gi.uid)
            {
               // pri.state_flag.team = 0;
                guild = m_guild_manager.findGuildByTeam(Guild.eTEAM.RED);
            }
            else if (m_ri.guilds.guild_2_uid == 0)
            {
                m_ri.guilds.guild_2_uid = _session.m_pi.gi.uid;
                m_ri.guilds.guild_2_nome = _session.m_pi.gi.name;
                m_ri.guilds.guild_2_mark = _session.m_pi.gi.mark_emblem;
                m_ri.guilds.guild_2_index_mark = (ushort)_session.m_pi.gi.index_mark_emblem;

               // pri.state_flag.team = 1;
                guild = m_guild_manager.addGuild(Guild.eTEAM.BLUE, m_ri.guilds.guild_2_uid);
            }
            else
            {
                //pri.state_flag.team = 1;
                guild = m_guild_manager.findGuildByTeam(Guild.eTEAM.BLUE);
            }

            if (guild != null)
            {
                guild.addPlayer(_session);
            }
            else
            {
                _smp.message_pool.getInstance().push(new message(
                    $"[room::updateGuild][Warning] PLAYER[UID={_session.m_pi.uid}] tentou entrar em uma guild da sala[NUMERO={m_ri.numero}], mas nao conseguiu criar ou achar nenhum guild na sala. Bug.",
                   type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

         //   m_teans[pri.state_flag.team].addPlayer(_session);
        }

        public int findIndexSession(Player _session)
        {
            if (_session == null)
                throw new Exception("[room::findIndexSession] [Error] _session is null.");

            int index = -1;

            lock (m_cs)
            {
                for (int i = 0; i < v_sessions.Count; ++i)
                {
                    if (v_sessions[i].getUID() == _session.getUID())
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        public int findIndexSession(uint _uid)
        {
            if (_uid == 0)
                throw new Exception("[room::findIndexSession] [Error] _uid is invalid(zero).");

            int index = -1;

            lock (m_cs)
            {
                for (int i = 0; i < v_sessions.Count; ++i)
                {
                    if (v_sessions[i].m_pi.uid == _uid)
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index;
        }
        public void @lock()
        {
            Monitor.Enter(m_lock_cs);

            if (m_destroying)
            {

                Monitor.Exit(m_lock_cs);

                throw new exception("[room::lock] [Error] room esta no estado para ser destruida, nao pode bloquear ela.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM, 150, 0));
            }

            m_lock_spin_state++;	// Bloqueado
        }

        public bool trylock()
        {
            bool ret = false;

            if (Monitor.TryEnter(m_lock_cs))
            {
                ret = true;

                if (m_destroying)
                {
                    Monitor.Exit(m_lock_cs);
                    throw new exception("[room::tryLock] [Error] room esta no estado para ser destruida, nao pode bloquear ela.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM, 150, 0));
                }

                m_lock_spin_state++; // Bloqueado
            }

            return ret;
        }

        public void unlock()
        {
            Monitor.Enter(m_lock_cs);

            int state = Interlocked.Decrement(ref m_lock_spin_state);

            if (state < 0)
            {
                _smp.message_pool.getInstance().push(
                    new message(
                        "[room::unlock][Warning] a sala[NUMERO=" + m_ri.numero + "] ja esta desbloqueada.",
                        type_msg.CL_ONLY_FILE_LOG
                    ));
            }

            Monitor.Exit(m_lock_cs);
        }

        public void setDestroying()
        {
            Monitor.Enter(m_lock_cs);
            // Destruindo a sala
            m_destroying = true;
            Monitor.Exit(m_lock_cs);
        }


        public List<InviteChannelInfo> getAllInvite()
        {
            return v_invite;
        }

        public void requestEndAfterEnter()
        {
            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestEndAfterEnter] [Error] tentou terminar o tempo que pode entrar no jogo depois que ele comecou na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo iniciado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1201, 0));
                }

                m_pGame.requestEndAfterEnter();

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestEndAfterEnter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Smart Calculator Command

        // Pede o Hole que o player está, 
        // se eles estiver jogando ou 0 se ele não está jogando
        public byte requestPlace(Player _session)
        {

            try
            {

                if (m_pGame != null)
                {
                    return m_pGame.requestPlace(_session);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestPlacePlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        // Time Tourney After Enter IN GAME
        public void requestStartAfterEnter(Action _job)
        {

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestStartAfterEnter] [Error] tentou comecar o tempo que pode entrar no jogo depois que ele comecou na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo iniciado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1200, 0));
                }

                m_pGame.requestStartAfterEnter(_job);

            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[room::requestStartAfterEnter][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void AddListGallery(Player pSession)
        {
            if (pSession == null)
            {
                Console.WriteLine("AddListGallery : session null");
                return;
            }

            // Procura o primeiro slot vazio na lista
            m_listGallery.Add(pSession);

            // Se não achou espaço, loga o ID do usuário
            string userId = pSession.getID();
            Console.WriteLine($"AddListGallery : {userId}");
        }

        public int GetSizeGalleryList()
        {
            int count = 0;

            // m_listGallery é assumido como uma lista ou array de sessões
            foreach (var session in m_listGallery)
            {
                if (session != null)
                    count++;
            }

            return count;
        }

        public void SendGalleryList(Player _session, byte code)
        {
            var galleryList = this.m_listGallery;
            int validSessions = galleryList.Count(s => s != null);

            if (validSessions == 0)
                return;

            var packet = new PangyaBinaryWriter(0xAE);
            packet.WriteByte(code);

            switch (code)
            {
                case 0:
                    byte size = (byte)this.GetSizeGalleryList();
                    packet.WriteByte(size);

                    foreach (var session in galleryList)
                    {
                        if (session != null)
                        {
                            packet.Write(getPlayerInfo(session).ToArray());
                        }
                    }

                    packet_func.session_send(packet, _session);
                    break;

                case 1:
                    if (_session != null)
                    {
                        PlayerRoomInfoEx src = getPlayerInfo(_session);
                        packet.Write(src.ToArray());
                        goto Broadcast;
                    }
                    break;

                case 2:
                case 3:
                    packet.Write(_session.getUID());
                    goto Broadcast;

                default:
                Broadcast:
                    packet_func.room_broadcast(this, packet);
                    break;
            }

            packet.Dispose(); // ou packet = null se não tiver IDisposable
        }

        public static void SQLDBResponse(uint32_t _msg_id,
            Pangya_DB _pangya_db,
            object _arg)
        {

            if (_arg == null)
            {
                _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Warning] _arg is null com msg_id = " + Convert.ToString(_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse] [Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            var _channel = Tools.reinterpret_cast<Channel>(_arg);

            switch (_msg_id)
            {
                case 7: // Update Character PCL
                    {
                        var cmd_ucp = Tools.reinterpret_cast<CmdUpdateCharacterPCL>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Info] Atualizou Character[TYPEID=" + Convert.ToString(cmd_ucp.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucp.getInfo().id) + "] PCL[C0=" + Convert.ToString((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_POWER]) + ", C1=" + Convert.ToString((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_CONTROL]) + ", C2=" + Convert.ToString((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_ACCURACY]) + ", C3=" + Convert.ToString((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_SPIN]) + ", C4=" + Convert.ToString((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_CURVE]) + "] do PLAYER[UID=" + Convert.ToString(cmd_ucp.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 8: // Update ClubSet Stats
                    {
                        var cmd_ucss = Tools.reinterpret_cast<CmdUpdateClubSetStats>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Info] Atualizou ClubSet[TYPEID=" + Convert.ToString(cmd_ucss.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucss.getInfo().id) + "] Stats[C0=" + Convert.ToString((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_POWER]) + ", C1=" + Convert.ToString((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_CONTROL]) + ", C2=" + Convert.ToString((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_ACCURACY]) + ", C3=" + Convert.ToString((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_SPIN]) + ", C4=" + Convert.ToString((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_CURVE]) + "] do PLAYER[UID=" + Convert.ToString(cmd_ucss.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 9: // Update Character Mastery
                    {
                        var cmd_ucm = Tools.reinterpret_cast<CmdUpdateCharacterMastery>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Info] Atualizou Character[TYPEID=" + Convert.ToString(cmd_ucm.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucm.getInfo().id) + "] Mastery[value=" + Convert.ToString(cmd_ucm.getInfo().mastery) + "] do PLAYER[UID=" + Convert.ToString(cmd_ucm.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 12: // Update ClubSet Workshop
                    {
                        var cmd_ucw = Tools.reinterpret_cast<CmdUpdateClubSetWorkshop>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Info] PLAYER[UID=" + Convert.ToString(cmd_ucw.getUID()) + "] Atualizou ClubSet[TYPEID=" + Convert.ToString(cmd_ucw.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_ucw.getInfo().id) + "] Workshop[C0=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[0]) + ", C1=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[1]) + ", C2=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[2]) + ", C3=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[3]) + ", C4=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.c[4]) + ", Level=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.level) + ", Mastery=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.mastery) + ", Rank=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.rank) + ", Recovery=" + Convert.ToString(cmd_ucw.getInfo().clubset_workshop.recovery_pts) + "] Flag=" + Convert.ToString(cmd_ucw.getFlag()) + "", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 26: // Update Mascot Info
                    {

                        var cmd_umi = Tools.reinterpret_cast<CmdUpdateMascotInfo>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[room::SQLDBResponse][Info] PLAYER[UID=" + Convert.ToString(cmd_umi.getUID()) + "] Atualizar Mascot Info[TYPEID=" + Convert.ToString(cmd_umi.getInfo()._typeid) + ", ID=" + Convert.ToString(cmd_umi.getInfo().id) + ", LEVEL=" + Convert.ToString((ushort)cmd_umi.getInfo().level) + ", EXP=" + Convert.ToString(cmd_umi.getInfo().exp) + ", FLAG=" + Convert.ToString((ushort)cmd_umi.getInfo().flag) + ", TIPO=" + Convert.ToString(cmd_umi.getInfo().tipo) + ", IS_CASH=" + Convert.ToString((ushort)cmd_umi.getInfo().is_cash) + ", PRICE=" + Convert.ToString(cmd_umi.getInfo().price) + ", MESSAGE=" + (cmd_umi.getInfo().message) + ", END_DT=" + (cmd_umi.getInfo().data.ConvertTime()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 0:
                default: // 25 é update item equipado slot
                    break;
            }
        }

        ~Room()
        {
            // Leave All Players
            leaveAll(0);

            if (m_pGame != null)
            {
                m_pGame.stopTime();//desliga o relogio...
                m_pGame.Dispose();      // elimina a referência
            }
            m_pGame = null;

            m_channel_owner = -1;

            m_weather_lounge = 0;

            if (v_sessions.Any())
            {
                v_sessions.Clear();
            }

            if (m_player_info.Any())
            {
                m_player_info.Clear();
            }

            clear_invite();

            clear_Player_kicked();

            clear_teans();

            m_bot_tourney = false; 

            m_destroying = false;      // Destruindo a sala
            try
            {

                @lock();

                m_destroying = true;

                unlock();

            }
            catch (exception e)
            {

                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                    STDA_ERROR_TYPE.ROOM, 150))
                {

                    unlock();

                    _smp.message_pool.getInstance().push(new message("[room::destroy][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                    // Leave All Players
                    leaveAll(0);

                    if (m_pGame != null)
                    {
                        m_pGame.stopTime();//desliga o relogio...
                        m_pGame.Dispose();      // elimina a referência
                    }
                    m_pGame = null;

                    m_channel_owner = -1;

                    m_weather_lounge = 0;

                    if (v_sessions.Any())
                    {
                        v_sessions.Clear();
                    }

                    if (m_player_info.Any())
                    {
                        m_player_info.Clear();
                    }

                    clear_invite();

                    clear_Player_kicked();

                    clear_teans();

                    m_bot_tourney = false; 

                    m_destroying = false;      // Destruindo a sala
                    try
                    {

                        @lock();

                        m_destroying = true;

                        unlock();

                    }
                    catch (exception e)
                    {

                        if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                            STDA_ERROR_TYPE.ROOM, 150))
                        {

                            unlock();

                            _smp.message_pool.getInstance().push(new message("[room::destroy][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }
                }

                // TODO: liberar recursos não gerenciados (objetos não gerenciados) e substituir o finalizador
                // TODO: definir campos grandes como nulos
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void requestChangePlayerStateReadyRoom(Player _session, packet _packet)
        {


            byte ready = _packet.ReadUInt8();

            PlayerRoomInfoEx pri = getPlayerInfo(_session);
            // Update state of ready
            pri.state_flag.ready = (byte)(ready == 0 ? 1 : 0);//invertido

            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x78); // Estado de Ready do player na sala

            p.WriteInt32(_session.m_oid);
            p.WriteByte(ready);

            packet_func.room_broadcast(this,
                p, 1);

            m_player_info[_session] = pri;
        }

        private void ThrowHackException(Player session, string motivo)
        {
            var ri = getInfo();

            string msg = $"[Room::ThrowHackException] [Error] PLAYER [UID={session.m_pi.uid}] " +
                         $"Channel[ID={this.m_channel_owner}] tentou criar sala [Nome={ri.nome}, PWD={ri.senha}, TIPO={ri.tipo}], {motivo}. Hacker ou Bug";

            throw new exception(msg, ExceptionError.STDA_MAKE_ERROR_TYPE(
                STDA_ERROR_TYPE.ROOM, 10, 0x770001));
        }
    }
}