using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;
namespace PangyaAPI.IFF.BR.S2.Models.Data
{

    #region Struct Character.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Character : IFFCommon
    {
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture1 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture2 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture3 { get; set; }


        public ushort Power { get; set; }
        public ushort Control { get; set; }
        public ushort Impact { get; set; }
        public ushort Spin { get; set; }
        public ushort Curve { get; set; }
        //daqui pra abaixo, ajustar
        //14 bytes
        //okay
        public byte NumberParts { get; set; }
        public byte NumberAcessory { get; set; }
        public int Unknown0 { get; set; }//2
        public int Unknown1 { get; set; }//2
        //okay
        public int ClubType { get; set; }//taco?
                                         //okay
        public float ClubScale { get; set; }
        //okay
        public byte PowerSlot { get; set; }
        public byte ControlSlot { get; set; }
        public byte ImpactSlot { get; set; }
        public byte SpinSlot { get; set; }
        public byte CurveSlot { get; set; }
        //sao 33 antes de chegar na camera
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 43)]
        public string Camera { get; set; }
    }
    #endregion
}
