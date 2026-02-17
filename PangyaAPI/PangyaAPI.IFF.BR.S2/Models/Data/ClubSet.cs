using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;

namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    #region Struct ClubSet.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ClubSet : IFFCommon
    {


        public ClubSet()
        {
            Stats = new IFFStats();
            SlotStats = new IFFSlotStats();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
        public class SubClubs
        {
            public uint Wood { get; set; }
            public uint Iron { get; set; }
            public uint Wedge { get; set; }
            public uint Putter { get; set; }
        }

        [field: MarshalAs(UnmanagedType.Struct)]
        public SubClubs Clubs { get; set; }
        [field: MarshalAs(UnmanagedType.Struct)]
        public IFFStats Stats { get; set; }

        [field: MarshalAs(UnmanagedType.Struct)]
        public IFFSlotStats SlotStats { get; set; }
        public int tipo;                           // -1 não pode up rank e nem level, 0 pode tudo
        public uint ulUnknown;     // Pode ser do WorkShop, mas ainda não sei 
        public uint text_pangya { get; set; }
    }
    #endregion

}
