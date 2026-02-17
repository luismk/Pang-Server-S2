using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;
namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    /// <summary>
    /// Is Struct file Course.iff
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Course : IFFCommon
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Mpet { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Gbin { get; set; }
        public byte Star { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 43)]
        public string XML { get; set; }
        public float RatePang { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] Seq { get; set; } 
    }
}
