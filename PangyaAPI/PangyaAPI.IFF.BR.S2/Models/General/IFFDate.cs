using System.Runtime.InteropServices;
using PangyaAPI.Utilities;
namespace PangyaAPI.IFF.BR.S2.Models.General
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
    public class IFFDate
    {
        public IFFDate()
        {
            Start = new SYSTEMTIME();
            End = new SYSTEMTIME();
        }
        //-------------------- TIME IFF--------------\\ 
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
        public SYSTEMTIME Start { get; set; }// 160 start position
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
        public SYSTEMTIME End { get; set; }// 176 start position
                                           //--------------------------------------------------\\
        public bool getActive()
        {
            return Start.Year > 0;
        }
        public void Clear()
        {
            Start = new SYSTEMTIME();
            End = new SYSTEMTIME();
        }
    }
}
