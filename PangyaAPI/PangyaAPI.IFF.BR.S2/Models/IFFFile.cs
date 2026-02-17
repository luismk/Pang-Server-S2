using PangyaAPI.IFF.BR.S2.Models;
using PangyaAPI.IFF.BR.S2.Models.Data;
using PangyaAPI.IFF.BR.S2.Models.General;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
namespace PangyaAPI.IFF.BR.S2.Models
{
    /// <summary>
    /// new version create By LuisMK D:
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial class IFFFile<T> : List<T>
    {
        /// <summary>
        /// Construtor/Construção IFF File
        /// </summary>
        /// <summary>
        /// Header IFF(cabeçario do IFF, contem Contagem dos itens existentes no *.iff, Index de ligacao, Versão do IFF 
        /// </summary>
        public IFFHeader Header { get; set; } = new IFFHeader();
        /// <summary>
        /// Atualiza o IFF/Update for IFF
        /// </summary>
        public bool Update { get; set; }
        public string IFFName { get; set; }

        public IFFFile()
        {
            Header = new IFFHeader();
            Update = false;
            IFFName = "*.iff";
        }

        /// <summary>
        /// class construtor(classe construtura do IFFList)
        /// </summary>
        /// <param name="path">local onde fica o arquivo/ local</param>
        public IFFFile(string path)
        {
            Header = new IFFHeader();
            Update = false;
            var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(path)));

            if (new string(reader.ReadChars(2)) == "PK")
            {
                throw new NotSupportedException("The given IFF file is a ZIP file, please unpack it before attempting to parse it");
            }    
        }

        public bool CheckVersionIFF()
        {
            if (Header.Version == 11)
            {
                return true;
            }
            else if (Header.Version == 13)
            {
                throw new Exception(
                      $"[IFFFile::Error]: version-incompatible file structure: ({Marshal.SizeOf((T)Activator.CreateInstance(typeof(T)))})");

            }
            else if (Header.Version != 13)
            {
                throw new Exception($"[IFFFile::Error]:" +
                    $"Version Actual: 13 \n " +
                    $"Version File: {Header.Version} \n" +
                    $"Version-incompatible file structure\n");
            }
            else
            {
                throw new Exception($"[IFFFile::Error]: Versao Atual: 13 \n Versao Arquivo: {Header.Version} \nVersao do IFF esta incorreta\n por favor coloque a versão atual");
            }
        }

        public virtual string GetItemName(uint TypeID)
        {
            try
            {
                foreach (var item in this)
                {
                    if (item is IFFCommon)//verifica se IFFCommon
                    {
                        var item2 = item as IFFCommon;
                        if (item2.ID == TypeID)
                        {
                            return item2.Name;
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch { return ""; }
            return "";
        }

        /// <summary>
        ///so obtem se for IFFCommon
        /// </summary>
        /// <param name="TypeID"></param>
        /// <returns>retorna o tipo</returns>
        public T GetItem(uint TypeID)
        {
            // Itera sobre a coleção para encontrar o item correspondente
            foreach (var item in this)
            {
                // Verifica se o item implementa a interface IFFCommon
                if (item is IFFCommon commonItem)
                {
                    // Se a ID do item corresponde ao TypeID, retorna o item
                    if (commonItem.ID == TypeID)
                    {
                        return item;
                    }
                }
                else
                {
                    // Usa reflexão para verificar se o item tem uma propriedade ID
                    var idProperty = item.GetType().GetProperty("ID");
                    if (idProperty != null)
                    {
                        // Obtém o valor da propriedade ID e verifica se corresponde ao TypeID
                        var itemID = (uint)idProperty.GetValue(item);
                        if (itemID == TypeID)
                        {
                            return item;
                        }
                    }
                }
            }

            // Se não encontrou, cria um novo item
            return CreateItem();
        }

        public IFFFile<IFFCommon> GetItem()
        {
            var List = new IFFFile<IFFCommon>();
            List.Header = Header;
            // Itera sobre a coleção para encontrar o item correspondente
            foreach (var item in this)
            {
                // Verifica se o item implementa a interface IFFCommon
                if (item is IFFCommon commonItem)
                {
                    List.Add(commonItem);
                }
                else
                {
                    return null;
                }
            }
            return List;
        }  

        /// <summary>
        ///so obtem se for IFFCommon
        /// </summary>
        /// <param name="TypeID"></param>
        /// <returns>retorna o tipo</returns>
        public T GetItem(uint TypeID, bool get_common = true)
        {
            foreach (var item in this)
            {
                if (item is IFFCommon && get_common)//verifica se IFFCommon
                {
                    var item2 = item as IFFCommon;
                    if (item2.ID == TypeID)
                    {
                        return item;
                    }
                }
                if (item is IFFCommon && !get_common)//verifica se IFFCommon
                {
                    var item2 = item as IFFCommon;
                    if (item2.ID == TypeID)
                    {
                        return item;
                    }
                }
                else
                {
                    return CreateItem();
                }
            }
            return CreateItem();
        }
         
        public IFFCommon GetItemCommon(uint TypeID)
        {
            var check = this.Any(c => (c as IFFCommon).ID == TypeID);
            if (check)
                return this.First(c => (c as IFFCommon).ID == TypeID) as IFFCommon;

            return default;
        }

        public virtual uint GetPrice(uint TypeID)
        {
            return GetItemCommon(TypeID).Shop.Price;
        }
            

        public virtual bool IsExist(uint TypeID)
        {
            var item = GetItemCommon(TypeID);

            return Convert.ToBoolean(item.Active);
        }

        public virtual bool LoadItem(uint ID, ref T item)
        {
            if (!TryGetValue(ID, out T value))
            {
                return false;
            }
            item = value;
            return true;
        }

        public virtual bool TryGetValue(uint ID, out T value)
        {
            if (GetItem(ID) != null)
            {
                value = GetItem(ID);
                return true;
            }
            value = CreateItem();
            return false;
        }

        //adiciona e atualiza o cabecario do iff
        public new void Add(T item)
        {
            try
            {
                base.Add(item);
                if (Count > Header.Count)
                {
                    Header.Count = (short)Count;
                }
                Update = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public new void Remove(T item)
        {
            try
            {
                base.Remove(item);
                if (Header.Count > Count)
                {
                    Header.Count = (short)Count;
                }
                Update = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public new void AddRange(IEnumerable<T> item)
        {
            base.AddRange(item);
            if (Count > Header.Count)
                Update = true;
            Header.Count = (short)Count;//sempre atualiza
        }

        public bool CheckItemSize(long recordLength)
        {
            long item_size = Marshal.SizeOf(CreateItem());
            if (recordLength != item_size)
            {
                throw new Exception($"[IFFFile::CheckItemSize][{CreateItem().GetType().Name}.iff]{item_size} != {recordLength}");
            }
            return true;
        }

        /// <summary>
        /// parses the *.iff file, if all goes well it should read all data present
        /// </summary>
        /// <param name="data">contains all Information about the *.iff file, size, item count, version, link id</param>
        /// <exception cref="Exception">if I get exception, I must have done something wrong, correct me please?</exception>
        public virtual void Load(byte[] data)
        {
            PangyaBinaryReader Reader = null;

            try
            {
                var reader = new BinaryReader(new MemoryStream(data));

                if (new string(reader.ReadChars(2)) == "PK")
                {
                    throw new NotSupportedException("The given IFF file is a ZIP file, please unpack it before attempting to parse it");
                }
                Reader = new PangyaBinaryReader(new MemoryStream(data));
                Header = Reader.Read<IFFHeader>();
                CheckVersionIFF();
                var real_size = (Reader.BaseStream.Length - 8L) / Header.Count;
                CheckItemSize(real_size);
                //reader object and convert is class IFF
                var item = Reader.ReadStruct<T>(Header.Count);
                //add item in List<T>
                if (item[0] is IFFCommon)
                    AddRange(item.OrderBy(c => (c as IFFCommon).ID));
                else
                    AddRange(item);
            }
            catch (Exception e)
            {
                //show log error :(
                throw e;
            }
            finally
            {
                //is dispose memory :D
                Reader.Dispose();
            }
        }

        ///// <summary>
        ///// parses the *.iff file, if all goes well it should read all data present
        ///// </summary>
        ///// <param name="data">contains all Information about the *.iff file, size, item count, version, link id</param>
        ///// <exception cref="Exception">if I get exception, I must have done something wrong, correct me please?</exception>
        public virtual void LoadOtherVersion(byte[] data)
        {
            PangyaBinaryReader Reader = null;

            try
            {
                var reader = new BinaryReader(new MemoryStream(data));

                if (new string(reader.ReadChars(2)) == "PK")
                {
                    throw new NotSupportedException("The given IFF file is a ZIP file, please unpack it before attempting to parse it");
                }
                Reader = new PangyaBinaryReader(new MemoryStream(data));
                Header = Reader.Read<IFFHeader>();
                CheckVersionIFF();
                var real_size = (Reader.BaseStream.Length - 8L) / Header.Count;
                CheckItemSize(real_size);
                //reader object and convert is class IFF
                var item = Reader.ReadStruct<T>(Header.Count);
                //add item in List<T>
                if (item[0] is IFFCommon)
                    AddRange(item.OrderBy(c => (c as IFFCommon).ID));
                else
                    AddRange(item);
            }
            catch (Exception e)
            {
                //show log error :(
                throw e;
            }
            finally
            {
                //is dispose memory :D
                Reader.Dispose();
            }
        }

        ///// <summary>
        ///// parses the *.iff file, if all goes well it should read all data present
        ///// </summary>
        ///// <param name="data">contains all Information about the *.iff file, size, item count, version, link id</param>
        ///// <exception cref="Exception">if I get exception, I must have done something wrong, correct me please?</exception>
        public virtual void Load(Stream data)
        {
            PangyaBinaryReader Reader = null;

            try
            {
                var reader = new BinaryReader(data);

                if (new string(reader.ReadChars(2)) == "PK")
                {
                    throw new NotSupportedException("The given IFF file is a ZIP file, please unpack it before attempting to parse it");
                }

                Reader = new PangyaBinaryReader(new MemoryStream());
                Header = Reader.Read<IFFHeader>();
                CheckVersionIFF();
                var real_size = (Reader.BaseStream.Length - 8L) / Header.Count;
                CheckItemSize(real_size);
                //reader object and convert is class IFF
                var item = Reader.ReadStruct<T>(Header.Count).ToArray();
                //add item in List<T>
                AddRange(item);
            }
            catch (Exception e)
            {
                //show log error :(
                throw e;
            }
            finally
            {
                //is dispose memory :D
                Reader.Dispose();
            }
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        protected virtual T CreateItem()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        public virtual int GetSize()
        {
            return Marshal.SizeOf(CreateItem());
        }

        public void SetIffName(string iff)
        {
            IFFName = iff;
        }   

        ~IFFFile()
        {
        }


    }
}
