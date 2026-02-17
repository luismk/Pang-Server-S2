using Microsoft.VisualBasic.FileIO;
using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.Models;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.PangyaEnums;
using Pangya_GameServer.Repository;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.Repository;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System.Text;
using static Pangya_GameServer.Models.DefineConstants;
using static PangyaAPI.SQL.NormalDB;
namespace Pangya_GameServer.Game
{
    /// <summary>
    /// Only Packets in Channel
    /// </summary>
    public partial class Channel
    {
        public void enterChannel(Player _session)
        {

            if (!_session.getState())
            {
                throw new exception("[Channel::enterChannel][Error] player nao esta conectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                    1, 1));
            }

            if (_session.m_pi.channel != DEFAULT_CHANNEL)
            {
                throw new exception("[Channel::enterChannel][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] ja esta conectado em outro canal.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                    2, 2));
            }

            addSession(_session);

            _smp.message_pool.getInstance().push(new message($"[Channel::EnterChannel][Sucess] CHANNEL[Id: {m_ci.id}, Users: {m_ci.curr_user}/{m_ci.max_user}, Rooms: {0}]", type_msg.CL_FILE_LOG_AND_CONSOLE));
            packet_func.session_send(packet_func.pacote12(m_ci.id), _session);
        }

        public void enterLobby(Player _session, sbyte _lobby)
        {

            try
            {
                if (!_session.getState())
                {
                    throw new exception("[Channel::enterLobby][Error] PLAYER [UID_TRASH=" + (_session.m_pi.uid) + "] nao esta conectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        1, 0));
                }

                if (_session.m_pi.lobby != DEFAULT_CHANNEL)
                {
                    throw new exception("[Channel::enterLobby][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] ja esta na lobby.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        5, 0));
                }

                _session.m_pi.lobby = (sbyte)((_lobby == DEFAULT_CHANNEL || _lobby == 0) ? 1 : _lobby);
                _session.m_pi.mi.sala_numero = DEFAULT_ROOM_ID;//reseta por que pode esta bugado
                _session.m_pi.place = 0;

                updatePlayerInfo(_session);

                PangyaBinaryWriter p = new PangyaBinaryWriter();

                List<PlayerLobbyInfo> v_pci = new List<PlayerLobbyInfo>();
                PlayerLobbyInfo pci = null;
                List<RoomInfoEx> v_ri = m_rm.getRoomsInfo();

                List<Player> v_sessions = getSessions(_session.m_pi.lobby);

                for (var i = 0; i < v_sessions.Count; ++i)
                {
                    if ((pci = getPlayerInfo(v_sessions[i])) != null)
                    {
                        v_pci.Add(pci);
                    }
                }

                pci = getPlayerInfo(_session);
                if (v_pci.Count == 0)
                {
                    v_pci.Add(pci);
                }

                if (v_ri.Count > 0)
                {
                    packet_func.session_send(packet_func.pacote09(v_ri, 0), _session, 0);
                }

                // Add o primeiro limpando a lobby
                packet_func.session_send(packet_func.pacote08(new List<PlayerLobbyInfo>() { v_pci[0] }, 4), _session, 0);

                if (v_pci.Count > 0)
                {
                    packet_func.session_send(packet_func.pacote08(v_pci, 5), _session, 0);
                }

                packet_func.channel_broadcast(this, packet_func.pacote08(new List<PlayerLobbyInfo>() { (pci == null) ? new PlayerLobbyInfo() : pci }, 1), 0);

                v_pci.Clear();

                packet_func.session_send(packet_func.pacote02("SYSTEM", "Seja Bem vindo ao canal S2!", eChatMsg.SPECIAL_NOTICE), _session);

            }
            catch (exception e)
            {
                throw;
            }
        }

        public void leaveLobby(Player _session)
        {

            /// !@tem que tira isso aqui por que tem que enviar para os outros player da lobby que ele sai,
            /// mesmo que o sock dele não pode mais enviar
            //if (!_session.getState())
            //throw exception("[Channel::leaveLobby][Error] player nao esta conectado.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE::CHANNEL, 1, 0));

            // Sai da sala se estiver em uma sala
            //try
            //{
            //    leaveRoom(_session, 0);
            //}
            //catch (exception e)
            //{

            //    _smp.message_pool.getInstance().push(new message("[Channel::leaveLobby][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            //}

            _session.m_pi.lobby = DEFAULT_CHANNEL;
            _session.m_pi.place = 0;

            updatePlayerInfo(_session);
        }

        public void requestExitLobby(Player _session, packet _packet)
        {
            try
            {
                leaveChannel(_session);//sai da channel

                packet_func.session_send(new byte[] { 0x13 }, _session);

                sgs.gs.getInstance().sendChannelListToSession(_session);
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[Channel::leaveLobbyMultiPlayer][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public void requestMakeRoom(Player _session, packet _packet)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                int option;
                RoomInfoEx ri = new RoomInfoEx();
                string s_tmp = "";

                //option = _packet.ReadUInt8(); 
                ri.time_vs = _packet.ReadUInt32();
                ri.time_30s = _packet.ReadUInt32();
                ri.max_player = _packet.ReadUInt8();
                ri.tipo = _packet.ReadUInt8();
                ri.qntd_hole = _packet.ReadUInt8();
                ri.course = RoomInfo.eCOURSE.BLUE_LAGOON;
                ri.modo = 0;//padrao 

                if (!_session.m_pi.m_cap.game_master && ri.max_player > 30)//criar comparacao melhor
                {
                    throw new exception("[Channel::requestMakeRoom][Error] Channel[ID=" + ((ushort)m_ci.id) + "] limite atingido, Hacker, por que o cliente nao deixa criar uma sala maior que 30, pois o cliente nao e gm/adm.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        7, 0));
                }

                s_tmp = _packet.ReadString();

                if (s_tmp.Length == 0)//criar comparacao melhor
                {
                    throw new exception("[Channel::requestMakeRoom][Error] Channel[ID=" + ((ushort)m_ci.id) + "] Nome da sala vazio, Hacker, por que o cliente nao deixa enviar esse pacote sem um nome da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        7, 0));
                }

                if (s_tmp.Length > 32)//criar comparacao melhor
                {
                    throw new exception("[Channel::requestMakeRoom][Error] Channel[ID=" + ((ushort)m_ci.id) + "] Nome da sala vazio, Hacker, por que o cliente nao deixa enviar esse pacote sem um nome da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        7, 0));
                }

                ri.nome = s_tmp;
                s_tmp = _packet.ReadString();

                if (s_tmp.Length > 8)//criar comparacao melhor
                {
                    throw new exception("[Channel::requestMakeRoom][Error] Channel[ID=" + ((ushort)m_ci.id) + "] tamanho da senha esta errado, Code[0].", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        7, 0));
                }

                if (!s_tmp.empty())
                {
                    ri.senha_flag = 0;
                    ri.senha = s_tmp;
                }

                // Flag Server
                uFlag flag = _session.m_pi.block_flag.m_flag;
                 

                switch (ri.getTipo())
                {
                    case RoomInfo.TIPO.STROKE:
                        if (flag.stroke)
                        {
                            throw new exception("[Channel::requestMakeRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou criar sala[TIPO=" + ((ushort)ri.getTipo()) + "], mas ele nao pode criar Stroke. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                2, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.MATCH:
                        if (flag.match)
                        {
                            throw new exception("[Channel::requestMakeRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou criar sala[TIPO=" + ((ushort)ri.getTipo()) + "], mas ele nao pode criar Match. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                3, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.TOURNEY:
                        if (flag.tourney)
                        {
                            throw new exception("[Channel::requestMakeRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou criar sala[TIPO=" + ((ushort)ri.getTipo()) + "], mas ele nao pode criar Tourney. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                4, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.TOURNEY_TEAM:
                        if (flag.team_tourney)
                        {
                            throw new exception("[Channel::requestMakeRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou criar sala[TIPO=" + ((ushort)ri.getTipo()) + "], mas ele nao pode criar Team Tourney. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                5, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.GUILD_BATTLE:
                        if (flag.guild_battle)
                        {
                            throw new exception("[Channel::requestMakeRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou criar sala[TIPO=" + ((ushort)ri.getTipo()) + "], mas ele nao pode criar Guild Battle. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                6, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.PANG_BATTLE:
                        if (flag.pang_battle)
                        {
                            throw new exception("[Channel::requestMakeRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou criar sala[TIPO=" + ((ushort)ri.getTipo()) + "], mas ele nao pode criar Pang Battle. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                7, 0x770001));
                        }
                        break;
                }

                if (ri.natural.short_game == 1 && (flag.team_tourney || flag.short_game))
                {
                    throw new exception("[Channel::requestMakeRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou criar a sala[TIPO=" + ((ushort)ri.getTipo()) + "], mas ele nao pode criar sala Short Game. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        1, 770001));
                }

                // Flag do canal, se for rookie passa para sala, que no jogo, essa type faz vir vento de 1m a 5m
                ri.channel_rookie = true;

                Room r = null;

                try
                {
                    r = m_rm.makeRoom(m_ci.id,
                        ri, _session);

                    if (r == null)
                    {
                        throw new exception("[Channel::requestMakeRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou criar a sala, mas deu erro na criacao. Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                            8, 0));
                    }

                    // Att PlayerCanalInfo
                    updatePlayerInfo(_session);

                    r.sendUpdate();

                    r.sendMake(_session);
                     
                    r.sendCharacter(_session, 0);

                    //r.sendWeatherLounge(_session); 

                    //   r.SendGalleryList(_session, 0);

                    if (r.getInfo().getTipo() == RoomInfo.TIPO.GUILD_BATTLE)
                    {
                        r.sendCharacter(_session, 0);
                    }

                    if (r.getInfo().getTipo() == RoomInfo.TIPO.TOURNEY)
                    {
                        try
                        {

                            if (r.isLocked() && r.checkPass("bot"))
                            {
                                r.makeBot(_session);
                            }

                        }
                        catch (exception e)
                        {
                            throw e;
                        }
                    }

                    //  Libera a sala
                    if (r != null)
                    {
                        m_rm.addRoom(r);

                        m_rm.unlockRoom(r);
                    }


                }
                catch (exception e)
                {
                    if (r != null)
                    {
                        m_rm.unlockRoom(r);
                    }

                    throw e;
                }
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[Channel::requestMakeRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta Error
                p.init_plain(0x49);

                p.WriteUInt16(2); // Error

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public void requestEnterRoom(Player _session, packet _packet)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                sbyte sala_numero = _packet.ReadSByte();
                string senha = _packet.ReadString();

                //aqui
                var r = m_rm.findRoom(sala_numero);

                if (r == null)
                {
                    throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[NUMERO=" + (sala_numero) + "], mas ela nao existe.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        2, 0));
                }


                // Flag Server
                uFlag flag = _session.m_pi.block_flag.m_flag;
                 

                switch (r.getInfo().getTipo())
                {
                    case RoomInfo.TIPO.STROKE:
                        if (flag.stroke)
                        {
                            throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[TIPO=" + ((ushort)r.getInfo().getTipo()) + ", NUMERO=" + (r.getNumero()) + "], mas ele nao pode entrar Stroke. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                2, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.MATCH:
                        if (flag.match)
                        {
                            throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[TIPO=" + ((ushort)r.getInfo().getTipo()) + ", NUMERO=" + (r.getNumero()) + "], mas ele nao pode entrar Match. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                3, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.TOURNEY:
                        if (flag.tourney)
                        {
                            throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[TIPO=" + ((ushort)r.getInfo().getTipo()) + ", NUMERO=" + (r.getNumero()) + "], mas ele nao pode entrar Tourney. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                4, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.TOURNEY_TEAM:
                        if (flag.team_tourney)
                        {
                            throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[TIPO=" + ((ushort)r.getInfo().getTipo()) + ", NUMERO=" + (r.getNumero()) + "], mas ele nao pode entrar Team Tourney. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                5, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.GUILD_BATTLE:
                        if (flag.guild_battle)
                        {
                            throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[TIPO=" + ((ushort)r.getInfo().getTipo()) + ", NUMERO=" + (r.getNumero()) + "], mas ele nao pode entrar Guild Battle. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                6, 0x770001));
                        }
                        break;
                    case RoomInfo.TIPO.PANG_BATTLE:
                        if (flag.pang_battle)
                        {
                            throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[TIPO=" + ((ushort)r.getInfo().getTipo()) + ", NUMERO=" + (r.getNumero()) + "], mas ele nao pode entrar Pang Battle. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                7, 0x770001));
                        }
                        break;  
                }

                if (r.getInfo().natural.short_game == 1 && (flag.team_tourney || flag.short_game))
                {
                    throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[TIPO=" + ((ushort)r.getInfo().getTipo()) + ", NUMERO=" + (r.getNumero()) + "], mas ele nao pode entrar sala Short Game. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        1, 770001));
                }
                 

                if (r.IsStarted() && _session.m_pi.m_cap.game_master) // GM Entra na sala depois que o jogo começou
                {
                    r.requestSendTimeGame(_session);
                }

                else if (r.isGaming()) // não é GM envia error para o player que ele nao pode entrar na sala depois de ter começado
                {
                    throw new exception("[Channel::requestEnterRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[NUMERO=" + (sala_numero) + "], mas a sala ja comecou o jogo. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        10, 0));
                }
                else
                {
                    if (!r.isLocked() || r.isInvited(_session) || (_session.m_pi.m_cap.game_master/* & 4/*GM*/) || (!senha.empty() && r.checkPass(senha)))
                    {
                        if (r.isInvited(_session))
                        {
                            // Deleta convite

                            // Add convidado a sala
                            if (!r.isFull() && r.getInvited(_session) != null)
                            {
                                var ici = r.deleteInvited(_session);//nao era pra deletar ?

                                r.enter(_session);

                                //deleteInviteTimeRequest(ici);
                            }
                        }
                        else if (!r.isFull())
                        {
                            // Verifica se o player foi convidado em outra sala
                            // e tira o convite dele
                            //deleteInviteTimeResquestByInvited(_session);

                            r.enter(_session);
                        }
                        else
                        {
                            throw new exception("[Channel::requestEnterRoom][Warning] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[NUMERO=" + (sala_numero) + "], mas a sala esta cheia.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                                3, 0));
                        }
                    }
                    else
                    {
                        throw new exception("[Channel::requestEnterRoom][Warning] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou entrar na sala[NUMERO=" + (sala_numero) + "], mas a senha nao é igual a da sala.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                            4, 0));
                    }

                    // Att PlayerCanalInfo
                    updatePlayerInfo(_session);

                    r.sendUpdate();

                    r.sendMake(_session);

                    r.sendCharacter(_session, 0);//zero e a lista

                    r.sendCharacter(_session, 1);//1 e o criador 
                    //envia o clima, mas nao foi feito ainda o packet
                    //r.sendWeatherLounge(_session);

                    sendUpdateRoomInfo(r.getInfo(), 3);
                      
                    // Guild Battle precisa enviar o sendCharacter opção 0 duas vezes.
                    // Uma na sua posição normal e outra depois de atualizar o info da sala na lobby
                    if (r.getInfo().getTipo() == RoomInfo.TIPO.GUILD_BATTLE)
                    {
                        r.sendCharacter(_session, 0);
                    }
                }
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[Channel::requestEnterRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                // Resposta Error
                p.init_plain(0x49);

                p.WriteUInt16(2); // Error

                packet_func.session_send(p,
                    _session, 1);
            }
        }

        public void requestChangeInfoRoom(Player _session, packet _packet)
        {
            try
            {
                var r = m_rm.findRoom(_session.m_pi.mi.sala_numero);

                if (r == null)
                {
                    throw new exception("[Channel::requestChangeInfoRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] Channel[ID=" + ((ushort)m_ci.id) + "] tentou trocar info da sala[NUMERO=" + (_session.m_pi.mi.sala_numero) + "], mas a sala nao existe.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        10, 0));
                }

                if (r.requestChangeInfoRoom(_session, _packet))
                {
                    sendUpdateRoomInfo(r.getInfo(), 3);
                }



            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[Channel::requestChangeInfoRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) != STDA_ERROR_TYPE.ROOM)
                {
                    throw;
                }
            }
        }

        //sai da sala....
        public void requestExitRoom(Player _session, packet _packet)
        {

            var state = leaveRoom(_session, 1);//sai dai né

            if (state > LEAVE_ROOM_STATE.DO_NOTHING)
            {
                packet_func.session_send(new byte[] { 0x10, 0x10 },
                    _session, 0);
            }
        }

        public void requestBuyItemShop(Player _session, packet _packet)
        {
            var p = new PangyaBinaryWriter();
            try
            {
                // 1. Verificação de Segurança Básica
                if (_session.m_pi.block_flag.m_flag.buy_and_gift_shop)
                {
                    throw new exception($"[Channel::requestBuyItemShop] PLAYER [UID={_session.m_pi.uid}] bloqueado.",
                        ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL, 1, 0x790001));
                }

                // 2. Leitura do Cabeçalho do Pacote S2
                byte option = _packet.ReadByte();    // Encode1 (Normal ou Rental)
                ushort count = _packet.ReadUInt16(); // Encode2 (Quantidade de entradas no loop)

                if (count <= 0)
                {
                    p.init_plain(0x68);
                    p.WriteUInt32(9); // Erro: Carrinho vazio
                    packet_func.session_send(p, _session, 0);
                    return;
                }

                List<stItem> v_item_to_add = new List<stItem>();
                ulong totalPangRequired = 0;
                ulong totalCookieRequired = 0;
                CPLog cp_log = new CPLog();
                cp_log.setType(CPLog.TYPE.BUY_SHOP);

                // 3. Loop de Processamento (6 bytes por item no S2)
                for (int i = 0; i < count; i++)
                {
                    // Lê TypeID (4 bytes) + Time/Qntd (2 bytes)
                    BuyItem bi = new BuyItem().ToRead(_packet);

                    // Busca os dados reais no IFF (O SERVIDOR manda no preço agora)
                    var iffItem = sIff.getInstance().FindCommonItem(bi._typeid);
                    if (iffItem == null || !sIff.getInstance().IsBuyItem(bi._typeid))
                    {
                        p.init_plain(0x68);
                        p.WriteUInt32(6); // Erro: Item não disponível ou não existe
                        packet_func.session_send(p, _session, 0);
                        return;
                    }

                    uint price = sIff.getInstance().getItemPrice(bi._typeid, bi.time);
                    bool isCookie = sIff.getInstance().IsItemCookie(bi._typeid);

                    if (isCookie)
                    {
                        totalCookieRequired += price;
                        bi.cookie = price; // Preenche para uso interno no log/cplog
                    }
                    else
                    {
                        totalPangRequired += price;
                        bi.pang = price;
                    }

                    // 5. Inicializa o stItem (Conversão lógica para o inventário)
                    stItem item = new stItem();
                    ItemManager.initItemFromBuyItem(_session.m_pi, item, bi, true, option);

                    if (item._typeid == 0)
                    {
                        p.init_plain(0x68);
                        p.WriteUInt32(1);
                        packet_func.session_send(p, _session, 0);
                        return;
                    }

                    // 6. Verifica Validade de Tempo (IFF entre datas)
                    if (!ItemManager.betweenTimeSystem(ref item.date))
                    {
                        p.init_plain(0x68);
                        p.WriteUInt32(5); // Erro: Item fora de data (evento expirado)
                        packet_func.session_send(p, _session, 0);
                        return;
                    }

                    // 7. Verifica se pode possuir (Duplicidade)
                    // Se não pode sobrepor e o player já tem, dá erro (Ex: Caddies, Mascotes, Roupas)
                    if (!sIff.getInstance().IsCanOverlapped(item._typeid) && _session.m_pi.ownerItem(item._typeid))
                    {
                        p.init_plain(0x68);
                        p.WriteUInt32(4); // Erro: Já possui o item
                        packet_func.session_send(p, _session, 0);
                        return;
                    }

                    // Adiciona na lista de processamento
                    v_item_to_add.Add(item);

                    // Log de Cash
                    if (isCookie && price > 0)
                        cp_log.putItem(item._typeid, (item.STDA_C_ITEM_TIME > 0 ? item.STDA_C_ITEM_TIME32 : item.STDA_C_ITEM_QNTD32), price);
                }

                // 8. Verificação Final de Saldo
                if (_session.m_pi.cookie < totalCookieRequired || _session.m_pi.ui.pang < totalPangRequired)
                {
                    p.init_plain(0x68);
                    p.WriteUInt32(7); // Erro: Saldo insuficiente
                    packet_func.session_send(p, _session, 0);
                    return;
                }

                // 9. Execução da Compra (DB e Memória)
                try
                {
                    _session.m_pi.consomeMoeda(totalPangRequired, totalCookieRequired);

                    // Adiciona os itens no Banco de Dados e no objeto do Player
                    var rai = ItemManager.addItem(v_item_to_add, _session, 0, 1);
                    if (rai.fails.Count > 0) throw new Exception("Falha ao inserir itens no DB.");
                }
                catch (Exception)
                {
                    _session.m_pi.addMoeda(totalPangRequired, totalCookieRequired); // Rollback de moedas
                    p.init_plain(0x68);
                    p.WriteUInt32(8); // Erro: Falha no DB
                    packet_func.session_send(p, _session, 0);
                    return;
                }

                // 10. Sucesso - Enviar Respostas ao Cliente

                // Atualiza visualmente o Pang (0xC8)
                if (totalPangRequired > 0)
                {
                    p.init_plain(0xC8);
                    p.WriteUInt64(_session.m_pi.ui.pang);
                    p.WriteUInt64(totalPangRequired);
                    packet_func.session_send(p, _session, 1);
                }

                // Atualiza visualmente o Cookie (0x96)
                if (totalCookieRequired > 0)
                {
                    _session.saveCPLog(cp_log); // Salva log de gastos
                    p.init_plain(0x96);
                    p.WriteUInt64(_session.m_pi.cookie);
                    packet_func.session_send(p, _session, 1);
                }


            }
            catch (Exception e)
            {
                _smp.message_pool.getInstance().push(new message($"[Shop Error] {e.Message}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                p.init_plain(0x68);
                p.WriteUInt32(10); // Erro desconhecido
                packet_func.session_send(p, _session, 0);
            }
        }
    }
}
