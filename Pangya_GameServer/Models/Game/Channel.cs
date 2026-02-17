using Pangya_GameServer.Models;
using Pangya_GameServer.Models.Manager;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.PangyaEnums;
using Pangya_GameServer.Repository;
using PangyaAPI.Network.Models;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using static Pangya_GameServer.Models.DefineConstants;
namespace Pangya_GameServer.Game
{
    /// <summary>
    /// Only Methods Channel
    /// </summary>
    public partial class Channel
    {
        protected enum ESTADO : byte
        {
            UNITIALIZED,
            INITIALIZED
        }

        public enum LEAVE_ROOM_STATE : int
        {
            DO_NOTHING = -1,        // Faz nada
            SEND_UPDATE_CLIENT = 0, // bug arm g++
            ROOM_DESTROYED,
        }

        protected ChannelInfo m_ci;
        private RoomManager m_rm;
        public object m_cs = new object();
        protected uProperty m_type;           // Type GrandPrix, Natural, Normal

        protected int m_state;

        protected List<Player> v_sessions;
        protected Dictionary<Player, PlayerLobbyInfo> m_player_info;
        protected List<InviteChannelInfo> v_invite;
        private object m_cs_invite = new object(); 
        public Channel(ChannelInfo _ci, uProperty _type)
        {
            m_ci = _ci;
            m_type = (_type);
            m_state = (int)ESTADO.INITIALIZED;
            v_sessions = new List<Player>(_ci.max_user);
            m_player_info = new Dictionary<Player, PlayerLobbyInfo>(_ci.max_user);
            v_invite = new List<InviteChannelInfo>();
            m_rm = new RoomManager(_ci.id);
        } 

        public ChannelInfo getInfo()
        {
            return m_ci;
        }
        
        public sbyte getId()
        {
            return m_ci.id;
        }

        public List<Player> getSessions(int _lobby = 255)//default
        {
            List<Player> v_session = new List<Player>();
            Monitor.Enter(m_cs);
            for (var i = 0; i < v_sessions.Count(); ++i)
            {
                if (v_sessions[i] != null && v_sessions[i].m_pi.channel != DEFAULT_CHANNEL
                    && (_lobby == DEFAULT_CHANNEL || v_sessions[i].m_pi.lobby != DEFAULT_CHANNEL))
                    v_session.Add(v_sessions[i]);

            }
            Monitor.Exit(m_cs);
            return v_session;
        }
         
        public void leaveChannel(Player _session)
        {
            try
            {

                if (_session.m_pi.lobby != DEFAULT_CHANNEL)
                {
                    leaveLobby(_session); // Sai da Lobby
                }
                else // Sai da Sala Practice que não entra na lobby, [SINGLE PLAY]
                {
                    //leaveRoom(_session, 0);
                }

                removeSession(_session);
                _smp.message_pool.getInstance().push(new message($"[Channel::leaveChannel][Sucess] CHANNEL[Id: {m_ci.id}, Users: {m_ci.curr_user}/{m_ci.max_user}, Rooms: {0}]", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            catch (exception e)
            {
                removeSession(_session);

                _smp.message_pool.getInstance().push(new message("[Channel::leaveChannel][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), // Diferente do error do channel
                    STDA_ERROR_TYPE.CHANNEL, 1))
                {
                    throw;
                }
            }
        }


        public void checkEnterChannel(Player _session)
        {
            // Não é GM verifica se o player pode entrar nesse canal
            if (!_session.m_pi.m_cap.game_master)
            {

                //if (_session.m_pi.level < m_ci.min_level_allow || _session.m_pi.level > m_ci.max_level_allow)
                //    throw new exception("[Channel::checkEnterChannel][Error] PLAYER [UID=" + (_session.m_pi.uid) + ", LEVEL=" + (_session.m_pi.level)
                //        + "] nao tem o level necessario para entrar no canal[ID=" + (m_ci.id) + ", MIN=" + m_ci.min_level_allow
                //        + ", MAX=" + m_ci.max_level_allow + "].");

                 
            }
        }

        public PlayerLobbyInfo getPlayerInfo(Player _session)
        {

            if (_session == null)
            {
                throw new exception("[Channel::getPlayerInfo][Error] _session is null.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                    12, 1));
            }

            PlayerLobbyInfo pci = null;

            return m_player_info.Any(c => c.Key == _session) ? m_player_info.First(c => c.Key == _session).Value : pci;
        }


        public bool isFull()
        {
            return m_ci.curr_user >= m_ci.max_user;
        }

    

        public void removeSession(Player _session)
        {

            if (_session == null)
            {
                throw new exception("[Channel::removeSession][Error] _session is null.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                    3, 0));
            }

            int index = -1;

            Monitor.Enter(m_cs);

            if ((index = findIndexSession(_session)) == -1)
            {
                Monitor.Exit(m_cs);
                return;
            }

            v_sessions.RemoveAt(index);

            m_ci.curr_user--;

            // reseta(default) o channel que o player está no player info
            _session.m_pi.channel = DEFAULT_CHANNEL;
            _session.m_pi.mi.sala_numero = DEFAULT_ROOM_ID;
            _session.m_pi.place = 0;

            deletePlayerInfo(_session);

            Monitor.Exit(m_cs);
        }

        public void addSession(Player _session)
        {

            if (_session == null || !_session.getState())
            {
                throw new exception("[Channel::addSession][Error] _session is null or invalid.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                    3, 1));
            }

            Monitor.Enter(m_cs);

            v_sessions.Add(_session);

            m_ci.curr_user++;

            // Channel id
            _session.m_pi.channel = m_ci.id;
            _session.m_channel = this;
            _session.m_pi.place = 0;

            // Calcula a condição do player e o sexo
            // Só faz calculo de Quita rate depois que o player
            // estiver no level Beginner E e jogado 50 games
            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
            {
                float rate = _session.m_pi.ui.getQuitRate();

                if (rate < GOOD_PLAYER_ICON)
                {
                    _session.m_pi.mi.state_flag.azinha = true;
                }
                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                {
                    _session.m_pi.mi.state_flag.quiter_1 = true;
                }
                else if (rate >= QUITER_ICON_2)
                {
                    _session.m_pi.mi.state_flag.quiter_2 = true;
                }
            }

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
            {
                _session.m_pi.mi.state_flag.icon_angel = _session.m_pi.ei.char_info.AngelEquiped() == 1;
            }
            else
            {
                _session.m_pi.mi.state_flag.icon_angel = false;
            }
            _session.m_pi.mi.sexo = (byte)(_session.m_pi.mi.state_flag.sexo == true ? 1 : 0);

            makePlayerInfo(_session);

            Monitor.Exit(m_cs);
        }

        public Player findSessionByOID(uint _oid)
        {
            return m_player_info.Keys.FirstOrDefault(c => c.m_oid == _oid);
        }
        protected Player findSessionByUID(int _uid)
        {
            return m_player_info.Keys.FirstOrDefault(c => c.getUID() == _uid);
        }
        protected Player findSessionByNickname(string _nickname)
        {
            return m_player_info.Keys.FirstOrDefault(c => c.getNickname() == _nickname);
        }
        public int findIndexSession(Player _session)
        {

            if (_session == null)
            {
                throw new exception("[Channel::findIndexSession][Error] _session is null.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                    3, 0));
            }

            for (var i = 0; i < v_sessions.Count; ++i)
            {
                if (v_sessions[i] == _session)
                {
                    return i;
                }
            }

            return -1;
        }

        //        ////
        protected void makePlayerInfo(Player _session)
        {
            PlayerLobbyInfo pci = new PlayerLobbyInfo
            {
                // Player Canal Info clear
                uid = _session.m_pi.uid,
                oid = _session.m_oid,
                id = _session.m_pi.id,
                sala_numero = _session.m_pi.mi.sala_numero,
                level = (byte)_session.m_pi.level,
                capability = _session.m_pi.m_cap,
                nickname = _session.m_pi.nickname,
                sDisplayID = "@NT_" + _session.m_pi.nickname,
                title = _session.m_pi.ue.m_title,
                team_point = 1000,
                guild_index_mark = _session.m_pi.gi.index_mark_emblem,
                guild_uid = _session.m_pi.gi.uid,
                guild_mark_img = _session.m_pi.gi.mark_emblem,
                flag_visible_gm = Convert.ToInt16(_session.m_pi.mi.state_flag.visible)
            };
            // Só faz calculo de Quita rate depois que o player
            // estiver no level Beginner E e jogado 50 games
            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
            {
                float rate = _session.m_pi.ui.getQuitRate();

                if (rate < GOOD_PLAYER_ICON)
                {
                    pci.state_flag.azinha = 0;
                }
                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                    pci.state_flag.quiter_1 = 1;
                else if (rate >= QUITER_ICON_2)
                    pci.state_flag.quiter_2 = 1;
            }

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                pci.state_flag.icon_angel = 0;
            else
                pci.state_flag.icon_angel = 0;

            pci.state_flag.sexo = _session.m_pi.mi.sexo;

            pci.guild_uid = _session.m_pi.gi.uid;

            if (!m_player_info.ContainsKey(_session))
            {
                m_player_info.Add(_session, pci);
            }
            // Update Player Location
            _session.m_pi.updateLocationDB();
        }

        public void updatePlayerInfo(Player _session)
        {
            PlayerLobbyInfo pci;

            if ((pci = getPlayerInfo(_session)) == null)
                return;//so retorna mesmo

            // Player Canal Info Update
            pci.nickname = _session.m_pi.nickname;
            pci.uid = _session.m_pi.uid;
            pci.oid = _session.m_oid;
            pci.id = _session.m_pi.id;
            pci.sala_numero = _session.m_pi.mi.sala_numero;
            pci.level = (byte)_session.m_pi.level;
            pci.team_point = 1000;
            pci.flag_visible_gm = Convert.ToInt16(_session.m_pi.mi.state_flag.visible);
            pci.capability = _session.m_pi.m_cap;
            pci.title = _session.m_pi.ue.m_title;
            pci.guild_index_mark = _session.m_pi.gi.index_mark_emblem;
            pci.guild_uid = _session.m_pi.gi.uid;
            pci.guild_mark_img = _session.m_pi.gi.mark_emblem;
            // Só faz calculo de Quita rate depois que o player
            // estiver no level Beginner E e jogado 50 games
            if (_session.m_pi.level >= 6 && _session.m_pi.ui.jogado >= 50)
            {
                float rate = _session.m_pi.ui.getQuitRate();

                if (rate < GOOD_PLAYER_ICON)
                    pci.state_flag.azinha = 1;
                else if (rate >= QUITER_ICON_1 && rate < QUITER_ICON_2)
                    pci.state_flag.quiter_1 = 1;
                else if (rate >= QUITER_ICON_2)
                    pci.state_flag.quiter_2 = 1;
            }

            if (_session.m_pi.ei.char_info != null && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
                pci.state_flag.icon_angel = 0;
            else
                pci.state_flag.icon_angel = 0;

            pci.state_flag.sexo = _session.m_pi.mi.sexo;

            // Update Location Player
            _session.m_pi.updateLocationDB();
        }

        public void deletePlayerInfo(Player _session)
        {
            // Update Location player
            _session.m_pi.updateLocationDB();

            // Delete Player Info of session(player)
            m_player_info.Remove(_session);
        }

        public LEAVE_ROOM_STATE leaveRoom(Player _session, int _option)
        {
            LEAVE_ROOM_STATE state = LEAVE_ROOM_STATE.DO_NOTHING;

            var r = m_rm.findRoom(_session.m_pi.mi.sala_numero);

            if (r != null)
            {
                int opt = 0;

                try
                {
                    // Deleta convidado
                    if (r.isInvited(_session))
                    { 
                    }
                    else
                    {
                        opt = r.leave(_session, _option);
                    }

                    // Verifica se todos os jogadores são convidados
                    var all_invite = r.getAllInvite();

                    if (r.getNumPlayers() == all_invite.Count)
                    {
                        InviteChannelInfo ici;
                        while (all_invite.Count > 0)
                        {
                            Player s;
                            var _ici = all_invite.FirstOrDefault();
                            if ((s = sgs.gs.getInstance().findPlayer(_ici.invited_uid)) == null && _ici != null)
                            {
                                // Player não está online no server, tenta deletar o convite com o uid do player
                                ici = r.deleteInvited(_ici.invited_uid);

                            }
                            else
                            {
                                // Player está online deleta o convite com o objeto do player
                                ici = r.deleteInvited(s);
                            }

                            // Deleta invite
                            //if (ici.room_number >= 0 && ici.invited_uid > 0u && ici.invite_uid > 0u)
                            //    deleteInviteTimeRequest(ici);
                        }
                    }
                }
                catch (Exception e)
                {
                    _smp.message_pool.getInstance().push(new message("[Channel::leaveRoom][Error] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                // Atualiza info do jogador
                updatePlayerInfo(_session);

                if (r.getNumPlayers() > 0 || opt == 0)
                {
                    r.sendUpdate();

                    try
                    {
                        r.sendCharacter(_session, 2);
                    }
                    catch (Exception e)
                    {
                        _smp.message_pool.getInstance().push(new message("[Channel::leaveRoom][Error] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                    sendUpdatePlayerInfo(_session, 3);
                    sendUpdateRoomInfo(r.getInfo(), 3);

                    try
                    {
                        // Se opt == 0x801, deleta todos os jogadores da sala
                        if (opt == 0x801 && r.getNumPlayers() > 0)
                        {
                            var players = r.getSessions().ToList(); // Clone
                            foreach (var player in players)
                            {
                                var result = leaveRoom(player, 0x800);
                                if (result == LEAVE_ROOM_STATE.ROOM_DESTROYED)
                                    break;
                            }

                            // Força destruição da sala se estiver vazia
                            if (r.getNumPlayers() == 0)
                            {
                                m_rm.destroyRoom(r);
                                state = LEAVE_ROOM_STATE.ROOM_DESTROYED;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _smp.message_pool.getInstance().push(new message("[Channel::leaveRoom][ErrorSystem] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
                else
                {
                    // Destruíndo a sala
                    r.setDestroying();

                    m_rm.destroyRoom(r);

                    sendUpdatePlayerInfo(_session, 3);
                    sendUpdateRoomInfo(r.getInfo(), 2);

                    state = LEAVE_ROOM_STATE.ROOM_DESTROYED;

                    if ((r.getNumPlayers() == 1 || r.getNumPlayers() == 0))//limpa tudo aqui
                        r.Dispose();
                }

                // Envia pacote de saída para o cliente se necessário
                if (state < LEAVE_ROOM_STATE.ROOM_DESTROYED)
                    state = LEAVE_ROOM_STATE.SEND_UPDATE_CLIENT;
            }
            else if (_option == 1)
            {
                _smp.message_pool.getInstance().push(new message(
                    $"[Channel::leaveRoom][Error][WARNNING] PLAYER [UID={_session.m_pi.uid}] tentou sair da sala[NUMERO={_session.m_pi.mi.sala_numero}], mas ela nao existe. Hacker ou Bug",
                    type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return state;
        }


        public void requestSendMsgChatRoom(Player _session, string _msg)
        {

            if (!_session.getState())
            {
                throw new exception("[Channel::requestSendMsgChatRoom][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                    1, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {


                var r = m_rm.findRoom(_session.m_pi.mi.sala_numero);

                if (r == null)
                {
                    throw new exception("[Channel::requestSendMsgChatRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] nao esta em uma sala[NUMERO=" + (_session.m_pi.mi.sala_numero) + "]. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        18, 0));
                }
                 
                packet_func.room_broadcast(r,
                     packet_func.pacote02(_session.m_pi.nickname, _msg,
                    ((_session.m_pi.m_cap.game_master) ? eChatMsg.GM_EVENT : 0)), 0); 
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[Channel::requestSendMsgChatRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // A função que chama ela tem que tratar as excpetion, relança elas
                throw;
            }
        }

        public void sendUpdateRoomInfo(RoomInfoEx _ri, int _option)
        {
            packet_func.channel_broadcast(this,
                       packet_func.pacote09(new List<RoomInfoEx>() { _ri },
                       _option), 0);
        }

        public void sendUpdatePlayerInfo(Player _session, int _option)
        {
            PlayerLobbyInfo pci = getPlayerInfo(_session);

            var p = packet_func.pacote08(new List<PlayerLobbyInfo>() { (pci == null) ? new PlayerLobbyInfo() : pci }, _option);

            packet_func.channel_broadcast(this, p, 0);
        }

        public void SQLDBResponse(int _msg_id,  Pangya_DB _pangya_db, object _arg)
        {

            if (_arg == null)
            {
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            var _channel = Tools.reinterpret_cast<Channel>(_arg);

            switch (_msg_id)
            {
                case 1: // Update Dolfini Locker Pass
                    { 
                        break;
                    }
                case 2: // Update Dolfini Locker Mode
                    {
 
                        break;
                    }
                case 3: // Update Dolfini Locker Pang
                    { 
                        break;
                    }
                case 4: // Delete Dolfini Locker Item
                    { 
                        break;
                    }
                case 5: // Extend Part Rental
                    {
                        var cmd_er = Tools.reinterpret_cast<CmdExtendRental>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Extendeu Part Rental[ID=" + (cmd_er.getItemID()) + "] ate o a date[value=" + cmd_er.getDate() + "] para o PLAYER [UID=" + (cmd_er.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 6: // Delete Part Rental
                    {
                        var cmd_dr = Tools.reinterpret_cast<CmdDeleteRental>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Deletou Part Rental[ID=" + (cmd_dr.getItemID()) + "] do PLAYER [UID=" + (cmd_dr.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 7: // Update Character PCL
                    {
                        var cmd_ucp = Tools.reinterpret_cast<CmdUpdateCharacterPCL>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Atualizou Character[TYPEID=" + (cmd_ucp.getInfo()._typeid) + ", ID=" + (cmd_ucp.getInfo().id) + "] PCL[C0=" + ((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_POWER]) + ", C1=" + ((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_CONTROL]) + ", C2=" + ((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_ACCURACY]) + ", C3=" + ((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_SPIN]) + ", C4=" + ((ushort)cmd_ucp.getInfo().pcl[(int)CharacterInfo.Stats.S_CURVE]) + "] do PLAYER [UID=" + (cmd_ucp.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 8: // Update ClubSet Stats
                    {
                        var cmd_ucss = Tools.reinterpret_cast<CmdUpdateClubSetStats>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Atualizou ClubSet[TYPEID=" + (cmd_ucss.getInfo()._typeid) + ", ID=" + (cmd_ucss.getInfo().id) + "] Stats[C0=" + ((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_POWER]) + ", C1=" + ((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_CONTROL]) + ", C2=" + ((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_ACCURACY]) + ", C3=" + ((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_SPIN]) + ", C4=" + ((ushort)cmd_ucss.getInfo().c[(int)CharacterInfo.Stats.S_CURVE]) + "] do PLAYER [UID=" + (cmd_ucss.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 9: // Update Character Mastery
                    {
                        var cmd_ucm = Tools.reinterpret_cast<CmdUpdateCharacterMastery>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Atualizou Character[TYPEID=" + (cmd_ucm.getInfo()._typeid) + ", ID=" + (cmd_ucm.getInfo().id) + "] Mastery[value=" + (cmd_ucm.getInfo().mastery) + "] do PLAYER [UID=" + (cmd_ucm.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 10: // Equipa Card
                    {
                        var cmd_ec = Tools.reinterpret_cast<CmdEquipCard>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Equipou Card[TYPEID=" + (cmd_ec.getInfo()._typeid) + "] no Character[TYPEID=" + (cmd_ec.getInfo().parts_typeid) + ", ID=" + (cmd_ec.getInfo().parts_id) + "] do PLAYER [UID=" + (cmd_ec.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 11: // Desequipa Card
                    {
                        var cmd_rec = Tools.reinterpret_cast<CmdRemoveEquipedCard>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Desequipou Card[TYPEID=" + (cmd_rec.getInfo()._typeid) + "] do Character[TYPEID=" + (cmd_rec.getInfo().parts_typeid) + ", ID=" + (cmd_rec.getInfo().parts_id) + "] do PLAYER [UID=" + (cmd_rec.getUID()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 12: // Update ClubSet Workshop
                    {
                        var cmd_ucw = Tools.reinterpret_cast<CmdUpdateClubSetWorkshop>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_ucw.getUID()) + "] Atualizou ClubSet[TYPEID=" + (cmd_ucw.getInfo()._typeid) + ", ID=" + (cmd_ucw.getInfo().id) + "] Workshop[C0=" + (cmd_ucw.getInfo().clubset_workshop.c[0]) + ", C1=" + (cmd_ucw.getInfo().clubset_workshop.c[1]) + ", C2=" + (cmd_ucw.getInfo().clubset_workshop.c[2]) + ", C3=" + (cmd_ucw.getInfo().clubset_workshop.c[3]) + ", C4=" + (cmd_ucw.getInfo().clubset_workshop.c[4]) + ", Level=" + (cmd_ucw.getInfo().clubset_workshop.level) + ", Mastery=" + (cmd_ucw.getInfo().clubset_workshop.mastery) + ", Rank=" + (cmd_ucw.getInfo().clubset_workshop.rank) + ", Recovery=" + (cmd_ucw.getInfo().clubset_workshop.recovery_pts) + "] Flag=" + (cmd_ucw.getFlag()) + "", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 13: // Update Tutorial
                    {
                        var cmd_ut = Tools.reinterpret_cast<CmdUpdateTutorial>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_ut.getUID()) + "] Atualizou Tutorial[Rookie=" + (cmd_ut.getInfo().rookie) + ", Beginner=" + (cmd_ut.getInfo().beginner) + ", Advancer=" + (cmd_ut.getInfo().advancer) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 14: // Tutorial Event Clear
                    {
                        var cmd_tec = Tools.reinterpret_cast<CmdTutoEventClear>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_tec.getUID()) + "] Concluiu Tutorial Event[Type=" + (cmd_tec.getType()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 15: // Use Item Buff
                    {
                        var cmd_uib = Tools.reinterpret_cast<CmdUseItemBuff>(_pangya_db);
                        break;
                    }
                case 16: // Update Item Buff
                    {
                        var cmd_uib = Tools.reinterpret_cast<CmdUpdateItemBuff>(_pangya_db);

                        // _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_uib.getUID()) + "] Atualizou o tempo do Item Buff[INDEX=" + (cmd_uib.getInfo().index) + ", TYPEID=" + (cmd_uib.getInfo()._typeid) + ", TIPO=" + (cmd_uib.getInfo().tipo) + ", DATE{REG_DT: " + _formatDate(cmd_uib.getInfo().use_date) + ", END_DT: " + _formatDate(cmd_uib.getInfo().end_date) + "}]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 17: // Update Card Special Time
                    {
                        var cmd_ucst = Tools.reinterpret_cast<CmdUpdateCardSpecialTime>(_pangya_db);

                        //  _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_ucst.getUID()) + "] Atualizou o tempo do Card Special[index=" + (cmd_ucst.getInfo().index) + ", TYPEID=" + (cmd_ucst.getInfo()._typeid) + ", EFEITO{TYPE: " + (cmd_ucst.getInfo().efeito) + ", QNTD: " + (cmd_ucst.getInfo().efeito_qntd) + "}, TIPO=" + (cmd_ucst.getInfo().tipo) + ", DATE{REG_DT: " + _formatDate(cmd_ucst.getInfo().use_date) + ", END_DT: " + _formatDate(cmd_ucst.getInfo().end_date) + "}]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 18: // Update Player Papel Shop Limit
                    {
                        break;
                    }
                case 19: // Insert Papel Shop Rare Win Log
                    {
                       
                        break;
                    }
                case 20: // Pay Caddie Holy Day (Paga as ferias do Caddie)
                    {
                        var cmd_pchd = Tools.reinterpret_cast<CmdPayCaddieHolyDay>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_pchd.getUID()) + "] Pagou as ferias do Caddie[ID=" + (cmd_pchd.getId()) + "] ate " + cmd_pchd.getEndDate(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 21: // Set Notice Caddie Holy Day (Seta Aviso de ferias do Caddie)
                    {
                        var cmd_snchd = Tools.reinterpret_cast<CmdSetNoticeCaddieHolyDay>(_pangya_db);

                        //_smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_snchd.getUID()) + "] setou Aviso[check=" + (cmd_snchd.getCheck() ? "ON" : "OFF") + "] de ferias do Caddie[ID=" + (cmd_snchd.getId()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 22: // Insert Box Rare Win Log
                    {
                        var cmd_ibrwl = Tools.reinterpret_cast<CmdInsertBoxRareWinLog>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_ibrwl.getUID()) + "] Inseriu Box[TYPEID=" + (cmd_ibrwl.getBoxTypeid()) + "] Rare[TYPEID=" + (cmd_ibrwl.getInfo()._typeid) + ", QNTD=" + (cmd_ibrwl.getInfo().qntd) + ", RARIDADE=" + ((ushort)cmd_ibrwl.getInfo().raridade) + "] Win Log", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 23: // Insert Spinning Cube Super Rare Win Broadcast
                    {
                        var cmd_ispcsrwb = Tools.reinterpret_cast<CmdInsertSpinningCubeSuperRareWinBroadcast>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Inseriu Spinning Cube Super Rare Win Broadcast[MSG=" + cmd_ispcsrwb.getMessage() + ", OPT=" + ((ushort)cmd_ispcsrwb.getOpt()) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 24: // Insert Memorial Shop Rare Win Log
                    {
                        var cmd_imrwl = Tools.reinterpret_cast<CmdInsertMemorialRareWinLog>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_imrwl.getUID()) + "] Inseriu Memorial Shop[COIN=" + (cmd_imrwl.getCoinTypeid()) + "] Rare[TYPEID=" + (cmd_imrwl.getInfo()._typeid) + ", QNTD=" + (cmd_imrwl.getInfo().qntd) + ", RARIDADE=" + (cmd_imrwl.getInfo().tipo) + "] Win Log", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        break;
                    }
                case 26: // Update Mascot Info
                    {

                        var cmd_umi = Tools.reinterpret_cast<CmdUpdateMascotInfo>(_pangya_db);

                        // _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_umi.getUID()) + "] Atualizar Mascot Info[TYPEID=" + (cmd_umi.getInfo()._typeid) + ", ID=" + (cmd_umi.getInfo().id) + ", LEVEL=" + ((ushort)cmd_umi.getInfo().level) + ", EXP=" + (cmd_umi.getInfo().exp) + ", FLAG=" + ((ushort)cmd_umi.getInfo().type) + ", TIPO=" + (cmd_umi.getInfo().tipo) + ", IS_CASH=" + ((ushort)cmd_umi.getInfo().is_cash) + ", PRICE=" + (cmd_umi.getInfo().price) + ", MESSAGE=" + (cmd_umi.getInfo().message) + ", END_DT=" + _formatDate(cmd_umi.getInfo().data) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 27: // Atualizou Guild Update Activity
                    {
                        // var cmd_uguai = Tools.reinterpret_cast<CmdUpdateGuildUpdateActiviy>(_pangya_db);

                        //_smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] Atualizou Guild Update Activity[INDEX=" + (cmd_uguai.getIndex()) + "] com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 28: // Atualizou Legacy Tiki Shop Point
                    {
                        var cmd_ultp = Tools.reinterpret_cast<CmdUpdateLegacyTikiShopPoint>(_pangya_db);

                        _smp.message_pool.getInstance().push(new message("[Channel::SQLDBResponse][Sucess] PLAYER [UID=" + (cmd_ultp.getUID()) + "] atualizou Legacy Tiki Shop Point(" + (cmd_ultp.getTikiShopPoint()) + ")", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        break;
                    }
                case 0:
                default: // 25 é update item equipado slot
                    break;
            }
        }
    }
}
