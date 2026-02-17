using Pangya_GameServer.Game;
using Pangya_GameServer.Game.Manager;

using Pangya_GameServer.Models;
using Pangya_GameServer.Models.Game;
using Pangya_GameServer.Models.Game.Base;
using Pangya_GameServer.PangyaEnums;
using Pangya_GameServer.Repository;
using Pangya_GameServer.UTIL;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.Repository;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static Pangya_GameServer.Models.DefineConstants;
namespace Pangya_GameServer.PacketFunc
{
    /// <summary>
    /// somente as requisicoes feitas pelo cliente
    /// </summary>
    public class packet_func : packet_func_base
    {
        public static int packet_svFazNada(object param, ParamDispatch pd)
        {
            string str_tmp = "Time: " + (double)(Environment.TickCount - pd._session.m_time_start) / 1000.0;
            pd._session.m_time_start = Environment.TickCount;
            _smp.message_pool.getInstance().push(new message("Packet ID: " + pd._packet.Id + " has send.", type_msg.CL_ONLY_FILE_LOG));
            return 0;
        }

        public static int packet_ClientFazNada(object param, ParamDispatch pd)
        {
            if (pd._packet.Id == 50)
            {
                stInitHole ctx_hole = new stInitHole().ToRead(pd._packet);
                _smp.message_pool.getInstance().push(new message("[packet_func_sv::packet_clientFazNada][Log] Player[UID=" + getPlayer(pd._session).m_pi.uid + "] Hole[NUMERO=" + ctx_hole.numero + ", PAR=" + ctx_hole.par + " OPT=" + ctx_hole.option + ", UNKNOWN=" + ctx_hole.ulUnknown + "] Tee[X=" + ctx_hole.tee.x + ", Z=" + ctx_hole.tee.z + "] Pin[X=" + ctx_hole.pin.x + ", Z=" + ctx_hole.pin.z + "]", type_msg.CL_ONLY_CONSOLE));
            }
            else
            {
                _smp.message_pool.getInstance().push(new message("[packet_func_sv::packet_clientFazNada][Log]: " + pd._packet.Log(), type_msg.CL_ONLY_CONSOLE));
            }
            return 0;
        }

        public static int packet_sv4D(object param, ParamDispatch pd)
        {
            string str_tmp = "Time: " + (double)(Environment.TickCount - pd._session.m_time_start) / 1000.0;
            pd._session.m_time_start = Environment.TickCount;
            _smp.message_pool.getInstance().push(new message(str_tmp, type_msg.CL_ONLY_FILE_TIME_LOG));
            return 0;
        }

        public static int packet_svRequestInfo(object param, ParamDispatch pd)
        {
            return 0;
        }

        public static int packet_as001(object param, ParamDispatch pd)
        {
            return 0;
        }

        public static int packet002(object param, ParamDispatch _arg1)
        {
            try
            {
                sgs.gs.getInstance().requestLogin((Player)_arg1._session, _arg1._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet002][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet005(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().requestChat((Player)pd._session, pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet004][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet007(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().requestEnterChannel((Player)pd._session, pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet004][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet008(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().findChannel(getPlayer(pd._session).m_channel)?.requestExitLobby(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet008][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet00E(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().findChannel(getPlayer(pd._session).m_channel)?.requestMakeRoom(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet008][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet00F(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().findChannel(getPlayer(pd._session).m_channel)?.requestEnterRoom(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet008][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet011(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().findChannel(getPlayer(pd._session).m_channel)?.requestChangeInfoRoom(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet008][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet5D(object param, ParamDispatch pd)
        {
            try
            {
                cGameChannel.RequestChangeNickName(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet6A][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet6A(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().RequestIdentity(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet6A][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet6C(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().sendServerListAndChannelListToSession(getPlayer(pd._session));
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet002][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet6D(object param, ParamDispatch pd)
        {
            try
            {
                cGameChannel.requestTitleList(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet002][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet52(object param, ParamDispatch pd)
        {
            try
            {
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet002][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet53(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().requestPlayerInfo(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet002][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            return 0;
        }

        public static int packet01D(object param, ParamDispatch pd)
        {
            try
            {
                sgs.gs.getInstance().findChannel(getPlayer(pd._session).m_channel)?.requestExitRoom(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet3A][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(exception2.getCodeError()) != 55)
                {
                    throw;
                }
                return 0;
            }
            return 0;
        }

        public static int packet3A(object param, ParamDispatch pd)
        {
            try
            {
                cGameChannel.requestUpdateEquip(getPlayer(pd._session), pd._packet);
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::packet3A][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(exception2.getCodeError()) != 55)
                {
                    throw;
                }
                return 0;
            }
            return 0;
        }

        public static byte[] pacote02(string nick, string msg, eChatMsg option)
        {
            if ((option == eChatMsg.NORMAL) && string.IsNullOrEmpty(nick))
            {
                throw new exception("Error PlayerInfo *pi is null. packet_func::pacote040()");
            }
            using PangyaBinaryWriter p = new PangyaBinaryWriter(0x02);
            p.WriteByte(option);
            if (option == eChatMsg.NORMAL|| option == eChatMsg.SPECIAL_NOTICE)
            {
                p.WritePStr(nick);
                if (option != eChatMsg.GM_EVENT)
                {
                    p.WritePStr(msg);
                }
            }
            return p.GetBytes;
        }

        public static byte[] pacote06(ServerInfoEx _si, eLoginAck option, PlayerInfo pi = null, int valor = 0)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter(new byte[1] { 6 });
            if (option == eLoginAck.ACK_LOGIN_OK && pi == null)
            {
                throw new exception("Erro PlayerInfo *pi is null. packet_func::pacote044()");
            }
            p.WriteByte(option);
            switch (option)
            {
                case eLoginAck.ACK_LOGIN_FAIL:
                    p.WriteByte(0);
                    break;
                case eLoginAck.ACK_AUTO_RECONNECT:
                    p.WriteByte(0);
                    break;
                case eLoginAck.ACK_UPDATE_LOGIN_UNIT:
                    p.WriteInt32(valor);
                    break;
                default:
                    if (option == eLoginAck.ACK_LOGIN_OK)
                    {
                        p.WritePStr(_si.version_client);
                        p.WriteBytes(packet_main(pi));
                        p.WriteTime();
                        p.WriteByte(1);
                    }
                    break;
            }
            return p.GetBytes;
        }

        public static PangyaBinaryWriter pacote12(sbyte id)
        {
            return new PangyaBinaryWriter(new byte[3]
            {
            18,
            0,
            Convert.ToByte(id)
            });
        }

        public static byte[] pacote14(NICK_CHECK code, string newNick = "")
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();
            p.Write((byte)20);
            p.Write((byte)code);
            if (code == NICK_CHECK.SUCCESS)
            {
                p.WritePStr(newNick);
            }
            return p.GetBytes;
        }

        public static PangyaBinaryWriter pacote48(List<WarehouseItemEx> v_element, int option = 0)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter(new byte[1] { 10 });
            try
            {
                p.WriteUInt16(v_element.Count);
                p.WriteUInt16((short)v_element.Count);
                foreach (WarehouseItemEx item in v_element)
                {
                    p.WriteBytes(item.ToArray());
                }
                return p;
            }
            catch
            {
                if (p.GetSize == 2)
                {
                    p.WriteUInt16(0);
                    p.WriteUInt16(0);
                }
                return p;
            }
        }

        public static byte[] pacote46(CaddieManager v_element, int option = 0)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter(new byte[1] { 70 });
            try
            {
                p.WriteInt16((short)v_element.Count);
                p.WriteInt16((short)v_element.Count);
                foreach (CaddieInfoEx char_info in v_element.Values)
                {
                    p.WriteBytes(char_info.getInfo().ToArray());
                }
                return p.GetBytes;
            }
            catch (Exception)
            {
                return p.GetBytes;
            }
        }

        public static byte[] pacote47(UserEquip _UserEquip)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter(new byte[1] { 71 });
            try
            {
                p.WriteBytes(_UserEquip.ToArray());
                return p.GetBytes;
            }
            catch (Exception)
            {
                return p.GetBytes;
            }
        }

        public static byte[] pacote97(List<TrofelEspecialInfo> _info, int option = 0)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter(new byte[1] { 151 });
            try
            {
                p.Write((byte)option);
                p.Write((byte)_info.Count);
                foreach (TrofelEspecialInfo trofeu_especial in _info)
                {
                    p.Write(trofeu_especial.ToArray());
                }
                return p.GetBytes;
            }
            catch (Exception)
            {
                return p.GetBytes;
            }
        }

        public static byte[] pacote45(CharacterManager v_element, int option = 0)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter(new byte[1] { 69 });
            try
            {
                p.WriteInt16((short)v_element.Count);
                p.WriteInt16((short)v_element.Count);
                foreach (CharacterInfoEx char_info in v_element.Values)
                {
                    p.WriteBytes(char_info.ToArray());
                }
                return p.GetBytes;
            }
            catch (Exception)
            {
                return p.GetBytes;
            }
        }

        public static byte[] pacote3F(PlayerInfo pi, byte type, int err_code = 4)
        {
            if (pi == null)
            {
                throw new exception("Erro PlayerInfo *pi is null. packet_func::pacote85()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1u, 0u));
            }
            PangyaBinaryWriter p = new PangyaBinaryWriter(63);
            p.WriteByte(err_code);
            p.WriteByte(type);
            if (err_code == 4)
            {
                switch (type)
                {
                    case 0:
                        if (pi.ei.char_info != null)
                        {
                            p.WriteBytes(pi.ei.char_info.ToArray());
                        }
                        else
                        {
                            p.WriteZero(131);
                        }
                        break;
                    case 1:
                        if (pi.ei.cad_info != null)
                        {
                            p.WriteInt32(pi.ei.cad_info.id);
                        }
                        else
                        {
                            p.WriteInt32(0);
                        }
                        break;
                    case 2:
                        if (pi.ue.item_slot.Count() == 8)
                        {
                            p.WriteUInt32(pi.ue.item_slot);
                        }
                        else
                        {
                            p.WriteInt32(0);
                        }
                        break;
                    case 3:
                        p.WriteUInt32(pi.ei.comet?._typeid ?? 0);
                        p.WriteUInt32(pi.ei.csi._typeid);
                        break;
                    case 4:
                        p.WriteUInt32(pi.ue.skin_typeid);
                        break;
                    case 5:
                        p.WriteInt32(pi.ei.char_info?.id ?? 0);
                        break;
                }
            }
            return p.GetBytes;
        }

        public static byte[] pacote51(List<MailBox> v_element, uint pagina, uint paginas, uint error = 0u)
        {
            using PangyaBinaryWriter p = new PangyaBinaryWriter(81);
            if (error != 0)
            {
                p.WriteUInt32(error);
            }
            if (error == 0)
            {
                p.WriteUInt16(pagina);
                p.WriteUInt16(paginas);
                p.WriteUInt16(v_element.Count);
                for (int i = 0; i < v_element.Count; i++)
                {
                    p.WriteBytes(v_element[i].ToArray());
                }
            }
            return p.GetBytes;
        }

        public static byte[] packet_main(PlayerInfo pi)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();
            try
            {
                if (pi == null)
                {
                    throw new exception("Erro PlayerInfo *pi is null. packet_func::principal()");
                }
                pi.mi.guild_name = "Baitolas";
                pi.mi.guild_mark_img = "269_2";
                pi.mi.oid = 2;
                pi.mi.guild_uid = 1u;
                pi.mi.flag_login_time = 0;
                p.WriteBytes(pi.getLoginInfo());
                p.WriteUInt32(pi.uid);
                p.WriteByte(pi.mi.sexo);
                p.WriteZero(20);
                p.WriteBytes(pi.getUserInfo());
                for (short i = 0; i < 33; i++)
                {
                    p.WriteInt16((short)(i + 1));
                }
                p.WriteBytes(pi.getUserEquip());
                for (int j = 0; j < 16; j++)
                {
                    MapStatisticsEx maps = new MapStatisticsEx();
                    if (j < 11)
                    {
                        maps.course = (sbyte)(j + 1);
                        maps.best_pang = (ulong)(100 * (j + 1));
                        maps.total_score = 1200 * j;
                        maps.character_typeid = (pi.ei.char_info?._typeid).Value;
                        maps.best_score = (sbyte)(-10 - j);
                        p.WriteBytes(maps.ToArray());
                    }
                    else
                    {
                        p.WriteBytes(maps.ToArray());
                    }
                }
                p.WriteBytes(new byte[(int)(1223 - p.BaseStream.Length)]);
                if (p.BaseStream.Length == 1223)
                {
                    Console.WriteLine("✅ Estrutura S2 Completa: 1223 bytes.");
                }
                else
                {
                    Console.WriteLine($"⚠\ufe0f Tamanho inesperado: {p.BaseStream.Length}");
                }
                File.WriteAllBytes("packet_main.hex", p.GetBytes);
                return p.GetBytes;
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func_gs::InitialLogin][ErrorSystem]: " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return new byte[1223];
            }
        }

        public static byte[] pacote09(List<RoomInfoEx> v_element, int option)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();
            p.init_plain(9);
            p.WriteByte((byte)((option != 0) ? 1 : v_element.Count()));
            p.WriteByte((byte)option);
            for (int i = 0; i < v_element.Count(); i++)
            {
                p.WriteBytes(v_element[i].ToArray());
            }
            return p.GetBytes;
        }

        public static List<PangyaBinaryWriter> pacote08(List<PlayerLobbyInfo> v_element, int option)
        {
            List<PangyaBinaryWriter> responses = new List<PangyaBinaryWriter>();
            int elements = v_element.Count;
            int itensPorPacote = 20;
            List<List<PlayerLobbyInfo>> splitList = ((elements * 200 < MAX_BUFFER_PACKET - 100) ? new List<List<PlayerLobbyInfo>> { v_element } : (from x in v_element.Select((PlayerLobbyInfo item2, int index) => new
            {
                item = item2,
                index = index
            })
                                                                                                                                                   group x by x.index / itensPorPacote into g
                                                                                                                                                   select g.Select(x => x.item).ToList()).ToList());
            foreach (List<PlayerLobbyInfo> lista in splitList)
            {
                PangyaBinaryWriter p = new PangyaBinaryWriter(8);
                p.WriteByte((byte)option);
                p.WriteByte((byte)lista.Count);
                foreach (PlayerLobbyInfo item in lista)
                {
                    p.WriteBytes(item.ToArray());
                }
                responses.Add(p);
            }
            return responses;
        }

        public static bool pacote0A(ref PangyaBinaryWriter p, Player _session, List<PlayerRoomInfoEx> v_element, int option = 0)
        {
            TPlayerRoom_Action opt = (TPlayerRoom_Action)(option & 0xFF);

            Debug.WriteLine($"pacote0A => enum: {opt}, code: {option & 0xFF}, code2: {option & 0x100}");

            try
            {

                if ((option & 0xFF) == 2)
                { // exit player
                    p.init_plain(0x0A);
                    p.WriteSByte((sbyte)option);
                    p.WriteUInt32(_session.m_pi.uid);
                    return true;
                }
                else if ((option & 0xFF) == 7)
                {
                    int elementSize = (option & 0x100) != 0 ? Marshal.SizeOf(new PlayerRoomInfo()) : Marshal.SizeOf(new PlayerRoomInfoEx());
                    int maxPacket = Marshal.SizeOf(new PlayerRoomInfoEx());
                    int total = v_element.Count;
                    int por_packet = (maxPacket - 100 > elementSize) ? (maxPacket - 100) / elementSize : 1;

                    int index = 0;

                    while (index < total)
                    {
                        p.init_plain(0x0A);
                        p.WriteSByte((sbyte)option);

                        if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                            p.WriteSByte((sbyte)Math.Min(por_packet, total - index));
                        else if ((option & 0xFF) == 7)
                            p.WriteSByte((sbyte)total);
                        else if ((option & 0xFF) == 3 || (option & 0xFF) == 3)
                        {
                            p.WriteUInt32(_session.m_pi.uid);
                        }
                        for (int i = 0; i < por_packet && index < total; i++, index++)
                        {
                            var playerRoom = v_element[index];
                            if (elementSize == 112)
                            {
                                p.WriteBytes(playerRoom.ToArray());
                            }
                            else
                            {
                                p.WriteBytes(playerRoom.ToArrayEx()); 
                            }
                        }
                        p.WriteByte(0);     // Final list de PlayerRoomInfo

                        session_send(p, _session, 1);//-> MAKE_END_SPLIT_PACKET
                    }
                    return true;
                }
                else
                {
                    int elementSize = (option & 0x100) != 0 ? Marshal.SizeOf(new PlayerRoomInfo()) : Marshal.SizeOf(new PlayerRoomInfoEx());
                    int elements = v_element.Count;
                    int totalSize = elements * elementSize;

                    try
                    {
                        if (totalSize < MAX_BUFFER_PACKET - 100)//-> MAKE_END_SPLIT_PACKET nao tem, so no else, OK?
                        {
                            p.init_plain(0x0A);
                            p.WriteSByte((sbyte)option);

                            if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                                p.WriteByte((byte)elements);
                            else if ((option & 0xFF) == 3 || (option & 0xFF) == 3)
                            {
                                p.WriteUInt32(_session.m_pi.uid);
                            }

                            foreach (var playerRoom in v_element)
                            {

                                if (elementSize == 112)
                                {
                                    p.WriteBytes(playerRoom.ToArray());
                                }
                                else
                                {
                                    p.WriteBytes(playerRoom.ToArrayEx());
                                } 
                            }
                            p.WriteByte(0);
                            return true;
                        }
                        else
                        {
                            int total = elements;
                            int por_packet = ((MAX_BUFFER_PACKET - 100) > elementSize) ? (MAX_BUFFER_PACKET - 100) / elementSize : 1;

                            int index = 0;

                            while (index < total)
                            {
                                p.init_plain(0x0A);

                                if ((option & 0xFF) == 0 && index != 0)
                                    p.WriteByte(5); // append players
                                else
                                    p.WriteByte((byte)option);



                                if ((option & 0xFF) == 0 || (option & 0xFF) == 5)
                                    p.WriteSByte((sbyte)Math.Min(por_packet, total - index));
                                else if ((option & 0xFF) == 3)
                                {
                                    elementSize = 112;
                                    p.WriteUInt32(_session.m_pi.uid);
                                }

                                for (int i = 0; i < por_packet && index < total; i++, index++)
                                {
                                    var playerRoom = v_element[index];
                                    if (elementSize == 112)
                                    {
                                        p.WriteBytes(playerRoom.ToArray());
                                    }
                                    else
                                    {
                                        p.WriteBytes(playerRoom.ToArrayEx());
                                    }

                                }
                                p.WriteByte(0); // Final list de PlayerRoomInfo

                                session_send(p, _session, 1);//-> MAKE_END_SPLIT_PACKET
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[pacote0A][Fatal] " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[pacote0A][Fatal] " + ex);
            }
            return false;
        }

        public static PangyaBinaryWriter pacote0E(RoomInfoEx _ri, short option)
        {
            PangyaBinaryWriter p = new PangyaBinaryWriter();
            p.init_plain(14);
            p.WriteBytes(_ri.ToArrayEx());
            return p;
        }

        public static byte[] pacote0D(Room _room, TGAME_CREATE_RESULT option = TGAME_CREATE_RESULT.CREATE_GAME_RESULT_SUCCESS)
        {
            try
            {
                if (_room == null)
                {
                    throw new exception("Error _room is null. EM packet_func::pacote049()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 3u, 0u));
                }
                PangyaBinaryWriter p = new PangyaBinaryWriter();
                p.init_plain(13);
                if (option == TGAME_CREATE_RESULT.CREATE_GAME_RESULT_SUCCESS)
                {
                    p.WriteInt16((short)option);
                    p.WriteBytes(_room.getInfo().ToArray());
                }
                else
                {
                    p.WriteByte((int)option);
                }
                return p.GetBytes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] pacote11(List<Channel> v_element, bool build_s = false)
        {
            try
            {
                using PangyaBinaryWriter p = new PangyaBinaryWriter();
                if (!build_s)
                {
                    p.init_plain(17);
                }
                p.WriteByte(v_element.Count & 0xFF);
                foreach (Channel channel in v_element)
                {
                    p.WriteBytes(channel.getInfo().ToArray());
                }
                return p.GetBytes;
            }
            catch (exception exception2)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func::pacote04D][ErrorSystem] " + exception2.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return new byte[3] { 17, 0, 0 };
            }
        }

        public static void channel_broadcast(Channel _channel, byte[] p, int __DEBUG = 1)
        {
            try
            {
                List<Player> channel_session = _channel.getSessions();
                for (int i = 0; i < channel_session.Count; i++)
                {
                    MAKE_SEND_BUFFER(p, channel_session[i]);
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[channel_broadcast(byte[])] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void channel_broadcast(Channel _channel, PangyaBinaryWriter p, int __DEBUG = 1)
        {
            try
            {
                List<Player> channel_session = _channel.getSessions();
                for (int i = 0; i < channel_session.Count; i++)
                {
                    MAKE_SEND_BUFFER(p.GetBytes, channel_session[i]);
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[channel_broadcast(byte[])] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void channel_broadcast(Channel _channel, List<byte[]> v_p, int __DEBUG = 1)
        {
            try
            {
                for (int i = 0; i < v_p.Count; i++)
                {
                    if (v_p[i] != null)
                    {
                        List<Player> channel_session = _channel.getSessions();
                        for (int ii = 0; ii < channel_session.Count; ii++)
                        {
                            MAKE_SEND_BUFFER(v_p[i], channel_session[ii]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[channel_broadcast(List<byte[]>)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void channel_broadcast(Channel _channel, List<PangyaBinaryWriter> v_p, int __DEBUG = 1)
        {
            try
            {
                for (int i = 0; i < v_p.Count; i++)
                {
                    PangyaBinaryWriter writer = v_p[i];
                    if (writer != null)
                    {
                        List<Player> channel_session = _channel.getSessions();
                        for (int ii = 0; ii < channel_session.Count; ii++)
                        {
                            MAKE_SEND_BUFFER(writer.GetBytes, channel_session[ii]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[channel_broadcast(List<PangyaBinaryWriter>)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void lobby_broadcast(Channel _channel, byte[] p, int __DEBUG = 1)
        {
            try
            {
                List<Player> channel_session = _channel.getSessions();
                for (int i = 0; i < channel_session.Count; i++)
                {
                    if (channel_session[i].m_pi.mi.sala_numero == 65535)
                    {
                        MAKE_SEND_BUFFER(p, channel_session[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[lobby_broadcast] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void room_broadcast(Room _room, PangyaBinaryWriter p, int __DEBUG = 1)
        {
            try
            {
                List<Player> room_session = _room.getSessions(null, _with_invited: false);
                for (int i = 0; i < room_session.Count; i++)
                {
                    if (room_session[i] != null)
                    {
                        MAKE_SEND_BUFFER(p.GetBytes, room_session[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[room_broadcast(writer)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void room_broadcast(Room _room, byte[] p, int __DEBUG = 1)
        {
            if (_room == null || p.Length == 0)
            {
                return;
            }
            try
            {
                List<Player> room_session = _room.getSessions(null, _with_invited: false);
                for (int i = 0; i < room_session.Count; i++)
                {
                    if (room_session[i] != null)
                    {
                        MAKE_SEND_BUFFER(p, room_session[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[room_broadcast(writer)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void room_broadcast(Room _room, List<PangyaBinaryWriter> v_p, int __DEBUG = 1)
        {
            try
            {
                for (int i = 0; i < v_p.Count; i++)
                {
                    if (v_p[i] != null)
                    {
                        List<Player> room_session = _room.getSessions();
                        for (int ii = 0; ii < room_session.Count; ii++)
                        {
                            MAKE_SEND_BUFFER(v_p[i].GetBytes, room_session[ii]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[room_broadcast(List)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void game_broadcast(GameBase _game, byte[] p, int __DEBUG = 1)
        {
            try
            {
                List<Player> game_session = _game.getSessions();
                for (int i = 0; i < game_session.Count; i++)
                {
                    MAKE_SEND_BUFFER(p, game_session[i]);
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[game_broadcast(byte[])] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void game_broadcast(GameBase _game, PangyaBinaryWriter p, int __DEBUG = 1)
        {
            try
            {
                List<Player> game_session = _game.getSessions();
                for (int i = 0; i < game_session.Count; i++)
                {
                    MAKE_SEND_BUFFER(p.GetBytes, game_session[i]);
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[game_broadcast(byte[])] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void game_broadcast(GameBase _game, List<PangyaBinaryWriter> v_p, int __DEBUG = 1)
        {
            try
            {
                for (int i = 0; i < v_p.Count; i++)
                {
                    if (v_p[i] != null)
                    {
                        List<Player> game_session = _game.getSessions();
                        for (int ii = 0; ii < game_session.Count; ii++)
                        {
                            MAKE_SEND_BUFFER(v_p[i].GetBytes, game_session[ii]);
                        }
                    }
                    else
                    {
                        _smp.message_pool.getInstance().push(new message("Error byte[] p is null, packet_func::game_broadcast()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[game_broadcast(List)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void vector_send(PangyaBinaryWriter _p, List<Player> _v_s, int __DEBUG = 1)
        {
            try
            {
                foreach (Player el in _v_s)
                {
                    MAKE_SEND_BUFFER(_p.GetBytes, el);
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[vector_send(Session)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void vector_send(List<PangyaBinaryWriter> _v_p, List<Player> _v_s, int __DEBUG = 1)
        {
            try
            {
                foreach (PangyaBinaryWriter el in _v_p)
                {
                    if (el != null)
                    {
                        foreach (Player el2 in _v_s)
                        {
                            MAKE_SEND_BUFFER(el.GetBytes, el2);
                        }
                    }
                    else
                    {
                        _smp.message_pool.getInstance().push(new message("Error byte[] p is null, packet_func::vector_send(Player)", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[vector_send(Player List)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(PangyaBinaryWriter p, Session s, int __DEBUG = 1)
        {
            try
            {
                if (s == null)
                {
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1u, 2u));
                }
                MAKE_SEND_BUFFER(p.GetBytes, s);
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[session_send(byte[])] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(byte[] p, Session s, int __DEBUG = 1)
        {
            try
            {
                if (s == null)
                {
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1u, 2u));
                }
                MAKE_SEND_BUFFER(p, s);
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[session_send(byte[])] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(PangyaBinaryWriter p, Player s, int __DEBUG = 1)
        {
            try
            {
                if (s == null)
                {
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1u, 2u));
                }
                MAKE_SEND_BUFFER(p.GetBytes, s);
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[session_send(writer)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(List<PangyaBinaryWriter> v_p, Player s, int __DEBUG = 1)
        {
            try
            {
                if (s == null)
                {
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1u, 2u));
                }
                for (int i = 0; i < v_p.Count; i++)
                {
                    if (v_p[i] != null && v_p[i].GetSize != 0)
                    {
                        MAKE_SEND_BUFFER(v_p[i].GetBytes, s);
                    }
                    else
                    {
                        _smp.message_pool.getInstance().push(new message("Error byte[] p is null, packet_func::session_send()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[session_send(writer list)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static void session_send(List<byte[]> v_p, Player s, int __DEBUG = 1)
        {
            try
            {
                if (s == null)
                {
                    throw new exception("Error session s is null, packet_func::session_send()", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1u, 2u));
                }
                for (int i = 0; i < v_p.Count; i++)
                {
                    if (v_p[i] != null)
                    {
                        MAKE_SEND_BUFFER(v_p[i], s);
                    }
                    else
                    {
                        _smp.message_pool.getInstance().push(new message("Error byte[] p is null, packet_func::session_send()", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    }
                }
            }
            catch (Exception ex)
            {
                _smp.message_pool.getInstance().push(new message("[session_send(byte[] list)] Exception: " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        public static Player getPlayer(Session _session)
        {
            return Tools.reinterpret_cast<Player>(_session);
        }

        private static void CHECK_SESSION_IS_AUTHORIZED(Player _session, string method)
        {
            if (!_session.m_is_authorized)
            {
                throw new exception("[packet_func::" + method + "][Error] PLAYER[UID=" + _session.m_pi.uid + "] Nao esta autorizado a fazer esse request por que ele ainda nao fez o login com o Server. Hacker ou Bug", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 1u, 117441793u));
            }
        }
    }
}
