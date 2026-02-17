using Pangya_GameServer.Models.Game;
using Pangya_GameServer.Repository;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
namespace Pangya_GameServer.Game.Manager
{
    public class LoginManager
    {
        private readonly List<LoginTask> v_task = new List<LoginTask>();

        private Thread m_pThread;

        // Quando true, indica que o thread deve parar
        private volatile bool m_check_task_finish_shutdown; 

        private readonly object m_cs = new object(); // Lock para sincronização

        public LoginManager()
        { 
            m_check_task_finish_shutdown = false;

            StartCheckTaskFinishThread();
        }

        ~LoginManager()
        {
            ShutdownCheckTaskFinishThread();
            clear();
        }

        public LoginTask createTask(Player _session)
        {
            var task = new LoginTask(_session);

            task.exec();

            lock (m_cs)
            {
                v_task.Add(task);
            }

            return task;
        }

        public void deleteTask(LoginTask _task)
        {
            lock (m_cs)
            {
                if (v_task.Remove(_task))
                {
                    _task.Dispose(); // Se LoginTask implementar IDisposable, ou qualquer cleanup necessário
                }
            }
        }

        public static void SQLDBResponse(int _msg_id, Pangya_DB _pangya_db, object _arg)
        {

            if (_arg == null)
            {
                _smp.message_pool.getInstance().push(new message("[LoginSystem.SQLDBResponse][Error] _arg is null na msg_id = " + (_msg_id), type_msg.CL_FILE_LOG_AND_CONSOLE));
                return;
            }
            // if (_arg is LoginTask && (_session = (LoginTask)_arg) != null)

            var task = (LoginTask)_arg;

            try
            {
                // Verifica se a session ainda é valida, essas funções já é thread-safe
                if (task == null || !task.getSession.isConnected())
                {
                    _smp.message_pool.getInstance().push(
                        new message("[LoginManager::SQLDBResponse][Warn] session is invalid, ignorando resposta do pangya_db",
                        type_msg.CL_FILE_LOG_AND_CONSOLE)
                    );
                    return; // sai do método aqui
                }


                // Por Hora só sai, depois faço outro tipo de tratamento se precisar
                if (_pangya_db.getException().getCodeError() != 0)
                    throw new exception(_pangya_db.getException().getFullMessageError());

                switch (_msg_id)
                {
                    case 2: // CmdUserEquipInfo
                        task.getSession.m_pi.ue = ((CmdUserEquip)_pangya_db).getInfo();
                      
                        // Próximas chamadas essenciais
                        snmdb.NormalManagerDB.getInstance().add(7, new CmdUserInfo(task.getSession.m_pi.uid), SQLDBResponse, task);
                        snmdb.NormalManagerDB.getInstance().add(12, new CmdCharacterInfo(task.getSession.m_pi.uid, CmdCharacterInfo.TYPE.ALL), SQLDBResponse, task);
                        snmdb.NormalManagerDB.getInstance().add(13, new CmdCaddieInfo(task.getSession.m_pi.uid, CmdCaddieInfo.TYPE.ALL), SQLDBResponse, task);
                        snmdb.NormalManagerDB.getInstance().add(15, new CmdWarehouseItem(task.getSession.m_pi.uid, CmdWarehouseItem.TYPE.ALL), SQLDBResponse, task);
                        break;

                    case 7: // CmdUserInfo
                        task.getSession.m_pi.ui = ((CmdUserInfo)(_pangya_db)).getInfo();
                        task.getSession.m_pi.ui.Cookies = ((CmdUserInfo)(_pangya_db)).getInfo().Cookies;
                        // Map Statistics (Apenas os que você usa)
                        snmdb.NormalManagerDB.getInstance().add(16, new CmdCookie(task.getSession.m_pi.uid), SQLDBResponse, task);
                        break;

                    case 12: // CmdCharacterInfo
                        task.getSession.m_pi.mp_ce = ((CmdCharacterInfo)(_pangya_db)).getAllInfo();
                        task.getSession.m_pi.ei.char_info = null;
                         
                        if (task.getSession.m_pi.ue.character_id != 0 && task.getSession.m_pi.mp_ce.Count > 0)
                        {
                            var it = task.getSession.m_pi.mp_ce.Where(c => c.Key == task.getSession.m_pi.ue.character_id);
                            if (it.Any()) task.getSession.m_pi.ei.char_info = it.First().Value;
                        }

                        // Lógica de flags/sexo simplificada
                        task.getSession.m_pi.mi.state_flag.sexo = task.getSession.m_pi.mi.sexo == 1;
                        break;

                    case 13: // CmdCaddieInfo
                        task.getSession.m_pi.mp_ci = ((CmdCaddieInfo)(_pangya_db)).getInfo();
                        PlayerManager.checkCaddie(task.getSession);
                        task.getSession.m_pi.ei.cad_info = null;

                        if (task.getSession.m_pi.ue.caddie_id != 0 && task.getSession.m_pi.mp_ci.Count > 0)
                        {
                            var it = task.getSession.m_pi.mp_ci.Where(c => c.Key == task.getSession.m_pi.ue.caddie_id);
                            if (it.Any()) task.getSession.m_pi.ei.cad_info = it.First().Value;
                        }
                        break;

                    case 15: // CmdWareHouseInfo (Item)
                        var cmd = ((CmdWarehouseItem)(_pangya_db));
                        task.getSession.m_pi.mp_wi = cmd.getInfo();
                        task.getSession.m_pi.ToTalClubsetCNT = cmd.getClubsetItemCount();
                        task.getSession.m_pi.ToTalPartsCNT = cmd.getPartsItemCount();

                        PlayerManager.checkWarehouse(task.getSession);

                        // Inicializa Clubset equipado
                        var it_cs = task.getSession.m_pi.findWarehouseItemById(task.getSession.m_pi.ue.clubset_id);
                        if (it_cs != null)
                        {
                            task.getSession.m_pi.ei.clubset = it_cs;
                            task.getSession.m_pi.ue.clubset_typeid = it_cs._typeid;
                            task.getSession.m_pi.ei.csi.setValues(it_cs.id, it_cs._typeid, it_cs.c);
                        }
                        break;
                    case 16: // CmdCookie
                        task.getSession.m_pi.ui.Cookies = (int)((CmdCookie)(_pangya_db)).getCookie();
                        task.getSession.m_pi.cookie = ((CmdCookie)(_pangya_db)).getCookie();
                        snmdb.NormalManagerDB.getInstance().add(26, new CmdMapStatistics(task.getSession.m_pi.uid, CmdMapStatistics.TYPE_SEASON.CURRENT, CmdMapStatistics.TYPE.NORMAL, CmdMapStatistics.TYPE_MODO.M_NORMAL), SQLDBResponse, task);
                        break;
                    case 26: // CmdMapStatistics
                        var v_ms = ((CmdMapStatistics)(_pangya_db)).getMapStatistics();
                        foreach (var i in v_ms) { task.getSession.m_pi.a_ms_normal[i.course] = i; }
                        break;
                }
                // Incrementa o contador
                task. incremenetCount();


                if (task.getCount() == 7)
                  task.sendCompleteData();
 
                if (task.getSession.devolve())
                {
                    _smp.message_pool.getInstance().push(new message("[LoginManager::LoginManager][Test1] ", type_msg.CL_ONLY_CONSOLE));
                    sgs.gs.getInstance().DisconnectSession(task.getSession);

                }

            }
            catch (exception ex)
            {
                _smp.message_pool.getInstance().push(new message(
                    $"[LoginSystem::SQLDBResponse][ErrorSystem] {ex}",
                    type_msg.CL_FILE_LOG_AND_CONSOLE));

                try
                {
                    if (task != null && task.getSession.isConnected())
                        sgs.gs.getInstance().DisconnectSession(task.getSession);
                }
                catch (Exception innerEx)
                {
                    _smp.message_pool.getInstance().push(new message(
                        $"[LoginSystem::SQLDBResponse][ErrorSystem] Falha ao desconectar sessão: {innerEx}",
                        type_msg.CL_FILE_LOG_AND_CONSOLE));
                }
            }
        }

        private void clear()
        {
            lock (m_cs)
            {
                foreach (var task in v_task)
                {
                    task.Dispose(); // Se implementa IDisposable, limpar recursos
                }
                v_task.Clear();
            }
        }

        
        private void StartCheckTaskFinishThread()
        {
            m_pThread = new Thread(CheckTaskFinish)
            {
                IsBackground = true,
                Name = "LoginManager_CheckTaskFinish"
            };
            m_pThread.Start();
        }

        private void CheckTaskFinish()
        {
            while (!m_check_task_finish_shutdown)
            {
                lock (m_cs)
                {
                    for (int i = 0; i < v_task.Count; i++)
                    {
                        if (v_task[i].isFinished())
                        {
                            v_task[i].Dispose();
                            v_task.RemoveAt(i);
                            i--;
                        }
                    }
                }
                Thread.Sleep(1000); // 1 segundo para não consumir CPU excessivamente
            }
            _smp.message_pool.getInstance().push(new message("[LoginManager::checkTaskFinish][Info] saindo de check task finish.", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        private void ShutdownCheckTaskFinishThread()
        {
            m_check_task_finish_shutdown = true;

            _smp.message_pool.getInstance().push(new message("[LoginManager::checkTaskFinish][Info] thread check task finish iniciada com sucesso!", type_msg.CL_FILE_LOG_AND_CONSOLE));

            m_pThread?.Join();
        }
    }
}