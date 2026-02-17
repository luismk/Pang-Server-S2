using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;
namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    #region Struct QuestStuff.iff

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class QuestDrop : IFFCommon
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class CounterItem
        {
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] _typeid = new uint[5];
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] qntd = new int[5];
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class RewardItem
        {
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] _typeid = new uint[3];
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] qntd = new uint[3];
            [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] time = new uint[3]; // !@[ACHO] que é isso aqui também
        }
        [field: MarshalAs(UnmanagedType.Struct)]
        public CounterItem counter_item = new CounterItem();
        [field: MarshalAs(UnmanagedType.Struct)]
        public RewardItem reward_item = new RewardItem();
    }

    #endregion

}
