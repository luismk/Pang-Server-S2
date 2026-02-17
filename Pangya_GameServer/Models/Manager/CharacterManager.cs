using System.Collections.Generic;
using System.Linq;
using PangyaAPI.Network.Models;

namespace Pangya_GameServer.Game.Manager
{
    public class CharacterManager : Dictionary<int/*ID*/, CharacterInfoEx>
    {
        public CharacterManager()
        {

        }

        public CharacterManager(Dictionary<int/*ID*/, CharacterInfoEx> keys)
        {
            // this.(keys);    add array 
        }

        public CharacterInfoEx findCharacterById(int _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id);
        }

        public CharacterInfoEx findCharacterByTypeid(uint _typeid)
        {
            return this.Values.FirstOrDefault(c => c._typeid == _typeid);
        }

        public CharacterInfoEx findCharacterByTypeidAndId(uint _typeid, int _id)
        {
            return this.Values.FirstOrDefault(c => c.id == _id && c._typeid == _typeid);
        }
    }
}
