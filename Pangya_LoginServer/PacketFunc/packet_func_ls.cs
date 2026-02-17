using Pangya_LoginServer.DataBase;
using Pangya_LoginServer.Models;
using Pangya_LoginServer.PangyaEnums;
using Pangya_LoginServer.Session;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using sls; 
using System.Text.RegularExpressions; 
namespace Pangya_LoginServer.PacketFunc
{
    public class packet_func_ls : packet_func_base
    {
        public static void SUCCESS_LOGIN(string from, object arg1, Player session)
        {
            session.m_pi.m_state = 1;

            _smp.message_pool.getInstance().push(new message($"[packet_func_ls::{from}][Log] PLAYER[ID: {session.m_pi.id}, UID: {session.m_pi.uid} Logged in]", type_msg.CL_FILE_LOG_AND_CONSOLE));

            succes_login(arg1, session);
        }


        public static int packet001(object param, ParamDispatch pd)
        {
            try
            {
                ls.getInstance().requestLogin((Player)pd._session, pd._packet);
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet001][Log][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.LOGIN_SERVER)
                    throw;
            }

            return 0;
        }

        public static int packet003(object param, ParamDispatch pd)
        {
            try
            {
                uint server_uid = pd._packet.ReadUInt32();

                if (server_uid <= 0)
                {
                    throw new exception("[packet_func_ls::packet003][Error] UID Server: " + (server_uid) + " is worng.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_LS, 21, 0));
                }

                var servers = CommandDB.GetGame();
                // Registra o logon no server_uid do player_uid
                if (servers.Any(c => c.uid == server_uid))
                {
                    CommandDB.RegisterLogonServer(getPlayer(pd._session).m_pi.uid, server_uid); 
					
					  var auth_key_game = CommandDB.GetAuthKeyGame(getPlayer(pd._session).m_pi.uid, server_uid);

                    session_send(pacoteFD(auth_key_game), getPlayer(pd._session), 1);
                    //força a desconexao...
                    ls.getInstance().DisconnectSession(pd._session);
                }
                else
                {
                    throw new exception("[packet_func_ls::packet003][Error] UID Server: " + (server_uid) + " no exist.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_LS, 21, 0));
                }
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet003][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR(e.getCodeError(), (uint)STDA_ERROR_TYPE.EXEC_QUERY, 6/*AuthKeyLogin*/))
                    throw;
            }

            return 0;
        }

        public static int packet004(object param, ParamDispatch pd)
        {


            try
            {
                ls.getInstance().requestDownPlayerOnGameServer(getPlayer(pd._session));
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet004][Error] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }
 
        public static int packet03E(object param, ParamDispatch pd)
        {
            var _session = getPlayer(pd._session);
            try
            {
                var test = pd._packet.Log();
                uint _typeid = pd._packet.ReadUInt32();
                var default_hair = pd._packet.ReadUInt8();//is flag, encrypt...
                byte default_shirts = 0;//nao tem na versao antigas..
                 
                if (sIff.getInstance().findCharacter(_typeid) == null)
                    throw new exception("[packet_func_ls::packet03E][Error] typeid character: " + (_typeid) + " is worng.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_LS, 21, 0));

                if (default_hair > 9)
                    throw new exception("[packet_func_ls::packet03E][Error] default_hair: " + (default_hair) + " is wrong. character: " + (_typeid), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_SV, 22, 0));

                if (default_shirts != 0)
                    throw new exception("[packet_func_ls::packet03E][Error] default_shirts: " + (default_shirts) + " is wrong. character: " + (_typeid), ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PACKET_FUNC_LS, 23, 0));

                CharacterInfoEx ci = new CharacterInfoEx
                {
                    id = -1,
                    _typeid = _typeid,
                    default_hair = default_hair,
                    default_shirts = default_shirts
                };

                // Default Parts
                ci.initComboDef();

                _session.m_pi.uid = CommandDB.AddFirstSet(_session.m_pi.uid);
                // Info Character Add com o Id gerado no banco de dados
                ci = CommandDB.AddCharacter(_session.m_pi.uid, ci, 0, 1);

                // Update Character Equipado no banco de dados
                CommandDB.UpdateCharacterEquiped(_session.m_pi.uid, ci.id);
                //atualiza aqui logo...
                CommandDB.UpdateFirstLogin(_session.m_pi.uid);
                // Success Login
                SUCCESS_LOGIN("packet03E", param, _session);
            }
            catch (exception e)
            { 
                _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet03E][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static int packet00B(object param, ParamDispatch pd)
        {
 
            try
            {
                ls.getInstance().requestTryReLogin(getPlayer(pd._session), pd._packet);
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet00B][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) != (uint)STDA_ERROR_TYPE.LOGIN_SERVER)
                    throw;
            }

            return 0;
        }

        public static int packet_sv003(object param, ParamDispatch pd)
        { 
            return 0;
        }

        public static int packet_sv006(object param, ParamDispatch pd)
        {


            _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet_sv006][Log] Time: " + ((Environment.TickCount - getPlayer(pd._session).m_time_start) / (double)10000), type_msg.CL_ONLY_FILE_TIME_LOG));

            _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet_sv006][Log] Send SUCCESS LOGIN Time: " + ((Environment.TickCount - getPlayer(pd._session).m_tick_bot) / (double)10000), type_msg.CL_ONLY_FILE_TIME_LOG));

            return 0;
        }

        public static int packet_svFazNada(object param, ParamDispatch pd)
        { 
            return 0;
        }

        public static int packet_svDisconectPlayerBroadcast(object param, ParamDispatch pd)
        {
            pd._session.m_client.Shutdown(System.Net.Sockets.SocketShutdown.Receive);
            //ls.getInstance().DisconnectSession(pd._session);
            return 0;
        }

        public static int packet_as001(object param, ParamDispatch pd)
        {
            try
            {

                // Log Teste
                _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet_as001][Log] Teste, so para deixar aqui, quando for usar um dia.", type_msg.CL_FILE_LOG_AND_CONSOLE));

            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[packet_func_ls::packet_as001][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return 0;
        }

        public static PangyaBinaryWriter pacoteFB(Player _session, byte option = 0, int sub_opt = 0)
        {
            var subID = (SubLoginCode)option;
            var p = new PangyaBinaryWriter();

            p.init_plain(0xFB); 
            p.WriteByte(option);  // OPTION 1 SENHA OU ID ERRADO
            switch (option)
            {
                case 0:
                    p.WriteString(_session.m_pi.id);
                    p.WriteTime(DateTime.Now);
                   p.WriteByte(1); // 0 -?IsPcBang
                p.WriteByte(1); // == 1 in PC Room ? HasIpLog
                p.WriteByte(1);//Level?
                p.WriteByte(1);//un
                p.WriteByte(1); //un
                    break; 
					case 0x11:
					 p.WriteByte(sub_opt); //un
					break;
                case 6:

                    var tempo = _session.m_pi.block_flag.m_id_state.block_time / 60 / 60/*Hora*/; // Hora

                    p.WriteInt32(_session.m_pi.block_flag.m_id_state.block_time == -1 || tempo == 0 ? 1/*Menos de uma hora*/ : tempo);   // Block Por Tempo
                    
                    break;

                default:
                    break;
            }
            return p;
        }

        public static PangyaBinaryWriter pacoteFC(List<ServerInfo> v_element)
        {

            var p = new PangyaBinaryWriter();
            p.init_plain(0xFC);

            p.WriteByte((byte)(v_element.Count & 0xFF)); // 1 Game Server online

            for (int i = 0; i < v_element.Count; i++)
                p.WriteBytes(v_element[i].ToArray());

            return p;
        }


        public static PangyaBinaryWriter pacoteFD(string AuthKeyLogin, int option = 0)
        {

            var p = new PangyaBinaryWriter();
            p.init_plain(0xFD);

            p.WriteInt32(option);

            p.WriteString(AuthKeyLogin);

            return p;
        }
         /// <summary>
        /// Tela de seleção de personagem
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static PangyaBinaryWriter pacote044(PlayerInfo pi)
        { 
            var p = new PangyaBinaryWriter();
            p.init_plain(0x44);
            p.WriteByte(pi.Sex); //tipo, 0 = Personagem masculino, 1= personagem feminino.
            p.WritePStr(pi.id);
            return p;
        }

        public static void succes_login(object _arg, Player _session, int option = 0)
        {
            List<ServerInfo> sis = new List<ServerInfo>(), msns = new List<ServerInfo>();  
            try
            { 
                // RegisterLogin do Player
                CommandDB.RegisterPlayerLogin(_session.m_pi.uid, _session.getIP(), ls.getInstance().getUID());
            }
            catch (exception e)
            {

                _smp.message_pool.getInstance().push(new message("[packet_func_ls::succes_login][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));

                if (ExceptionError.STDA_SOURCE_ERROR_DECODE(e.getCodeError()) == (uint)STDA_ERROR_TYPE.EXEC_QUERY)
                {
                    if (ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 7/*getServerList*/ && ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 9/*MacroUser*/
               && ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 8/*getMsnList*/ && ExceptionError.STDA_ERROR_DECODE(e.getCodeError()) != 5/*AuthKey*/)
                        throw;
                }
                else
                    throw;
            }
             
            if (option == 0)
            {
                session_send(pacoteFB(_session), _session, 1);//no send packet, force disconnect-> FB
            }
            session_send(pacoteFC(sis), _session, 1); 
        }

        public static void session_send(PangyaBinaryWriter p, Player _session, int _debug = 1)
        {
            MAKE_SEND_BUFFER(p.GetBytes, _session);
        }
		
		public static Player getPlayer(PangyaAPI.Network.PangyaSession.Session _session)
		{
			return (Player)_session;
		}
    }
}
