using PangyaAPI.IFF.BR.S2.Models.Flags;
using PangyaAPI.IFF.BR.S2.Models.General;
using System;
using System.Runtime.InteropServices;
using System.Text;


namespace PangyaAPI.IFF.BR.S2.Models.Data
{
    #region Struct Part.iff
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Part : IFFCommon
    {

        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string MPet { get; set; }
        public PART_TYPE type_item { get; set; }// o tipo do item, 0, 2 normal, 8 e 9 UCC, 5 acho que é base ou commom Item
        [field: MarshalAs(UnmanagedType.Struct)]
        public u_part_type position_mask { get; set; }  //aqui saõ slot das roupa
        [field: MarshalAs(UnmanagedType.Struct)]
        public u_part_type HideMask { get; set; } //aqui são slot das roupas
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture1 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture2 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture3 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture4 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture5 { get; set; }
        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Texture6 { get; set; }
        [field: MarshalAs(UnmanagedType.Struct, SizeConst = 10)]
        public IFFStats Stats { get; set; }
        [field: MarshalAs(UnmanagedType.Struct)]
        public IFFSlotStats SlotStats { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        byte[] addWithInBytes { get; set; }//8 start position
        public string addWith//correcao para n�o causar conflito ao escrever
        {
            get => Encoding.GetEncoding("Shift_JIS").GetString(addWithInBytes != null ? addWithInBytes : new byte[40]).Replace("\0", "");
            set => addWithInBytes = Encoding.GetEncoding("Shift_JIS").GetBytes(value.PadRight(40, '\0'));
        }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] SubPart { get; set; }

        public uint EquipmentCategory { get => Convert.ToUInt32(type_item); set => type_item = (PART_TYPE)value; }
        public uint CharacterType => (uint)((ID & 0x3fc0000) / Math.Pow(2.0, 18.0));
        public ushort Character_Raw => (ushort)(((double)(ID & 0x3fc0000)) / Math.Pow(2.0, 18.0));

        public string getCharacterNome()
        {
            var Character = (CharacterType)CharacterType;

            return Character.ToString();
        }

        public int getCharacter(bool _char = false)
        {
            if (_char == false)
            {
                return (int)CharacterType;
            }
            switch (CharacterType)
            {
                case 0:
                    return 1; //nuri classic
                case 1:
                    return 2;
                case 2:
                    return 3;
                case 3:
                    return 4;
                case 4:
                    return 5;
                case 5:
                    return 6;
                case 6:
                    return 7;
                case 7:
                    return 8;
                case 8:
                    return 9;
                case 9:
                    return 10;//nell
                case 10:   //Spika 
                    return 11;
                case 11:   //Nuri Renew
                    return 12;
                case 12:  //Hana Renew
                    return 13;
                case 13: //Azer Renew
                    return 0;// 13 azer Renew, porem tem que retornar 0;
                case 14: //CC Renew
                    return 14;
                default:
                    return 0;
            }
        }
        public object newTypeid(object charSerial, object Pos, object Group, object Type, object serial)
        {
            int num = 0;
            try
            {
                num = (int)(2.0 * Math.Pow(2.0, 26.0) +
                            (double)charSerial * Math.Pow(2.0, 18.0) +
                            (double)Pos * Math.Pow(2.0, 13.0) +
                            (double)Group * Math.Pow(2.0, 11.0) +
                            (double)Type * Math.Pow(2.0, 9.0) +
                            (double)serial);
            }
            catch (Exception)
            {
                num = 0;
            }
            return num;
        }



        public void Copy(Part part)
        {
            MPet = part.MPet;
            type_item = part.type_item; // Aqui você pode definir o valor padrão desejado
            position_mask = part.position_mask;
            HideMask = part.HideMask;
            Texture1 = part.Texture1;
            Texture2 = part.Texture2;
            Texture3 = part.Texture3;
            Texture4 = part.Texture4;
            Texture5 = part.Texture5;
            Texture6 = part.Texture6;
            Stats = part.Stats; // Supondo que IFFStats tenha um construtor padrão
            SlotStats = part.SlotStats; // Supondo que IFFSlotStats tenha um construtor padrão
            addWithInBytes = part.addWithInBytes;
            SubPart = part.SubPart;
        }

        public bool IsUCC()
        {
            return type_item == PART_TYPE.UCC_DRAW_ONLY || type_item == PART_TYPE.UCC_COPY_ONLY;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class u_part_type
    {
        public uint ul_part_type;
        public bool getSlot(int index)
        {
            if (index < 0 || index > 23)
                return false;


            return (ul_part_type & (1 << index)) != 0;
        }
    }

    #endregion
}
