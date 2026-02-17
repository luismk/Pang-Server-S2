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
using static Pangya_GameServer.Models.DefineConstants;
namespace Pangya_GameServer.Models.Game
{
    public class cGameChannel
    {

        public void requestOpenMailBox(Player _session, packet _packet)
        {
            //

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {

                if (_session.m_pi.block_flag.m_flag.mail_box)
                {
                    throw new exception("[cGameChannel::requestOpenMailBox][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou abrir Mail Box, mas ele nao pode. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        5, 0x790001));
                }

                uint pagina = _packet.ReadUInt32();

                if (pagina <= 0)
                {
                    throw new exception("[cGameChannel::requestOpenMailBox][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou abrir Mail Box[Pagina=" + (pagina) + "], mas a pagina é invalida.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.CHANNEL,
                        6, 0x790002));
                }

#if RELEASE
        			_smp.message_pool.getInstance().push(new message("[cGameChannel::requestOpenMailBox][Sucess] PLAYER [UID=" + (_session.m_pi.uid) + "]\tRequest Pagina: " + (pagina) + " MailBox", type_msg.CL_FILE_LOG_AND_CONSOLE));
#endif





                var mails = _session.m_pi.m_mail_box.GetPage((uint)pagina);

                if (mails.Any())
                {

                    //Log
                    _smp.message_pool.getInstance().push(new message("[cGameChannel::requestOpenMailBox][Sucess] PLAYER [UID=" + (_session.m_pi.uid)
                                              + "] abriu o MailBox[Pagina=" + (pagina) + "] com sucesso.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    // pagina existe, envia ela
                    packet_func.session_send(packet_func.pacote51(mails, pagina, _session.m_pi.m_mail_box.getTotalPages()/*cmd_mbi.getTotalPage()*/), _session);

                }
                else
                { // MailBox Vazio                                                  
                    packet_func.session_send(packet_func.pacote51(new List<MailBox>(), pagina, 1), _session);
                }
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[cGameChannel::requestOpenMailBox][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                p.init_plain(51);

                p.WriteUInt32((ExceptionError.STDA_SOURCE_ERROR_DECODE_TYPE(e.getCodeError()) == STDA_ERROR_TYPE.CHANNEL) ? ExceptionError.STDA_SYSTEM_ERROR_DECODE(e.getCodeError()) : 0x5500200);

                packet_func.session_send(p,
                    _session, 1);
            }
        }
        public static void RequestChangeNickName(Player session, packet packet)
        {
            // S1/S2 lê a string do pacote recebido
            string newNick = packet.ReadPStr();
            var code = NICK_CHECK.SUCCESS;
            uint itemID = 0x1A000003;
            try
            {
                // [VALIDAÇÕES]
                if (session.m_pi.nickname.Equals(newNick, StringComparison.OrdinalIgnoreCase))
                {
                    code = NICK_CHECK.SAME_NICK_USED;
                    throw new Exception("Nick igual ao atual.");
                }

                // [LOGICA DE CUSTO / ITEM]
                bool bypassCost = false;

                if (session.m_pi.m_cap.game_master)
                {
                    bypassCost = true; // GM não gasta item nem cookie
                }
                //// else if (session.Inventory.HasItem(itemID))
                //// {
                //  //  REMOVE O ITEM - Igual ao original
                //    // session.Inventory.RemoveItem(itemID, 1); 
                //    // bypassCost = true;
                //// }
                //else if (session.m_pi.cookie >= 7000)
                //{
                //    session.consomeCookie(7000);
                //}
                //else 
                //{
                //    code = NICK_CHECK.EMPETY_ERROR; // No original: Sem fundos/item
                //    throw new Exception("Sem item ou cookies.");
                //}

                var cmd_change_nick = new CmdUpdateNickName(session.m_pi.uid, newNick); // Waiter

                snmdb.NormalManagerDB.getInstance().add(0, cmd_change_nick, null, null);


                if (cmd_change_nick.getException().getCodeError() != 0)
                    throw cmd_change_nick.getException();
                else
                {
                    // Envia Resposta 0x14 (Sucesso)
                    packet_func.session_send(packet_func.pacote14(NICK_CHECK.SUCCESS, newNick), session);

                    session.m_pi.nickname = newNick;
                    session.m_pi.mi.nick_name = newNick;
                    if (session.m_channel != null && session.m_channel.getId() != -1)
                    {
                        session.m_channel.updatePlayerInfo(session);
                        session.m_channel.sendUpdatePlayerInfo(session, 3);
                    }
                }
            }
            catch (Exception e)
            {
                packet_func.session_send(packet_func.pacote14(code), session);
                Console.WriteLine($"[ChangeNick] Erro: {e.Message}");
            }
        }

        public static void requestUpdateEquip(Player _session, packet _packet)
        {
            byte type = 0;
            int item_id;
            int error = 4;

            PangyaBinaryWriter p = new PangyaBinaryWriter();

            try
            {
                type = _packet.ReadUInt8();

                switch (type)
                {
                    case 0: // Character Equipado Parts Complete
                        {
                            CharacterInfoEx ci = new CharacterInfoEx().ToRead(_packet);
                            CharacterInfoEx pCe = null;
                            if (ci.id != 0
                                && (pCe = _session.m_pi.findCharacterById(ci.id)) != null
                                && (sIff.getInstance().getItemGroupIdentify(pCe._typeid) == sIff.getInstance().CHARACTER && sIff.getInstance().getItemGroupIdentify(ci._typeid) == sIff.getInstance().CHARACTER))
                            {



                                // Checks Parts Equiped
                                _session.checkCharacterEquipedPart(ci);

                                // Check AuxPart Equiped
                                _session.checkCharacterEquipedAuxPart(ci);


                                pCe = ci;
                                _session.m_pi.ei.char_info = ci;
                                _session.m_pi.mp_ce[ci.id] = ci;
                                snmdb.NormalManagerDB.getInstance().add(0,
                                    new CmdUpdateCharacterAllPartEquiped(_session.m_pi.uid, ci),
                                    null, null);
                            }
                            else
                            {

                                error = (ci.id == 0) ? 1 : (pCe == null ? 2 : 3);

                                _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou Atualizar os Parts do Character[ID=" + (ci.id) + "], mas deu Error[VALUE=" + (error) + "]. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }


                            //var r = m_rm.findRoom((short)_session.m_pi.mi.sala_numero);

                            //if (r != null)
                            //{
                            //    r.updatePlayerInfo(_session);

                            //    PlayerRoomInfoEx pri = r.getPlayerInfo(_session);

                            //    if (packet_func.pacote048(ref p, _session, new List<PlayerRoomInfoEx>() { pri ?? new PlayerRoomInfoEx() }, 0x103))
                            //        packet_func.room_broadcast(r, p, 0);

                            //    packet_func.room_broadcast(r,
                            //         packet_func.pacote04B(_session, 4), 0);
                            //}



                            packet_func.session_send(packet_func.pacote3F(_session.m_pi, type,
                                error),
                                _session, 1);
                            break;
                        }
                    case 1: // Caddie
                        {
                            if ((item_id = _packet.ReadInt32()) != 0)
                            {
                                var pCi = _session.m_pi.findCaddieById(item_id);

                                if (pCi != null && sIff.getInstance().getItemGroupIdentify(pCi._typeid) == sIff.getInstance().CADDIE)
                                {

                                    _session.m_pi.ei.cad_info = pCi;
                                    _session.m_pi.ue.caddie_id = item_id;

                                    // Verifica se o Caddie pode ser equipado
                                    if (_session.checkCaddieEquiped(_session.m_pi.ue))
                                    {
                                        item_id = _session.m_pi.ue.caddie_id; // Desequipa caddie
                                    }

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0,
                                        new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)item_id),
                                        null, null);

                                }
                                else
                                {

                                    error = (pCi == null ? 2 : 3);

                                    _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou equipar Caddie[ID=" + (item_id) + "], mas deu Error[VALUE=" + (error) + "]. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }

                            }
                            else if (_session.m_pi.ue.caddie_id > 0 && _session.m_pi.ei.char_info != null)
                            { // Desequipa Caddie

                                _session.m_pi.ei.cad_info = null;
                                _session.m_pi.ue.caddie_id = 0;

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0,
                                    new CmdUpdateCaddieEquiped(_session.m_pi.uid, (int)item_id),
                                    null, null);

                            } // else Não tem nenhum caddie equipado, para desequipar, então o cliente só quis atualizar o estado

                            packet_func.session_send(packet_func.pacote3F(
                                _session.m_pi, type,
                                error),
                                _session, 1);
                            break;
                        }
                    case 2: // Itens Equipáveis
                        {
                            // Aqui tenho que copiar para uma struct temporaria antes,
                            // para verificar os itens que ele está equipando.
                            // Se está tudo certo, Salva na struct da session dele, e depois manda pra salvar no db por meio do Asyc query update
                            // Se não da mensagem de erro
                            // Error: 0 = "código 'errado(tenho que traduzir direito ainda)'", 1 = "DB Item Errado", 2, 3 = "Unknown ainda",
                            // 4 = "Sucesso"

                            UserEquip ue = new UserEquip();
                            ue.item_slot = _packet.ReadUInt32(8);

                            try
                            {

                                Dictionary<uint, uint> mp_same_item_count = new Dictionary<uint, uint>();
                                uint c_it;

                                for (var i = 0; i < ue.item_slot.Length; ++i)
                                {

                                    if (ue.item_slot[i] != 0)
                                    {

                                        if (!sIff.getInstance().ItemEquipavel(ue.item_slot[i]))
                                        {
                                            throw new exception("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou atualizar item[TYPEID=" + (ue.item_slot[i]) + "] equipaveis, mas nao é um item equipavel. Hacker ou Bug", STDA_ERROR_TYPE.CHANNEL);
                                        }

                                        // Verifica se esse item existe pela chave do map se não lança uma exception se nao existir
                                        if (sIff.getInstance().findItem(ue.item_slot[i]) == null)
                                        {
                                            throw new IndexOutOfRangeException("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou atualizar item[TYPEID=" + (ue.item_slot[i]) + "] equipaveis, mas nao tem o item no iff Item do IFF_STRUCT do Server. Hacker ou Bug");
                                        }

                                        // E se não tiver quantidade para equipar lança outra exception
                                        var pWi = _session.m_pi.findWarehouseItemByTypeid(ue.item_slot[i]);

                                        if (pWi == null)
                                        {
                                            throw new exception("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou atualizar item[TYPEID=" + (ue.item_slot[i]) + "] equipaveis, mas ele nao tem esse item. Hacker ou Bug", 2);
                                        }
                                        else
                                        {
                                            c_it = (mp_same_item_count.FirstOrDefault(c => c.Key == ue.item_slot[i]).Value);
                                            if (c_it > 0)
                                            {
                                                if (active_item_cant_have_2_inventory.Any(
                                                   c => c == ue.item_slot[i]))
                                                {
                                                    throw new exception("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou equipar item[TYPEID=" + (ue.item_slot[i]) + "] mas ele ja tem 1 item desse equipado, so e permitido equipar 1, nao pode equipar mais do que 1. Hacker ou Bug", 2);
                                                }
                                                // 
                                                else if ((pWi.STDA_C_ITEM_QNTD) < (int)(c_it + 1))
                                                {
                                                    throw new exception("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou atualizar item[TYPEID=" + (ue.item_slot[i]) + "] equipaveis, mas ele nao tem quantidade dele. Hacker ou Bug", 2);
                                                }
                                                else // Increase Count Same Item
                                                {
                                                    // 
                                                    mp_same_item_count[ue.item_slot[i]] = c_it++; // Count
                                                }

                                            }
                                            else
                                            {

                                                if (pWi.STDA_C_ITEM_QNTD < 1)
                                                {
                                                    throw new exception("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou atualizar item[TYPEID=" + (ue.item_slot[i]) + "] equipaveis, mas ele nao tem quantidade dele. Hacker ou Bug", 2);
                                                }

                                                // insert
                                                mp_same_item_count.Add(ue.item_slot[i], 1);
                                            }

                                        }
                                    }
                                }
                                _session.m_pi.ue.item_slot =
                                ue.item_slot;

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(25,
                                    new CmdUpdateItemSlot(_session.m_pi.uid, ue.item_slot),
                                    null, null);

                            }
                            catch (exception e)
                            {

                                _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                                if (e.getCodeError() == 0)
                                {
                                    error = 0;
                                }
                                else if (e.getCodeError() == 1)
                                {
                                    error = 1;
                                }
                                else // System Error
                                {
                                    error = 10;
                                }

                            }
                            catch
                            {

                                _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][ErrorSystem] Unknown Error", type_msg.CL_FILE_LOG_AND_CONSOLE));

                                // System Error
                                error = 10;
                            }


                            packet_func.session_send(packet_func.pacote3F(_session.m_pi, type,
                                error),
                                _session, 1);
                            break;
                        }
                    case 3: // Bola e Taqueira
                        {
                            // Ball(COMET)
                            WarehouseItemEx pWi = null;

                            if ((item_id = _packet.ReadInt32()) != 0
                                && (pWi = _session.m_pi.findWarehouseItemByTypeid((uint)item_id)) != null
                                && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().BALL)
                            {

                                _session.m_pi.ei.comet = pWi;
                                _session.m_pi.ue.ball_typeid = (uint)item_id; // Ball(Comet) é o typeid que o cliente passa

                                // Verifica se a Bola pode ser equipada
                                if (_session.checkBallEquiped(_session.m_pi.ue))
                                {
                                    item_id = (int)_session.m_pi.ue.ball_typeid;
                                }

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0,
                                    new CmdUpdateBallEquiped(_session.m_pi.uid, (uint)item_id),
                                    null, null);

                            }
                            else if (item_id == 0)
                            { // Bola 0 coloca a bola padrão para ele, se for premium user coloca a bola de premium user

                                // Zera para equipar a bola padrão
                                _session.m_pi.ei.comet = null;
                                _session.m_pi.ue.ball_typeid = 0;

                                // Verifica se a Bola pode ser equipada (Coloca para equipar a bola padrão
                                if (_session.checkBallEquiped(_session.m_pi.ue))
                                {
                                    item_id = (int)_session.m_pi.ue.ball_typeid;
                                }

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0,
                                    new CmdUpdateBallEquiped(_session.m_pi.uid, (uint)item_id),
                                    null, null);

                            }
                            else
                            {

                                error = (pWi == null ? 2 : 3);

                                _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou equipar Ball[TYPEID=" + (item_id) + "], mas deu Error[VALUE=" + (error) + "]. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }

                            // ClubSet
                            if ((item_id = _packet.ReadInt32()) != 0
                                && (pWi = _session.m_pi.findWarehouseItemById(item_id)) != null
                                && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().CLUBSET)
                            {

                                // Update ClubSet
                                _session.m_pi.ei.clubset = pWi;

                                // Esse C do WarehouseItem, que pega do DB, não é o ja updado inicial da taqueira é o que fica tabela enchant, 
                                // que no original fica no warehouse msm, eu só confundi quando fiz
                                _session.m_pi.ei.csi.setValues(pWi.id, pWi._typeid, pWi.c);

                                var cs = sIff.getInstance().findClubSet(pWi._typeid);

                                if (cs != null)
                                {

                                    for (var j = 0; j < 5; ++j)
                                    {
                                        _session.m_pi.ei.csi.enchant_c[j] = (short)(cs.SlotStats.getSlot[j] + pWi.clubset_workshop.c[j]);
                                    }

                                    _session.m_pi.ue.clubset_id = item_id;

                                    // Verifica se o ClubSet pode ser equipado
                                    if (_session.checkClubSetEquiped(_session.m_pi.ue))
                                    {
                                        item_id = _session.m_pi.ue.clubset_id;
                                    }

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0,
                                        new CmdUpdateClubsetEquiped(_session.m_pi.uid, (int)item_id),
                                        null, null);

                                }
                                else // O Cliente é que tem que saber do erro, não posso passa essa excessão para função anterior
                                {
                                    _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou atualizar o ClubSet[TYPEID=" + (pWi._typeid) + ", ID=" + (pWi.id) + "] equipado, mas ClubSet Not exists on IFF_STRUCT do Server. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                }

                            }
                            else
                            {

                                error = (item_id == 0) ? 1 : (pWi == null ? 2 : 3);

                                _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou equipar ClubSet[ID=" + (item_id) + "], mas deu Error[VALUE=" + (error) + "]. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }


                            packet_func.session_send(packet_func.pacote3F(_session.m_pi, type,
                                error),
                                _session, 1);
                            break;
                        }
                    case 4: // Skins
                        {
                            for (var i = 0; i < 6; ++i)
                            {

                                if ((item_id = _packet.ReadInt32()) != 0)
                                {

                                    var pWi = _session.m_pi.findWarehouseItemByTypeid((uint)item_id);

                                    if (pWi != null && sIff.getInstance().getItemGroupIdentify(pWi._typeid) == sIff.getInstance().SKIN)
                                    {

                                        _session.m_pi.ue.skin_id[i] = (uint)pWi.id;
                                        _session.m_pi.ue.skin_typeid[i] = pWi._typeid;

                                        // Update ON DB
                                        snmdb.NormalManagerDB.getInstance().add(0,
                                            new CmdUpdateSkinEquiped(_session.m_pi.uid, _session.m_pi.ue),
                                            null, null);

                                    }
                                    else
                                    {

                                        error = (pWi == null ? 2 : 3);

                                        _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou equipar SKIN[TYPEID=" + (item_id) + ", SLOT=" + (i) + "], mas deu Error[VALUE=" + (error) + "]. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                                    }

                                }
                                else
                                { // Zera o Skin equipado

                                    _session.m_pi.ue.skin_id[i] = 0u;
                                    _session.m_pi.ue.skin_typeid[i] = 0u;

                                    // Update ON DB
                                    snmdb.NormalManagerDB.getInstance().add(0,
                                        new CmdUpdateSkinEquiped(_session.m_pi.uid, _session.m_pi.ue),
                                        null, null);
                                }
                            }

                            // Verifica se a Skin pode ser equipada
                            if (_session.checkSkinEquiped(_session.m_pi.ue))
                            {
                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0,
                                    new CmdUpdateSkinEquiped(_session.m_pi.uid, _session.m_pi.ue),
                                    null, null);
                            }
                            var _p = packet_func.pacote3F(_session.m_pi, type,
                                    error);
                            packet_func.session_send(_p,
                                _session, 1);
                            break;
                        }
                    case 5: // only Character ID EQUIPADO
                        {
                            CharacterInfoEx pCe = null;

                            if ((item_id = _packet.ReadInt32()) != 0
                                && (pCe = _session.m_pi.findCharacterById(item_id)) != null
                                && sIff.getInstance().getItemGroupIdentify(pCe._typeid) == sIff.getInstance().CHARACTER)
                            {

                                _session.m_pi.ei.char_info = pCe;
                                _session.m_pi.ue.character_id = item_id;

                                _session.m_pi.UpdateCharacter(item_id, pCe);
 
                                var _ps = packet_func.pacote3F(_session.m_pi, type,
                                     error);
                                packet_func.session_send(_ps,
                                    _session, 1);

                                // Update ON DB
                                snmdb.NormalManagerDB.getInstance().add(0,
                                    new CmdUpdateCharacterEquiped(_session.m_pi.uid, (int)item_id),
                                    null, null);

                            }
                            else
                            {

                                error = (item_id == 0) ? 1 : (pCe == null ? 2 : 3);

                                _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][Error] PLAYER [UID=" + (_session.m_pi.uid) + "] tentou equipar o Character[ID=" + (item_id) + "], mas deu Error[VALUE=" + (error) + "]. Hacker ou Bug", type_msg.CL_FILE_LOG_AND_CONSOLE));
                            }
                            var _p = packet_func.pacote3F(_session.m_pi, type,
                                    error);
                            packet_func.session_send(_p,
                                _session, 1);
                            break;
                        }
                } 
            }
            catch (exception e)
            {
                packet_func.session_send(packet_func.pacote3F(_session.m_pi, type,
                               1),
                               _session, 1);

                _smp.message_pool.getInstance().push(new message("[cGameChannel::requestChangePlayerItemMyRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void requestTitleList(Player _session, packet _packet)
        { }
    }
}
