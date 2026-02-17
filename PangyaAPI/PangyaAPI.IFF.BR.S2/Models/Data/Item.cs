using PangyaAPI.IFF.BR.S2.Models.General;
using System.Runtime.InteropServices;

namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    #region Struct Item.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Item : IFFCommon
    {
        public ushort ItemType { get; set; }

        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 10)]
        public IFFStats Stats { get; set; } //no shop é quantidade
        public ushort Price1Day { get => Stats.Power; set => Stats.Power = value; }
        public ushort Price7Day { get => Stats.Control; set => Stats.Control = value; }
        public ushort Price15Day { get => Stats.Impact; set => Stats.Impact = value; }
        public ushort Price30Day { get => Stats.Spin; set => Stats.Spin = value; }
        public ushort Price365Day { get => Stats.Curve; set => Stats.Curve = value; }

        public Item()
        {
        }
    }
    #endregion
}