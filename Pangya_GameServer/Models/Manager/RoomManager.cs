using Pangya_GameServer.Game;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace Pangya_GameServer.Models.Manager
{
    public class RoomManager
    {
        // Member 
        private Dictionary<sbyte, bool> m_map_index = new Dictionary<sbyte, bool>(sbyte.MaxValue);
        private sbyte m_channel_id;
        private readonly object _lock = new object(); // se ainda não tiver 
        sbyte m_next_index;
        protected List<Room> v_rooms = new List<Room>();
        public RoomManager(sbyte _channel_id)
        {
            this.m_channel_id = _channel_id;
            if (m_map_index.Count == 0)
            {
                for (sbyte i = 0; i < sbyte.MaxValue; i++)
                {
                    m_map_index.Add(i, false);
                }
            }
        }

        public void destroy()
        {
            foreach (var el in v_rooms)
            {

                if (el != null)
                {

                    // Sala está destruindo
                    el.setDestroying();

                    // Libera a sala se ela estiver bloqueada
                    el.unlock();
                }
            }

            v_rooms.Clear();
            m_channel_id = -1;
        }

        public Room makeRoom(sbyte _channel_owner,
            RoomInfoEx _ri,
            Player _session,
            int _option = 0)
        {
            Room r = null;

            try
            {

                if (_session != null && _session.m_pi.mi.sala_numero != -1)
                {
                    throw new exception("[RoomManager::makeRoom][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] sala[NUMERO=" + Convert.ToString(_session.m_pi.mi.sala_numero) + "], ja esta em outra sala, nao pode criar outra. Hacker ou Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                        120, 0));
                }

                _ri.numero = getNewIndex();
                _ri.roomId = Guid.NewGuid();
                if (_option == 0 && _session != null)
                {
                    _ri.master = (int)_session.m_pi.uid;
                }
                else if (_option == 1) // Room Sem Master Grand Prix ou Grand Zodiac Event Time
                {
                    _ri.master = -2;
                }
                else // Room sem master
                {
                    _ri.master = -1;
                }

                r = new Room(_channel_owner, _ri);

                if (r == null)
                {
                    throw new exception("[RoomManager::makeRoom][Error] PLAYER[UID=" + Convert.ToString(_session.m_pi.uid) + "] tentou criar a sala[TIPO=" + Convert.ToString((ushort)_ri.tipo) + "], mas nao conseguiu criar o objeto da classe room. Bug.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                        130, 0));
                }

                // Verifica se é um room válida e bloquea ela 
                r.trylock();

                if (_session != null)
                    r.enter(_session);

                r.unlock();
            }
            catch (exception e)
            {
                if (r != null)
                {

                    // Destruindo a sala, não conseguiu
                    r.setDestroying();

                    // Desbloqueia para
                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.ROOM, 150))
                    {
                        r.unlock();
                    }

                    // Deletando o Objeto
                    r = null;

                    // Limpa o ponteiro
                    r = null;
                }

                _smp.message_pool.getInstance().push(new message("[RoomManager::makeRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return r;
        }


        public void addRoom(Room r)
        {
            // Adiciona a sala no Vector
            v_rooms.Add(r);

            // Log
            _smp.message_pool.getInstance().push(new message("[RoomManager::addRoom][Sucess] Channel[ID=" + Convert.ToString((ushort)m_channel_id) + "] Maked Room[TIPO=" + Convert.ToString((ushort)r.getInfo().tipo) + ", NUMERO=" + Convert.ToString(r.getNumero()) + ", MASTER=" + Convert.ToString((int)r.getMaster()) + ", NOME=" + r.getInfo().nome + ", SENHA=" + r.getInfo().senha + "]", type_msg.CL_FILE_LOG_AND_CONSOLE));
        }

        public void destroyRoom(Room _room)
        {

            if (_room == null)
            {
                throw new exception("[RoomManager::destroyRoom][Error] _room is nullptr.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                    4, 0));
            }

            int index = findIndexRoom(_room);

            if (index == -1)
            {
                throw new exception("[RoomManager::destroyRoom][Error] room nao existe na lista de salas.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                    5, 0));
            }

            try
            {
                _smp.message_pool.getInstance().push(new message($"[RoomManager::destroyRoom][Log] ROOM[ID={(_room?.getNumero())}, NAME={(_room?.getInfo().nome)}, MASTER={(_room?.getInfo().max_player)}].", type_msg.CL_ONLY_FILE_LOG));

                // Sala vai ser deletada
                _room.setDestroying();

                _room.unlock();

                //limpa tudo
                _room = null; 

                clearIndex((sbyte)index);

                v_rooms.RemoveAt(index);
            }
            catch (exception e)
            {
                if (_room != null)
                {

                    _room.setDestroying();

                    _room.unlock();

                    _room = null; 
                }

                _smp.message_pool.getInstance().push(new message("[RoomManager::destroy][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        } 

        public int getCount()
        {

            try
            {
                return v_rooms.Count;
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[RoomManager::findRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_ONLY_CONSOLE));
            }
            return 0;
        }

        public Room findRoom(sbyte _numero)
        {

            if (_numero == -1)
            {
                return null;
            }

            Room r = null;

            try
            {

                for (var i = 0; i < v_rooms.Count; ++i)
                {
                    if (v_rooms[i].getNumero() == _numero)
                    {
                        r = v_rooms[i];
                        break;
                    }
                } 
            }
            catch (exception e)
            {

                // Libera Crictical Session do Room Manager


                if (r != null)
                {

                    if (!ExceptionError.STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(e.getCodeError(),
                        STDA_ERROR_TYPE.ROOM, 150))
                    {
                        r.unlock();
                    }

                    r = null;
                }
                _smp.message_pool.getInstance().push(new message("[RoomManager::findRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_ONLY_CONSOLE));

            }

            return r;
        }

       
        public void WAIT_ROOM_UNLOCK(Room _r)
        {

            if (_r == null)
            {
                return;
            }
            try
            { 
                _r.trylock();
            }
            catch (exception e)
            {
            }
        }
         
        // Opt sem sala practice, se não todas as salas
        public List<RoomInfoEx> getRoomsInfo(bool _without_practice_room = true)
        {

            List<RoomInfoEx> v_ri = new List<RoomInfoEx>();

            for (var i = 0; i < v_rooms.Count; ++i)
            {
                if (v_rooms[i] != null)
                {
                    v_ri.Add((RoomInfoEx)v_rooms[i].getInfo());
                }
            }
            return v_ri;
        } 

        // Unlock Room
        public void unlockRoom(Room _r)
        {
            // _r is invalid
            if (_r == null)
                return;

            try
            {
                foreach (var el in v_rooms)
                {

                    if (el != null && el == _r)
                    {

                        // Libera a sala
                        el.unlock();

                        // Acorda as outras threads que estão esperando                 
                        break;
                    }
                }
            }
            catch (exception e)
            {
                _smp.message_pool.getInstance().push(new message("[RoomManager::unlockRoom][ErrorSystem] " + e.getFullMessageError(), type_msg.CL_FILE_LOG_AND_CONSOLE));
            }
        }

        protected int findIndexRoom(Room _room)
        {

            if (_room == null)
            {
                throw new exception("[RoomManager::findIndexRoom][Error] _room is nullptr.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                    4, 0));
            }

            int index = ~0;

            for (var i = 0; i < v_rooms.Count; ++i)
            {
                if (v_rooms[i] == _room)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private sbyte getNewIndex()
        {
            sbyte index = 0;

            lock (_lock) // substitui o CriticalSection
            {
                for (sbyte i = 0; i < sbyte.MaxValue; ++i)
                {
                    sbyte candidate_index = (sbyte)((m_next_index + i) % sbyte.MaxValue);

                    if (!m_map_index[candidate_index])
                    {
                        index = candidate_index;
                        m_map_index[index] = true; // marca como ocupado
                        m_next_index = (sbyte)((index + 1) % sbyte.MaxValue);
                        break;
                    }
                }

                if (m_next_index >= ushort.MaxValue)
                    m_next_index = 0;
            }

            return index;
        }

        private void clearIndex(sbyte _index)
        {

            if (_index >= sbyte.MaxValue)
            {
                throw new exception("[RoomManager::clearIndex][Error] _index maior que o limite do mapa de indexes.", ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.ROOM_MANAGER,
                    3, 0));
            }

            m_map_index[_index] = false; // Livre 
        }
    }
}
