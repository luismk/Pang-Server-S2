using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangya_LoginServer.PangyaEnums
{
    public enum PacketIDClient
    {
        /// <summary>
        /// Player digita o usuário e senha e clica em login
        /// </summary>
        CLIENT_LOGIN = 0x01,
        /// <summary>
        /// Player Seleciona um Servidor para entrar
        /// </summary>
        CLIENT_SELECT_GS = 0x03,
        /// <summary>
        /// login com duplicidade 
        /// </summary>
        CLIENT_USER_MSG = 0x04,
        CLIENT_RECONNECT = 0x0B,
        /// <summary>
        /// naosei
        /// </summary>
        CLIENT_NOTHING = 0xFF
    }

    public enum PacketIDServer
    {
        SEND_KEY = 0xFA,//key
        REQUEST_LOGIN = 0xFB,//server
        REQUEST_SERVER_LIST = 0xFC,//server 
        REQUEST_ENTER_SERVER = 0xFD,//server 
        FIRST_LOGIN = 0x44,
        SET_FIRST_LOGIN = 0x3E
    }
}
