using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.Models;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.PangyaEnums;
using Pangya_GameServer.Repository;
using Pangya_GameServer.UTIL;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.Repository;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Text;
using static Pangya_GameServer.Models.DefineConstants; 
namespace Pangya_GameServer.Game
{
    /// <summary>
    /// Only Packets in Room
    /// </summary>
    public partial class Room : IDisposable
    { 
        public void requestChangeTeam(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestChangeTeam] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestChangeTeam] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                byte team = _packet.ReadByte();

                PlayerRoomInfoEx pPri = getPlayerInfo(_session);

                if (pPri == null)
                {
                    throw new exception("[room::requestChangeTeam] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o team(time) na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem o info do player. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1505, 0));
                }

                if (m_teans.Count() < 2)
                {
                    throw new exception("[room::requestChangeTeam] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar o team(time) na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem teans(times) suficiente. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1506, 0));
                }

                  

                p = new PangyaBinaryWriter((ushort)0x7D);

                p.WriteInt32(_session.m_oid);

                p.WriteByte(team);

                packet_func.room_broadcast(this,
                    p, 0);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChangeTeam][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Request Game
        public virtual bool requestStartGame(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestStartGame] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestStartGame] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            bool ret = true;

            try
            {

                if (m_ri.master != _session.m_pi.uid)
                {
                    if (!_session.m_pi.mi.capability.game_master)
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o jogo na sala[NUMERO=" + m_ri.numero + "], mas ele nao é o master da sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1, 0x5900201));
                }


                if (m_pGame != null && (m_pGame.m_game_init_state == 2 || m_pGame.m_game_init_state == 1))//um bug aqui
                {
                    if (m_pGame != null)
                        m_pGame.stopTime();//desliga o relogio...

                    m_pGame = null;      // elimina a referência 
                }

                // Verifica se já tem um jogo inicializado e lança error se tiver, para o cliente receber uma resposta
                if (m_pGame != null)
                {
                    throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o jogo na sala[NUMERO=" + m_ri.numero + "], mas ja tem um jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        8, 0x5900202));
                }

                // Verifica se todos estão prontos se não da erro
                if (!isAllReady())
                {
                    if (!_session.m_pi.mi.capability.game_master)
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o jogo na sala[NUMERO=" + m_ri.numero + ", MASTER=" + Convert.ToString(m_ri.master) + "], mas nem todos jogadores estao prontos. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        8, 0x5900202));
                    else
                        SetAllReady();//seta todos com ready
                }

                // Coloquei para verificar se a type de Bot tourney não está ativo verifica o resto das condições
                if (!m_bot_tourney
                    && v_sessions.Count() == 1)
                {
                    if (m_ri.flag_gm != 1)
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o jogo na sala[NUMERO=" + m_ri.numero + "], mas nao tem quantidade de jogadores suficiente para da comecar. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            1, 0x5900202));
                }

                // Match
                if (m_ri.getTipo() == RoomInfo.TIPO.MATCH)
                {

                    if (m_teans.empty())
                    {
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Match na sala[NUMERO=" + m_ri.numero + "], mas o vector do teans esta vazio. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            2, 0x5900202));
                    }

                    if (m_teans.Count() == 1)
                    {
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Match na sala[NUMERO=" + m_ri.numero + "], mas o vector do teans só tem um team. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            3, 0x5900202));
                    }

                    if (v_sessions.Count() % 2 == 1)
                    {
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Match na sala[NUMERO=" + m_ri.numero + "], mas o numero de jogadores na sala é impar. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            4, 0x5900202));
                    }

                    if (v_sessions.Count() == 2 && (m_teans[0].getNumPlayers() == 0 || m_teans[1].getNumPlayers() == 0))
                    {
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Match na sala[NUMERO=" + m_ri.numero + "], mas um team nao tem jogador. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            5, 0x5900202));
                    }

                    if (v_sessions.Count() == 4 && (m_teans[0].getNumPlayers() < 2 || m_teans[1].getNumPlayers() < 2))
                    {
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Match na sala[NUMERO=" + m_ri.numero + "], mas um team nao tem jogador suficiente. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            6, 0x5900202));
                    }

                    if (m_ri.max_player == 4 && v_sessions.Count() < 4)
                    {
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Match na sala[NUMERO=" + m_ri.numero + "], mas o max player sala é 4, mas nao tem os 4 jogadores na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            7, 0x5900202));
                    }
                }

                // Guild Battle
                if (m_ri.getTipo() == RoomInfo.TIPO.GUILD_BATTLE)
                {

                    if (v_sessions.Count() % 2 == 1)
                    {
                        throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Guild Battle na sala[NUMERO=" + m_ri.numero + "], mas o numero de jogadores na sala é impar. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                            9, 0x5900202));
                    }

                    var error_check = m_guild_manager.isGoodToStart();

                    if (error_check <= 0)
                    {

                        switch (error_check)
                        {
                            case 0: // Não tem duas guilds na sala
                                throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Guild Battle na sala[NUMERO=" + m_ri.numero + "], mas nao tem guilds suficientes para comecar o jogo. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                                    10, 0x5900202));
                            case -1: // Não tem o mesmo número de jogadores na sala as duas guilds
                                throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Guild Battle na sala[NUMERO=" + m_ri.numero + "], mas as duas guilds nao tem o mesmo numero de jogadores na sala. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                                    11, 0x5900202));
                            case -2: // Uma das Guilds ou as duas não tem 2 jogadores
                                throw new exception("[room::requestStartGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o Guild Battle na sala[NUMERO=" + m_ri.numero + "], mas uma ou as duas guilds tem menos que 2 jogadores na sala. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                                    12, 0x5900202));
                        }
                    }
                }

                if (m_ri.getMap() >= 0x7Fu)
                {
                    Lottery lottery = new Lottery();

                    foreach (var el in sIff.getInstance().getCourse())
                    {

                        var course_id = sIff.getInstance().getItemIdentify(el.ID);

                        if (course_id != 17 && course_id != 0x40)
                        {
                            lottery.Push(100, course_id);
                        }
                    }

                    var lc = lottery.spinRoleta();

                    if (lc != null)
                    {
                        m_ri.course = (RoomInfo.eCOURSE)(0x80 | Convert.ToByte(lc.Value));
                    }

                    if (!_session.m_pi.m_cap.premium_user || !_session.m_pi.m_cap.game_master)
                    {
                        // Verifica se ele tem o ticket para criar o Bot se não manda mensagem dizenho que ele não tem ticket para criar o bot
                        var pWi = _session.m_pi.findWarehouseItemByTypeid(TICKET_BOT_TYPEID) != null ? _session.m_pi.findWarehouseItemByTypeid(TICKET_BOT_TYPEID) : _session.m_pi.findWarehouseItemByTypeid(TICKET_BOT_TYPEID2);

                        if (pWi != null && pWi.STDA_C_ITEM_QNTD > 0)
                        {

                            stItem item = new stItem();

                            item.type = 2;
                            item.id = pWi.id;
                            item._typeid = pWi._typeid;
                            item.qntd = 1;
                            item.c[0] = (short)(item.qntd * -1);

                            if (ItemManager.removeItem(item, _session) > 0)
                            {

                                //// Atualiza o item no Jogo e Add o Bot e manda a mensagem que o bot foi add
                                //p.init_plain(0x216);

                                //p.WriteUInt32((uint)UtilTime.GetSystemTimeAsUnix());
                                //p.WriteUInt32(1); // Count;

                                //p.WriteByte(item.type);
                                //p.WriteUInt32(item._typeid);
                                //p.WriteInt32(item.id);
                                //p.WriteUInt32(item.flag_time);
                                //p.WriteBytes(item.stat.ToArray());
                                //p.WriteUInt32((item.c[3] > 0) ? item.c[3] : item.c[0]);
                                //p.WriteZero(25);

                                //packet_func.session_send(p,
                                //    _session, 1);
                            }
                            else
                            {

                                _smp.message_pool.getInstance().push(new message("[room::makeBot] [Error] PLAYER[UID=" + _session.m_pi.uid + "] nao conseguiu deletar o TICKET_BOT[TYPEID=" + Convert.ToString(TICKET_BOT_TYPEID) + ", ID=" + Convert.ToString(item.id) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));

                            }

                        }

                    }
                    RateValue rv = new RateValue
                    {
                        // Att Exp rate, e Pang rate, que começou o jogo


                        exp = m_ri.rate_exp = (uint)sgs.gs.getInstance().getInfo().rate.exp,
                        pang = m_ri.rate_pang = (uint)sgs.gs.getInstance().getInfo().rate.pang
                    };

                    // Angel Event
                    m_ri.angel_event = sgs.gs.getInstance().getInfo().rate.angel_event == 1;

                    rv.clubset = (uint)sgs.gs.getInstance().getInfo().rate.club_mastery;
                    rv.rain = (uint)sgs.gs.getInstance().getInfo().rate.chuva;
                    rv.treasure = (uint)sgs.gs.getInstance().getInfo().rate.treasure;

                    rv.persist_rain = 0; // Persist rain type isso é feito na classe game 

                    switch (m_ri.getTipo())
                    {
                        //case RoomInfo.TIPO.STROKE:
                        //    m_pGame = new Versus(v_sessions, m_ri, rv, m_ri.channel_rookie);
                        //    break;
                        //case RoomInfo.TIPO.MATCH:
                        //    m_pGame = new Match(v_sessions, m_ri, rv, m_ri.channel_rookie, m_teans);
                        //    break;
                        //case RoomInfo.TIPO.PANG_BATTLE: // Ainda não está feio, usa o  Versus Normal
                        //    m_pGame = new PangBattle(v_sessions, m_ri, rv, m_ri.channel_rookie);
                        //    break; 
                        //case RoomInfo.TIPO.TOURNEY:
                        //    m_pGame = new Tourney(v_sessions, m_ri, rv, m_ri.channel_rookie);
                        //    break; 
                    }

                    // Update Room State
                    m_ri.state = 0; // IN GAME

                    //p.init_plain(0x230);

                    //packet_func.room_broadcast(this,
                    //    p, 1);

                    //p.init_plain(0x231);

                    //packet_func.room_broadcast(this,
                    //    p, 1);

                    //p.init_plain(0x77);

                    //p.WriteUInt32((uint)sgs.gs.getInstance().getInfo().rate.pang); // Rate Pang

                    //packet_func.room_broadcast(this,
                    //    p, 1);

                    //m_room_log.roomId = Guid.Empty;//seta toda vez que inicia sala

                    ////insert dados do player
                    //foreach (var _sessions in v_sessions)
                    //    CreateRoomLogSql(_sessions);//criar de todos

                    ////set room 
                    //_session.m_room = this;
                    ////set game 
                    //_session.m_pGame = m_pGame;
                }
            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[room::requestStartGame] [Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                //// Error
                //p.init_plain(0x253);

                //p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 0x5900200);
                //packet_func.session_send(p,
                //    _session, 1);

                ret = false; // Error ao inicializar o Jogo
            }

            return ret;
        }

        public void requestInitHole(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestInitHole] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestInitHole] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestInitHole] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou inicializer o hole do jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5200201));
                }

                m_pGame.requestInitHole(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestInitHole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public bool requestFinishLoadHole(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestFinishLoadHole] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestFinishLoadHole] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            bool ret = false;

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestFinishLoadHole] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou finalizar carregamento do hole do jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5200301));
                }

                ret = m_pGame.requestFinishLoadHole(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestFinishLoadHole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public void requestFinishCharIntro(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestFinishCharIntro] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestFinishCharIntro] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestFinishCharIntro] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou finalizar Char Intro do jogo na sala[NUMEROR=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5200401));
                }

                m_pGame.requestFinishCharIntro(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestFinishCharIntro][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestFinishHoleData(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestFinishHoleData] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestFinishHoleData] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestFinishHoleData] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou finalizar dados do hole, no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201901));
                }

                m_pGame.requestFinishHoleData(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestFinishHoleData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // Server enviou a resposta do InitShot para o cliente
        public void requestInitShotSended(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestInitShotSended] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestInitShotSended] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestInitShotSended] [Error] PLAYER[UID=" + _session.m_pi.uid + "] o server enviou o pacote de InitShot para o cliente, mas na sala[NUMERO=" + m_ri.numero + "] nao tem mais nenhum jogo inicializado. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5905001));
                }

                m_pGame.requestInitShotSended(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestInitShotSended][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestInitShot(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestInitShot] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestInitShot] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestInitShot] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou inicializar o shot no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201401));
                }

                m_pGame.requestInitShot(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestInitShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestSyncShot(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestSyncShot] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestSyncShot] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestSyncShot] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou sincronizar tacada no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201501));
                }

                m_pGame.requestSyncShot(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestSyncShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestInitShotArrowSeq(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestInitShotArrowSeq] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestInitShotArrowSeq] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestInitShotArrowSeq] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou inicializar a sequencia de setas no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado, Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201601));
                }

                m_pGame.requestInitShotArrowSeq(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestInitShotArrowSeq][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestShotEndData(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestShotEndData] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestShotEndData] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestShotEndData] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou finalizar local da tacada no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201701));
                }

                m_pGame.requestShotEndData(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestShotEndData][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public RetFinishShot requestFinishShot(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestFinishShot] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestFinishShot] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            RetFinishShot rfs = new RetFinishShot();

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestFinishShot] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou finalizar tacada no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201801));
                }

                rfs = m_pGame.requestFinishShot(_session, _packet);

                if (rfs.ret > 0)
                {

                    // Acho que não usa mais isso então vou deixar ai, e o ret == 2 vou deixar no channel,
                    // por que se ele for o ultimo da sala tem que excluir ela
                    if (rfs.ret == 1)
                    {
                        finish_game();
                    }
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestFinishShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return (rfs);
        }

        public void requestChangeMira(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestChangeMira] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestChangeMira] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestChangeMira] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar a mira no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5200501));
                }

                m_pGame.requestChangeMira(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChangeMira][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestChangeStateBarSpace(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestChangeStateBarSpace] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestChangeStateBarSpace] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestChangeStateBarSpace] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar estado da barra de espaco no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5200601));
                }

                m_pGame.requestChangeStateBarSpace(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChangeStateBarSpace][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestActivePowerShot(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActivePowerShot] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActivePowerShot] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActivePowerShot] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativar power shot no jogo na sala[NUMEROR=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5200701));
                }

                m_pGame.requestActivePowerShot(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestActivePowerShot][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestChangeClub(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestChangeClub] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestChangeClub] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestChangeClub] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou trocar taco no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5200801));
                }

                m_pGame.requestChangeClub(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChangeClub][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestUseActiveItem(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestUseActiveItem] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestUseActiveItem] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestUseActiveItem] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou usar active item no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5200901));
                }

                m_pGame.requestUseActiveItem(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestUseActiveItem][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestChangeStateTypeing(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestChangeStateTypeing] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestChangeStateTypeing] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestChangeStateTypeing] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou mudar estado do escrevendo icon no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201001));
                }

                m_pGame.requestChangeStateTypeing(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChangeStateTypeing][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestMoveBall(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestMoveBall] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestMoveBall] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestMoveBall] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou recolocar a bola no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201101));
                }

                m_pGame.requestMoveBall(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestMoveBall][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestChangeStateChatBlock(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestChangeStateChatBlock] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestChangeStateChatBlock] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestChangeStateChatBlock] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou mudar estado do chat block no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201201));
                }

                m_pGame.requestChangeStateChatBlock(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestChangeStateChatBlock][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestActiveBooster(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActiveBooster] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActiveBooster] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActiveBooster] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativar time booster no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201301));
                }

                m_pGame.requestActiveBooster(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestActiveBooster][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestActiveReplay(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActiveReplay] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActiveReplay] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActiveReplay] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativar Replay no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5301001));
                }

                m_pGame.requestActiveReplay(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestActiveReplay][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestActiveCutin(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActiveCutin] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActiveCutin] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActiveCutin] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativar cutin no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201701));
                }

                m_pGame.requestActiveCutin(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestActiveCutin][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestActiveAutoCommand(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActiveAutoCommand] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActiveAutoCommand] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActiveAutoCommand] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativar var Command no jogo na sala[NUMEROR=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x550001));
                }

                m_pGame.requestActiveAutoCommand(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestActiveAutoCommand][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestActiveAssistGreen(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActiveAssistGreen] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActiveAssistGreen] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActiveAssistGreen] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativar Assist Green no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5201801));
                }

                m_pGame.requestActiveAssistGreen(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestActiveAssistGreen][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        // VersusBase
        public void requestLoadGamePercent(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestLoadGamePercent] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestLoadGamePercent] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestLoadGamePercent] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou mandar a porcentagem carregada do jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x551001));
                }

                m_pGame.requestLoadGamePercent(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestLoadGamePercent][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestMarkerOnCourse(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestMarkerOnCourse] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestMarkerOnCourse] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestMarkerOnCourse] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou marcar no course no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x552001));
                }

                m_pGame.requestMarkerOnCourse(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestMarkerOnCourse][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestStartTurnTime(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestStartTurnTime] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestStartTurnTime] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestStartTurnTime] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou comecar o tempo do turno no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x553001));
                }

                m_pGame.requestStartTurnTime(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestStartTurnTime][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestUnOrPauseGame(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestUnOrPauseGame] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestUnOrPauseGame] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestUnOrPauseGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou pausar ou despausar o jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x554001));
                }

                m_pGame.requestUnOrPause(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestUnOrPauseGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public bool requestLastPlayerFinishVersus(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestLastPlayerFinishVersus] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestLastPlayerFinishVersus] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            bool ret = false;

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestLastPlayerFinishVersus] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou finalizar Versus na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x555001));
                }

                // Finaliza o Versus
                if (m_pGame.getSessions().Count() > 0)
                {

                    if (m_pGame.finish_game(m_pGame.getSessions().FirstOrDefault(), 2))
                    {
                        finish_game();
                    }

                }
                else
                {
                    finish_game();
                }

                ret = true;

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestLastPlayerFinishVersus][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public bool requestReplyContinueVersus(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestReplyContinueVersus] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestReplyContinueVersus] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            bool ret = false;

            try
            {

                byte opt = _packet.ReadByte();

                if (m_pGame == null)
                {
                    throw new exception("[room::requestReplyContinueVersus] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou responder se quer continuar o versus ou nao na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x556001));
                }

                if (opt == 0)
                {

                    if (m_pGame.getSessions().Count() > 0)
                    {

                        if (m_pGame.finish_game(m_pGame.getSessions().First(), 2))
                        {
                            finish_game();
                        }

                    }
                    else
                    {
                        finish_game();
                    }

                    ret = true;

                }
                else if (opt == 1)
                {
                    m_pGame.requestReplyContinue();
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestReplyContinueVersus][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        // Match
        public void requestTeamFinishHole(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestTeamFinishHole] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestTeamFinishHole] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestTeamFinishHole] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou finalizar hole do Match na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x562001));
                }

                m_pGame.requestTeamFinishHole(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestTeamFinishHole][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }
         
        // Tourney
        public bool requestUseTicketReport(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestUseTicketReport] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestUseTicketReport] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            bool ret = false;

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestUseTicketReport] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou usar Ticket Report no Tourney no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x6301001));
                }

                ret = m_pGame.requestUseTicketReport(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestUseTicketReport][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public void requestActiveWing(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActiveWing] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActiveWing] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActiveWing] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativar Asa no joga na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x6201601));
                }

                m_pGame.requestActiveWing(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestActiveWing][ErrorSystem] " + e.getMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestActivePaws(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActivePaws] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActivePaws] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActivePaws] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativar Patinha no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x6201701));
                }

                m_pGame.requestActivePaws(_session, _packet);

            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[room::requestActivePaws][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestActiveGlove(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestActiveGlove] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestActiveGlove] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestActiveGlove] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou ativat Luva 1m no jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x6201801));
                }

                m_pGame.requestActiveGlove(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestActiveGlove][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestSendTimeGame(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "requestSendTimeGame] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {
                if (isKickedPlayer(_session.m_pi.uid))
                {
                    throw new exception("[room::requestSendTimeGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou entrar na sala[NUMERO=" + m_ri.numero + "] ja em jogo, mas o player foi chutado da sala antes de comecar o jogo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2704, 7));
                }

                if (m_pGame == null)
                {
                    throw new exception("[room::requestSendTimeGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou pegar o tempo do tourney que comecou na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2705, 1));
                }

                m_pGame.requestSendTimeGame(_session);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[room::requestSendTimeGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                //// Resposta erro
                //p.init_plain(0x113);

                //p.WriteByte(6); // Option Error

                //// Error Code
                //p.WriteByte((byte)((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 1));

                //packet_func.session_send(p,
                //    _session, 1);
            }
        }

        public bool requestEnterGameAfterStarted(Player _session)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "requestEnterGameAfterStarted] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            bool ret = false;

            try
            {

                if (isKickedPlayer(_session.m_pi.uid))
                {
                    throw new exception("[room::requestEnterGameAfterStarted][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou entrar na sala[NUMERO=" + m_ri.numero + "] ja em jogo, mas o player foi chutado da sala antes de comecar o jogo.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2704, 7));
                }

                if (m_pGame == null)
                {
                    throw new exception("[room::requestEnterGameAfterStarted] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou entrar na sala[NUMERO=" + m_ri.numero + "] ja em jogo, mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2705, 1));
                }

                if (isGamingBefore(_session.m_pi.uid))
                {
                    throw new exception("[room::requestEnterGameAfterStarted][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou entrar na sala[NUMERO=" + m_ri.numero + "] ja em jogo, mas o player ja tinha jogado nessa sala e saiu, e nao pode mais entrar.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2703, 6));
                }

                long tempo = (m_ri.qntd_hole == 18) ? 10 * 60000 : 5 * 60000;

                var remain = UtilTime.GetLocalDateDiff(m_pGame.getTimeStart());

                if (remain > 0)
                {
                    remain /= STDA_10_MICRO_PER_MILLI; // miliseconds
                }

                if (remain >= tempo)
                {
                    throw new exception("[room::requestEnterGameAfrerStarted][Warning] PLAYER[UID=" + _session.m_pi.uid + "] tentou entrar na sala[NUMERO=" + m_ri.numero + "] ja em jogo, mas o tempo de entrar no tourney acabou.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM, // Acabou o tempo de entrar na sala
                        2706, 2));
                }

                // Add Player a sala
                enter(_session);

                ret = true;

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestEnterGameAfterStarted][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Excluí player da sala se adicionou ele antes
                if (findSessionByUID(_session.m_pi.uid) != null)
                {
                    leave(_session, 0);
                }

                //// Resposta erro
                //p.init_plain(0x113);

                //p.WriteByte(6); // Option Error

                //// Error Code
                //p.WriteByte((byte)((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 1));

                //packet_func.session_send(p,
                //    _session, 1);
            }

            return ret;
        }

        public void requestUpdateEnterAfterStartedInfo(Player _session, EnterAfterStartInfo _easi)
        {
            if (!_session.getState())
            {
                throw new exception("[room::" + "requestUpdateEnterAfterStartedInfo] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestUpdateEnterAfterStartedInfo] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou atualizar info do player que entrou depois na sala[NUMERO=" + m_ri.numero + "] ja em jogo, mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2705, 1));
                }

                m_pGame.requestUpdateEnterAfterStartedInfo(_session, _easi);

            }
            catch (exception e)
            {

                //_smp.message_pool.getInstance().push(new message("[room::requestUpdateEnterAfterStartedInfo][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                //// Resposta erro
                //p.init_plain(0x113);

                //p.WriteByte(6); // Option Error

                //// Error Code
                //p.WriteByte((byte)((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.ROOM) ? ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) : 1));

                //packet_func.session_send(p,
                //    _session, 1);
            }
        }

        public bool requestFinishGame(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestFinishGame] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestFinishGame] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            bool ret = false;

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestFinishGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou finalizar jogo na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5202101));
                }

                if (m_pGame.requestFinishGame(_session, _packet))
                { // Terminou o Jogo

                    finish_game();
                    m_ri.roomId = Guid.NewGuid();//gera a proxima roomId para evitar problemas com players que tentarem reconectar no jogo antigo
                    ret = true;
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestFinishGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return ret;
        }

        public void requestPlayerReportChatGame(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestPlayerReportChatGame] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestPlayerReportChatGame] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_pGame == null)
                {
                    throw new exception("[room::requestPlayerReportChatGame] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou reporta o chat do jogo na sala[NUMERO=" + m_ri.numero + "], mas nao tem nenhum jogo inicializado na sala. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        0x580200, 0));
                }

                // Report Chat Game
                m_pGame.requestPlayerReportChatGame(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestPlayerReportChatGame][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != STDA_ERROR_TYPE.GAME)
                {
                    throw;
                }
            }
        }

        // Common Command GM
        public void requestExecCCGChangeWindVersus(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestExecCCGChangeWindVersus] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestExecCCGChangeWindVersus] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                if (m_ri.getTipo() != RoomInfo.TIPO.PANG_BATTLE || m_ri.getTipo() != RoomInfo.TIPO.STROKE || m_ri.getTipo() != RoomInfo.TIPO.MATCH)
                {
                    throw new exception("[room::requestExecCCGChangeWindVersus] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou executar o comando de troca de vento na sala[NUMERO=" + m_ri.numero + ", TIPO=" + Convert.ToString(m_ri.tipo) + "], mas o tipo da sala nao é Stroke ou Match modo. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        1, 0x5700100));
                }

                if (m_pGame == null)
                {
                    throw new exception("[room::requestExecCCGChangeWindVersus] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou executar o comando de troca de vento na sala[NUMERO=" + m_ri.numero + "], mas a sala nao tem nenhum jogo inicializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        2, 0x5700100));
                }

                m_pGame.requestExecCCGChangeWind(_session, _packet);

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestExecCCGChangeWindVersus][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
        }

        public void requestExecCCGChangeWeather(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestExecCCGChangeWeather] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestExecCCGChangeWeather] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

               
                if (m_pGame != null)
                {

                    m_pGame.requestExecCCGChangeWeather(_session, _packet);

                }
                else
                {
                    throw new exception("[room::requestExecCCGChangeWeather] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou executar o comando de troca de tempo(weather) na sala[NUMERO=" + m_ri.numero + "], mas a sala nao é lounge ou nao tem um jogo iniclializado. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                        3, 0x5700100));
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestExecCCGChangeWeather][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
        }

        public void requestExecCCGGoldenBell(Player _session, packet _packet)
        {
            if (!_session.getState())
            {
                throw new exception("[room::requestExecCCGGoldenBell] [Error] player nao esta connectado", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }
            if (_packet == null)
            {
                throw new exception("[room::requestExecCCGGoldenBell] [Error] _packet is null", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM,
                    12, 0));
            }

            try
            {

                uint item_typeid = _packet.ReadUInt32();
                uint item_qntd = _packet.ReadUInt32();

                if (item_typeid == 0)
                {
                    throw new exception("[room::requestExecCCGGoldenBell] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou enviar presente para todos da sala[NUMERO=" + m_ri.numero + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + "QNTD = " + Convert.ToString(item_qntd) + "], mas item is invalid. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
                        3, 0x5700100));
                }

                if (item_qntd > 20000)
                {
                    throw new exception("[room::requestExecCCGGoldenBell] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou enviar presente para todos da sala[NUMERO=" + m_ri.numero + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + "QNTD = " + Convert.ToString(item_qntd) + "], mas a quantidade passa de 20mil. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
                        4, 0x5700100));
                }

                var @base = sIff.getInstance().FindCommonItem(item_typeid);

                if (@base == null)
                {
                    throw new exception("[room::requestExecCCGGoldenBell] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou enviar presente para todos da sala[NUMERO=" + m_ri.numero + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + "QNTD = " + Convert.ToString(item_qntd) + "], mas o item nao existe no IFF_STRUCT do Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
                        6, 0));
                }

                BuyItem bi = new BuyItem();

                bi.id = -1;
                bi._typeid = item_typeid;
                bi.qntd = item_qntd;

                var msg = ("GM enviou um item para voce: item[ " + (@base.Name) + " ]");

                foreach (var el in v_sessions)
                {

                    // Limpa item
                    stItem item = new stItem();

                    ItemManager.initItemFromBuyItem(el.m_pi,
                        item, bi, false, 0, 0, 1);

                    if (item._typeid == 0)
                    {
                        throw new exception("[room::requestExecCCGGoldenBell] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou enviar presente para todos da sala[NUMERO=" + m_ri.numero + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + "QNTD = " + Convert.ToString(item_qntd) + "], mas nao conseguiu inicializar o item. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
                            5, 0));
                    }

                    if (MailBoxManager.sendMessageWithItem(0,
                        el.m_pi.uid, msg, item) <= 0)
                    {
                        throw new exception("[room::requestExecCCGGoldenBell] [Error] PLAYER[UID=" + _session.m_pi.uid + "] tentou enviar presente para o PLAYER[UID=" + Convert.ToString(el.m_pi.uid) + "] o Item[TYPEID=" + Convert.ToString(item_typeid) + ", QNTD=" + Convert.ToString(item_qntd) + "], mas nao conseguiu colocar o item no mail box dele. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.GAME_SERVER,
                            7, 0));
                    }

                    // Log
                    _smp.message_pool.getInstance().push(new message("[room::requestExecCCGGoldenBell][Info] PLAYER[UID=" + _session.m_pi.uid + "] enviou um Item[TYPEID=" + Convert.ToString(item_typeid) + ", QNTD=" + Convert.ToString(item_qntd) + "] para o PLAYER[UID=" + Convert.ToString(el.m_pi.uid) + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[room::requestExecCCGGoldenBell][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                throw;
            }
        }

    }
}
