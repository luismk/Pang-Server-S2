using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
namespace PangyaAPI.Network.PangyaPacket
{
    public class packet_func_base
    {
        public static func_arr funcs = new func_arr();      // Cliente
        public static func_arr funcs_sv = new func_arr();   // Server (Retorno)
        public static func_arr funcs_as = new func_arr(); // Auth Server


        public static int MAX_BUFFER_PACKET = 1000;
        public static void MakeBeginPacket(object arg)
        {
            var pd = (ParamDispatch)arg;
            _smp.message_pool.getInstance().push(new message($"Trata pacote {pd._packet.getTipo()}(0x{pd._packet.getTipo():X})", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

         
        public static void MAKE_SEND_BUFFER(byte[] rawPacket, Session _session)
        {

            try
            {
                if (_session.m_client != null && _session.m_client.Connected)
                {

                    _session.requestSendBuffer(rawPacket);

                    if (_session.devolve())
                        _session.Disconnect();
                }
            }
            catch (exception e)
            {

                if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE.SESSION, 6/*n�o pode usa session*/))
                    if (_session.devolve())
                        _session.Disconnect();

                if (ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(), STDA_ERROR_TYPE.SESSION, 2))
                    throw;
            }
        }
    }
}
