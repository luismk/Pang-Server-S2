using PangyaAPI.Utilities.Log;
using System.Text;
namespace Pangya_LoginServer
{
    public class LoginServer
    {
        static void Main(string[] args)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
             
            var sjis = Encoding.GetEncoding("Shift_JIS");

            Console.InputEncoding = sjis;
            Console.OutputEncoding = sjis; 
            try
            { 
                sls.ls.getInstance().Start();
                for (; ; )
                {
                    var comando = Console.ReadLine().Split([' '], 2);
                    if (sls.ls.getInstance().CheckCommand(new Queue<string>(comando)))
                        _smp.message_pool.getInstance().push(new message("[LoginServer::CheckCommand][Log] Command executed.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                    else
                        _smp.message_pool.getInstance().push(new message("[LoginServer::CheckCommand][Log] Command no executed.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
			catch (Exception ex)
            { 
                _smp.message_pool.getInstance().push(new message("[LoginServer::Main][Error] " + ex.ToString(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            } 
        }
    }
}
