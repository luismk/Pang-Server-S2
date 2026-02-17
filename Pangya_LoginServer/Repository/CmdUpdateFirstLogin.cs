using Pangya_LoginServer.Models;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using System;

namespace Pangya_LoginServer.Repository
{
    public class CmdUpdateFirstLogin : Pangya_DB
    {
        public CmdUpdateFirstLogin()
        {
            this.m_uid = 0;
            this.m_check = false;
        }

        public CmdUpdateFirstLogin(uint _uid)
        {
            this.m_uid = _uid;
            this.m_check = false;
        }

        public void Dispose()
        {
        }

        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }

        public bool getLastCheck()
        {
            return m_check;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
        }

        protected override Response prepareConsulta()
        {

            m_check = false;

            var r = consulta(m_szConsulta + Convert.ToString(m_uid));

            checkResponse(r, "nao conseguiu setar o first login do player: " + Convert.ToString(m_uid));

            return r;
        }

        private uint m_uid = new uint();
        private bool m_check;

        private const string m_szConsulta = "UPDATE pangya.account set FIRST_LOGIN = 1 WHERE uid = ";
    }
}
