//using System.Collections.Generic;
//using System.Linq;
//using PangyaAPI.Utilities;
//using PangyaAPI.Utilities.Log;

//namespace Pangya_GameServer.Models;

//public class GMInfo
//{
//	public bool visible = true;

//	public bool whisper = true;

//	public bool channel = true;

//	public uint m_uid;

//	private SortedDictionary<uint, bool> map_open;

//	public GMInfo()
//	{
//		m_uid = 0u;
//		visible = false;
//		whisper = true;
//		channel = false;
//		map_open = new SortedDictionary<uint, bool>();
//	}

//	public void clear()
//	{
//		m_uid = 0u;
//		visible = false;
//		whisper = true;
//		channel = false;
//		map_open.Clear();
//	}

//	public void openPlayerWhisper(uint _uid)
//	{
//		if (_uid == 0)
//		{
//			throw new exception("[GMInfo::openPlayerWhisper][Error] GM[UID=" + m_uid + "] tentou adicionar PLAYER[UID=" + _uid + "] a lista de whisper, mas o _uid é invalido. Hacker ou Bug.");
//		}
//		IEnumerable<KeyValuePair<uint, bool>> it = map_open.Where((KeyValuePair<uint, bool> c) => c.Key == _uid);
//		if (it.Any())
//		{
//			map_open[_uid] = true;
//			return;
//		}
//		Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message("[GMInfo::openPlayerWhisper][Warning] GM[UID=" + m_uid + "] tentou add PLAYER[UID=" + _uid + "] a lista de whisper abertos, mas ele ja esta na lista", 0));
//	}

//	public void closePlayerWhisper(uint _uid)
//	{
//		if (_uid == 0)
//		{
//			throw new exception("[GMInfo::openPlayerWhisper][Error] GM[UID=" + m_uid + "] tentou excluir PLAYER[UID=" + _uid + "] da lista de whisper, mas o _uid é invalido. Hacker ou Bug.");
//		}
//		IEnumerable<KeyValuePair<uint, bool>> it = map_open.Where((KeyValuePair<uint, bool> c) => c.Key == _uid);
//		if (it.Any())
//		{
//			map_open.Remove(_uid);
//			return;
//		}
//		Singleton<list_fifo_console_asyc<message>>.getInstance().push(new message("[[GMInfo::openPlayerWhisper][Warning] GM[UID=" + m_uid + "] tentou excluir PLAYER[UID=" + _uid + "] da lista de whisper, mas ele nao esta na lista.", 0));
//	}

//	public bool isOpenPlayerWhisper(uint _uid)
//	{
//		bool ret = false;
//		foreach (KeyValuePair<uint, bool> item in map_open)
//		{
//			if (item.Key == _uid)
//			{
//				ret = true;
//				break;
//			}
//		}
//		return ret;
//	}

//	public void setGMUID(uint _uid)
//	{
//		if (_uid == 0)
//		{
//			throw new exception("[GMInfo::setGMUID][Error] GM[UID=" + m_uid + "] tentou setar o UID do GM para UID[value=" + _uid + "], mas o m_uid é invalido. Hacker ou Bug.");
//		}
//		m_uid = _uid;
//	}
//}
