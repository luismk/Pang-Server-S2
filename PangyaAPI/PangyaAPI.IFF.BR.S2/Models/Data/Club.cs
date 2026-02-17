using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;

namespace PangyaAPI.IFF.BR.S2.Models.Data
{

    #region Struct Club.iff

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Club : IFFCommon
    {

        public Club()
        { }

        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        public ushort ClubType { get; set; }

        [field: MarshalAs(UnmanagedType.Struct)]
        public IFFStats Stats { get; set; }
    }
    #endregion

}
