using System;
using Pangya_GameServer.Models;
using PangyaAPI.SQL;
namespace Pangya_GameServer.Repository
{
    public class CmdAccountInfo : Pangya_DB
    {
        string m_id = "";
        uint m_uid = 0;
        public CmdAccountInfo(string _id)
        {
            m_id = _id; 
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(2);
            try
            {
                // Aqui faz as coisas
                string id = "";
                if (is_valid_c_string(_result.data[0].ToString()))
                    id = _result.data[0].ToString();

                m_uid = uint.Parse(_result.data[1].ToString());

                if (m_id != id)
                    throw new Exception("[CmdAccountInfo::lineResult][Error] UID do player info nao e igual ao requisitado. ID Req: " + (m_id) + " != " + (id));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {
            var r = consulta($"select ID, UID from pangya.account where ID = {makeText(m_id)}");
            checkResponse(r, "nao conseguiu pegar o info do player: " + (m_uid));
            return r;
        }


        public uint getUID()
        {
            return m_uid;
        }

    }
}
