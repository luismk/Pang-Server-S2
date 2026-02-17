using System.Runtime.InteropServices;

namespace PangyaAPI.IFF.BR.S2.Models.General
{
    /// <summary>
    /// Have Struct for IFF Header/ Contem a estrutura do IFF Cabecario
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
    public class IFFHeader
    {
        /// <summary>
        /// size file data
        /// </summary>
        public short Count { get; set; }

        /// <summary>
        /// Index determining relation to other IFF files
        /// </summary>
        public short BindingID { get; set; }

        /// <summary>
        /// Version of this IFF file
        /// </summary>
        public uint Version { get; set; }    
    }
}
