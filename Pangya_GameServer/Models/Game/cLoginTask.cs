using Pangya_GameServer.Game.Manager;
using Pangya_GameServer.PacketFunc;
using Pangya_GameServer.PangyaEnums;
using Pangya_GameServer.Repository;
using PangyaAPI.IFF.BR.S2.Extensions;
using PangyaAPI.Network.Repository;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using System;
using System.Xml.Linq;

namespace Pangya_GameServer.Models.Game
{
    public class LoginTask : IDisposable
    {
        private Player m_session;
        private uint m_count;
        private bool m_finish;
        private bool disposedValue;

        public LoginTask(Player session)
        {
            m_session = session;
            m_count = 0;
            m_finish = false;
        }

        public void exec()
        {
            snmdb.NormalManagerDB.getInstance().add(2, new CmdUserEquip(m_session.m_pi.uid), LoginManager.SQLDBResponse, this);
        }

        public Player getSession { get => m_session; set => m_session = value; }
        public void finishSessionInvalid() => m_finish = true;

        public void sendFailLogin()
        {
            try
            {
                var p = new PangyaBinaryWriter(0x44);
                p.WriteByte(eLoginAck.ACK_LOGIN_FAIL);
                p.WriteInt32(1);
                packet_func.session_send(p, m_session, 1);
            }
            catch (Exception e)
            {
                _smp.message_pool.getInstance().push(new message("[LoginTask::sendFailLogin][ErrorSystem] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
            m_finish = true;
        }

        public void sendCompleteData()
        {
            if (!m_session.isConnected() || !m_session.isCreated() || !m_session.getState())
            {
                _smp.message_pool.getInstance().push(new message("[LoginTask::sendCompleteData][Error] session is invalid.", type_msg.CL_FILE_LOG_AND_CONSOLE));
                finishSessionInvalid();
                return;
            }

            try
            {
                var p = new PangyaBinaryWriter();
                var pi = m_session.m_pi;

                //// Check All Character All Item Equiped is on Warehouse Item of Player
                foreach (var el in pi.mp_ce) 
                    m_session.checkCharacterAllItemEquiped(el.Value);

                // Check All Item Equiped
                m_session.checkAllItemEquiped(pi.ue);

                // Envia todos pacotes aqui, alguns envia antes, por que agora estou usando o jeito o pangya original   
                packet_func.session_send(packet_func.pacote06(sgs.gs.getInstance().getInfo(), 0, pi), m_session);
                // characters
                packet_func.session_send(packet_func.pacote45(pi.mp_ce), m_session);//0x45
                //caddies   
                packet_func.session_send(packet_func.pacote46(pi.mp_ci), m_session);//0x46
                // equip selected                  
                packet_func.session_send(packet_func.pacote47(pi.ue), m_session);//0x47u
                 
                packet_func.session_send(pi.mp_wi.Build(), m_session);//0x48
                //TrofelEspecial


               packet_func.session_send(packet_func.pacote97(pi.v_tsi_current_season, 1/*season atual*/), m_session);
                //envia a lista porra...
                sgs.gs.getInstance().sendChannelListToSession(m_session); 
            }
            catch (Exception e)
            {
                _smp.message_pool.getInstance().push(new message("[LoginTask::sendCompleteData][ErrorSystem] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            m_finish = true;
        }

        public void sendReply(int msg_id)
        {
            var p = new PangyaBinaryWriter(0x6);
            p.WriteByte(eLoginAck.ACK_UPDATE_LOGIN_UNIT);
            p.WriteInt32(msg_id);
            packet_func.session_send(p, m_session, 1);
        }

        public uint getCount() => m_count;

        public uint incremenetCount() => ++m_count;

        public bool isFinished() => m_finish;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_session = null;
                    m_count = 0;
                    m_finish = false;
                }

                // TODO: liberar recursos não gerenciados (objetos não gerenciados) e substituir o finalizador
                // TODO: definir campos grandes como nulos
                disposedValue = true;
            }
        }

        // // TODO: substituir o finalizador somente se 'Dispose(bool disposing)' tiver o código para liberar recursos não gerenciados
        ~LoginTask()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
