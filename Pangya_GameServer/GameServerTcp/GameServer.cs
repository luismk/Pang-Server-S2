using Pangya_GameServer.Game;
using Pangya_GameServer.Game.Manager; 
using Pangya_GameServer.Models;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.PangyaEnums;
using Pangya_GameServer.Repository;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaServer;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.Repository;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System.Diagnostics;
using static Pangya_GameServer.Models.DefineConstants;
namespace Pangya_GameServer.GameServerTcp
{
    public class GameServer : Server
    {
        public int m_access_flag { get; private set; }
        public int m_create_user_flag { get; private set; }
        public int m_same_id_login_flag { get; private set; }
        DailyQuestInfo m_dqi;
        protected List<Channel> v_channel;
        public BroadcastManager m_ticker = new BroadcastManager(30/*30 segundos para o ticker*/);
        public BroadcastManager m_notice = new BroadcastManager(60/*60 segundos 1 minuto para o notice*/);
        static PlayerManager m_player_manager = new PlayerManager();
        LoginManager m_login_manager;
        private bool m_active_room_log;

        [Obsolete]
        public GameServer() : base(m_player_manager)
        {
            // Inicializa config do Game Server
            config_init();
            // init Request Client packets
            init_Packets();
            //init create/load channels
            init_load_channels();
            // Inicializa os sistemas Globais
            init_systems();
            // Initialized complete
            m_state = ServerState.Initialized;
        }


        public override void config_init()
        {
            base.config_init();

            // Server Tipo
            m_si.tipo = 1;

            m_si.img_no = m_reader_ini.ReadInt16("SERVERINFO", "ICONINDEX");
            m_si.rate.exp = (short)m_reader_ini.readInt("SERVERINFO", "EXPRATE");
            m_si.rate.scratchy = (short)m_reader_ini.readInt("SERVERINFO", "SCRATCHY_RATE");
            m_si.rate.pang = (short)m_reader_ini.readInt("SERVERINFO", "PANGRATE");
            m_si.rate.club_mastery = (short)m_reader_ini.readInt("SERVERINFO", "CLUBMASTERYRATE");
            m_si.rate.papel_shop_rare_item = (short)m_reader_ini.readInt("SERVERINFO", "PAPEL_rate_RATE");
            m_si.rate.papel_shop_cookie_item = (short)m_reader_ini.readInt("SERVERINFO", "PAPEL_COOKIE_ITEM_RATE");
            m_si.rate.treasure = (short)m_reader_ini.readInt("SERVERINFO", "TREASURE_RATE");
            m_si.rate.memorial_shop = (short)m_reader_ini.readInt("SERVERINFO", "MEMORIAL_RATE");
            m_si.rate.chuva = (short)m_reader_ini.readInt("SERVERINFO", "CHUVA_RATE");
            m_si.rate.grand_zodiac_event_time = (short)(m_reader_ini.readInt("SERVERINFO", "GZ_EVENT") >= 1 ? 1 : 0);// Ativo por padrão
            m_si.rate.grand_prix_event = (short)(m_reader_ini.readInt("SERVERINFO", "GP_EVENT") >= 1 ? 1 : 0);// Ativo por padrão
            m_si.rate.golden_time_event = ((short)(m_reader_ini.readInt("SERVERINFO", "GOLDEN_TIME_EVENT") >= 1 ? 1 : 0));// Ativo por padrão
            m_si.rate.login_reward_event = ((short)(m_reader_ini.readInt("SERVERINFO", "LOGIN_REWARD") >= 1 ? 1 : 0));// Ativo por padrão
            m_si.rate.bot_gm_event = ((short)(m_reader_ini.readInt("SERVERINFO", "BOT_GM_EVENT") >= 1 ? 1 : 0));// Ativo por padrão
            m_si.rate.smart_calculator = (/*m_reader_ini.readInt("SERVERINFO", "SMART_CALC") >= 1 ? true :*/ 0);// Atibo por padrão
            m_si.rate.angel_event = ((short)(m_reader_ini.readInt("SERVERINFO", "ANGEL_EVENT") >= 1 ? 1 : 0));// Atibo por padrão
            m_active_room_log = (m_reader_ini.readInt("LOG", "ACTIVE_ROOM_LOG") >= 1 ? true : false);// Atibo por padrão

            try
            {
                m_si.flag.ullFlag = m_reader_ini.ReadUInt64("SERVERINFO", "FLAG");

                m_active_room_log = (m_reader_ini.readInt("LOG", "ACTIVE_ROOM_LOG") >= 1 ? true : false);// Atibo por padrão

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::config_init][ErrorSystem] Config.FLAG" + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }


            // Recupera Valores de rate do gs do banco de dados
            var cmd_rci = new CmdRateConfigInfo(m_si.uid);  // Waiter

            snmdb.NormalManagerDB.getInstance().add(0, cmd_rci, SQLDBResponse, this);


            if (cmd_rci.getException().getCodeError() != 0 || cmd_rci.isError()/*Deu erro na consulta não tinha o rate config info para esse gs, pode ser novo*/)
            {

                if (cmd_rci.getException().getCodeError() != 0)
                    _smp.message_pool.getInstance().push(new message("[GameServer::config_init][ErrorSystem] " + cmd_rci.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));


                setAngelEvent(m_si.rate.angel_event);
                setRatePang(m_si.rate.pang);
                setRateExp(m_si.rate.exp);
                setRateClubMastery(m_si.rate.club_mastery);

                snmdb.NormalManagerDB.getInstance().add(8, new CmdUpdateRateConfigInfo(m_si.uid, m_si.rate), SQLDBResponse, this);
            }
            else
            {   // Conseguiu recuperar com sucesso os valores do gs

                setAngelEvent(m_si.rate.angel_event);
                setRatePang(m_si.rate.pang);
                setRateExp(m_si.rate.exp);
                setRateClubMastery(m_si.rate.club_mastery);
            }
            m_si.app_rate = 100;    // Esse aqui nunca usei, deixei por que no DB do s4 tinha só cópiei
        }

        public bool getAccessFlag()
        {
            return m_access_flag == 1;
        }

        public bool getCreateUserFlag()
        {
            return m_create_user_flag == 1;
        }

        public bool canSameIDLogin()
        {
            return m_same_id_login_flag == 1;
        }

        // Set Event Server
        private void setAngelEvent(short _angel_event)
        {
            // Evento para reduzir o quit rate, diminui 1 quit a cada jogo concluído
            m_si.event_flag.angel_wing = _angel_event > 0;
            // Update rate Pang
            m_si.rate.angel_event = _angel_event; //precisa fazer isso, pois pode querer desativar
        }

        private void setRatePang(short _pang)
        {
            // Update Flag Event
            m_si.event_flag.pang_x_plus = (_pang >= 200) ? true : false;

            // Update rate Pang
            m_si.rate.pang = _pang;
        }

        private void setRateExp(short _exp)
        {// Reseta flag antes de atualizar ela 
            m_si.event_flag.exp_x2 = m_si.event_flag.exp_x_plus = false;

            // Update Flag Event
            if (_exp > 200)
                m_si.event_flag.exp_x_plus = true;
            else if (_exp == 200)
                m_si.event_flag.exp_x2 = true;
            else
                m_si.event_flag.exp_x2 = m_si.event_flag.exp_x_plus = false;

            // Update rate Experiência
            m_si.rate.exp = _exp;
        }

        private void setRateClubMastery(short _club_mastery)
        {
            // Update Flag Event
            m_si.event_flag.club_mastery_x_plus = (_club_mastery >= 200) ? true : false;

            // Update rate Club Mastery
            m_si.rate.club_mastery = _club_mastery;
        }

        public override void OnHeartBeat()
        {
            try
            {
                // Server ainda não está totalmente iniciado
                if (!this._isRunning)
                    return;
                 
                if (!sIff.getInstance().isLoad())
                    sIff.getInstance().initilation();
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::onHeartBeat][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public new void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {

            if (_arg == null)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::SQLDBResponse][Error] _arg is null na msg_id = " + (_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
                throw new exception("[GameServer::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError());

            var gs = (GameServer)(_arg);

            switch (_msg_id)
            {
                case 1: // DailyQuest Info
                    {

                        break;
                    }
                case 2: // Atualiza DailyQuest Info do server
                    {
                        // Atualiza daily quest
                        getDailyQuestInfo = ((CmdDailyQuestInfo)_pangya_db).getInfo(); // cmd_dqi.getInfo();
                        break;
                    }
                case 3: // Atualiza Chat Macro User
                    {
                        break;
                    }
                case 4: // Insert Msg Off
                    {
                        break;
                    }
                case 5: // Register Player Logon ON DB, 0 Login, 1 Logout
                    {
                        // Não usa por que é um UPDATE
                        break;
                    }
                case 6: // Insert Ticker no DB
                    {
                        break;
                    }
                case 7: // Register Logon do player no Server
                    {
                        // Não usa por que é um update
                        break;
                    }
                case 8: // Update Server Rate Config Info
                    {
                        break;
                    }
                case 9:     // Insert Block IP
                    {
                        break;
                    }
                case 10:    // Insert Block MAC
                    {
                        break;
                    }
                case 0:
                default:
                    break;
            }
        }


        public void sendDateTimeToSession(Player _session)
        {
            using (var p = new PangyaBinaryWriter((ushort)0xBA))
            {
                p.WriteTime();
                packet_func.session_send(p, _session);
            }
        }

        public void sendRankServer(Player _session)
        {

            try
            {

                if (_session.m_pi.block_flag.m_flag.rank_server)
                    throw new exception("[GameServer::sendRankServer][Error] PLAYER[UID=" + (_session.m_pi.uid)
                            + "] esta bloqueado o Rank Server, ele nao pode acessar o rank server.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 7010, 0));

                var cmd_sl = new CmdServerList(TYPE_SERVER.RANK);   // Waiter

                snmdb.NormalManagerDB.getInstance().add(0, cmd_sl, null, null);

                if (cmd_sl.getException().getCodeError() != 0)
                    throw cmd_sl.getException();

                var sl = cmd_sl.getServerList();

                if (sl.Count == 0)
                    throw new exception("[GameServer::sendRankServer][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                            + "] requisitou o Rank Server, mas nao tem nenhum Rank Server online no DB.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 7011, 0));

                using (var p = new PangyaBinaryWriter(0xA2))
                {
                    p.WritePStr(sl[0].ip);
                    p.WriteInt32(sl[0].port);
                    packet_func.session_send(p, _session);
                }


            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::sendRankServer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                using (var p = new PangyaBinaryWriter(0xA2))
                {
                    // Erro manda tudo 0
                    p.WriteUInt16(0);  // String IP
                    p.WriteUInt32(0);  // Port
                    packet_func.session_send(p, _session);
                }
            }
        }

        public Channel findChannel(sbyte _channel)
        {
            if (_channel == 255)
                return null;

            for (var i = 0; i < v_channel.Count; ++i)
                if (v_channel[i].getId() == _channel)
                    return v_channel[i];

            return null;
        }

        public Channel findChannel(Channel _channel)
        {
            if (_channel == null)
                return null;

            for (var i = 0; i < v_channel.Count; ++i)
                if (v_channel[i].getId() == _channel.getId())
                    return v_channel[i];

            return null;
        }

        public Player findPlayer(uint _uid, bool _oid = false)
        {
            return (Player)(_oid ? FindSessionByOid(_uid) : FindSessionByUid(_uid));
        }

        public void blockOID(int _oid) { m_player_manager.blockOID(_oid); }
        public void unblockOID(int _oid) { m_player_manager.unblockOID(_oid); }

        public DailyQuestInfo getDailyQuestInfo { get => m_dqi; set => m_dqi = value; }


        // Update Daily Quest Info
        public void updateDailyQuest(DailyQuestInfo _dqi)
        {
            m_dqi = _dqi;
        }



        public override void authCmdShutdown(int _time_sec)
        {
            try
            {

                
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[RankingServer::authCmdShutdown][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }


        public override void shutdown_time(int _time_sec)
        {

            if (_time_sec <= 0) // Desliga o Server Imediatemente
                base.shutdown();
            else
            {
                       }
        }
        public override void authCmdBroadcastNotice(string _notice)
        {

            try
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdBroadcastNotice][Error] Auth Server Comando Notice[MESSAGE=" + _notice + "].", type_msg.CL_FILE_LOG_AND_CONSOLE));

                var notice = _notice;

#if DEBUG
                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdBroadcasNotice][Error] Message. Hex: " + notice, type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif // DEBUG

                m_notice.push_back(0, notice, BroadcastManager.TYPE.GM_NOTICE);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdBroadcastNotice][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void authCmdBroadcastTicker(string _nickname, string _msg)
        {

            try
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdBroadcastTicker][Error] Auth Server Comando Ticker[NICKNAME="
                        + _nickname + ", MESSAGE=" + _msg + "].", type_msg.CL_FILE_LOG_AND_CONSOLE));

                m_ticker.push_back(0, _nickname, _msg, BroadcastManager.TYPE.TICKER);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdBroadcastTicker][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void authCmdBroadcastCubeWinRare(string _msg, uint _option)
        {

            try
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdBroadcastCubeWinRare][Error] Auth Server Comando Cube Win Rare Notice[MESSAGE="
                        + _msg + ", OPTION=" + (_option) + "].", type_msg.CL_FILE_LOG_AND_CONSOLE));

                m_notice.push_back(0, _msg, _option, BroadcastManager.TYPE.CUBE_WIN_RARE);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdBroadcastCubeWinRare][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void authCmdDisconnectPlayer(uint _req_server_uid, uint _player_uid, byte _force)
        {

            try
            {

                var s = m_player_manager.findPlayer(_player_uid);

                if (s != null)
                {

                    // Log
                    //_smp.message_pool.getInstance().push(new message("[GameServer::authCmdDisconnectPlayer][log] Comando do Auth Server, Server[UID=" + (_req_server_uid)
                    //        + "] pediu para desconectar o PLAYER[UID=" + (s.m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Deconecta o Player
                    if (_force == 1) // Força o Disconect do player, sem verificar as regras do Game Server
                        DisconnectSession(s);
                    else
                    {

                        // Read Ini File for take Flag Same Id Login

                        int same_id_login = 0;

                        try
                        {
                            same_id_login = m_reader_ini.readInt("OPTION", "SAME_ID_LOGIN", 0);
                        }
                        catch
                        {

                        }

                        // Só desconecta aqui se a flag do server de poder logar com o mesmo id estiver desativada
                        if (!(same_id_login == 1))
                            DisconnectSession(s);
                    }

                }
                else
                {

                    // Não encontrou o player no server, então desconecta no banco de dados
                    snmdb.NormalManagerDB.getInstance().add(5, new CmdRegisterLogon(_player_uid, 1/*Logout*/), SQLDBResponse, this);

                    // Log
                    //_smp.message_pool.getInstance().push(new message("[GameServer::authCmdDisconnectPlayer][Warning] Comando do Auth Server, Server[UID=" + (_req_server_uid)
                    //        + "] pediu para desconectar o PLAYER[UID=" + (_player_uid) + "], mas nao encontrou ele no server, entao desconecta ele no banco de dados.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

                // UPDATE ON Auth Server
                m_unit_connect.sendConfirmDisconnectPlayer(_req_server_uid, _player_uid);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdDisconnectPlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void authCmdConfirmDisconnectPlayer(uint _player_uid)
        {
            // Game Server não usa esse Comando
            return;
        }

        public override void authCmdNewMailArrivedMailBox(uint _player_uid, int _mail_id)
        {

            try
            {

                var s = m_player_manager.findPlayer(_player_uid);

                if (s == null)
                {
                    _smp.message_pool.getInstance().push("[GameServer::authCmdNewMailArrivedMailBox][Warning] Auth Server Comando New Mail[ID=" + (_mail_id)
                           + "] Arrived no Mailbox do PLAYER[UID=" + (_player_uid) + "], mas o player nao esta conectado no server.", type_msg.CL_ONLY_FILE_LOG);
                    return;
                }
                if (_player_uid <= 0)
                {
                    return;
                }

                s.m_pi.m_mail_box.addNewEmailArrived(_player_uid, _mail_id);

                var v_mi = s.m_pi.m_mail_box.getAllUnreadEmail();

                if (v_mi.empty())
                    throw new exception("[GameServer::authCmdNewMailArrivedMailBox][Error] Auth Server Comando New Mail[ID=" + (_mail_id)
                            + "] Arrived no Mailbox do PLAYER[UID=" + (_player_uid) + "], mas nao tem nenhum email nao lido no Mailbox dele.",
                           ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 131, 0));

                // UPDATE ON GAME
                var p = new PangyaBinaryWriter(0x210);

                p.WriteUInt32(0);   // OK

                p.WriteInt32(v_mi.Count);   // Count

                foreach (var el in v_mi)
                    p.WriteBytes(el.ToArray());

                packet_func.session_send(p, s, 1);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdNewMailArrivedMailBox][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void authCmdNewRate(uint _tipo, uint _qntd)
        {

            try
            {

                updateRateAndEvent((int)_tipo, _qntd);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdNewRate][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void authCmdReloadGlobalSystem(uint _tipo)
        {

            try
            {

                reloadGlobalSystem(_tipo);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdReloadGlobalSystem][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override void authCmdConfirmSendInfoPlayerOnline(uint _req_server_uid, AuthServerPlayerInfo _aspi)
        {

            // Game Server ainda não usa esse funcionalidade de pedir o info do player que está conectado em outro server
            //ignorar . UNREFERENCED_PARAMETER(_req_server_uid);

            try
            {

                var s = m_player_manager.findPlayer(_aspi.uid);

                if (s != null)
                {

                    //confirmLoginOnOtherServer(*s, _req_server_uid, _aspi);

                }
                else
                    _smp.message_pool.getInstance().push(new message("[GameServer::authCmdConfirmSendInfoPlayerOnline][Warning] PLAYER[UID=" + (_aspi.uid)
                            + "] retorno do confirma login com Auth Server do Server[UID=" + (_req_server_uid) + "], mas o palyer nao esta mais conectado.", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::authCmdConfirmSendInfoPlayerOnline][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override bool CheckCommand(Queue<string> _command)
        {
            Console.ResetColor();

            if (_command.Count == 0)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::CheckCommand][Error] Missing parameter", type_msg.CL_ONLY_CONSOLE));
                return true;
            }

            string s = _command.Dequeue();

            if (!string.IsNullOrEmpty(s) && s == "exit")
            {
                return true;
            }
            else if (!string.IsNullOrEmpty(s) && s == "reload_files")
            {
                reload_files();
                return true;
            }
            else if (!string.IsNullOrEmpty(s) && s == "rate")
            {
                string sTipo = _command.Dequeue();
                int tipo = -1;

                if (!string.IsNullOrEmpty(sTipo))
                {
                    switch (sTipo)
                    {
                        case "pang": tipo = 0; break;
                        case "exp": tipo = 1; break;
                        case "club": tipo = 2; break;
                        case "chuva": tipo = 3; break;
                        case "treasure": tipo = 4; break;
                        case "scratchy": tipo = 5; break;
                        case "pprareitem": tipo = 6; break;
                        case "ppcookieitem": tipo = 7; break;
                        case "memorial": tipo = 8; break;
                        default:
                            _smp.message_pool.getInstance().push(new message($"[GameServer::checkCommand][Error] Unknown Command: \"rate {sTipo}\"", type_msg.CL_ONLY_CONSOLE));
                            break;
                    }
                }
                else
                {
                    _smp.message_pool.getInstance().push(new message($"[GameServer::checkCommand][Error] Unknown Command: \"rate {sTipo}\"", type_msg.CL_ONLY_CONSOLE));
                }

                if (tipo != -1 && tipo >= 0 && tipo <= 8)
                {
                    if (uint.TryParse(_command.Dequeue(), out uint qntd) && qntd > 0)
                    {
                        updateRateAndEvent(tipo, qntd);
                    }
                    else
                    {
                        _smp.message_pool.getInstance().push(new message($"[GameServer::checkCommand][Error] Unknown value, Command: \"rate {sTipo}\"", type_msg.CL_ONLY_CONSOLE));
                    }
                }
                return true;
            }
            else if (!string.IsNullOrEmpty(s) && s == "event")
            {
                s = _command.Dequeue();
                uint qntd = 0;

                if (!string.IsNullOrEmpty(s))
                {
                    qntd = uint.Parse(_command.Dequeue());

                    switch (s)
                    {
                        case "grand_zodiac_event":
                            updateRateAndEvent(9, qntd);
                            break;
                        case "angel_event":
                            updateRateAndEvent(10, qntd);
                            break;
                        case "grand_prix":
                            updateRateAndEvent(11, qntd);
                            break;
                        case "golden_time":
                            updateRateAndEvent(12, qntd);
                            break;
                        case "login_reward":
                            updateRateAndEvent(13, qntd);
                            break;
                        case "bot_gm_event":
                            updateRateAndEvent(14, qntd);
                            break;
                        case "smart_calc":
                            updateRateAndEvent(15, qntd);
                            break;
                        default:
                            _smp.message_pool.getInstance().push(new message($"[GameServer::checkCommand][Error] Unknown Comamnd: \"Event {s}\"", type_msg.CL_ONLY_CONSOLE));
                            break;
                    }
                }
                return true;
            }
            else if (!string.IsNullOrEmpty(s) && s == "reload_system")
            {
                string sTipo = _command.Dequeue();
                int tipo = -1;

                if (!string.IsNullOrEmpty(sTipo))
                {
                    switch (sTipo)
                    {
                        case "all": tipo = 0; break;
                        case "iff": tipo = 1; break;
                        case "card": tipo = 2; break;
                        case "comet_refill": tipo = 3; break;
                        case "papel_shop": tipo = 4; break;
                        case "box": tipo = 5; break;
                        case "memorial_shop": tipo = 6; break;
                        case "cube_coin": tipo = 7; break;
                        case "treasure_hunter": tipo = 8; break;
                        case "drop": tipo = 9; break;
                        case "attendance_reward": tipo = 10; break;
                        case "map_course": tipo = 11; break;
                        case "approach_mission": tipo = 12; break;
                        case "grand_zodiac_event": tipo = 13; break;
                        case "coin_cube_location": tipo = 14; break;
                        case "golden_time": tipo = 15; break;
                        case "login_reward": tipo = 16; break;
                        case "bot_gm_event": tipo = 17; break;
                        case "smart_calc": tipo = 18; break;
                        default:
                            _smp.message_pool.getInstance().push(new message($"[GameServer::checkCommand][Error] Unknown Command: \"reload_system {sTipo}\"", type_msg.CL_ONLY_CONSOLE));
                            break;
                    }
                }
                else
                {
                    _smp.message_pool.getInstance().push(new message($"[GameServer::checkCommand][Error] Unknown Command: \"reload_system {sTipo}\"", type_msg.CL_ONLY_CONSOLE));
                }

                if (tipo != -1 && tipo >= 0 && tipo <= 18)
                {
                    reloadGlobalSystem((uint)tipo);
                }
                return true;

            }

            else if (s == "scratch")		// !@ Teste
            {
                var p = new PangyaBinaryWriter(0x1EB);
                p.WriteUInt32(0); // Count
                p.WriteByte(0); // Count
                foreach (var c in v_channel)
                    packet_func.channel_broadcast(c, p, 1);
                return true;
            }

            else if (s == "cls" || s == "clear")
            {
                Console.Clear();
                return true;
            }
            else
            {
                _smp.message_pool.getInstance().push(new message($"[GameServer::CheckCommand][Error] Command No Exist-> {s}", type_msg.CL_ONLY_CONSOLE));
                return false;
            }
        }

        public void reload_files()
        {
            base.config_init();
            config_init();

            // Reload All Globals Systems
            reload_systems();

            _smp.message_pool.getInstance().push(new message("[game_server::reload_files][Log] Reload System now sucess!", type_msg.CL_FILE_LOG_AND_CONSOLE));

            // UPDATE ON GAME
            var p = new PangyaBinaryWriter(0xF9);

            p.WriteBytes(m_si.ToArray());

            foreach (var el in v_channel)
                packet_func.channel_broadcast(el, p, 1);
        }

        public void init_systems()
        {
            m_login_manager = new LoginManager();

            // SINCRONAR por que se não alguem pode pegar lixo de memória se ele ainda nao estiver inicializado
            var cmd_dqi = new CmdDailyQuestInfo();

            snmdb.NormalManagerDB.getInstance().add(1, cmd_dqi, SQLDBResponse, this);

            if (cmd_dqi.getException().getCodeError() != 0)
                throw new exception("[GameServer::game_server][Error] nao conseguiu pegar o Daily Quest Info[Exption: "
                    + cmd_dqi.getException().getFullMessageError() + "]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 277, 0));

            // Initialize Daily Quest of Server
            m_dqi = cmd_dqi.getInfo();

            if (!sIff.getInstance().isLoad())
                sIff.getInstance().initilation();  
        }

        [Obsolete]
        private void init_Packets()
        {
            // --- REQUESTS DO CLIENTE (LOBBY / SISTEMA) ---
            // packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_KEEP_ALIVE, packet_func.packet001, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQ_LOGIN, packet_func.packet002, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CHAT_MESSAGE, packet_func.packet005, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQ_ENTER_CHANNEL, packet_func.packet007, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_LEAVE_LOBBY, packet_func.packet008, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CREATE_ROOM, packet_func.packet00E, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_JOIN_ROOM, packet_func.packet00F, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CHANGE_ROOM_INFO, packet_func.packet011, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_USER_INFO_CHANGE, packet_func.packet013, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_START_GAME, packet_func.packet01B, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_EXIT_ROOM, packet_func.packet01D, this);//sai da sala né
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_USER_LIST, packet_func.packet01F, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CADDIE_ABILITY, packet_func.packet02E, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_UPDATE_EQUIP, packet_func.packet3A, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_INVITE, packet_func.packet049, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_QUICK_INVITE, packet_func.packet04A, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQUEST_USER_LIST, packet_func.packet04C, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQUEST_ROOM_LIST, packet_func.packet04D, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQUEST_HELP_DESC, packet_func.packet04E, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQUEST_DETAIL_ROOM, packet_func.packet050, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQUEST_USER_STATE, packet_func.packet52, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQUEST_USER_INFO, packet_func.packet53, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_SLEEP, packet_func.packet057, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REPORT_ERROR, packet_func.packet058, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REPORT_HACK, packet_func.packet066, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_JOIN_GALLERY, packet_func.packet067, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_GAME_SHOT_COMMAND, packet_func.packet06B, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQ_SERVER_LIST, packet_func.packet6C, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQ_TITLE_LIST, packet_func.packet6D, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_SET_JJANG, packet_func.packet070, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_ENCHANT, packet_func.packet075, this);
            //packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_SET_TITLE, packet_func.packet07B, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQ_IDENTITY, packet_func.packet6A, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CHECK_PCBANG, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Stats, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQ_BUY_SHOP, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Stats, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_MAIL_BOX, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Stats, this);

            // --- REQUESTS DENTRO DA SALA (PARTIDA/GAMEPLAY) ---
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_STATISTICS, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Stats, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_USER_READY, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Ready, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CHANGE_TEAM, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Team, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_GCP_LOAD_OK, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_LoadOk, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_GCP_SHOT, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Shot, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_GAME_CAMERA, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Camera, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CLICK, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Click, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_POWER_SHOT, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Power, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CLUB, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Club, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_USE_ITEM, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Item, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_EMOTICON, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Emo, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_DROP, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Drop, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_HOLE_INFO, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_HoleInfo, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_SHOT_RESULT, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_ShotResult, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_SHOT_ACK, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_ShotAck, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_TIME_CHECK, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Time, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_SKIP, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Skip, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQUEST_VOTE, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_VoteReq, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_VOTE_FOR_BANISH, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_VoteAction, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_PAUSE, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Pause, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_HOLE_STAT, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_HoleStat, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_TEESHOT_READY, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_TeeShot, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_TEAM_HOLEIN_PANG, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_TeamPang, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_ANSWER_GOSTOP, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_GoStop, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_END_STROKE_GAME, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_EndGame, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REQ_CHANGE_NICKNAME, packet_func.packet5D, this);//packet_func.packet_Room_EndGame, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REPORT, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Report, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CHANGE_TARGET, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Target, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_PLAYINFO, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_PlayInfo, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_LOADING_INFO, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_LoadInfo, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_REPLAY, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_Replay, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_BANISH_ALL, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_BanishAll, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_CHAT_PENALTY, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_ChatPenalty, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_MATCH_HOLEIN_PANG, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_MatchPang, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_TEAM_CHAT, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_TeamChat, this);
            packet_func.funcs.addPacketCall((ushort)PacketIDClient.CLIENT_GAMEPLAY_INFO, packet_func.packet_ClientFazNada, this);//packet_func.packet_Room_GameInfo, this);

            //RESPONSE -> REQUEST_CLIENT
            packet_func.funcs_sv.addPacketCall(0x01, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x06, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x11, packet_func.packet_svFazNada, this);//chanais
            packet_func.funcs_sv.addPacketCall(0x45, packet_func.packet_svFazNada, this);//perso
            packet_func.funcs_sv.addPacketCall(0x46, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x47, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x48, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x49, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x97, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0x3F, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0xC6, packet_func.packet_svFazNada, this);
            packet_func.funcs_sv.addPacketCall(0xC1, packet_func.packet_svFazNada, this);

            // --- COMANDOS DO AUTH SERVER ---
            packet_func.funcs_as.addPacketCall(0x1, packet_func.packet_as001, this);

        }

        Game.Channel CreateChannel(sbyte _id, string _name, short _max_user, short players, uint flag)
        {

            string flagsAtivas = GetActiveFlags(new ChannelInfo.UFlag(flag));
            _smp.message_pool.getInstance().push(new message(
                $"[Channel Loaded] ID: {_id} | Name: {_name} | Flags: {flagsAtivas}",
                type_msg.CL_ONLY_CONSOLE));

            return new Game.Channel(new ChannelInfo() { id = _id, name = _name, max_user = _max_user, curr_user = players, type = new ChannelInfo.UFlag(flag) }, m_si.propriedade);
        }

        public void init_load_channels()
        {
            v_channel = [];
            try
            {
                // Lê a quantidade total de canais da seção principal
                int num_channel = m_reader_ini.readInt("CHANNELINFO", "NUM_CHANNEL");

                for (sbyte i = 0; i < num_channel; ++i)
                {
                    ChannelInfo ci = new();
                    string section = "CHANNEL" + (i + 1);

                    try
                    {
                        ci.id = i;
                        ci.name = m_reader_ini.ReadString(section, "NAME");
                        ci.max_user = m_reader_ini.ReadInt16(section, "MAXUSER");
                        ci.min_level_allow = m_reader_ini.ReadUInt32(section, "LOWLEVEL");
                        ci.max_level_allow = m_reader_ini.ReadUInt32(section, "MAXLEVEL");

                        // Lê a FLAG (Bitmask) que ativa as propriedades da classe UFlag
                        ci.type.ulFlag = m_reader_ini.ReadUInt32(section, "FLAG");

                        // Log Informativo para conferência de Flags (Opcional)
                        string flagsAtivas = GetActiveFlags(ci.type);
                        _smp.message_pool.getInstance().push(new message(
                            $"[Channel Loaded] ID: {ci.id} | Name: {ci.name} | Flags: {flagsAtivas}",
                            type_msg.CL_ONLY_CONSOLE));
                    }
                    catch (Exception e)
                    {
                        _smp.message_pool.getInstance().push(new message(
                            $"[GameServer::init_load_channels][Error na Seção {section}] " + e.Message,
                            type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }

                    v_channel.Add(new Channel(ci, m_si.propriedade));
                }
            }
            catch (Exception e)
            {
                _smp.message_pool.getInstance().push(new message(
                    "[GameServer::init_load_channels][ErrorSystem Global] " + e.Message,
                    type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Helper para debugar quais flags o INI ativou no bitmask
        private string GetActiveFlags(ChannelInfo.UFlag type)
        {
            List<string> f = new List<string>();
             if (type.ladder) f.Add("Ladder");
            if (type.tourney) f.Add("Tourney");
            if (type.vs) f.Add("Versus"); 
            return f.Count > 0 ? string.Join("|", f) : "Normal";
        }

        public void reload_systems()
        { 
            // Recarrega IFF_STRUCT
            sIff.getInstance().reload(); 
        }


        public void reloadGlobalSystem(uint _tipo)
        {
            try
            {
                switch (_tipo)
                {
                    case 0:     // Reload All Globals Systems
                        reload_systems();
                        break;

                    case 1:     // IFF
                                // Recarrega IFF_STRUCT
                        sIff.getInstance().reload();
                        break;

                    case 18:    // Smart Calculator Lib
                                // Recarrega Smart Calculator Lib
                                // sSmartCalculator.getInstance().load();
                        break;

                    default:
                        throw new Exception($"[GameServer::reloadGlobalSystem][Error] Tipo[VALUE={_tipo}] desconhecido.");
                }

                // Log
                _smp.message_pool.getInstance().push(
                     new message($"[GameServer::reloadGlobalSystem][Log] Recarregou o Sistema[Tipo={_tipo}] com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE)
                 );
            }
            catch (Exception e)
            {
                _smp.message_pool.getInstance().push(
                     new message($"[GameServer::reloadGlobalSystem][ErrorSystem] {e.Message}", type_msg.CL_FILE_LOG_AND_CONSOLE)
                 );
            }
        }


        // Update rate e Event of Server

        public void updateRateAndEvent(int _tipo, uint _qntd)
        {
            try
            {

                if (_qntd == 0u && _tipo != 9/*Grand Zodiac Event Time*/ && _tipo != 10/*Angel Event*/
                    && _tipo != 11/*Grand Prix Event*/ && _tipo != 12/*Golden Time Event*/ && _tipo != 13/*Login Reward Event*/
                    && _tipo != 14/*Bot GM Event*/ && _tipo != 15/*Smart Calculator*/)
                    throw new exception("[GameServer::updateRateAndEvent][Error] Rate[TIPO=" + (_tipo) + ", QNTD="
                            + (_qntd) + "], qntd is invalid(zero).", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 120, 0));

                switch (_tipo)
                {
                    case 0: // Pang
                        setRatePang((short)_qntd);
                        break;
                    case 1: // Exp
                        setRateExp((short)_qntd);
                        break;
                    case 2: // Mastery
                        setRateClubMastery((short)_qntd);
                        break;
                    case 3: // Chuva
                        m_si.rate.chuva = (short)_qntd;
                        break;
                    case 4: // Treasure Hunter
                        m_si.rate.treasure = (short)_qntd;
                        break;
                    case 5: // Scratchy
                        m_si.rate.scratchy = (short)_qntd;
                        break;
                    case 6: // Papel Shop Rare Item
                        m_si.rate.papel_shop_rare_item = (short)_qntd;
                        break;
                    case 7: // Papel Shop Cookie Item
                        m_si.rate.papel_shop_cookie_item = (short)_qntd;
                        break;
                    case 8: // Memorial shop
                        m_si.rate.memorial_shop = (short)_qntd;
                        break;
                    case 9: // Event Grand Zodiac Time Event [Active/Desactive]
                        {
                            m_si.rate.grand_zodiac_event_time = (short)_qntd;

                            // Recarrega o Grand Zodiac Event se ele foi ativado
                            if (m_si.rate.grand_zodiac_event_time == 1)
                                reloadGlobalSystem(13/*Grand Zodiac Event*/);

                            break;
                        }
                    case 10: // Event Angel (Reduce 1 quit per game done)
                        setAngelEvent((short)_qntd);
                        break;
                    case 11: // Grand Prix Event
                        m_si.rate.grand_prix_event = (short)_qntd;
                        break;
                    case 12: // Golden Time Event
                        {
                            m_si.rate.golden_time_event = (short)_qntd;

                            // Recarrega o Golden Time Event se ele foi ativado
                            if (m_si.rate.golden_time_event == 1)
                                reloadGlobalSystem(15/*Golden Time Event*/);

                            break;
                        }
                    case 13: // Login Reward System Event
                        {
                            m_si.rate.login_reward_event = (short)_qntd;

                            // Recarrega o Login Reward Event se ele foi ativado
                            if (m_si.rate.login_reward_event == 1)
                                reloadGlobalSystem(16/*Login Reward Event*/);

                            break;
                        }
                    case 14: // Bot GM Event
                        {
                            m_si.rate.bot_gm_event = (short)_qntd;

                            // Recarrega o Bot GM Event se ele foi ativado
                            if (m_si.rate.bot_gm_event == 1)
                                reloadGlobalSystem(17/*Bot GM Event*/);

                            break;
                        }
                    case 15: // Smart Calculator
                        {
                            m_si.rate.smart_calculator = (short)_qntd;

                            // Recarrega o Smart Calculator System se ele foi ativado
                            if (m_si.rate.smart_calculator == 1)
                                reloadGlobalSystem(18/*Smart Calculator*/);

                            break;
                        }
                    default:
                        throw new exception("[GameServer::updateRateAndEvent][Error] troca Rate[TIPO=" + (_tipo) + ", QNTD="
                                + (_qntd) + "], tipo desconhecido.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 120, 0));
                }

                // Update no DB os server do server que foram alterados
                snmdb.NormalManagerDB.getInstance().add(8, new CmdUpdateRateConfigInfo(m_si.uid, m_si.rate), SQLDBResponse, this);

                // Log
                _smp.message_pool.getInstance().push(new message("[GameServer::updateRateAndEvent][Error] New Rate[Tipo=" + (_tipo) + ", QNTD="
                        + (_qntd) + "] com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // UPDATE ON GAME
                var p = new PangyaBinaryWriter(0xF9);

                p.WriteBytes(m_si.ToArray());

                foreach (var el in v_channel)
                    packet_func.channel_broadcast(el, p, 1);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::updateRateAndEvent][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }


        // Shutdown With Time


        // Check Player Itens

        public void check_player()
        {
            while (this._isRunning)
            {
                if (m_state != ServerState.Good)
                    continue;

                // Verifica Game Guard Auth do player
                //if (m_GameGuardAuth)
                //    m_player_manager.checkPlayersGameGuard();

                // Verifica se os itens dos players está tudo normal
                m_player_manager.checkPlayersItens();
                //vai dormir por 10000 milessimos
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Send Hello Packet
        /// </summary>
        /// <param name="_session"></param>
        protected override void onAcceptCompleted(Session _session)
        {
            try
            {  
                var _packet = new packet(0x01);    // Tipo Packet Game Server initial packet no compress e no crypt 
                _packet.AddByte(1); // OPTION 1
                _packet.AddByte(1); // OPTION 2
                _packet.AddInt32(_session.m_key); // key
                 var mb = _packet.MakePacketComplete(-1);

                _session.requestSendBuffer(mb, true); 

                _smp.message_pool.getInstance().push(
                    new message($"[GameServer::onAcceptCompleted][Sucess] [IP: {_session.getIP()}, Key: {_session.m_key}]",
                                type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            catch (exception ex)
            {
                _smp.message_pool.getInstance().push(new message(
              $"[GameServer::onAcceptCompleted][ErrorSt]: {ex.getFullMessageError()}",
              type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public override bool CheckPacket(Session _session, packet packet, int opt = 0)
        {
            var player = (Player)_session;
            var packetId = packet.Id;
            var uid = player.m_pi.uid;


            switch (opt)
            {
                case 1:
                    // Verifica se o valor de packetId é válido no enum PacketIDClient
                    if (Enum.IsDefined(typeof(PacketIDClient), (PacketIDClient)packetId))
                    {
                        if (packetId != 244)
                            _smp.message_pool.getInstance().push(new message("[GameServer::CheckPacket][Debug] PLAYER[UID: " + (uid == 0 ? player.m_ip : uid.ToString()) + ", PID: " + (PacketIDClient)packetId + "]", type_msg.CL_ONLY_CONSOLE));
                        return true;
                    }
                    else// nao tem no PacketIDClient
                    {
                        _smp.message_pool.getInstance().push(new message($"[GameServer::CheckPacket][Info]: PLAYER[UID: {player.m_pi.uid}, CGPID: 0x{packet.Id:X}, CGPID_LOG: {packet.Log()}]\n", type_msg.CL_ONLY_CONSOLE));
                        return true;
                    }
                default:
                    // Verifica se o valor de packetId é válido no enum PacketIDServer
                    if (Enum.IsDefined(typeof(PacketIDServer), (PacketIDServer)packetId))
                    {
                        Debug.WriteLine($"[GameServer::CheckPacket][Info]: PLAYER[UID: {player.m_pi.uid}, SGPID: {(PacketIDServer)packetId}]", ConsoleColor.Cyan);
                        return true;
                    }
                    else// nao tem no PacketIDServer
                    {
                        Debug.WriteLine($"[GameServer::CheckPacket][Info]: PLAYER[UID: {player.m_pi.uid}, SGPID: 0x{packet.Id:X}]");
                        return true;
                    }
            }
        }


        public override void onDisconnected(Session _session)
        {
            if (_session == null)
                throw new exception("[GameServer::onDisconnected][Error] _session is null");

            var _player = (Player)_session;

            _smp.message_pool.getInstance().push(new message("[GameServer::onDisconnected][Warning] PLAYER[ID: " + _player.m_pi.id + "  UID: " + _player.m_pi.uid + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            /// Novo
            var _channel = findChannel(_player.m_pi.channel);

            try
            {

                if (_channel != null)
                    _channel.leaveChannel(_player);

            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::onDisconnect][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        //chama alguma coisa aqui!
        public override void OnStart()
        {
            Console.Title = $"Game Server - P: {m_si.curr_user}";

            // Inicilializa Thread que cuida de verificar todos os itens do players, estão com o tempo normal
            new Thread(check_player)
            {
                IsBackground = true
            }.Start();
        }

        public Channel enterChannel(Player _session, sbyte _channel)
        {
            Channel enter = null, last = null;
            try
            {

                if ((enter = findChannel(_channel)) == null)
                {
                    return null;
                }


                if (enter.getId() == _session.m_pi.channel)
                {
                    return enter;   // Ele já está nesse canal
                }

                if (enter.isFull())
                {
                    // Não conseguiu entrar no canal por que ele está cheio, deixa o enter como null
                    enter = null;
                }
                else
                {
                    try
                    {
                        // Verifica se pode entrar no canal
                        enter.checkEnterChannel(_session);

                        // Sai do canal antigo se ele estiver em outro canal
                        if (_session.m_pi.channel != DEFAULT_CHANNEL && (last = findChannel(_session.m_pi.channel)) != null)
                            last.leaveChannel(_session);

                        _session.m_channel = enter;
                        // Entra no canal
                        enter.enterChannel(_session);
                    }

                    catch (exception e)
                    {
                        _smp.message_pool.getInstance().push(new message("[GameServer::enterChannel][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::enterChannel][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return enter;
        }

        public void requestChangeChatMacroUser(Player _session, packet _packet)
        {
            try
            {

                // Verifica se session está autorizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login

                chat_macro_user cmu = new chat_macro_user();
                for (int i = 0; i < 9; i++)
                {
                    cmu.setMacro(i, _packet.ReadPStr(64));

                    // Se vazio substitiu por um macro padrão
                    if (string.IsNullOrEmpty(cmu.macro[i].text))
                        cmu.macro[i].text = "PangYa! Por favor configure seu chat macro";

                }
                // UPDATE ON GAME

                _session.m_pi.cmu = cmu;

                // UPDATE ON DB
                snmdb.NormalManagerDB.getInstance().add(3, new CmdUpdateChatMacroUser(_session.m_pi.uid, _session.m_pi.cmu), SQLDBResponse, this);

            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::requestChangeChatMacroUser][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
         
        public void requestChangeWhisperState(Player _session, packet _packet)
        {
            try
            {

                var whisper = _packet.ReadByte();

                // Verifica se session está autorizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //CHECK_SESSION_IS_AUTHORIZED("ChangeWisperState");

                if (whisper > 1)
                    throw new exception("[GameServer::requestChangeWhisperState][Error] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou alterar o estado do Whisper[state="
                            + ((ushort)whisper) + "], mas ele mandou um valor invalido. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 0x5300101));

                _session.m_pi.mi.state_flag.whisper = (_session.m_pi.whisper = whisper) == 1 ? true : false;

                // Log
                _smp.message_pool.getInstance().push(new message("[Whisper::ChangeState][Info] PLAYER[UID=" + (_session.m_pi.uid) + "] trocou o Whisper State para : " + (whisper.IsTrue() ? ("ON") : ("OFF")), type_msg.CL_FILE_LOG_AND_CONSOLE));


            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::requestChangeWhisperState][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestChat(Player _session, packet _packet)
        {

            try
            {
                string nickname = _packet.ReadPStr();
                string msg = _packet.ReadPStr();

                // 4. Envia mensagem para GMs
                var c = findChannel(_session.m_pi.channel);
                if (c != null)
                {
                    var gmList = FindAllGM();

                    if (gmList.Any())
                    {
                        string msg_gm = "\\5" + _session.m_pi.nickname + ": '" + msg + "'";
                        string from = "\\1[Channel=" + c.getInfo().name + ", \\1ROOM=" + _session.m_pi.mi.sala_numero + "]";

                        int index = from.IndexOf(' ');
                        if (index != -1)
                            from = from.Substring(0, index) + " \\1" + from.Substring(index + 1);

                        foreach (Player el in gmList)
                        {
                            if (((el.m_gi.channel && el.m_pi.channel == c.getInfo().id) || el.m_gi.whisper.IsTrue() || el.m_gi.isOpenPlayerWhisper(_session.m_pi.uid))
                                && (el.m_pi.channel != _session.m_pi.channel || el.m_pi.mi.sala_numero != _session.m_pi.mi.sala_numero))
                            {
                                packet_func.session_send(packet_func.pacote02(from, msg_gm, 0), el);
                            }
                        }
                    }

                    // 5. Executa comandos e envia a mensagem para sala ou lobby
                    var comando = new Queue<string>(msg.Split(' '));

                    if (_session.m_pi.mi.sala_numero != -1)
                    {
                        c.requestSendMsgChatRoom(_session, msg);
                    }
                    else
                    {
                        var flag = _session.m_pi.m_cap.game_master ? eChatMsg.GM_EVENT : 0;
                        packet_func.channel_broadcast(c, packet_func.pacote02(_session.m_pi.nickname, msg, flag), 1);
                    }
                }
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::requestChat][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestCheckGameGuardAuthAnswer(Player _session, packet _packet)
        {
        }

        public void requestCommandNoticeGM(Player _session, packet _packet)
        {
            try
            {

                if (!(_session.m_pi.m_cap.game_master/* & 4*/))
                    throw new exception("[GameServer::requestCommandNoticeGM][Error] PLAYER[UID=" + (_session.m_pi.uid)
                            + "] nao eh GM mas tentou executar comando GM. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 0x5700100));

                string notice = _packet.ReadString();

                if (notice.empty())
                    throw new exception("[GameServer::requestCommandNoticeGM][Error] PLAYER[UID=" + (_session.m_pi.uid)
                            + "] tentou executar o comando de notice, mas a notice is empty. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 8, 0x5700100));

                // Log
                _smp.message_pool.getInstance().push(new message("[GameServer::requestCommandNoticeGM][Info] PLAYER[UID=" + (_session.m_pi.uid) + "] enviou notice[NOTICE="
                        + notice + "] para todos do game server.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                using (var p = new PangyaBinaryWriter(0x40))
                {
                    p.WriteByte(7); // Notice

                    p.WritePStr(_session.m_pi.nickname);
                    p.WritePStr(notice);
                    foreach (var c in v_channel)
                        packet_func.channel_broadcast(c, p.GetBytes);
                }
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::requestCommandNoticeGM][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                using (var p = new PangyaBinaryWriter(0x40))
                {
                    p.WriteByte(7); // Notice

                    p.WritePStr(_session.m_pi.nickname);
                    p.WritePStr("Nao conseguiu executar o comando.");
                    packet_func.session_send(p, _session);

                }
            }

        }



        public void requestLogin(Player session, packet pkt)
        {
            PangyaBinaryWriter p = null;

            try
            {
                string clientVersion = string.Empty;
                // --- Ler packet ---
                ReadLoginPacket(session, pkt, out ushort command, out clientVersion, out bool hasClientVersion);

                // --- Validações básicas do pacote e do cliente ---
                if (!ValidateLoginPacket(session, hasClientVersion, clientVersion))
                {
                    _smp.message_pool.getInstance().push(new message("[GameServer::requestLogin][Warning] PLAYER[UID=" + (session.m_pi.uid) + $", UserID= {session.m_pi.id}, CVersion= {clientVersion}]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    session.m_is_authorized = false;
                    SendLoginAck(session, eLoginAck.ACK_INVALID_VERSION);
                    return;
                }

                _smp.message_pool.getInstance().push(new message("[GameServer::requestLogin][Info] " + pkt.Log(), type_msg.CL_ONLY_FILE_LOG));

                // --- Ban checks (IP / MAC) ---
                if (this.haveBanList(session.getIP(), "", false))
                    throw new exception($"PLAYER[UID={session.m_pi.uid}, IP={session.getIP()} blocked by banlist.");

                // --- sanity: id non-empty ---
                if (string.IsNullOrEmpty(session.m_pi.id))
                {
                    _smp.message_pool.getInstance().push(new message(
                        $"[GameServer::requestLogin][Warning] PLAYER[UID={session.m_pi.uid}, IP={session.getIP()}] invalid id: {session.m_pi.id}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));

                    session.m_is_authorized = false;
                    SendLoginAck(session, eLoginAck.ACK_INVALID_VERSION);
                    return;
                }
                var cmdAi = new CmdAccountInfo(session.m_pi.id); // waiter, vamos ter que chamar pelo ID
                snmdb.NormalManagerDB.getInstance().add(0, cmdAi, null, null);
                if (cmdAi.getException().getCodeError() != 0) throw cmdAi.getException();

                session.m_pi.uid = cmdAi.getUID();

                // --- Retrieve player info from DB ---
                var cmdPi = new CmdPlayerInfo(session.m_pi.uid); // waiter, vamos ter que chamar pelo ID
                snmdb.NormalManagerDB.getInstance().add(0, cmdPi, null, null);
                if (cmdPi.getException().getCodeError() != 0) throw cmdPi.getException();

                session.m_pi.set_info(cmdPi.getInfo());

                if (session.m_pi.uid <= 0)
                {
                    _smp.message_pool.getInstance().push(new message($"[GameServer::requestLogin][Warning] PLAYER[UID={session.m_pi.uid}] not found in DB", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    session.m_is_authorized = false;
                    SendLoginAck(session, eLoginAck.ACK_INVALID_ID);
                    return;
                }

                // --- Anti-hack: verify client-supplied ID matches DB ID ---
                if (!string.Equals(cmdPi.getInfo().id, session.m_pi.id, StringComparison.Ordinal))
                {
                    _smp.message_pool.getInstance().push(new message(
                        $"[GameServer::requestLogin][Warning] PLAYER[UID={session.m_pi.uid}] client ID mismatch: client={session.m_pi.id}, db={cmdPi.getInfo().id}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));

                    session.m_is_authorized = false;
                    SendLoginAck(session, eLoginAck.ACK_INVALID_ID);
                    return;
                }

                // --- Account block checks (temporary / forever / all-ip) ---
                CheckAccountBlock(session);

                // --- Client version checks (region/season/high/low) --- 
                EvaluateClientVersion(session);

                // --- Member Info ---
                var cmdMi = new CmdMemberInfo(session.m_pi.uid);
                snmdb.NormalManagerDB.getInstance().add(0, cmdMi, null, null);
                if (cmdMi.getException().getCodeError() != 0) throw cmdMi.getException();
                session.setMemberInfo(cmdMi.getInfo());

                // --- GM handling ---
                session.m_pi.mi.state_flag.visible = false;
                if (session.m_pi.m_cap.game_master)
                {
                    session.m_gi.setGMUID(session.m_pi.uid);
                    session.m_pi.mi.state_flag.whisper = session.m_gi.whisper;
                    session.m_pi.mi.state_flag.channel = session.m_gi.channel;
                }

                // --- GS property checks (rookie, mantle) ---
                if (this.m_si.propriedade.only_rookie && session.m_pi.level >= 6)
                    throw new exception($"PLAYER[UID={session.m_pi.uid}, LEVEL={session.m_pi.level}] not allowed (rookie-only GS).");

                if (this.m_si.propriedade.mantle && !(session.m_pi.m_cap.mantle || session.m_pi.m_cap.game_master))
                    throw new exception($"PLAYER[UID={session.m_pi.uid}] lacks mantle capability.");

                // --- Overlap: if another session with same UID exists ---
                var alreadyLogged = this.HasLoggedWithOuterSocket(session);
                if (alreadyLogged != null && !this.canSameIDLogin())
                {
                    _smp.message_pool.getInstance().push(new message(
                        $"[GameServer::requestLogin][Error] existing session for UID={session.m_pi.uid}, disconnecting existing.",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));

                    if (!this.DisconnectSession(alreadyLogged))
                        throw new exception($"Failed to disconnect existing session UID={alreadyLogged.getUID()}");
                }

                // --- Merge block flags and authorize session ---
                session.m_pi.block_flag.m_flag.ullFlag |= this.m_si.flag.ullFlag;
                session.m_is_authorized = true;

                // --- DB registration: player logged into GS ---
                snmdb.NormalManagerDB.getInstance().add(5, new CmdRegisterLogon(session.m_pi.uid, 1), this.SQLDBResponse, this);
                snmdb.NormalManagerDB.getInstance().add(7, new CmdRegisterLogonServer(session.m_pi.uid, this.m_si.uid), this.SQLDBResponse, this);

                // Create login manager task to load all data
                m_login_manager.createTask(session);

                _smp.message_pool.getInstance().push(new message($"[GameServer::requestLogin][Sucess] PLAYER[OID={session.m_oid}, UID={session.m_pi.uid}, NICK={session.m_pi.nickname}] Stage = Check.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Anti-bot timestamp
                session.m_tick_bot = Environment.TickCount;
            }
            catch (exception ex)
            {
                _smp.message_pool.getInstance().push(new message($"[GameServer::requestLogin][Error] {ex.getFullMessageError()}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                session.m_is_authorized = false;

                // Disconnect session to be safe
                this.DisconnectSession(session);
            }
        }

        /* ----------------------
           Helper methods used above
           ---------------------- */

        private void ReadLoginPacket(Player session, packet pkt, out ushort outCommand,
           out string outClientVersion, out bool outHasClientVersion)
        {
            outClientVersion = string.Empty;

            session.m_pi.id = pkt.ReadString();
            outCommand = pkt.ReadUInt16();

            outHasClientVersion = pkt.ReadPStr(out outClientVersion) ? true : false;
        }

        private bool ValidateLoginPacket(Player session,
            bool hasClientVersion, string cversion)
        {
            if (!hasClientVersion || string.IsNullOrEmpty(cversion))
            {
                SendLoginAck(session, eLoginAck.ACK_INVALID_VERSION);
                return false;
            }

            if (string.IsNullOrEmpty(session.m_pi.id) || session.m_pi.id.Length >= 0x40)
            {
                SendLoginAck(session, eLoginAck.ACK_INVALID_ID);
                return false;
            }
            return true;
        }

        private void SendLoginAck(Player session, eLoginAck ack)
        {
            using (var p = new PangyaBinaryWriter(0x44))
            {
                p.WriteUInt32((byte)ack);
                packet_func.session_send(p, session);
            }

            DisconnectSession(session);
        }

        private void CheckAccountBlock(Player _session)
        {
            // Verifica aqui se a conta do player está bloqueada
            if (_session.m_pi.block_flag.m_id_state.ull_IDState != 0)
            {

                if (_session.m_pi.block_flag.m_id_state.L_BLOCK_TEMPORARY && (_session.m_pi.block_flag.m_id_state.block_time == -1 || _session.m_pi.block_flag.m_id_state.block_time > 0))
                {

                    throw new exception("[GameServer::requestLogin][Error] Bloqueado por tempo[Time="
                            + (_session.m_pi.block_flag.m_id_state.block_time == -1 ? ("indeterminado") : ((_session.m_pi.block_flag.m_id_state.block_time / 60)
                            + "min " + (_session.m_pi.block_flag.m_id_state.block_time % 60) + "sec"))
                            + "]. player [UID=" + (_session.m_pi.uid) + ", ID=" + (_session.m_pi.id) + "]");

                }
                else if (_session.m_pi.block_flag.m_id_state.L_BLOCK_FOREVER)
                {

                    throw new exception("[GameServer::requestLogin][Error] Bloqueado permanente. player [UID=" + (_session.m_pi.uid)
                            + ", ID=" + (_session.m_pi.id) + "]");
                }

                else if (_session.m_pi.block_flag.m_id_state.L_BLOCK_ALL_IP)
                {

                    // Bloquea todos os IP que o player logar e da error de que a area dele foi bloqueada

                    // Add o ip do player para a lista de ip banidos
                    snmdb.NormalManagerDB.getInstance().add(9, new CmdInsertBlockIp(_session.getIP(), "255.255.255.255"), this.SQLDBResponse, this);

                    // Resposta
                    throw new exception("[GameServer::requestLogin][Error] PLAYER[UID=" + (_session.m_pi.uid) + ", IP=" + (_session.getIP())
                            + "] Block ALL IP que o player fizer login.");
                }
                else if (_session.m_pi.block_flag.m_id_state.L_BLOCK_MAC_ADDRESS)
                {

                    // Bloquea o MAC Address que o player logar e da error de que a area dele foi bloqueada

                    // Add o MAC Address do player para a lista de MAC Address banidos
                    snmdb.NormalManagerDB.getInstance().add(10, new CmdInsertBlockMac(_session.m_MacAdress), this.SQLDBResponse, this);

                    // Resposta
                    throw new exception("[GameServer::requestLogin][Error] PLAYER[UID=" + (_session.m_pi.uid)
                            + ", IP=" + (_session.getIP()) + ", MAC=" + _session.m_MacAdress + "] Block MAC Address que o player fizer login.");

                }
            }
        }

        private void EvaluateClientVersion(Player session)
        {
            if ("2.15a" != m_si.version_client)
            {
                session.m_pi.block_flag.m_flag.all_game = true;
            }
        }

        public void requestEnterChannel(Player _session, packet _packet)
        {
            try
            {
                var channel = Convert.ToSByte(_packet.ReadByte());
                // Enter Channel
                var c = enterChannel(_session, channel);
                if (c != null)
                    c.enterLobby(_session, channel);
                else
                    DisconnectSession(_session);
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::requestEnterChannel][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestEnterOtherChannelAndLobby(Player _session, packet _packet)
        {
            try
            {

                // Lobby anterior que o player estava
                var lobby = _session.m_pi.lobby;

                var c = enterChannel(_session, _packet.ReadSByte());

                if (c != null)
                    c.enterLobby(_session, lobby);
                else
                    DisconnectSession(_session);
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::requestEnterOtherChannelAndLobby][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
        }

        public void requestExceptionClientMessage(Player _session, packet _packet)
        {
            byte tipo = _packet.ReadByte();

            var exception_msg = _packet.ReadPStr();
            if (tipo == 1)
            {
                //cheat?
            }
            _smp.message_pool.getInstance().push(new message("[GameServer::requestExceptionClientMessage][Error] PLAYER[UID=" + (_session.m_pi.uid) + ", EXTIPO="
                    + ((ushort)tipo) + ", MSG=" + exception_msg + "]", type_msg.CL_ONLY_FILE_LOG));
            //
            onDisconnected(_session);//send desconection
        }

        public void requestNotifyNotDisplayPrivateMessageNow(Player _session, packet _packet)
        {
            try
            {
                string nickname = _packet.ReadPStr();

                if (nickname.empty())
                    throw new exception("[GameServer::requestNotifyNotDisplayPrivateMessageNow][Error] PLAYER[UID=" + (_session.m_pi.uid)
                            + "] nao pode ver mensagem agora, mas o nickname de quem enviou a mensagem para ele eh invalido(empty). Hacker ou Bug.",
                            ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 0x750050, 0));
                // Procura o player pelo nickname, para ver se ele está online
                var s = (Player)FindSessionByNickname(nickname);
                if (s != null && s.isConnected())
                {
                    // Log
                    _smp.message_pool.getInstance().push(new message("[GameServer::requestNotifyNotDisplayPrivateMessageNow][Error] PLAYER[UID=" + (_session.m_pi.uid)
                            + "] recebeu mensagem do PLAYER[UID=" + (s.m_pi.uid) + ", NICKNAME=" + nickname + "], mas ele nao pode ver a mensagem agora.", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    packet_func.session_send(packet_func.pacote02(nickname, "", eChatMsg.GM_EVENT), s);

                }
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::requestNotifyNotDisplayPrivateMessageNow][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }


        public void requestPlayerInfo(Player _session, packet _packet)
        {
            try
            {
                uint uid = _packet.ReadUInt32();
                Player s = null;
                PlayerInfo pi = null; 
                pi = _session.m_pi;

                _smp.message_pool.getInstance().push(new message($"[GameServer::RequestPlayerInfo][Log] {uid}", type_msg.CL_ONLY_CONSOLE));


                if (uid == _session.m_pi.uid)
                { 
                    pi = _session.m_pi; 
                }
                else if ((s = findPlayer(uid)) != null)
                {
                    pi = s.m_pi;
                }
                else
                {
                    #region STATISTIC INFO (OTHER PLAYER) 

                    // Verifica se não é o mesmo UID, pessoas diferentes
                    // Quem quer ver a info não é GM aí verifica se o player é GM
                    if (uid != _session.m_pi.uid && !_session.m_pi.m_cap.game_master && pi.mi.capability.game_master/* & 4/*(GM)*/)
                    {
                        //packet_func.session_send(packet_func.pacote089(uid, season, 3), _session); // No permission to see info of GM
                    }
                    else
                    {
                        packet_func.session_send(new PangyaBinaryWriter(new byte[] { 0x66 }, packet_func.packet_main(pi)), _session);
                    }
                    return;
                    #endregion 
                }
                //call for you player/info
                #region MY Statistics (MY PLAYER)
                // Verifica se não é o mesmo UID, pessoas diferentes
                // Quem quer ver a info não é GM aí verifica se o player é GM
                if (uid != _session.m_pi.uid && !_session.m_pi.m_cap.game_master && pi.m_cap.game_master/* & 4/*(GM)*/)
                {
                    // packet_func.session_send(packet_func.pacote089(uid, season, 3), _session);
                }
                else
                {
                     ////write struct member info player (1223)     
                    packet_func.session_send(new PangyaBinaryWriter(new byte[] { 0x66 }, packet_func.packet_main(pi)), _session);
                }
                #endregion
            }
            catch (Exception e)
            {
                _smp.message_pool.getInstance().push(new message($"[GameServer::RequestPlayerInfo][ErrorSystem] {e.Message}", type_msg.CL_ONLY_CONSOLE));
            }
        }


        public void requestPrivateMessage(Player _session, packet _packet)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();
            Player s = null;
            string nickname = "";

            try
            {

                // Verifica se session está autorizada para executar esse ação, 
                // se ele não fez o login com o Server ele não pode fazer nada até que ele faça o login
                //    CHECK_SESSION_IS_AUTHORIZED("PrivateMessage");

                nickname = _packet.ReadPStr();
                string msg = _packet.ReadPStr();

                if (nickname.empty())
                    throw new exception("[GameServer::requestPrivateMessage][Error] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o PLAYER[NICKNAME="
                            + nickname + "], mas o nick esta vazio. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 5));

                if (msg.empty())
                    throw new exception("[GameServer::requestPrivateMessage][Error] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o PLAYER[NICKNAME="
                        + nickname + "], mas message esta vazia. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 4, 5));

                // Verifica se o player tem os itens necessários(PREMIUM USER OR GM) para usar essa função
                if (nickname.Contains("#SC") || nickname.Contains("#CS"))
                {

                    // Só sai do Private message se for comando do Smart Calculator, se não faz as outras verificações para enviar o PM
                    //if (m_si.rate.smart_calculator && checkSmartCalculatorCmd(_session, msg, (nickname.compare("#SC") == 0 ? eTYPE_CALCULATOR_CMD::SMART_CALCULATOR : eTYPE_CALCULATOR_CMD::CALCULATOR_STADIUM)))
                    //    return;
                }

                s = (Player)FindSessionByNickname(nickname);

                if (s == null || !s.getState() || !s.isConnected())
                    throw new exception("[GameServer::requestPrivateMessage][Warning] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o PLAYER[NICKNAME="
                            + nickname + "], mas o player nao esta online nesse server.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 2, 5));

                // Whisper Block
                if (!s.m_pi.whisper.IsTrue())
                    throw new exception("[GameServer::requestPrivateMessage][Warning] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o PLAYER[NICKNAME="
                            + nickname + "], mas o whisper do player esta bloqueado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 5, 4));

                if ((s.m_pi.lobby == 255/*não está na lobby*/ && s.m_pi.mi.sala_numero == -1/*e não está em nenhum sala*/) || s.m_pi.place != 2)
                    throw new exception("[GameServer::requestPrivateMessage][Warning] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou enviar message privada[msg=" + msg + "] para o PLAYER[NICKNAME="
                            + nickname + "], mas o player nao pode receber message agora, por que nao pode ver o chat. pode estar no Papel Shop e Etc.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 3, 4));

                // Arqui procura por palavras inapropriadas na message

                // Envia para todo os GM do serve   r essa message
                var gm = FindAllGM();

                if (!gm.Any())
                {

                    var msg_gm = "\\5" + (_session.m_pi.nickname) + ">" + (s.m_pi.nickname) + ": '" + msg + "'";

                    foreach (Player el in gm)
                    {
                        if ((el.m_gi.whisper.IsTrue() || el.m_gi.isOpenPlayerWhisper(_session.m_pi.uid) || el.m_gi.isOpenPlayerWhisper(s.m_pi.uid))
                            && /*Nao envia o log de PM novamente para o GM que enviou ou recebeu PM*/(el.m_pi.uid != _session.m_pi.uid && el.m_pi.uid != s.m_pi.uid))
                        {
                            // Responde no chat do player
                            p.init_plain(0x40);

                            p.WriteByte(0);

                            p.WritePStr("\\1[PM]"); // Nickname

                            p.WritePStr(msg_gm);    // Message
                            packet_func.session_send(p, el);
                        }
                    }

                }

                // Log
                _smp.message_pool.getInstance().push(new message("[PrivateMessage][Error] PLAYER[UID=" + (_session.m_pi.uid) + "] enviou a Message[" + msg + "] para o PLAYER[UID=" + (s.m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta para o que enviou a private message
                p.init_plain(0x84);

                p.WriteByte(0); // FROM

                p.WritePStr(s.m_pi.nickname);   // Nickname TO
                p.WritePStr(msg);
                packet_func.session_send(p, _session);

                // Resposta para o player que vai receber a private message
                p.init_plain(0x84);

                p.WriteByte(1); // TO

                p.WritePStr(_session.m_pi.nickname);    // Nickname FROM
                p.WritePStr(msg);
                packet_func.session_send(p, s);

                // Envia a mensagem para o Chat History do discord se ele estiver ativo

                // Verifica se o m_chat_discod flag está ativo para enviar o chat para o discord
                //     if (m_si.rate.smart_calculator && m_chat_discord)
                //sendMessageToDiscordChatHistory(
                //	"[PM]",                                                                                                             // From
                //             (_session.m_pi.nickname) + ">" + (s.m_pi.nickname) + ": '" + msg + "'"						// Msg
                //);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::requestPrivateMessage][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x40);

                p.WriteByte((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.GAME_SERVER) ? (byte)ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 5);
                if (s != null && s.isConnected())
                    p.WritePStr(s.m_pi.nickname);
                else
                    p.WritePStr(nickname);  // Player não está online usa o nickname que ele forneceu
                packet_func.session_send(p, _session);
            }
        }

        public void requestQueueTicker(Player _session, packet _packet)
        {
            //////REQUEST_BEGIN("QueueTicker");

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                if (_session.m_pi.block_flag.m_flag.ticker)
                    throw new exception("[GameServer::requestQueueTicker][Error] PLAYER[UID=" + (_session.m_pi.uid)
                            + "] tentou abrir a fila do Ticker, mas o ticker esta bloqueado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 10, 1/*UNKNOWN ERROR*/));

                var count = m_ticker.getSize();

                var time_left_milisecond = count * 30000;

                // Send Count Ticker and time left for send ticker
                p.init_plain(0xCA);//

                p.WriteUInt16((ushort)count);
                p.WriteUInt32(time_left_milisecond);
                packet_func.session_send(p, _session);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::requestQueueTicker][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // estou usando pacote de troca nickname, por que n�o sei qual o pangya manda, quando da erro no mandar ticker, nunca peguei esse erro
                p.init_plain(0x50);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.GAME_SERVER) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 1/*UNKNOWN ERROR*/);

                packet_func.session_send(p, _session);
            }
        }

        public void requestSendTicker(Player _session, packet _packet)
        {
            var p = new PangyaBinaryWriter();

            try
            {


                if (_session.m_pi.block_flag.m_flag.ticker)
                    throw new exception("[GameServer::requestSendTicker][Error] PLAYER[UID=" + (_session.m_pi.uid)
                            + "] tentou abrir a fila do Ticker, mas o ticker esta bloqueado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 10, 1/*UNKNOWN ERROR*/));

                var msg = _packet.ReadString();//fazer um translation aqui

                if (msg.empty())
                    throw new exception("[GameServer::requestSendTicker][Error] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou enviar ticker[MESSAGE="
                            + msg + "], mas msg is empty(vazia). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 1/*UNKNOWN ERROR*/));

                try
                {

                    // Log de Gastos de CP(Cookie Point)
                    CPLog cp_log = new CPLog();

                    cp_log.setType(CPLog.TYPE.TICKER);

                    cp_log.setCookie(5);
                    // fim do inicializa log de gastos de CP

                    _session.m_pi.consomeCookie(5);

                    // Add o Ticker para lista de ticker do server
                    m_ticker.push_back((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3/*Segundos*/, _session.m_pi.nickname, msg, BroadcastManager.TYPE.TICKER);

                    // Add o Ticker para Commando DB para o Auth Server mandar para os outros serveres
                    snmdb.NormalManagerDB.getInstance().add(6, new CmdInsertTicker(_session.m_pi.uid, (uint)m_si.uid, msg), SQLDBResponse, this);

                    // Salva CP Log
                    _session.saveCPLog(cp_log);

                    // Log
                    _smp.message_pool.getInstance().push(new message("[GameServer::requestSendTicker][Sucess] PLAYER[UID=" + (_session.m_pi.uid) + ", NICKNAME="
                            + (_session.m_pi.nickname) + "] enviou Ticker[MESSAGE=" + msg + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // UPDATE ON GAME
                    p.init_plain(0x76);

                    p.WriteInt32((int)_session.m_pi.cookie);
                    _session.m_pi.ui.Cookies = (int)_session.m_pi.cookie;
                    packet_func.session_send(p, _session, 1);

                }
                catch (exception e)
                {

                    if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE.PLAYER_INFO, 20))
                    {

                        throw new exception("[GameServer::requestSendTicker][Error] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou enviar ticker[MESSAGE="
                                + msg + "], mas ele nao tem cookies suficiente[HAVE=" + (_session.m_pi.cookie) + ", REQ=1]", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 4/*NÃO TEM COOKIES SUFICIENTE*/));

                    }
                    else if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE.PLAYER_INFO, 200/*Tem alterações no Cookie do player no DB*/))
                    {

                        throw new exception("[GameServer::requestSendTicker][Error] PLAYER[UID=" + (_session.m_pi.uid) + "] tentou enviar ticker[MESSAGE="
                                + msg + ", mas tem alteracoes no Cookie dele no Banco de dados.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER, 1, 4/*Tem alterações no Cookie do player no DB*/));

                    }
                    else
                    {

                        // Devolve os Cookies gasto do player, por que não conseguiu enviar o ticker dele
                        _session.m_pi.addCookie(1);

                        // Relança a exception para da uma resposta para o cliente
                        throw;
                    }
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameServer::requestSendTicker][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // estou usando pacote de troca nickname, por que não sei qual o pangya manda, quando da erro no mandar ticker, nunca peguei esse erro
                p.init_plain(0x50);

                p.WriteUInt32((ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.GAME_SERVER) ? ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) : 1/*UNKNOWN ERROR*/);

                packet_func.session_send(p, _session, 1);
            }
        }

        public void sendChannelListToSession(Player _session)
        {
            try
            {
                packet_func.session_send(packet_func.pacote11(v_channel), _session);
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::sendChannelListToSession][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void RequestIdentity(Player session, packet _packet)
        {
            //Console.WriteLine(_packet.Log());
            //byte mode = _packet.ReadByte();
            //byte _byte2 = _packet.ReadByte();//eu vi 2, algum codigo...
            //byte _byte3 = _packet.ReadByte();//eu vi 0
            //byte _byte4 = _packet.ReadByte();//eu vi 0
            //string _nick = _packet.ReadPStr();//deve ser o game_master

            //if (mode == 0)
            //{
            //    session.m_pi.mi.capability.ulCapability = 4;
            //}
            //else
            //{
            //    session.m_pi.mi.capability.ulCapability = 4;
            //}
            //var p = new PangyaBinaryWriter(new byte[] { 0xC1 });
            //p.WriteInt32(0); // O valor atualizado
            //packet_func.session_send(p, session);
        }


        public void sendServerListAndChannelListToSession(Player player)
        {
            //envia a lista de servers + canais;
            using (var p = new PangyaBinaryWriter(new byte[] { 0xFC }))
            {
                p.WriteByte((byte)m_server_list.Count);

                for (var i = 0; i < m_server_list.Count; ++i)
                    p.WriteBytes(m_server_list[i].ToArray());

                packet_func.session_send(p.GetBytes, player);
            }
        }
         
        public void GameNotice()
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();

            p.init_plain(0x42);
            p.WriteString("Welcome to PangYa Mania!");
            foreach (var ci in v_channel)
                packet_func.channel_broadcast(ci, p);
        }

        public string getClientVersionSideServer()
        {
            return m_si.version_client;
        }

        public bool getActiveRoomLog()
        {
            return m_active_room_log;
        }


        private void Version_Decrypt(ref uint packet_version)
        {
            string PacketVerKey = "{782AE110-2EEF-4c61-B030-A53F17634F7D}";

            byte[] tmpPVer = BitConverter.GetBytes(packet_version);
            int index = 0;

            for (int i = 0; i < PacketVerKey.Length; i++)
            {
                tmpPVer[index] ^= (byte)PacketVerKey[i];
                index = (index == 3) ? 0 : index + 1;
            }

            packet_version = BitConverter.ToUInt32(tmpPVer, 0);
        }

    }
}

namespace sgs
{
    public class gs : Singleton<Pangya_GameServer.GameServerTcp.GameServer>
    {
    }
}