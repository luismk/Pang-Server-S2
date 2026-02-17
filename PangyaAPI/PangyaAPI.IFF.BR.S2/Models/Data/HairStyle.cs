using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;
namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    #region Struct HairStyle.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HairStyle : IFFCommon
    {
        public byte Color { get; set; }
        public byte Character { get; set; }
        public ushort Blank { get; set; }
    }
    #endregion
}
