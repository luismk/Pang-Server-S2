// Arquivo Game.cs
using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.Repository;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using sgs;
using static Pangya_GameServer.Models.DefineConstants;
using int32_t = System.Int32;
using int64_t = System.Int64;
using size_t = System.Int32;
using uint32_t = System.UInt32;
using uint64_t = System.UInt64;
namespace Pangya_GameServer.Models.Game.Base
{
    public abstract class GameBase : IDisposable
    {
        protected List<Player> m_players;
        protected Dictionary<Player, PlayerGameInfo> m_player_info;
        protected List<PlayerGameInfo> m_player_order;
        protected Dictionary<uint, uint> m_player_report_game;
        protected RoomInfoEx m_ri;
        protected RateValue m_rv;
        /// <summary>
        /// 1 = iniciou, 2 acabou, -1 default
        /// </summary>
        public int m_game_init_state;
        protected bool m_state;
        protected DateTime m_start_time;
        //protected PangyaSyncTimer m_timer;
        protected bool m_channel_rookie;
        protected volatile int m_sync_send_init_data;
       // protected CourseManager m_course;
        public RoomInfoLog m_room_log;
        private bool disposedValue;
        // 
        #region Abstract Methods 
        // Game
        public abstract bool requestFinishGame(Player _session, packet _packet);

        // Inicializa Jogo e Finaliza Jogo
        public abstract bool init_game();

        // Trata Shot Sync Data
        public abstract void requestTranslateSyncShotData(Player _session, ShotSyncData _ssd);
        public abstract void requestReplySyncShotData(Player _session);

        // Metôdos do Game.Course.Hole
        public abstract void requestInitHole(Player _session, packet _packet);
        public abstract bool requestFinishLoadHole(Player _session, packet _packet);
        public abstract void requestFinishCharIntro(Player _session, packet _packet);
        public abstract void requestFinishHoleData(Player _session, packet _packet);

        // São implementados na suas classe base

        // Esses 2 Aqui é do modo VS
        //virtual void changeHole() = 0;
        //virtual void finishHole() = 0;

        // Esses 2 Aqui é do Modo Tourney
        //virtual void changeHole(Player& _session) = 0;
        //virtual void finishHole(Player& _session) = 0;

        // Server enviou a resposta do InitShot para o cliente
        // Esse aqui é exclusivo do VersusBase 
        public abstract void requestInitShot(Player _session, packet _packet);
        public abstract void requestSyncShot(Player _session, packet _packet);
        public abstract void requestInitShotArrowSeq(Player _session, packet _packet);
        public abstract void requestShotEndData(Player _session, packet _packet);
        public abstract RetFinishShot requestFinishShot(Player _session, packet _packet);

        public abstract void requestChangeMira(Player _session, packet _packet);
        public abstract void requestChangeStateBarSpace(Player _session, packet _packet);
        public abstract void requestActivePowerShot(Player _session, packet _packet);
        public abstract void requestChangeClub(Player _session, packet _packet);
        public abstract void requestUseActiveItem(Player _session, packet _packet);
        public abstract void requestChangeStateTypeing(Player _session, packet _packet); // Escrevendo
        public abstract void requestMoveBall(Player _session, packet _packet);
        public abstract void requestChangeStateChatBlock(Player _session, packet _packet);
        public abstract void requestActiveBooster(Player _session, packet _packet);
        public abstract void requestActiveReplay(Player _session, packet _packet);
        public abstract void requestActiveCutin(Player _session, packet _packet);

        // Hability Item
        public abstract void requestActiveRing(Player _session, packet _packet);
        public abstract void requestActiveRingGround(Player _session, packet _packet);
        public abstract void requestActiveRingPawsRainbowJP(Player _session, packet _packet);
        public abstract void requestActiveRingPawsRingSetJP(Player _session, packet _packet);
        public abstract void requestActiveRingPowerGagueJP(Player _session, packet _packet);
        public abstract void requestActiveRingMiracleSignJP(Player _session, packet _packet);
        public abstract void requestActiveWing(Player _session, packet _packet);
        public abstract void requestActivePaws(Player _session, packet _packet);
        public abstract void requestActiveGlove(Player _session, packet _packet);
        public abstract void requestActiveEarcuff(Player _session, packet _packet);

        #endregion

        #region Virtual 

        public virtual bool finish_game(Player _session, int option = 0) { return false; }

        /// <summary>
        /// metodo para decriptografar dados do shot(tiro) do player...
        /// </summary>
        /// <param name="_buffer">dados do tiro criptogrfado</param>
        /// <returns></returns>
        protected ShotSyncData DecryptShot(byte[] _buffer)
        {
            if (_buffer.Length < 38 || _buffer.Length > 38)
                return null;

            for (int i = 0; i < _buffer.Length; i++)
                _buffer[i] = (byte)(_buffer[i] ^ m_ri.key[i % 16]);

            //decrypt shot
            var reader = new PangyaBinaryReader(new MemoryStream(_buffer));
            var ssd = new ShotSyncData
            {
                oid = reader.ReadInt32(), //oid

                location = new ShotSyncData.Location()
                {
                    x = reader.ReadSingle(),
                    y = reader.ReadSingle(),
                    z = reader.ReadSingle(),
                },

                state = (ShotSyncData.SHOT_STATE)reader.ReadByte(),

                bunker_flag = reader.ReadByte(),
                ucUnknown = reader.ReadByte(),

                pang = reader.ReadUInt32(),

                bonus_pang = reader.ReadUInt32(),

                state_shot = new ShotSyncData.stStateShot()
                {
                    display = new ShotSyncData.stStateShot.uDisplayState()
                    {
                        ulState = reader.ReadUInt32(),
                    },
                    shot = new ShotSyncData.stStateShot.uShotState()
                    {
                        ulState = reader.ReadUInt32()
                    }
                },

                tempo_shot = reader.ReadInt16(),
                grand_prix_penalidade = reader.ReadByte()
            };
            return ssd;
        }

        public virtual PlayerGameInfo INIT_PLAYER_INFO(string _method, string _msg, Player __session)
        {
            var pgi = getPlayerInfo((__session));
            if (pgi == null)
                throw new exception($"[{GetType().Name}::" + _method + "][Error] PLAYER[UID=" + __session.m_pi.uid + "] " + _msg + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 1, 4));

            return pgi;
        }

        public virtual void INIT_PLAYER_INFO(string _method, string _msg, Player __session, out PlayerGameInfo pgi)
        {
            pgi = getPlayerInfo(__session);
            if (pgi == null)
                throw new exception($"[{GetType().Name}::" + _method + "][Error] PLAYER[UID=" + __session.m_pi.uid + "] " + _msg + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 1, 4));
        }

        #endregion

        public GameBase(List<Player> _players, RoomInfoEx _ri, RateValue _rv, bool _channel_rookie)
        {
            this.m_players = new List<Player>(_players);

            this.m_ri = _ri;
            this.m_rv = _rv;
            this.m_channel_rookie = _channel_rookie;
            this.m_start_time = DateTime.MinValue;
            this.m_player_info = new Dictionary<Player, PlayerGameInfo>();
           // this.m_course = null;
            this.m_game_init_state = -1;
            this.m_state = false;
            this.m_player_order = new List<PlayerGameInfo>();
            //this.m_timer = null;
            this.m_player_report_game = new Dictionary<uint, uint>();

            Interlocked.Exchange(ref m_sync_send_init_data, 0);

            // Inicializar Artefact Info Of Game
            initArtefact();

            // Inicializar o rate chuva dos itens equipado dos players no jogo
            initPlayersItemRainRate();

            // Inicializa a flag persist rain next hole
            initPlayersItemRainPersistNextHole();

            //// Map Dados Estáticos
            //if (!MapSystem.getInstance().isLoad())
            //{
            //    MapSystem.getInstance().load();
            //}

            //var map = MapSystem.getInstance().getMap((byte)((int)m_ri.course & 0x7F));

            //if (map == null)
            //{
            //    _smp.message_pool.getInstance().push(new message("[GameBase::Game][Error][Warning] tentou pegar o Map dados estaticos do course[COURSE=" + Convert.ToString((ushort)((int)m_ri.course & 0x7F)) + "], mas nao conseguiu encontra na classe do Server.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            //}

            //// Cria Course
            //m_course = new CourseManager(m_ri,
            //    _channel_rookie,
            //    (map == null) ? 1.0f : map.star,
            //    m_rv.rain, m_rv.persist_rain);
        }

        public virtual void clear_player_order()
        {
            m_player_order.Clear();
        }


        protected void clear_time()
        {
            // Garantir que qualquer exception derrube o server
            //try
            //{

            //    if (m_timer != null)
            //        sgs.gs.getInstance().unMakeTime(m_timer);
            //}
            //catch (exception e)
            //{
            //    _smp.message_pool.getInstance().push(new message("[GameBase::clear_time][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            //}

            //m_timer = null;
        }

        public virtual void sendInitialData(Player _session)
        {

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                // Course
                p.init_plain(0x52);

                p.WriteByte((byte)m_ri.course);
                p.WriteByte(m_ri.tipo_show);
                p.WriteByte(m_ri.modo);
                p.WriteByte(m_ri.qntd_hole);
                p.WriteUInt32(m_ri.trofel);
                p.WriteUInt32(m_ri.time_vs);
                p.WriteUInt32(m_ri.time_30s);
                // Hole Info, Hole Spinning Cube, end Seed Random Course
               // m_course.makePacketHoleInfo(p);

                packet_func.session_send(p,
                    _session, 1);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameBase::sendInitialData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

        }

        // Envia os dados iniciais para quem entra depois no Game
        public virtual void sendInitialDataAfter(Player _session)
        {
            // UNREFERENCED_PARAMETER(_session);
        }

        protected Player findSessionByOID(int32_t _oid)
        {
            return m_players.FirstOrDefault(el => el.m_oid == _oid);
        }

        protected Player findSessionByUID(uint32_t _uid)
        {
            return m_players.FirstOrDefault(el => el.m_pi.uid == _uid);
        }

        protected Player findSessionByNickname(string _nickname)
        {
            return m_players.FirstOrDefault(el =>
            {
                return (string.CompareOrdinal(_nickname, el.m_pi.nickname) == 0);
            });
        }

        protected Player findSessionByPlayerGameInfo(PlayerGameInfo _pgi)
        {

            if (_pgi == null)
            {
                _smp.message_pool.getInstance().push(new message("[GameBase::findSessionByPlayerGameInfo][Error] PlayerGameInfo* _pgi is invalid(null)", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return null;
            }

            return m_player_info.FirstOrDefault(_el =>
            {
                return _el.Value == _pgi;
            }).Key;
        }

        public PlayerGameInfo getPlayerInfo(Player _session)
        {

            if (_session == null)
            {
                throw new exception("[GameBase::getPlayerInfo][Error] _session is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }

            return m_player_info.FirstOrDefault(_el =>
            {
                return _el.Key == _session;
            }).Value;
        }



        // Se _session for diferente de null retorna todas as session, menos a que foi passada no _session
        public List<Player> getSessions(Player _session = null)
        {

            List<Player> v_sessions = new List<Player>();
            // Se _session for diferente de null retorna todas as session, menos a que foi passada no _session
            foreach (var el in m_players)
            {
                if (el != null
                    && el.getState()
                    && el.m_pi.mi.sala_numero != ushort.MaxValue
                    && (_session == null || _session != el))
                {
                    v_sessions.Add(el);
                }
            }
            return v_sessions;
        }

        public virtual DateTime getTimeStart()
        {
            return m_start_time;
        }

        public virtual void addPlayer(Player _session)
        {
            m_players.Add(_session);

            makePlayerInfo(_session);
        }

        public virtual bool deletePlayer(Player _session, int _option)
        {
            if (_session == null)
            {
                throw new exception("[GameBase::deletePlayer][Error] tentou deletar um player, mas o seu endereco eh null.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    50, 0));
            }

            var it = m_players.Any(c => c == _session);

            if (it)
            {
                m_players.Remove(_session);//limpar ou deletar o jogador da lista
            }
            else
            {
                _smp.message_pool.getInstance().push(new message("[GameBase::deletePlayer][Warning] player ja foi excluido do game.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return false;
        }

        public virtual void requestActiveAutoCommand(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[GameBase::" + (("request" + "ActiveAutoCommand")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }

            if (_packet == null)
            {
                throw new exception("[GameBase::request" + "ActiveAutoCommand" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    6, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                var pgi = getPlayerInfo((_session));
                if (pgi == null)
                {
                    throw new exception("[GameBase::" + "requestActiveAutoCommand" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou ativar var Command no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                        1, 4));
                }

                if (!pgi.premium_flag)
                { // (não é)!PREMIUM USER

                    var pWi = _session.m_pi.findWarehouseItemByTypeid(AUTO_COMMAND_TYPEID);

                    if (pWi == null)
                    {
                        throw new exception("[GameBase::requestActiveAutoCommand][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar o var Command Item[TYPEID=" + Convert.ToString(AUTO_COMMAND_TYPEID) + "], mas ele nao tem o item. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 1, 0x550001));
                    }

                    if (pWi.STDA_C_ITEM_QNTD < 1)
                    {
                        throw new exception("[GameBase::requestActiveAutoCommand][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar o var Command Item[TYPEID=" + Convert.ToString(AUTO_COMMAND_TYPEID) + "], mas ele nao tem quantidade suficiente do item[QNTD=" + Convert.ToString(pWi.STDA_C_ITEM_QNTD) + ", QNTD_REQ=1]. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            2, 0x550002));
                    }

                    var it = pgi.used_item.v_passive.find(pWi._typeid);

                    if (it.Key <= 0)
                    {
                        throw new exception("[GameBase::requestActiveAutoCommand][Error] PLAYER[UID = " + Convert.ToString(_session.m_pi.uid) + "] tentou ativar var Command, mas ele nao tem ele no item passive usados do server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            13, 0));
                    }

                    if ((short)it.Value.count >= pWi.STDA_C_ITEM_QNTD)
                    {
                        throw new exception("[GameBase::requestActiveAutoCommand][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar var Command, mas ele ja usou todos os var Command. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                            14, 0));
                    }

                    // Add +1 ao item passive usado
                    it.Value.count++;
                }

            }
            catch (exception e)
            {
 
            }
        }


        public virtual void requestActiveAssistGreen(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[GameBase::" + (("request" + "ActiveAssistGreen")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }
            
            if (_packet == null)
            {
                throw new exception("[GameBase::request" + "ActiveAssistGreen" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    6, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                uint32_t item_typeid = _packet.ReadUInt32();

                if (item_typeid == 0)
                    throw new exception("[GameBase::requestActiveAssistGreen][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Assist[TYPEID=" + Convert.ToString(item_typeid) + "] do Green, mas o item_typeid is invalid(zero). Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            1, 0x5200101));


                if (item_typeid != ASSIST_ITEM_TYPEID)
                    throw new exception("[GameBase::requestActiveAssistGreen][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Assist[TYPEID=" + Convert.ToString(item_typeid) + "] do Green, mas o item_typeid esta errado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                               1, 0x5200101));

                var pWi = _session.m_pi.findWarehouseItemByTypeid(item_typeid);

                if (pWi == null)
                    throw new exception("[GameBase::requestActiveAssistGreen][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou ativar Assist[TYPEID=" + Convert.ToString(item_typeid) + "] do Green, mas o Assist Mode do player nao esta ligado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            2, 0x5200102));

               
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameBase::requestActiveAssistGreen][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                
            }
        }
        // Esse Aqui só tem no VersusBase e derivados dele
        public virtual void requestMarkerOnCourse(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestLoadGamePercent(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestStartTurnTime(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestUnOrPause(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        // Common Command GM Change Wind Versus
        public virtual void requestExecCCGChangeWind(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestExecCCGChangeWeather(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        // Continua o versus depois que o player saiu no 3 hole pra cima e se for de 18h o game
        public virtual void requestReplyContinue()
        {
        }

        // Esse Aqui só tem no TourneyBase e derivados dele
        public virtual bool requestUseTicketReport(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);

            return false;
        }

        // Apenas no Practice que ele é implementado
        public virtual void requestChangeWindNextHoleRepeat(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        // Exclusivo do Modo Tourney
        public virtual void requestStartAfterEnter(Action _job)
        {

            // ignore : UNREFERENCED_PARAMETER(_job);
        }

        public virtual void requestEndAfterEnter()
        {
        }

        public virtual void requestUpdateTrofel()
        {
        }

        // Excluviso do Modo Match
        public virtual void requestTeamFinishHole(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }
        // Pede o Hole que o player está, 
        // se eles estiver jogando ou 0 se ele não está jogando
        public virtual byte requestPlace(Player _session)
        {

            if (!_session.getState())
            {
                throw new exception("[GameBase::requestPlace][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.TOURNEY_BASE,
                    1, 0));
            }

            // Valor padrão
            ushort hole = 0;

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "requestPlace" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou pegar o lugar[Hole] do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (pgi.hole != 255)
            {

                //hole = m_course.findHoleSeq(pgi.hole);

                //if (hole == ushort.MaxValue)
                //{
                //    // Valor padrão
                //    hole = 0;

                //    _smp.message_pool.getInstance().push(new message("[GameBase::requestPlace][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pegar a sequencia do hole[NUMERO=" + Convert.ToString(pgi.hole) + "], mas ele nao encontrou no course do game na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                //}

            }
            else if (pgi.init_first_hole) // Só cria mensagem de log se o player já inicializou o primeiro hole do jogo e tem um valor inválido no pgi->hole (não é uma sequência de hole válida)
            {
                _smp.message_pool.getInstance().push(new message("[GameBase::requesPlace][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou pegar o hole[NUMERO=" + Convert.ToString(pgi.hole) + "] em que o player esta na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "], mas ele esta carregando o course ou tem algum error.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return (byte)hole;
        }

        // Verifica se o player já esteve na sala
        public virtual bool isGamingBefore(uint32_t _uid)
        {
            if (_uid == 0u)
                throw new exception("[GameBase::isGamingBefore][Error] _uid is invalid(zero)", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                        1000, 0));

            return m_player_info.Any(_el =>
            {
                return _el.Value.uid == _uid;
            });
        }



        // Exclusivo do Modo Tourney
        public virtual void requestSendTimeGame(Player _session)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
        }

        public virtual void requestUpdateEnterAfterStartedInfo(Player _session, EnterAfterStartInfo _easi)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_easi);
        }

        // Exclusivo do Grand Zodiac Modo
        public virtual void requestStartFirstHoleGrandZodiac(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public virtual void requestReplyInitialValueGrandZodiac(Player _session, packet _packet)
        {
            // ignore : UNREFERENCED_PARAMETER(_session);
            // ignore : UNREFERENCED_PARAMETER(_packet);
        }

        public void requestReadSyncShotData(Player _session, packet _packet, ref ShotSyncData _ssd)
        {
            try
            {
                //check player connection
                if (!_session.getState())
                    throw new exception("[GameBase::" + (("request" + "readSyncShotData")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                           1, 0));

                //check packet
                if (_packet == null)
                    throw new exception("[GameBase::request" + "readSyncShotData" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            6, 0));

                //check size packet
                if (_packet.GetRemainingData.Length < 38 || _packet.GetRemainingData.Length > 38)
                    throw new exception($"[GameBase::requestReadSyncShotData][Error] DecryptShot" + (_packet.GetRemainingData.Length < 38 ? "is null" : _packet.GetRemainingData.Length > 38 ? "bad struct" : ""), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                       7, 0));

                //decript shot 
                _ssd = DecryptShot(_packet.GetRemainingData);

                if (_ssd == null)
                    throw new exception($"[GameBase::requestReadSyncShotData][Error] DecryptShot" + (_packet.GetRemainingData.Length < 38 ? "is null" : _packet.GetRemainingData.Length > 38 ? "bad struct" : ""), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                           8, 0));

                var oid = _ssd.oid;

                if (_ssd.oid == -1)
                    throw new exception($"[GameBase::requestReadSyncShotData][Error] Player no exist:" + oid, ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                              9, 0));


                if (!m_players.Any(c => c.m_oid == oid))
                    throw new exception($"[GameBase::requestReadSyncShotData][Error] Player no exist:" + oid, ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                              9, 0));

                if (_ssd.pang > 37000u)
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestReadSyncShotDate][Warning] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] pode esta usando hack, PANG[" + Convert.ToString(_ssd.pang) + "] maior que 40k. Hacker ou Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                if (_ssd.bonus_pang > 10000u)
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestReadSyncShotDate][Warning] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] pode esta usando hack, BONUS PANG[" + Convert.ToString(_ssd.bonus_pang) + "] maior que 10k. Hacker ou Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameBase::requestReadSyncShotData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Usa o Padrão delas
        public bool stopTime()
        {
            clear_time();

            return true;
        }

        public bool pauseTime()
        {

            //if (m_timer != null)
            //{
            //    m_timer.Pause();


            //    _smp.message_pool.getInstance().push(new message("[GameBase::pauseTime][Log] pausou o Timer[Tempo=" + Convert.ToString(m_ri.time_30s > 0 ? m_ri.time_30s : m_ri.time_vs) + "" + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));


            //    return true;
            //}

            return false;
        }

        public bool resumeTime()
        {
            //if (m_timer != null)
            //{
            //    m_timer.Resume();
            //    _smp.message_pool.getInstance().push(new message("[GameBase::resumerTime][Log] Retomou o Timer[Tempo=" + Convert.ToString(m_ri.time_30s > 0 ? m_ri.time_30s : m_ri.time_vs) + "" + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            //    return true;
            //}

            return false;
        }

        // Report Game
        public void requestPlayerReportChatGame(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[GameBase::" + (("request" + "PlayerReportChatGame")) + "][Error] player nao esta connectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 0));
            }
            ;
            if (_packet == null)
            {
                throw new exception("[GameBase::request" + "PlayerReportChatGame" + "][Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    6, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                // Verifica se o player já reportou o jogo
                var it = m_player_report_game.find(_session.m_pi.uid);

                if (it.Key != 0)//(it.Key != m_player_report_game.end().Key)
                {

                    // Player já reportou o jogo
                    p.init_plain(0x94);

                    p.WriteByte(1); // Player já reportou o jogo

                    packet_func.session_send(p,
                        _session, 1);

                }
                else
                { // Primeira vez que o palyer report o jogo

                    // add ao mapa de uid de player que reportaram o jogo
                    m_player_report_game[_session.m_pi.uid] = _session.m_pi.uid;

                    // Faz Log de quem está na sala, quando pangya, o update enviar o chat log verifica o chat
                    // por que parece que o pangya não envia o chat, ele só cria um arquivo, acho que quem envia é o update
                    string log = "";

                    foreach (var el in m_players)
                    {
                        if (el != null)
                        {
                            log = log + "UID: " + Convert.ToString(_session.m_pi.uid) + "\tID: " + el.m_pi.id + "\tNICKNAME: " + el.m_pi.nickname + "\n";
                        }
                    }

                    // Log
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestPlayerReportChatGame][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] reportou o chat do jogo na sala[NUMERO=" + Convert.ToString(m_ri.numero) + "] Log{" + log + "}", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    // Reposta para o cliente
                    p.init_plain(0x94);

                    p.WriteByte(0); // Sucesso

                    packet_func.session_send(p,
                        _session, 1);
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameBase::requestPlayerReportChatGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(0x94);

                p.WriteByte(1); // 1 já foi feito report do jogo por esse player

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        protected void initPlayersItemRainRate()
        {

            // Characters Equip
            foreach (var s in m_players)
            {
                if (s.getState() && s.isConnected())
                { // Check Player Connected

                    if (s.m_pi.ei.char_info == null)
                    { // Player não está com character equipado, kika dele do jogo
                        _smp.message_pool.getInstance().push(new message("[GameBase::initPlayersItemRainRate][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        continue;
                    }

                    // Devil Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      devil_wings.Contains(_element)))
                    {

                        _smp.message_pool.getInstance().push(new message("[GameBase::initPlayersItemRainRate][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Devil Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }

                    // Obsidian Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      obsidian_wings.Contains(_element)))
                    {

                        _smp.message_pool.getInstance().push(new message("[GameBase::initPlayersItemRainRate][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Obsidian Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }

                    // Corrupt Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
    corrupt_wings.Contains(_element)))
                    {

                        _smp.message_pool.getInstance().push(new message("[GameBase::initPlayersItemRainRate][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Corrupt Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 15;
                    }

                    // Hasegawa Chirain
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      hasegawa_chirain.Contains(_element)))
                    {

                        _smp.message_pool.getInstance().push(new message("[GameBase::initPlayersItemRainRate][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Hasegawa Chirain Item Part no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }

                    // Hat Spooky Halloween -- Só funciona na época do Halloween (ex: outubro)
                    if (DateTime.Now.Month == 10 && s.m_pi.ei.char_info.parts_typeid.Any(_element => hat_spooky_halloween.Contains(_element)))
                    {
                        _smp.message_pool.getInstance().push(new message(
                            "[GameBase::initPlayersItemRainRate][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) +
                            "] esta equipado com Hat Spooky Halloween no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) +
                            ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]",
                            type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }
                      
                    // Caddie Big Black Papel
                    if (s.m_pi.ei.cad_info != null && s.m_pi.ei.cad_info._typeid == 0x1C00000E)
                    {

                        _smp.message_pool.getInstance().push(new message("[GameBase::initPlayersItemRainRate][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Caddie Big Black Papel[TYPEID=" + Convert.ToString(s.m_pi.ei.cad_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.cad_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        m_rv.rain += 10;
                    }
                }
            }
        }

        public virtual void initPlayersItemRainPersistNextHole()
        {

            // Characters Equip
            foreach (var s in m_players)
            {
                if (s.getState() && s.isConnected())
                { // Check Player Connected

                    if (s.m_pi.ei.char_info == null)
                    { // Player não está com character equipado, kika dele do jogo
                        _smp.message_pool.getInstance().push(new message("[GameBase::initPlayersItemRainPersistNextHole][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        continue;
                    }

                    // Devil Wings
                    if (s.m_pi.ei.char_info.parts_typeid.Any(_element =>
      devil_wings.Contains(_element)))
                    {

                        _smp.message_pool.getInstance().push(new message("[GameBase::initPlayersItemRainPersistNextHole][Log] PLAYER[UID=" + Convert.ToString(s.m_pi.uid) + "] esta equipado com Devil Wings no Character[TYPEID=" + Convert.ToString(s.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(s.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        // sai por que só precisa que 1 player tenha o item para valer para o game todo
                        m_rv.persist_rain = 1;
                        return;
                    } 
                }
            }
        }

        /// <summary>
        /// gerar o item do artefato, pode dar exp, pang, e etc...
        /// </summary>
        private void initArtefact()
        {

            switch (m_ri.artefato)
            {
                // Artefact of EXP
                case ART_LUMINESCENT_CORAL:
                    m_rv.exp += 2;
                    break;
                case ART_TROPICAL_TREE:
                    m_rv.exp += 4;
                    break;
                case ART_TWIN_LUNAR_MIRROR:
                    m_rv.exp += 6;
                    break;
                case ART_MACHINA_WRENCH:
                    m_rv.exp += 8;
                    break;
                case ART_SILVIA_MANUAL:
                    m_rv.exp += 10;
                    break;
                // End
                // Artefact of Rain Rate
                case ART_SCROLL_OF_FOUR_GODS:
                    m_rv.rain += 5;
                    break;
                case ART_ZEPHYR_TOTEM:
                    m_rv.rain += 10;
                    break;
                case ART_DRAGON_ORB:
                    m_rv.rain += 20;
                    break;
                    // End
            }
        }

        private PlayerGameInfo.eCARD_WIND_FLAG getPlayerWindFlag(Player _session)
        {

            if (_session.m_pi.ei.char_info == null)
            { // Player n�o est� com character equipado, kika dele do jogo
                _smp.message_pool.getInstance().push(new message("[GameBase::getPlayerWindFlag][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return PlayerGameInfo.eCARD_WIND_FLAG.NONE;
            } 
            return PlayerGameInfo.eCARD_WIND_FLAG.NONE;
        }

        public int initCardWindPlayer(PlayerGameInfo _pgi, byte _wind)
        {

            if (_pgi == null)
            {
                throw new exception("[GameBase::initCardWindPlayer][Error] PlayerGameInfo* _pgi is invalid(null). Ao tentar inicializar o card wind player no jogo. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            switch (_pgi.card_wind_flag)
            {
                case PlayerGameInfo.eCARD_WIND_FLAG.NORMAL:
                    if (_wind == 8) // 9m Wind
                    {
                        return -1;
                    }
                    break;
                case PlayerGameInfo.eCARD_WIND_FLAG.RARE:
                    if (_wind > 0) // All Wind
                    {
                        return -1;
                    }
                    break;
                case PlayerGameInfo.eCARD_WIND_FLAG.SUPER_RARE:
                    if (_wind >= 5) // High(strong) Wind
                    {
                        return -2;
                    }
                    break;
                case PlayerGameInfo.eCARD_WIND_FLAG.SECRET:
                    if (_wind >= 5) // High(strong) Wind
                    {
                        return -2;
                    }
                    else if (_wind > 0) // Low(weak) Wind, 1m não precisa diminuir
                    {
                        return -1;
                    }
                    break;
            }

            return 0;
        }

        private PlayerGameInfo.stTreasureHunterInfo getPlayerTreasureInfo(Player _session)
        {

            PlayerGameInfo.stTreasureHunterInfo pti = new PlayerGameInfo.stTreasureHunterInfo();

            if (_session.m_pi.ei.char_info == null)
            { // Player não está com character equipado, kika dele do jogo
                _smp.message_pool.getInstance().push(new message("[GameBase::getPlayerTreasureInfo][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return pti;
            }
             
            /// Todos que dão Drop Rate da treasue hunter point, então aonde dá o drop rate já vai dá o treasure point
            /// Angel Wings deixa que ela é uma excessão não tem os valores no IFF, é determinado pelo server e o ProjectG
            // Passarinho gordo aumenta 30 treasure hunter point para todos scores
            //if (_session.m_pi.ei.mascot_info != null && _session.m_pi.ei.mascot_info->_typeid == MASCOT_FAT_BIRD)
            //pti.all_score += 30;	// +30 all score

            // Verifica se está com asa de anjo equipada (shop ou gacha), aumenta 30 treasure hunter point para todos scores
            if (_session.m_pi.ei.char_info.AngelEquiped() == 1 && _session.m_pi.ui.getQuitRate() < GOOD_PLAYER_ICON)
            {
                pti.all_score += 30; // +30 all score
            }

            return pti;
        }

        public virtual void updatePlayerAssist(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "updatePlayerAssist" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou atualizar assist pang no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (pgi.assist_flag == 1 && pgi.level > 10)
                pgi.data.pang = Convert.ToUInt64(pgi.data.pang * 0.7f); // - 30% dos pangs
        }

        public virtual void initGameTime()
        {
            m_start_time = DateTime.Now;
        }

        public virtual uint32_t getRankPlace(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "getRankPlace" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou pegar o lugar no rank do jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            int index = m_player_order.IndexOf(pgi);

            return (index != -1) ? (uint)index : uint32_t.MaxValue;
        }

        
        public virtual void requestCalculePang(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "requestCalculePang" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou calcular o pang do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            // Course Rate of Pang
            var course = sIff.getInstance().findCourse((uint)((int)m_ri.course & 0x7F) | 0x28000000u);

            // Rate do course, tem uns que é 10% a+ tem outros que é 30% a mais que o pangya JP deixou
            float course_rate = (course != null && course.RatePang >= 1.0f) ? course.RatePang : 1.0f;
            float pang_rate = 0.0f;

            pang_rate = TRANSF_SERVER_RATE_VALUE(pgi.used_item.rate.pang) * TRANSF_SERVER_RATE_VALUE((uint)(m_rv.pang)) * course_rate;

            pgi.data.bonus_pang = (uint64_t)(((pgi.data.pang * pang_rate) - pgi.data.pang) + (pgi.data.bonus_pang * pang_rate));
        }

        public virtual void requestSaveInfo(Player _session, int option)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "requestSaveInfo" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou salvar o info dele no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            try
            {

                // Aqui dados do jogo ele passa o holein no lugar do mad_conduta <-> holein, agora quando ele passa o info user é invertido(Normal)
                // Inverte para salvar direito no banco de dados
                var tmp_holein = pgi.ui.hole_in;

                pgi.ui.hole_in = pgi.ui.mad_conduta;
                pgi.ui.mad_conduta = tmp_holein;

                if (option == 0)
                { // Terminou VS

                    // Verifica se o Angel Event está ativo de tira 1 quit do player que concluí o jogo
                    if (m_ri.angel_event)
                    {
                        pgi.ui.quitado = -1;
                    }

                    pgi.ui.exp = 0;
                    pgi.ui.combo = 1;
                    pgi.ui.jogado = 1;
                    pgi.ui.media_score = pgi.data.score;

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC; // NanoSeconds To Seconds
                    }

                    pgi.ui.tempo = (int32_t)diff;

                }
                else if (option == 1)
                { // Quitou ou tomou DC

                    // Quitou ou saiu não ganha pangs
                    pgi.data.pang = 0;
                    pgi.data.bonus_pang = 0;

                    pgi.ui.exp = 0;
                    pgi.ui.combo = (int)(DECREASE_COMBO_VALUE * -1);
                    pgi.ui.jogado = 1;

                    // Verifica se tomou DC ou Quitou, ai soma o membro certo
                    if (!_session.m_connection_timeout)
                    {
                        pgi.ui.quitado = 1;
                    }
                    else
                    {
                        pgi.ui.disconnect = 1;
                    }

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    pgi.ui.media_score = pgi.data.score;

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC; // NanoSeconds To Seconds
                    }

                    pgi.ui.tempo = (int32_t)diff;

                }
                else if (option == 2)
                { // Não terminou o hole 1, alguem saiu ai volta para sala sem contar o combo, só conta o jogo que começou

                    pgi.data.pang = 0;
                    pgi.data.bonus_pang = 0;

                    pgi.ui.exp = 0;
                    pgi.ui.jogado = 1;

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC; // NanoSeconds To Seconds
                    }

                    pgi.ui.tempo = (int32_t)diff;

                }
                else if (option == 4)
                { // SSC

                    pgi.ui.clear();

                    // Verifica se o Angel Event está ativo de tira 1 quit do player que concluí o jogo
                    if (m_ri.angel_event)
                    {

                        pgi.ui.quitado = -1;
                    }

                    pgi.ui.exp = 0;
                    pgi.ui.combo = 1;
                    pgi.ui.jogado = 1;
                    pgi.ui.media_score = 0;

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC;
                    }

                    pgi.ui.tempo = (int32_t)diff;

                }
                else if (option == 5)
                {

                    // Quitou ou saiu não ganha pangs
                    pgi.data.pang = 0;
                    pgi.data.bonus_pang = 0;

                    pgi.ui.exp = 0;
                    pgi.ui.jogado = 1;
                    pgi.ui.media_score = pgi.data.score;

                    // Os valores que eu não colocava
                    pgi.ui.jogados_disconnect = 1; // Esse aqui é o contador de jogos que o player começou é o mesmo do jogado, só que esse aqui usa para o disconnect

                    var diff = UtilTime.GetLocalDateDiff(m_start_time);

                    if (diff > 0)
                    {
                        diff /= STDA_10_MICRO_PER_SEC; // NanoSeconds To Seconds
                    }

                    pgi.ui.tempo = (int32_t)diff;
                }
                  
                // Pode tirar pangs
                int64_t total_pang = (long)(pgi.data.pang + pgi.data.bonus_pang);

                // UPDATE ON SERVER AND DB
                _session.m_pi.addUserInfo(pgi.ui, (ulong)total_pang); // add User Info

                if (total_pang > 0)
                {
                    _session.m_pi.addPang((ulong)total_pang); // add Pang
                }
                else if (total_pang < 0)
                {
                    _session.m_pi.consomePang((ulong)(total_pang * -1)); // consome Pangs
                } 
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[GameBase::requestSaveInfo][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public virtual void requestUpdateItemUsedGame(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "requestUpdateItemUsedGame" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou atualizar itens usado no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            var ui = pgi.used_item;

            // Club Mastery // ((int)((int)m_ri.course & 0x7F) == RoomInfo.TIPO.SPECIAL_SHUFFLE_COURSE ? 1.5f : 1.f), SSSC sobrecarrega essa função para colocar os valores dele
            ui.club.count += (uint32_t)(1.0f * 10.0f * ui.club.rate * TRANSF_SERVER_RATE_VALUE(m_rv.clubset) * TRANSF_SERVER_RATE_VALUE(ui.rate.club));

            // Passive Item exceto Time Booster e var Command, que soma o contador por uso, o cliente passa o pacote, dizendo que usou o item
            foreach (var el in ui.v_passive)
            {

                // Verica se é o ultimo hole, terminou o jogo, ai tira soma 1 ao count do pirulito que consome por jogo
                if (CHECK_PASSIVE_ITEM(el.Value._typeid)
                    && el.Value._typeid != TIME_BOOSTER_TYPEID
                    && el.Value._typeid != AUTO_COMMAND_TYPEID)
                {

                    // Item de Exp Boost que só consome 1 Por Jogo, só soma no requestFinishItemUsedGame
                    if (passive_item_exp_1perGame.Contains(el.Value._typeid))
                    {
                        el.Value.count++;
                    }

                }
                else if (sIff.getInstance().getItemGroupIdentify(el.Value._typeid) == sIff.getInstance().BALL || sIff.getInstance().getItemGroupIdentify(el.Value._typeid) == sIff.getInstance().AUX_PART) //AuxPart(Anel)
                {
                    el.Value.count++;
                }
            }
        }

        public virtual void requestFinishItemUsedGame(Player _session)
        {

            List<stItemEx> v_item = new List<stItemEx>();

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "requestFinishItemUsedGame" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou finalizar itens usado no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            // Player já finializou os itens usados, verifica para não finalizar dua vezes os itens do player
            if (pgi.finish_item_used == 1)
            {

                _smp.message_pool.getInstance().push(new message("[GameBase::requestFinishItemUsedGame][Warning] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] ja finalizou os itens. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return;
            }

            var ui = pgi.used_item;

            // Add +1 ao itens que consome 1 só por jogo
            // Item de Exp Boost que só consome 1 Por Jogo
            foreach (var _el in ui.v_passive)
            {
                if (passive_item_exp_1perGame.Contains(_el.Value._typeid))
                {
                    _el.Value.count++;
                }
            }

             
            // Passive Item
            foreach (var el in ui.v_passive)
            {

                if (el.Value.count > 0)
                {
                      
                }
            }

            // Active Item
            foreach (var el in ui.v_active)
            {

                if (el.Value.count > 0)
                {

                    
                    // Só tira os itens Active se a sala não estiver com o artefact Frozen Flame,
                    // se ele estiver com artefact Frozen Flame ele mantém os Itens Active, não consome e nem desequipa do inventório do player
                    if (m_ri.artefato != ART_FROZEN_FLAME)
                    {

                        // Limpa o Item Slot do player, dos itens que foram usados(Ativados) no jogo
                        if (el.Value.count <= el.Value.v_slot.Count)
                        {

                            for (var i = 0; i < el.Value.count; ++i)
                            {
                                _session.m_pi.ue.item_slot[el.Value.v_slot[i]] = 0;
                            }
                        }

                        var pWi = _session.m_pi.findWarehouseItemByTypeid(el.Value._typeid);

                        if (pWi != null)
                        {
                            // Init Item
                            var item = new stItemEx();

                            item.type = 2;
                            item._typeid = pWi._typeid;
                            item.id = pWi.id;
                            item.qntd = (int)el.Value.count;
                            item.STDA_C_ITEM_QNTD = (short)(item.qntd * -1);

                            // Add On Vector
                            v_item.Add(new stItemEx(item));

                        }
                        else
                        {
                            _smp.message_pool.getInstance().push(new message("[GameBase::requestFinishItemUsedGame][Warning] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou atualizar item[TYPEID=" + Convert.ToString(el.Value._typeid) + "] que ele nao possui. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                        }
                    }
                }
            }

            // Update Item Equiped Slot ON DB
            snmdb.NormalManagerDB.getInstance().add(25,
                new CmdUpdateItemSlot(_session.m_pi.uid, _session.m_pi.ue.item_slot),
                SQLDBResponse, this);

            // Se for o Master da sala e ele estiver com artefato tira o mana dele
            // Antes tirava assim que começava o jogo, mas aí o cliente atualizava a sala tirando o artefact aí no final não tinha como ver se o frozen flame estava equipado
            // e as outras pessoas que estão na lobby não sabe qual artefect que está na sala, por que o master mesmo mando o pacote pra tirar da sala quando o server tira o mana dele no init game
            if (m_ri.artefato != 0 && m_ri.master == _session.m_pi.uid)
            {

                // Tira Artefact Mana do master da sala
                var pWi = _session.m_pi.findWarehouseItemByTypeid(m_ri.artefato + 1);

                if (pWi != null)
                {

                    var item = new stItemEx();

                    item.type = 2;
                    item.id = pWi.id;
                    item._typeid = pWi._typeid;
                    item.qntd = (int)((pWi.STDA_C_ITEM_QNTD <= 0) ? 1 : pWi.STDA_C_ITEM_QNTD32);
                    item.STDA_C_ITEM_QNTD = (short)(item.qntd * -1);

                    // Add on Vector Update Itens
                    v_item.Add(new stItemEx(item));

                }
                else
                {
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestFinishItemUsedGame][Warning] Master[UID=" + Convert.ToString(_session.m_pi.uid) + "] do jogo nao tem Mana do Artefect[TYPEID=" + Convert.ToString(m_ri.artefato) + ", MANA=" + Convert.ToString(m_ri.artefato + 1) + "] e criou e comecou um jogo com artefact sem mana. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            // Update Item ON Server AND DB
            if (v_item.Count > 0)
            {

                if (ItemManager.removeItem(v_item, _session) <= 0)
                {
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestFinishItemUsedGame][Warning] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu deletar os item do player. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            // Club Mastery
            if (ui.club.count > 0u && ui.club._typeid > 0u)
            {

                var pClub = _session.m_pi.findWarehouseItemByTypeid(ui.club._typeid);

                if (pClub != null)
                {

                    pClub.clubset_workshop.mastery += ui.club.count;

                    var item = new stItemEx();

                    item.type = 0xCC;
                    item.id = (int)pClub.id;
                    item._typeid = pClub._typeid;

                    item.clubset_workshop.c = pClub.clubset_workshop.c;

                    item.clubset_workshop.level = (byte)pClub.clubset_workshop.level;
                    item.clubset_workshop.mastery = pClub.clubset_workshop.mastery;
                    item.clubset_workshop.rank = (uint)pClub.clubset_workshop.rank;
                    item.clubset_workshop.recovery = pClub.clubset_workshop.recovery_pts;

                    snmdb.NormalManagerDB.getInstance().add(12,
                        new CmdUpdateClubSetWorkshop(_session.m_pi.uid,
                            pClub,
                            CmdUpdateClubSetWorkshop.FLAG.F_TRANSFER_MASTERY_PTS),
                        SQLDBResponse, this);

                    v_item.Add(new stItemEx(item));
                }
                else
                {
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestFinishItemUsedGame][Warning] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou salvar mastery do ClubSet[TYPEID=" + Convert.ToString(ui.club._typeid) + "] que ele nao tem. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            // Flag de que o palyer já finalizou os itens usados no jogo, para não finalizar duas vezes
            pgi.finish_item_used = 1;

            // Atualiza ON Jogo
            if (v_item.Count > 0)
            {
                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x216);

                p.WriteUInt32((uint32_t)UtilTime.GetSystemTimeAsUnix());
                p.WriteUInt32((uint32_t)v_item.Count);

                foreach (var el in v_item)
                {
                    p.WriteByte(el.type);
                    p.WriteUInt32(el._typeid);
                    p.WriteInt32(el.id);
                    p.WriteUInt32(el.flag_time);
                    p.WriteBytes(el.stat.ToArray());
                    p.WriteUInt32((el.STDA_C_ITEM_TIME > 0) ? el.STDA_C_ITEM_TIME : el.STDA_C_ITEM_QNTD);
                    p.WriteZeroByte(25); // 10 PCL[C0~C4] 2 Bytes cada, 15 bytes desconhecido
                    if (el.type == 0xCC)
                    {
                        p.WriteBytes(el.clubset_workshop.ToArray());
                    }
                }

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        /// <summary>
        /// 100%, verificar o acerto o hole 100%
        /// </summary>
        /// <param name="_session"></param>
        /// <param name="option"></param>
        /// <exception cref="exception"></exception>
        public virtual void requestFinishHole(Player _session, int option)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "requestFinishHole" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou finalizar o dados do hole do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (pgi.hole == 255)
                return;

            //var hole = m_course.findHole(pgi.hole);

            //if (hole == null)
            //{
            //    throw new exception("[GameBase::finishHole][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou finalizar hole[NUMERO=" + Convert.ToString((ushort)pgi.hole) + "] no jogo, mas o numero do hole is invalid. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
            //        20, 0));
            //}

            int score_hole = 0;//negativo 
            // Finish Hole Dados
            if (option == 0) // melhorar depois@@@@@
            {
                pgi.data.total_tacada_num += pgi.data.tacada_num;
                // Tacadas do hole
                var tacada_hole = pgi.data.tacada_num;

                // Tacadas do hole 
                //pgi.data.score += Convert.ToInt32(pgi.data.tacada_num - hole.getPar().par);
                //score_hole = Convert.ToInt32(pgi.data.tacada_num - hole.getPar().par);
                 

                //_smp.message_pool.getInstance().push(new message("[GameBase::requestFinishHole][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] terminou o hole[COURSE=" + Convert.ToString(hole.getCourse()) + ", NUMERO=" + Convert.ToString(hole.getNumero()) + ", PAR=" + Convert.ToString(hole.getPar().par) + ", SHOT=" + pgi.data.tacada_num + ", SCORE=" + Convert.ToString(pgi.data.score) + ", TOTAL_SHOT=" + Convert.ToString(pgi.data.total_tacada_num) + ", TOTAL_SCORE=" + Convert.ToString(pgi.data.score) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Zera dados
                pgi.data.time_out = 0;

                // Giveup Flag
                pgi.data.giveup = 0;

                // Zera as penalidades do hole
                pgi.data.penalidade = 0;

            }
            else if (option == 1)
            { // Não acabou o hole então faz os calculos para o jogo todo

                //var range = m_course.findRange(pgi.hole);

                //foreach (var kv in range)
                //{
                //    if (kv.Key > m_ri.qntd_hole)
                //    {
                //        break;  // Igual à condição it->first <= m_ri.qntd_hole
                //    }

                //    pgi.data.total_tacada_num += (uint)kv.Value.getPar().total_shot;
                //    pgi.data.score += kv.Value.getPar().range_score[1];  // Max Score
                //}
                 
                // Zera dados
                pgi.data.time_out = 0;

                pgi.data.tacada_num = 0;

                // Giveup Flag
                pgi.data.giveup = 0;

                // Zera as penalidades do hole do player
                pgi.data.penalidade = 0;
            }

            // Aqui tem que atualiza o PGI direitinho com outros dados
           // pgi.progress.hole = (short)m_course.findHoleSeq(pgi.hole);

            // Dados Game Progress do Player
            if (option == 0)
            {

                if (pgi.progress.hole > 0)
                {

                    if (pgi.shot_sync.state_shot.display.acerto_hole)
                    {
                        pgi.progress.finish_hole[pgi.progress.hole - 1] = 1; // Terminou o hole
                    }

                    //pgi.progress.par_hole[pgi.progress.hole - 1] = hole.getPar().par;
                    pgi.progress.score[pgi.progress.hole - 1] = (sbyte)score_hole;
                    pgi.progress.tacada[pgi.progress.hole - 1] = pgi.data.tacada_num;
                }

            }
            else
            {

                //var range = m_course.findRange(pgi.hole);

                //foreach (var kv in range)
                //{
                //    int index = kv.Key - 1;

                //    pgi.progress.finish_hole[index] = 0; // não terminou

                //    pgi.progress.par_hole[index] = kv.Value.getPar().par;

                //    pgi.progress.score[index] = kv.Value.getPar().range_score[1]; // Max Score

                //    pgi.progress.tacada[index] = (uint)kv.Value.getPar().total_shot;
                //}

            }

        }

        public virtual void requestSaveRecordCourse(Player _session,
            int game, int option)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "requestSaveRecordCourse" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou salvar record do course do player no jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            if (_session.m_pi.ei.char_info == null)
            { // Player não está com character equipado, kika dele do jogo
                _smp.message_pool.getInstance().push(new message("[GameBase::requestSaveRecordCourse][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            MapStatistics pMs = null;

            if (pgi.assist_flag == 1)
            { // Assist

                pMs = _session.m_pi.a_msa_normal[(int)((int)m_ri.course & 0x7F)];
            }
            else
            { // Sem Assist


                pMs = _session.m_pi.a_ms_normal[(int)((int)m_ri.course & 0x7F)];
            }

            bool make_record = false;

            // UPDATE ON SERVER
            if (option == 1)
            { // 18h pode contar record

                // Fez Record
                if (pMs.best_score == 127
                    || pgi.data.score < pMs.best_score
                    || pgi.data.pang > pMs.best_pang)
                {

                    // Update Best Score Record
                    if (pgi.data.score < pMs.best_score)
                    {
                        pMs.best_score = (sbyte)pgi.data.score;
                    }

                    // Update Best Pang Record
                    if (pgi.data.pang > pMs.best_pang)
                    {
                        pMs.best_pang = pgi.data.pang;
                    }

                    // Update Character Record
                    pMs.character_typeid = _session.m_pi.ei.char_info._typeid;

                    make_record = true;
                }
            }

            // Salva os dados normais
            pMs.tacada += (uint)pgi.ui.tacada;
            pMs.putt += (uint)pgi.ui.putt;
            pMs.hole += (uint)pgi.ui.hole;
            pMs.fairway += (uint)pgi.ui.fairway;
            pMs.hole_in += (uint)pgi.ui.hole_in;
            pMs.putt_in += (uint)pgi.ui.putt_in;
            pMs.total_score += pgi.data.score;
            pMs.event_score = 0;

            MapStatisticsEx ms = new MapStatisticsEx(pMs);

            ms.tipo = (byte)game;

            // UPDATE ON DB
            snmdb.NormalManagerDB.getInstance().add(5,
                new CmdUpdateMapStatistics(_session.m_pi.uid,
                    ms, pgi.assist_flag),
                SQLDBResponse, this);

            // UPDATE ON GAME, se ele fez record, e add 1000 para ele
            if (make_record)
            {

                // Log
                _smp.message_pool.getInstance().push(new message("[GameBase::requestSaveRecordCourse][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] fez record no Map[COURSE=" + Convert.ToString((ushort)(int)((int)m_ri.course & 0x7F)) + " (" + Convert.ToString((ushort)pMs.course) + "), SCORE=" + Convert.ToString((short)pMs.best_score) + ", PANG=" + Convert.ToString(pMs.best_pang) + ", CHARACTER=" + Convert.ToString(pMs.character_typeid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Add 1000 pang por ele ter quebrado o  record dele
                _session.m_pi.addPang(1000);

                // Resposta para make record
                PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0xB9);

                p.WriteByte(((int)m_ri.course) & 0x7F);

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public virtual void requestInitItemUsedGame(Player _session, PlayerGameInfo _pgi)
        {

            //INIT_PLAYER_INFO("requestInitItemUsedGame", "tentou inicializar itens usado no jogo", _session, out PlayerGameInfo pgi);

            // Characters Equip
            if (_session.getState() && _session.isConnected())
            { // Check Player Connected

                if (_session.m_pi.ei.char_info == null)
                { // Player não está com character equipado, kika dele do jogo
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestInitItemUsedGame][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    return;
                }

                if (_session.m_pi.ei.comet == null)
                { // Player não está com Comet(Ball) equipado, kika dele do jogo
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestInitItemUsedGame][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Ball equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    return;
                }

                var ui = _pgi.used_item;

                // Zera os Itens usados
                ui.clear();

                /// ********** Itens Usado **********

                // Passive Item Equipado
                _session.m_pi.mp_wi.ToList().ForEach(_el =>
                {
                    if (passive_item.Any(c => c == _el.Value._typeid))
                    {
                        ui.v_passive.insert(Tuple.Create(_el.Value._typeid, new UsedItem.Passive(_el.Value._typeid, 0u)));
                    }
                });
                // Ball Equiped 
                if (_session.m_pi.ei.comet._typeid != DEFAULT_COMET_TYPEID && (!_session.m_pi.m_cap.premium_user))
                {
                    ui.v_passive.insert(Tuple.Create((uint32_t)_session.m_pi.ei.comet._typeid, new UsedItem.Passive(_session.m_pi.ei.comet._typeid, 0u)));
                }

                // AuxParts
                for (var i = 0; i < (_session.m_pi.ei.char_info.auxparts.Length); ++i)
                {
                    if (_session.m_pi.ei.char_info.auxparts[i] >= 0x70000000 && _session.m_pi.ei.char_info.auxparts[i] < 0x70010000)
                    {
                        ui.v_passive.insert(Tuple.Create((uint32_t)_session.m_pi.ei.char_info.auxparts[i], new UsedItem.Passive(_session.m_pi.ei.char_info.auxparts[i], 0u)));
                    }
                }

                // Item Active Slot 
                for (var i = 0; i < (_session.m_pi.ue.item_slot.Length); ++i)
                {
                    // Diferente de 0 item está equipado
                    if (_session.m_pi.ue.item_slot[i] != 0)
                    {
                        if (!ui.v_active.ContainsKey(_session.m_pi.ue.item_slot[i])) // Não tem add o novo
                        {
                            ui.v_active.insert(Tuple.Create(_session.m_pi.ue.item_slot[i], new UsedItem.Active(_session.m_pi.ue.item_slot[i], 0u, new List<byte> { (byte)i })));
                        }

                        else // Já tem add só o slot
                        {
                            ui.v_active[(uint32_t)_session.m_pi.ue.item_slot[i]].v_slot.Add((byte)i); // Slot
                        }
                    }
                }

                // ClubSet For ClubMastery
                ui.club._typeid = _session.m_pi.ei.csi._typeid;
                ui.club.count = 0;
                ui.club.rate = 1.0f;
                 
                /// ********** Itens Usado **********
                 

                // Pang
                ui.v_passive.ToList().ForEach(_el =>
                {
                    if (Array.IndexOf(passive_item_pang_x2, _el.Value._typeid) != passive_item_pang_x2.Length - 1)
                    {
                        ui.rate.pang += 200;
                        _pgi.boost_item_flag.pang = 1;
                    }

                    if (Array.IndexOf(passive_item_pang_x4, _el.Value._typeid) != passive_item_pang_x4.Length - 1)
                    {
                        ui.rate.pang += 400;
                        _pgi.boost_item_flag.pang_nitro = 1;
                    }

                    if (Array.IndexOf(passive_item_pang_x1_5, _el.Value._typeid) != passive_item_pang_x1_5.Length - 1)
                    {
                        ui.rate.pang += 50;
                        _pgi.boost_item_flag.pang = 1;
                    }

                    if (Array.IndexOf(passive_item_pang_x1_4, _el.Value._typeid) != passive_item_pang_x1_4.Length - 1)
                    {
                        ui.rate.pang += 40;
                        _pgi.boost_item_flag.pang = 1;
                    }

                    if (Array.IndexOf(passive_item_pang_x1_2, _el.Value._typeid) != passive_item_pang_x1_2.Length - 1)
                    {
                        ui.rate.pang += 20;
                        _pgi.boost_item_flag.pang = 1;
                    }
                });


                // Exp
                ui.v_passive.ToList().ForEach(_el =>
                {
                    if (Array.IndexOf(passive_item_exp, _el.Value._typeid) != -1)
                    {
                        ui.rate.exp += 200;
                    }
                });

                // Club Mastery Boost
                ui.v_passive.ToList().ForEach(_el =>
                {
                    if (Array.IndexOf(passive_item_club_boost, _el.Value._typeid) != -1)
                    {
                        ui.rate.club += 200;
                    }
                });

                // Character Parts Equipado
                if (_session.m_pi.ei.char_info.parts_typeid.Any(_element =>
                    Array.IndexOf(hat_birthday, _element) != -1))
                {
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestInitItemUsedGame][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Hat Birthday no Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(_session.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    ui.rate.exp += 20; // 20% Hat Birthday
                }

                // Hat Lua e Sol
                if (_session.m_pi.ei.char_info.parts_typeid.Any(_element =>
                    Array.IndexOf(hat_lua_sol, _element) != -1))
                {
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestInitItemUsedGame][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Hat Lua e Sol no Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(_session.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    ui.rate.exp += 20;  // 20% Hat Lua e Sol
                    ui.rate.pang += 20; // 20% Hat Lua e Sol
                }

                // Kurafaito Ring Club Mastery
                if (Array.IndexOf(_session.m_pi.ei.char_info.auxparts, KURAFAITO_RING_CLUBMASTERY) != -1)
                {
                    _smp.message_pool.getInstance().push(new message("[GameBase::requestInitItemUsedGame][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] esta equipado com Anel (Kurafaito) que da Club Mastery +1.1% no Character[TYPEID=" + Convert.ToString(_session.m_pi.ei.char_info._typeid) + ", ID=" + Convert.ToString(_session.m_pi.ei.char_info.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    ui.rate.club += 10; // Kurafaito Ring da + 10% no Club Mastery
                }

                // Character AuxParts Equipado
                // Aux parts tem seus próprios valores de rate no iff
                foreach (var _el in _session.m_pi.ei.char_info.auxparts)
                {
                    if (_el != 0 && sIff.getInstance().getItemGroupIdentify(_el) == sIff.getInstance().AUX_PART)
                    {
                        var auxpart = sIff.getInstance().findAuxPart(_el);
                        if (auxpart != null)
                        {
                            if (auxpart.Pang_Rate > 100)
                            {
                                ui.rate.pang += (uint)(auxpart.Pang_Rate - 100);
                            }
                            else if (auxpart.Pang_Rate > 0)
                            {
                                ui.rate.pang += auxpart.Pang_Rate;
                            }

                            if (auxpart.Exp_Rate > 100)
                            {
                                ui.rate.exp += (uint)(auxpart.Exp_Rate - 100);
                            }
                            else if (auxpart.Exp_Rate > 0)
                            {
                                ui.rate.exp += auxpart.Exp_Rate;
                            }

                            if (auxpart.Drop_Rate > 100)
                            {
                                ui.rate.drop += (uint)(auxpart.Drop_Rate - 100);
                            }
                            else if (auxpart.Drop_Rate > 0)
                            {
                                ui.rate.drop += auxpart.Drop_Rate;
                            }

                            _pgi.thi.all_score += 15;
                        }
                    }
                } 
            }
        }

        public virtual void requestSendTreasureHunterItem(Player _session)
        {

            var pgi = getPlayerInfo((_session));

            if (pgi == null)
            {
                throw new exception("[GameBase::" + "requestSendTreasureHunterItem" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou enviar os itens ganho no Treasure Hunter do jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            List<stItem> v_item = new List<stItem>();

            if (!pgi.thi.v_item.empty())
            {
                foreach (var el in pgi.thi.v_item)
                {

                    var bi = new BuyItem();
                    var item = new stItem();

                    bi.id = -1;
                    bi._typeid = el._typeid;
                    bi.qntd = el.qntd;

                    ItemManager.initItemFromBuyItem(_session.m_pi,
                        item, bi, false, 0, 0, 1);

                    if (item._typeid == 0)
                    {
                        _smp.message_pool.getInstance().push(new message("[GameBase::requestSendTreasureHunterItem][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou inicializar item[TYPEID=" + Convert.ToString(bi._typeid) + "], mas nao consgeuiu. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));

                        continue;
                    }

                    v_item.Add(new stItem(item));
                }

                // Add Item, se tiver Item
                if (v_item.Count > 0)
                {

                    var rai = ItemManager.addItem(v_item,
                        _session, 0, 0);

                    if (rai.fails.Count > 0 && rai.type != ItemManager.RetAddItem.T_SUCCESS_PANG_AND_EXP_AND_CP_POUCH)
                    {
                        _smp.message_pool.getInstance().push(new message("[GameBase::requestSendTreasureHunterItem][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao conseguiu adicionar os itens que ele ganhou no Treasure Hunter. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }

            // UPDATE ON GAME
            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x134);

            p.WriteByte((byte)v_item.Count);

            foreach (var el in v_item)
            {
                p.WriteUInt32(_session.m_pi.uid);

                p.WriteUInt32(el._typeid);
                p.WriteInt32(el.id);
                p.WriteInt32(el.qntd);
                p.WriteByte(0); // Opt Acho, mas nunca vi diferente de 0

                p.WriteUInt16((ushort)(el.stat.qntd_dep / 0x8000));
                p.WriteUInt16((ushort)(el.stat.qntd_dep % 0x8000));
            }

            packet_func.session_send(p,
                _session, 1);
        }

        public virtual byte checkCharMotionItem(Player _session)
        {

            // Characters Equip
            if (_session.getState() && _session.isConnected())
            { // Check Player Connected

                if (_session.m_pi.ei.char_info == null)
                { // Player não está com character equipado, kika dele do jogo
                    _smp.message_pool.getInstance().push(new message("[GameBase::checkCharMotionItem][Log] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] nao esta com Character equipado. kika ele do jogo. pode ser Bug.", type_msg.CL_FILE_LOG_AND_CONSOLE));


                    return 0;
                }

                // Motion Item
                if (_session.m_pi.ei.char_info.parts_typeid.Any(_element =>
    motion_item.Contains(_element)))
                {
                    return 1;
                }

            }

            return 0;
        }

        // Atualiza o Info do usuario, Info Trofel e Map Statistics do Course
        // Opt 0 Envia tudo, -1 não envia o map statistics
        public virtual void sendUpdateInfoAndMapStatistics(Player _session, int _option)
        {

            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x45);

            p.WriteBytes(_session.m_pi.ui.ToArray());

            p.WriteBytes(_session.m_pi.ti_current_season.ToArray());

            // Ainda tenho que ajeitar esses Map Statistics no Pacote Principal, No Banco de dados e no player_info class
            if (_option == -1)
            {

                // -1 12 Bytes, os 2 tipos de dados do Map Statistics
                p.WriteInt64(-1);
                p.WriteInt32(-1);

            }
            else
            {
                // Normal essa season
                if (_session.m_pi.a_ms_normal[(int)((int)m_ri.course & 0x7F)].course != ((int)m_ri.course & 0x7F))
                {
                    p.WriteSByte(-1); // Não tem
                }
                else
                {
                    p.WriteByte((char)m_ri.course & 0x7F);
                    p.WriteBytes(_session.m_pi.a_ms_normal[(int)((int)m_ri.course & 0x7F)].ToArray());
                }

                p.WriteSByte(-1); // Não tem 
            }

            packet_func.session_send(p,
                _session, 1);
        }

        // Envia a message no char para todos player do Game que o player terminou o jogo
        protected virtual void sendFinishMessage(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "sendFinishMessage" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou enviar message no chat que o player terminou o jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                    1, 4));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter((ushort)0x40);

            p.WriteByte(16); // Msg que terminou o game

            p.WriteString(_session.m_pi.nickname);
            p.WriteUInt16(0); // Size Msg

            p.WriteInt32(pgi.data.score);
            p.WriteUInt64(pgi.data.pang);
            p.WriteByte(pgi.assist_flag);

            packet_func.game_broadcast(this,
                p.GetBytes, 1);
        }

        public virtual void requestCalculeRankPlace()
        {
            if (m_player_order.Count > 0)
            {
                m_player_order.Clear();
            }

            foreach (var el in m_player_info)
            {
                if (el.Value.flag != PlayerGameInfo.eFLAG_GAME.QUIT) // menos os que quitaram
                {
                    m_player_order.Add(el.Value);
                }
            }

            m_player_order.Sort(sort_player_rank);
        }

        public int sort_player_rank(PlayerGameInfo _pgi1, PlayerGameInfo _pgi2)
        {
            if (_pgi1.data.score == _pgi2.data.score)
                return _pgi2.data.pang.CompareTo(_pgi1.data.pang); // decrescente de pang (maior pang primeiro)

            return _pgi1.data.score.CompareTo(_pgi2.data.score); // crescente de score (menor score primeiro)
        }

        // Set Flag Game and finish_game flag
        public virtual void setGameFlag(PlayerGameInfo _pgi, PlayerGameInfo.eFLAG_GAME _fg)
        {

            if (_pgi == null)
            {

                _smp.message_pool.getInstance().push(new message("[GameBase::setGameFlag][Error] PlayerGameInfo* _pgi is invalid(null).", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return;
            }

            _pgi.flag = _fg;
        }

        public virtual void setFinishGameFlag(PlayerGameInfo _pgi, byte _finish_game)
        {

            if (_pgi == null)
            {

                _smp.message_pool.getInstance().push(new message("[GameBase::setFinishGameFlag][Error] PlayerGameInfo* _pgi is invlaid(null).", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return;
            }
            _pgi.finish_game = _finish_game;
        }

        // Check And Clear
        public virtual bool AllCompleteGameAndClear()
        {
            uint32_t count = 0;
            // Da error Aqui
            foreach (var el in m_players)
            {

                try
                {

                    var pgi = getPlayerInfo((el));
                    if (pgi == null)
                    {
                        throw new exception("[GameBase::" + "PlayersCompleteGameAndClear" + "][Error] PLAYER[UID=" + Convert.ToString((el).m_pi.uid) + "] " + "tentou verificar se o player terminou o jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            1, 4));
                    }

                    if (pgi.flag != PlayerGameInfo.eFLAG_GAME.PLAYING)
                    {
                        count++;
                    }

                }
                catch (exception e)
                {

                    _smp.message_pool.getInstance().push(new message("[GameBase::AllCompleteGameAndClear][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            return count == m_players.Count;
        }

        public bool PlayersCompleteGameAndClear()
        {
            uint32_t count = 0;
            // Da error Aqui
            foreach (var el in m_players)
            {

                try
                {

                    var pgi = getPlayerInfo(el);
                    if (pgi == null)
                    {
                        throw new exception("[GameBase::" + "PlayersCompleteGameAndClear" + "][Error] PLAYER[UID=" + Convert.ToString((el).m_pi.uid) + "] " + "tentou verificar se o player terminou o jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                            1, 4));
                    }

                    if (pgi.finish_game == 1)
                    {
                        count++;
                    }

                }
                catch (exception e)
                {

                    _smp.message_pool.getInstance().push(new message("[GamePlayersCompleteGameAndClear][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }

            return count == m_players.Count;
        }

        // Verifica se é o ultimo hole feito
        protected virtual bool checkEndGame(Player _session)
        {

            var pgi = getPlayerInfo((_session));
            if (pgi == null)
            {
                throw new exception("[GameBase::" + "checkEndGame" + "][Error] PLAYER[UID=" + Convert.ToString((_session).m_pi.uid) + "] " + "tentou verificar se eh o final do jogo" + ", mas o game nao tem o info dele guardado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME,
                1, 4));
            }
            return false;
            //return (m_course.findHoleSeq(pgi.hole) == m_ri.qntd_hole);
        }

        // Retorna todos os player que entrou no jogo, exceto os que quitaram
        public virtual uint32_t getCountPlayersGame()
        {

            size_t count = 0;

            count = m_player_info.Count(_el =>
            {
                return _el.Value.flag != PlayerGameInfo.eFLAG_GAME.QUIT;
            });

            return (uint32_t)count;
        }
        
       
        public virtual void setEffectActiveInShot(Player _session, uint64_t _effect)
        {
            try
            {

                INIT_PLAYER_INFO("setEffectActiveInShot", "tentou setar o efeito ativado na tacada", _session, out PlayerGameInfo pgi);

                pgi.effect_flag_shot.ullFlag |= _effect; // Ativa o efeito na tacada
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameBase::setEffectActiveInShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Limpa os dados que são usados para cada tacada, reseta ele para usar na próxima tacada 
        public virtual void clearDataEndShot(PlayerGameInfo _pgi)
        {

            if (_pgi == null)
                throw new exception("[GameBase::clearDataEndShot][Error] PlayerGameInfo *_pgi is invalid(null). Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME, 100, 0));

            try
            {
                _pgi.effect_flag_shot.clear();
                _pgi.item_active_used_shot = 0;
                _pgi.earcuff_wind_angle_shot = 0.0f;
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[GameBase::clearDataEndShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        } 


        protected static void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {

            if (_arg == null)
            {
                _smp.message_pool.getInstance().push(new message("[GameBase::SQLDBResponse][Warning] _arg is null com msg_id = " + (_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            // Por Hora só sai, depois faço outro tipo de tratamento se precisar
            if (_pangya_db.getException().getCodeError() != 0)
            {
                _smp.message_pool.getInstance().push(new message("[GameBase::SQLDBResponse][Error] " + _pangya_db.getException().getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }

            switch (_msg_id)
            {
                case 12:    // Update ClubSet Workshop
                    {
                        break;
                    }
                case 1: // Insert Ticket Report Dados
                    {
                        break;
                    }
                case 43:    // Insert Ticket Report Dados
                    {
                        break;
                    }
                case 44:    // Insert Ticket Report Dados
                    {
                        break;
                    }
                case 0:
                default:    // 25 é update item equipado slot
                    break;
            }
        }

        public void makePlayerInfo(Player _session)
        {
            try
            {
                PlayerGameInfo pgi = makePlayerInfoObject(_session);

                // Bloqueia o OID para ninguém pegar ele até o torneio acabar
                sgs.gs.getInstance().blockOID(_session.m_oid);

                // Update Place player
                _session.m_pi.place = 0;   // Jogando

                pgi.uid = _session.m_pi.uid;
                pgi.oid = _session.m_oid;
                pgi.level = _session.m_pi.mi.level;

                // Entrou no Jogo depois de ele ter começado
                if (m_state)
                    pgi.enter_after_started = 1;

                // Typeid do Mascot Equipado
                if (_session.m_pi.ei.mascot_info != null && _session.m_pi.ei.mascot_info._typeid > 0)
                    pgi.mascot_typeid = _session.m_pi.ei.mascot_info._typeid;

                // Premium User
                if (_session.m_pi.m_cap.premium_user)
                    pgi.premium_flag = true;

                // Card Wind Flag
                pgi.card_wind_flag = getPlayerWindFlag(_session);

                // Treasure Hunter Points Card Player Initialize Data
                // Não pode ser chamado depois do Init Item Used Game, por que ele vai add os pontos dos itens que dá Drop rate e treasure hunter point
                pgi.thi = getPlayerTreasureInfo(_session);

                // Flag Assist 
                if (_session.m_pi.assist_flag)
                    pgi.assist_flag = 1;

                // Verifica se o player está com o motion item equipado
                pgi.char_motion_item = checkCharMotionItem(_session);

                // Motion Item da Treasure Hunter Point também
                if (pgi.char_motion_item == 1)
                    pgi.thi.all_score += 20;    // +20 all score

                pgi.data.clear();
                pgi.location.clear();
                if (!m_player_info.ContainsKey(_session)) // ainda nao
                    m_player_info.Add(_session, pgi);
                else //ja tem ele 
                {
                    try
                    {

                        // pega o antigo PlayerGameInfo para usar no Log
                        var pgi_ant = m_player_info[_session];

                        // Novo PlayerGameInfo
                        m_player_info[_session] = pgi;

                        // Log de que trocou o PlayerGameInfo da session
                        _smp.message_pool.getInstance().push(new message("[GameBase::makePlayerInfo][Warning][Log] PLAYER[UID=" + (_session.m_pi.uid)
                                + "] esta trocando o PlayerGameInfo[UID=" + (pgi_ant.uid) + "] do player anterior que estava conectado com essa session, pelo o PlayerGameInfo[UID="
                                + (pgi.uid) + "] do player atual da session.", type_msg.CL_FILE_LOG_AND_CONSOLE));


                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        _smp.message_pool.getInstance().push(new message("[GameBase::makePlayerInfo][Error][Warning] PLAYER[UID=" + (_session.m_pi.uid)
                                + "], nao conseguiu atualizar o PlayerGameInfo da session para o novo PlayerGameInfo do player atual da session. Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }

                // Init Item Used Game(Dados)
                requestInitItemUsedGame(_session, pgi);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void clearAllPlayerInfo()
        {
            foreach (var el in m_player_info)
            {
                if (el.Value != null)
                    sgs.gs.getInstance().unblockOID(_oid: el.Value.oid);   // Desbloqueia o OID 
            }

            m_player_info.Clear();
        }

        public virtual void initAllPlayerInfo()
        {
            foreach (var el in m_players)
                makePlayerInfo(el);
        }

        // Make Object Player Info Polimofirsmo
        public virtual PlayerGameInfo makePlayerInfoObject(Player _session)
        {
            // ignore : UNREFERENCED_PARAMETER(_session); == ignore

            return new PlayerGameInfo();
        }
        /// <summary>
        /// metodo é ignorante
        /// </summary>
        /// <param name="_session"></param>
        /// <param name="_packet"></param>
        public virtual void requestInitShotSended(Player _session, packet _packet)
        {

        }  

        string getNameMap(uint map)
        {
            switch ((RoomInfo.eCOURSE)map)
            {
                case RoomInfo.eCOURSE.BLUE_LAGOON:
                    return "Blue Lagoon";
                case RoomInfo.eCOURSE.BLUE_WATER:
                    return "Blue Water";
                case RoomInfo.eCOURSE.SEPIA_WIND:
                    return "Sepia Wind";
                case RoomInfo.eCOURSE.WIND_HILL:
                    return "Wind Hill";
                case RoomInfo.eCOURSE.WIZ_WIZ:
                    return "Wiz Wiz";
                case RoomInfo.eCOURSE.WEST_WIZ:
                    return "West Wiz";
                case RoomInfo.eCOURSE.BLUE_MOON:
                    return "Blue Moon";
                case RoomInfo.eCOURSE.SILVIA_CANNON:
                    return "Silvia Cannon";
                case RoomInfo.eCOURSE.ICE_CANNON:
                    return "Ice Cannon";
                case RoomInfo.eCOURSE.WHITE_WIZ:
                    return "White Wiz"; 
                default:
                    return "Unknown";
            }
        }
        //retorna o tipo da tacada = 0(HIO), 1(ALBA), 2(EAGLE),3(BIRDIE), 4(PAR), -1(tacadas não feitas )
        public int getScore(uint _tacada_num, sbyte _par_hole)
        {
            int tipo = Convert.ToInt32(_tacada_num - _par_hole);
            if (_tacada_num == 1) // HIO
                return 0;
            else
            {
                switch (tipo)
                {
                    case -3:    // Alba
                        return 1;
                    case -2:    // Eagle
                        return 2;
                    case -1:    // Birdie
                        return 3;
                    case 0: // Par
                        return 4;
                    case 1: // bogey
                        return 5;
                    case 2: // Double bogey
                        return 6;
                    case 3: // Triple bogey
                        return 7;
                    default: // give up
                        return 8;

                }
            }
        }

        public int _getScore(uint _tacada_num, sbyte _par_hole)
        {
            int tipo = Convert.ToInt32(_tacada_num - _par_hole);
            if (_tacada_num == 1) // HIO
                return 0;//hio(tava -4)
            else
            {
                switch (tipo)
                {
                    case -3:    // Alba
                        return 1;
                    case -2:    // Eagle
                        return 2;//okay
                    case -1:    // Birdie
                        return 3;
                    case 0: // Par
                        return 4;//nao calcula, pq é zero
                    case 1: // bogey
                        return 5;
                    case 2: // Double bogey
                        return 6;
                    case 3: // Triple bogey
                        return 7;
                    default: // give up
                        return 8;

                }
            }
        }

        string getScoreStr(uint _tacada_num, sbyte _par_hole)
        {

            var tipo = getScore(_tacada_num, _par_hole);
            switch (tipo)
            {
                case 0:
                    return ("HIO");
                case 1:
                    return ("ALBATROSS");
                case 2:
                    return ("EAGLE");
                case 3:
                    return ("BIRDIE");
                case 4:
                    return ("PAR");
                case 5:
                    return ("BOGEY");
                case 6:
                    return ("DOUBLE BOGEY");
                case 7:
                    return ("TRIPLE BOGEY");
                default:
                    return ("GIVE UP");
            }
        }

        public float TRANSF_SERVER_RATE_VALUE(uint rate)
        {
            return DefineConstants.TRANSF_SERVER_RATE_VALUE((int)rate);
        }

        public enum GAMESTATEFLAG : int
        {
            DEFAULT = -1,
            INIT = 1,
            GAME_FINISH = 2
        }
        public GAMESTATEFLAG getGameState()
        {
            switch (m_game_init_state)
            {
                case 1:
                    return GAMESTATEFLAG.INIT;
                case 2:
                    return GAMESTATEFLAG.GAME_FINISH;
                default:
                    return GAMESTATEFLAG.DEFAULT;
            }

        }
        /// <summary>
        /// GameBase.cs precisa ser chamado por ultimo(remove o conflito de dados)
        /// </summary>
        /// <param name="disposing">limpe agora = true</param>
        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _smp.message_pool.getInstance().push(new message("[GameBase::Dispose][Log] GameBase destroyed on Room[Number=" + (m_ri.numero) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                    //if (m_course != null)
                    //    m_course.Dispose();

                    clear_player_order();

                    clearAllPlayerInfo();

                    clear_time();

                    if (!m_player_report_game.empty())
                        m_player_report_game.Clear();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}