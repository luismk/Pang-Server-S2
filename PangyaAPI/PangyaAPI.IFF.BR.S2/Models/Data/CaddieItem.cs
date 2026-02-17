using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;


namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    #region Struct CaddieItem.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CaddieItem : IFFCommon
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string TexTure { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] price { get; set; }

        public uint unit_power_guage_start { get; set; }

        enum CaddieType : byte
        {
            COOKIE,     // CASH
            PANG,       // PANG
            ESPECIAL,   // ACHO, por que não tem nenhum item com esse, não vi pelo menos
            UPGRADE
        }
        public CaddieItem()
        { }
    }
    #endregion

}
