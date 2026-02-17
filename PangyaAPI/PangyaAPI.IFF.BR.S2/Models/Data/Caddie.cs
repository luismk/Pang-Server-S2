using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;

namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    #region Struct Caddie.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Caddie : IFFCommon
    {
        public uint valor_mensal { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 10)]
        public IFFStats Stats { get; set; }
        public ushort Point { get; set; }


        public Caddie()
        { }


    }
    #endregion
}
