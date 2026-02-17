using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;


namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    #region Struct SetItem.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Packege
    {
        public byte Total { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] unk { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] item_typeid { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] item_qntd { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SetItem : IFFCommon
    {

        [field: MarshalAs(UnmanagedType.Struct)]
        public Packege packege { get; set; }
        [field: MarshalAs(UnmanagedType.Struct)]
        public IFFStats Stats { get; set; } // aqui deve ser algum tempo
        public ushort Point { get; set; }
        public uint TypeSet => (uint)((ID & ~0xFC000000) >> 21);
    }
    #endregion     
}
