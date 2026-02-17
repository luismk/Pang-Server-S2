using Pangya_GameServer.Models;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using System.Runtime.InteropServices;
using System.Text;

namespace Pangya_GameServer
{
    public class GameServer
    {
        static void Main()
        {
            // 1. REGISTRE O PROVEDOR PRIMEIRO
            // Certifique-se de ter instalado o pacote: System.Text.Encoding.CodePages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 2. Agora você pode obter a codificação sem o erro de ArgumentException
            var sjis = Encoding.GetEncoding("Shift_JIS");

            Console.InputEncoding = sjis;
            Console.OutputEncoding = sjis;
            try
            {
                sgs.gs.getInstance().Start();

                for (; ; )
                {
                    var input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input)) continue;

                    var comando = new Queue<string>(input.Split(' '));
                    if (sgs.gs.getInstance().CheckCommand(comando))
                    {
                        _smp.message_pool.getInstance().push(new message($"[GameServer::CheckCommand][Log] Command Executed-> {input}", type_msg.CL_ONLY_CONSOLE));
                    }
                }
            }
            catch (Exception e) // Corrigido 'exception' para 'Exception'
            {
                _smp.message_pool.getInstance().push(new message("[GameServer::Main][Error] " + e.Message + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
                throw e;
            }
        }
    }
}