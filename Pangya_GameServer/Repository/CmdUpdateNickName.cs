using System;
using PangyaAPI.SQL;
namespace Pangya_GameServer.Repository
{
    public class CmdUpdateNickName : Pangya_DB
    { 
        public CmdUpdateNickName(uint _uid, string _new_nick)
        {
            this.m_uid = _uid;
            m_nick = _new_nick;
        }
         
        public uint getUID()
        {
            return m_uid;
        }

        public void setUID(uint _uid)
        {
            m_uid = _uid;
        }   
        protected override void lineResult(ctx_res _result, uint _index_result)
        {

            // N�o usa por que � um UPDATE
            return;
        }

        protected override Response prepareConsulta()
        {

            var r = _update($"update pangya.account set {makeEscapeKeyword("NICK")} = {makeText(m_nick)} where UID = {m_uid}");

            checkResponse(r, "nao conseguiu atualizar do player: " + Convert.ToString(m_uid));

            return r;
        }

        private uint m_uid = new uint();
        private string m_nick = ""; 
         
    }
}
