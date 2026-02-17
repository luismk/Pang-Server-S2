using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using PangyaAPI.IFF.BR.S2.Models;
using PangyaAPI.IFF.BR.S2.Models.Data;
using PangyaAPI.IFF.BR.S2.Models.Flags;
using PangyaAPI.IFF.BR.S2.Models.General;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace PangyaAPI.IFF.BR.S2.Extensions
{
    public class IFFHandle
    {
        public IFFFile<Part> Part { get; set; }
        public IFFFile<Item> Item { get; set; }
        public IFFFile<SetItem> Set_item { get; set; }
        public IFFFile<QuestDrop> _QuestDrop { get; set; }
        public IFFFile<Quest> Quests { get; set; }
        public IFFFile<AuxPart> Aux_part { get; set; }
        public IFFFile<Ball> Ball { get; set; }
        public IFFFile<Caddie> Caddie { get; set; }
        public IFFFile<CaddieItem> Caddie_item { get; set; }
        public IFFFile<Character> Character { get; set; }
        public IFFFile<Club> m_club { get; set; }
        public IFFFile<ClubSet> m_club_set { get; set; }
        public IFFFile<Course> m_course { get; set; }
        public IFFFile<Enchant> m_enchant { get; set; }
        public IFFFile<HairStyle> m_hair_style { get; set; }
        public IFFFile<Match> m_match { get; set; }
        public IFFFile<Skin> m_skin { get; set; }
        public IFFFile<Desc> m_desc { get; set; }
        public IFFFile<Title> _Title { get; set; }

        string PATH_PANGYA_IFF = "data/pangya_brs.iff";
        bool m_loaded;
        ZipFileEx Zip { get; set; }

        public int CHARACTER = (int)IFF_GROUP.CHARACTER;      // 4
        public int PART = (int)IFF_GROUP.PART;        // 8
        public int CLUB = (int)IFF_GROUP.CLUB;        // 12
        public int CLUBSET = (int)IFF_GROUP.CLUBSET;     // 16
        public int BALL = (int)IFF_GROUP.BALL;        // 20
        public int ITEM = (int)IFF_GROUP.ITEM;        // 24
        public int CADDIE = (int)IFF_GROUP.CADDIE;      // 28
        public int CAD_ITEM = (int)IFF_GROUP.CAD_ITEM;    // 32
        public int SET_ITEM = (int)IFF_GROUP.SET_ITEM;    // 36
        public int COURSE = (int)IFF_GROUP.COURSE;      // 40
        public int MATCH = (int)IFF_GROUP.MATCH;       // 44 Trofel
        public int TITLE = (int)IFF_GROUP.TITLE;
        public int SKIN = (int)IFF_GROUP.SKIN;        // 56
        public int HAIR_STYLE = (int)IFF_GROUP.HAIR_STYLE;  // 60 
        public int AUX_PART = (int)IFF_GROUP.AUX_PART;    // 112
        public int QUESTDROP = (int)IFF_GROUP.QUESTDROP; // 116
        public int QUEST = (int)IFF_GROUP.QUEST;  // 120 
        public IFFHandle()
        {
            m_loaded = false;
        }
        ~IFFHandle()
        {
            m_loaded = false;
        }

        private IFFFile<T> MakeUnzipLoad<T>(string iffName) where T : new()
        {
            var mapIFF = new IFFFile<T>();

            try
            {
                if (!File.Exists(PATH_PANGYA_IFF))
                    throw new Exception($"[IFFHandle::MakeUnzipLoad][StError]: Falha ao ler arquivo: {PATH_PANGYA_IFF}");

                if (Zip == null)
                    Zip = new ZipFileEx(PATH_PANGYA_IFF);

                using (var zipArchive = ZipFile.OpenRead(PATH_PANGYA_IFF))
                {
                    mapIFF.Load(Zip.GetEntryBytes(iffName));
                    mapIFF.SetIffName(iffName);
                }
                _smp.message_pool.getInstance().push($"[IFFHandle::MakeUnzipLoad][Log] [Name: {iffName}, Item(s): {mapIFF.Count}]", type_msg.CL_ONLY_CONSOLE);

                return mapIFF;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[IFFHandle::MakeUnzipLoad][StError]: {ex.Message}");
                return default;
            }
        }

        public void initilation()
        {
            try
            {
                if (m_loaded)
                    reset();

                Item = load_item();
                Aux_part = load_aux_part();
                Ball = load_ball();
                Caddie = load_caddie();
                Caddie_item = load_caddie_item();
                Character = load_character();
                m_club = load_club();
                m_club_set = load_club_set();
                m_course = load_course();
                m_enchant = load_enchant();
                m_hair_style = load_hair_style();
                m_match = load_match();
                m_skin = load_skin();
                m_desc = load_desc();
                _Title = load_title();
                Quests = load_quest_item();
                _QuestDrop = load_quest_drop();
                Set_item = load_set_item();
                Part = load_part();
                m_loaded = true;

                _smp.message_pool.getInstance().push("[IFFHandle::Load] Loading with Sucess!", type_msg.CL_ONLY_CONSOLE);

            }
            catch (exception ex)
            {
                _smp.message_pool.getInstance().push(new message($"[IFFHandle::Load][Error]: {ex.getFullMessageError()}", type_msg.CL_FILE_LOG_AND_CONSOLE));
                throw ex;
            }
        }

        private void reset()
        {

            Item.Clear();//_item();
            Aux_part.Clear();//_aux_part();
            Ball.Clear();//_ball();
            Caddie.Clear();//_caddie();
            Caddie_item.Clear();//_caddie_item();       
            Character.Clear();//_character();
            m_club.Clear();//_club();
            m_club_set.Clear();//_club_set();
            m_course.Clear();//_course();
            m_enchant.Clear();//_enchant();
            _Title.Clear();//_furniture();
            m_hair_style.Clear();//_hair_style();
            m_match.Clear();//_match();
            m_skin.Clear();//_skin();
            m_desc.Clear();//_desc();      

            Quests.Clear();//_quest_item();            
            _QuestDrop.Clear();//_quest_stuff();        
            Set_item.Clear();//_set_item(); 
            Part.Clear();//_part();   
        }
        public void reload()
        {
            reset();
            m_loaded = false;
            initilation();
        }

        public void reload(string data)
        {
            if (m_loaded)
                reset();
            PATH_PANGYA_IFF = data;
            m_loaded = false;
            Zip = new ZipFileEx(data);
            initilation();
        }
        private IFFFile<Title> load_title()
        {
            return MakeUnzipLoad<Title>("Title.iff");
        }

        private IFFFile<Quest> load_quest_item()
        {
            return MakeUnzipLoad<Quest>("Quest.iff");
        }

        private IFFFile<QuestDrop> load_quest_drop()
        {
            return MakeUnzipLoad<QuestDrop>("QuestDrop.iff");
        }

        private IFFFile<Item> load_item()
        {
            return MakeUnzipLoad<Item>("Item.iff");
        }

        private IFFFile<SetItem> load_set_item()
        {
            return MakeUnzipLoad<SetItem>("SetItem.iff");
        }

        private IFFFile<Part> load_part()
        {
            return MakeUnzipLoad<Part>("Part.iff");
        }

        private IFFFile<AuxPart> load_aux_part()
        {
            return MakeUnzipLoad<AuxPart>("AuxPart.iff");
        }

        private IFFFile<Ball> load_ball()
        {
            return MakeUnzipLoad<Ball>("Ball.iff");
        }

        private IFFFile<Caddie> load_caddie()
        {
            return MakeUnzipLoad<Caddie>("Caddie.iff");
        }

        private IFFFile<CaddieItem> load_caddie_item()
        {
            return MakeUnzipLoad<CaddieItem>("CaddieItem.iff");
        }

        private IFFFile<Character> load_character()
        {
            return MakeUnzipLoad<Character>("Character.iff");
        }

        private IFFFile<Club> load_club()
        {
            return MakeUnzipLoad<Club>("Club.iff");
        }

        private IFFFile<ClubSet> load_club_set()
        {
            return MakeUnzipLoad<ClubSet>("ClubSet.iff");
        }

        private IFFFile<Course> load_course()
        {
            return MakeUnzipLoad<Course>("Course.iff");
        }


        private IFFFile<Enchant> load_enchant()
        {
            return MakeUnzipLoad<Enchant>("Enchant.iff");
        }

        private IFFFile<HairStyle> load_hair_style()
        {
            return MakeUnzipLoad<HairStyle>("HairStyle.iff");
        }

        private IFFFile<Match> load_match()
        {
            return MakeUnzipLoad<Match>("Match.iff");
        }

        private IFFFile<Skin> load_skin()
        {
            return MakeUnzipLoad<Skin>("Skin.iff");
        }
        private IFFFile<Desc> load_desc()
        {
            return MakeUnzipLoad<Desc>("Desc.iff");
        }

        private T MAKE_FIND_MAP_IFF<T>(IFFFile<T> _iff, uint ID)
        {
            if (!m_loaded)
            {
                _smp.message_pool.getInstance().push(new message("[IFF::Find][Error] IFF not loaded", type_msg.CL_FILE_LOG_AND_CONSOLE));

                return default(T); // se for struct = valor default (0), se for class = null
            }

            try
            {
                return _iff.GetItem(ID);
            }
            catch (Exception e)
            {
                _smp.message_pool.getInstance().push(new message("[IFF::Find][ErrorSystem] " + e.Message, type_msg.CL_FILE_LOG_AND_CONSOLE));
            }

            return default(T); // se for struct = valor default (0), se for class = null
        }


        public Title findTitle(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(_Title, _typeid);
        }

        public AuxPart findAuxPart(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Aux_part, _typeid);
        }

        public Ball findBall(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Ball, _typeid);
        }

        public Caddie findCaddie(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Caddie, _typeid);
        }

        public CaddieItem findCaddieItem(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Caddie_item, _typeid);
        }

        public Character findCharacter(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Character, _typeid);
        }
        public Club findClub(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(m_club, _typeid);
        }

        public ClubSet findClubSet(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(m_club_set, _typeid);
        }

        // Find
        public Part findPart(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Part, _typeid);
        }

        public Item findItem(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Item, _typeid);
        }

        public Quest findQuestItem(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Quests, _typeid);
        }

        public QuestDrop findQuestDrop(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(_QuestDrop, _typeid);
        }

        public SetItem findSetItem(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(Set_item, _typeid);
        }


        public Course findCourse(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(m_course, _typeid);
        }
        public Enchant findEnchant(uint _typeid)
        {
            return m_enchant.FirstOrDefault(_el => _el.TypeID == _typeid);
        }

        public HairStyle findHairStyle(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(m_hair_style, _typeid);
        }

        public Match findMatch(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(m_match, _typeid);
        }

        public Skin findSkin(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(m_skin, _typeid);
        }

        public Desc findDesc(uint _typeid)
        {
            return MAKE_FIND_MAP_IFF(m_desc, _typeid);
        }

        public bool ItemEquipavel(uint _typeid)
        {
            return ((_typeid & 0xFE000000) >> 25 & 3) == 0;
        }

        public bool IsBuyItem(uint _typeid)
        {

            var commom = FindCommonItem(_typeid);

            if (commom != null)
                return (commom.Active && !commom.IsHide);

            return false;
        }


        public T findItem<T>(uint _typeid)
        {
            T commom = default;

            try
            {
                switch ((IFF_GROUP)getItemGroupIdentify(_typeid))
                {
                    case IFF_GROUP.CHARACTER:
                        commom = (T)Activator.CreateInstance(findCharacter(_typeid).GetType());
                        break;
                    case IFF_GROUP.PART:
                        commom = (T)Activator.CreateInstance(findPart(_typeid).GetType());
                        break;
                    case IFF_GROUP.CLUB:
                        commom = (T)Activator.CreateInstance(findClub(_typeid).GetType());
                        break;
                    case IFF_GROUP.CLUBSET:
                        commom = (T)Activator.CreateInstance(findClubSet(_typeid).GetType());
                        break;
                    case IFF_GROUP.BALL:
                        commom = (T)Activator.CreateInstance(findBall(_typeid).GetType());
                        break;
                    case IFF_GROUP.ITEM:
                        commom = (T)Activator.CreateInstance(findItem(_typeid).GetType());
                        break;
                    case IFF_GROUP.CADDIE:
                        commom = (T)Activator.CreateInstance(findCaddie(_typeid).GetType());
                        break;
                    case IFF_GROUP.CAD_ITEM:
                        commom = (T)Activator.CreateInstance(findCaddieItem(_typeid).GetType());
                        break;
                    case IFF_GROUP.SET_ITEM:
                        commom = (T)Activator.CreateInstance(findSetItem(_typeid).GetType());
                        break;
                    case IFF_GROUP.COURSE:
                        commom = (T)Activator.CreateInstance(findCourse(_typeid).GetType());
                        break;
                    case IFF_GROUP.SKIN:
                        commom = (T)Activator.CreateInstance(findSkin(_typeid).GetType());
                        break;
                    case IFF_GROUP.HAIR_STYLE:
                        commom = (T)Activator.CreateInstance(findHairStyle(_typeid).GetType());
                        break;
                    case IFF_GROUP.AUX_PART:
                        commom = (T)Activator.CreateInstance(findAuxPart(_typeid).GetType());
                        break;
                    case IFF_GROUP.QUESTDROP:
                        commom = (T)Activator.CreateInstance(findQuestDrop(_typeid).GetType());
                        break;
                    case IFF_GROUP.QUEST:
                        commom = (T)Activator.CreateInstance(findQuestItem(_typeid).GetType());
                        break;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return commom;
        }

        public IFFCommon FindCommonItem(uint _typeid)
        {
            IFFCommon commom = null;

            try
            {
                switch ((IFF_GROUP)getItemGroupIdentify(_typeid))
                {
                    case IFF_GROUP.CHARACTER:
                        commom = Character.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.PART:
                        commom = Part.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.CLUB:
                        commom = m_club.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.CLUBSET:
                        commom = m_club_set.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.BALL:
                        commom = Ball.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.ITEM:
                        commom = Item.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.CADDIE:
                        commom = Caddie.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.CAD_ITEM:
                        commom = Caddie_item.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.SET_ITEM:
                        commom = Set_item.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.COURSE:
                        commom = m_course.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.SKIN:
                        commom = m_skin.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.HAIR_STYLE:
                        commom = m_hair_style.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.AUX_PART:
                        commom = Aux_part.GetItemCommon(_typeid);
                        break;
                    case IFF_GROUP.QUESTDROP:
                        commom = _QuestDrop.GetItemCommon(_typeid);
                        break; 
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return commom;
        }
        public string GetItemName(uint typeid)
        {
            var common = FindCommonItem(typeid);
            if (common != null)
            {
                return common.Name;
            }
            return "null";
        } 

        public bool IsGiftItem(uint _typeid)
        {
            var commom = FindCommonItem(_typeid);

            // É saleable ou giftable nunca os 2 juntos por que é a flag composta Somente Purchase(compra)
            // então faço o xor nas 2 flag se der o valor de 1 é por que ela é um item que pode presentear
            // Ex: 1 + 1 = 2 Não é
            // Ex: 1 + 0 = 1 OK
            // Ex: 0 + 1 = 1 OK
            // Ex: 0 + 0 = 0 Não é
            if (commom != null)
                return (commom.Active && commom.Shop.flag_shop.IsCash
                    && (commom.Shop.flag_shop.is_saleable ^ commom.Shop.flag_shop.IsGift));

            return false;
        }

        public bool IsOnlyDisplay(uint _typeid)
        {
            var commom = FindCommonItem(_typeid);

            if (commom != null)
                return (commom.Active && commom.Shop.flag_shop.IsDisplay);

            return false;
        }

        public bool IsOnlyPurchase(uint _typeid)
        {
            var commom = FindCommonItem(_typeid);

            if (commom != null)
                return (commom.Active && commom.Shop.flag_shop.is_saleable
                    && commom.Shop.flag_shop.IsGift);

            return false;
        }

        public bool IsOnlyGift(uint _typeid)
        {
            var commom = FindCommonItem(_typeid);

            if (commom != null)
                return commom.IsOnlyGift();

            return false;
        }

        public IFF_GROUP _getItemGroupIdentify(uint _typeid)
        {
            return (IFF_GROUP)((_typeid & 0xFC000000) >> 26);
        }
        public uint getItemGroupIdentify(uint _typeid)
        {
            return (uint)((_typeid & 0xFC000000) >> 26);
        }


        public uint getItemSubGroupIdentify24(uint _typeid)
        {
            return (uint)((_typeid & ~0xFC000000) >> 24);       // aqui é >> 24, mas deixei 25 por causa do item equipável e o passivo, mas posso mudar depois isso
        }

        public uint getItemSubGroupIdentify22(uint _typeid)
        {
            return (uint)((_typeid & ~0xFC000000) >> 22);       // esse retorno os grupos divididos em 0x40 0x80 0xC0, 0x100, 0x140
        }

        public uint getItemSubGroupIdentify21(uint _typeid)
        {
            return (uint)((_typeid & ~0xFC000000) >> 21);       // esse retorno os grupos divididos em 0x20 0x40 0x60, 0x80, 0xA0, 0xC0, 0xE0, 0x100
        }
         
        public uint getItemCharIdentify(uint _typeid)
        {
            return (uint)((_typeid & 0x03FF0000) >> 18);
        }

        public uint getItemCharPartNumber(uint _typeid)
        {
            return (uint)((_typeid & 0x0003FF00) >> 13);
        }

        public uint getItemCharTypeNumber(uint _typeid)
        {
            return (uint)((_typeid & 0x00001FFF) >> 8);
        }

        public uint getItemIdentify(uint _typeid)
        {
            return (uint)(_typeid & 0x000000FF);
        }

        public uint getItemTitleNum(uint _typeid)
        {
            var restul = (_typeid & 0x3FFFFF);
            return restul;
        }


        public uint getItemSkin(uint _typeid)
        {
            var restul = (_typeid & 0x3C00000u);
            if (restul == 0)
            {
                return 0;
            }

            if (restul == 4194304)
            {
                return 1;
            }

            if (restul == 8388608)
            {
                return 2;
            }

            if (restul == 12582912)
            {
                return 3;
            }

            if (restul == 20971520)
            {
                return 4;
            }

            if (restul == 25165824)
            {
                restul = 5;
            }
            return restul;
        }


        public uint getMatchTypeIdentity(uint _typeid)
        {
            return (uint)((_typeid & ~0xFC000000) >> 16);
        }

        public uint getCaddieItemType(uint _typeid)
        {
            return (uint)((_typeid & 0x0000FF00) >> 13);
        }

        public uint getCaddieIdentify(uint _typeid)
        {
            return (uint)(((_typeid & 0x0FFF0000) >> 21)/*Caddie Base*/ + ((_typeid & 0x000F0000) >> 16)/*Caddie Type, N, R, S e etc*/);
        }

        // Acho que eu fiz para usar no enchant de up stat de taqueira e character
        public uint getEnchantSlotStat(uint _typeid)
        {
            return (uint)((_typeid & 0x03FF0000) >> 20);
        }
          
        public uint getItemAuxPartNumber(uint _typeid)
        {
            return (uint)((_typeid & 0x0003FF00) >> 16);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public byte GetAuxType(uint ID)
        {
            byte result;

            result = (byte)System.Math.Round((ID & 0x001F0000) / System.Math.Pow(2.0, 16.0));

            return result;
        }
          
        public bool IsExist(uint _typeid)
        {
            if (_typeid <= 0)
                return false;

            return FindCommonItem(_typeid) != null;
        }
        public bool ExistIcon(uint _typeid)
        {
            var _base = FindCommonItem(_typeid);
            if (_base == null)
            {
                return false;
            }
            return !string.IsNullOrEmpty(_base.ShopIcon);
        }



        public bool IsSelfDesign(uint TypeId)
        {
            switch (TypeId)
            {
                case 134258720:
                case 134242351:
                case 134258721:
                case 134242355:
                case 134496433:
                case 134496434:
                case 134512665:
                case 134496344:
                case 134512666:
                case 134496345:
                case 134783001:
                case 134758439:
                case 134783002:
                case 134758443:
                case 135020720:
                case 135020721:
                case 135045144:
                case 135020604:
                case 135045145:
                case 135020607:
                case 135299109:
                case 135282744:
                case 135299110:
                case 135282745:
                case 135545021:
                case 135545022:
                case 135569438:
                case 135544912:
                case 135569439:
                case 135544915:
                case 135807173:
                case 135807174:
                case 135823379:
                case 135807066:
                case 135823380:
                case 135807067:
                case 136093719:
                case 136069163:
                case 136093720:
                case 136069166:
                case 136331407:
                case 136331408:
                case 136355843:
                case 136331271:
                case 136355844:
                case 136331272:
                case 136593549:
                case 136593550:
                case 136617986:
                case 136593410:
                case 136617987:
                case 136593411:
                case 136880144:
                case 136855586:
                case 136880145:
                case 136855587:
                case 136855588:
                case 136855589:
                case 137379868:
                case 137379869:
                case 137404426:
                case 137379865:
                case 137404427:
                case 137379866:
                case 137904143:
                case 137904144:
                case 137928708:
                case 137904140:
                case 137928709:
                case 137904141:
                    return true;
                default:
                    return false;
            }
        }
        public bool IsItemCookie(uint _typeid)
        {
            var iff = FindCommonItem(_typeid);
            if (iff.ID == _typeid)
            {
                return iff.Shop.flag_shop.IsCash;
            }
            return false;
        }

        public uint getItemPrice(uint _typeid, short time)
        {
            var iff = FindCommonItem(_typeid);
            if (iff.ID == _typeid)
            {
                return iff.Shop.Price;
            }
            return 0;
        }
        public bool IsCanOverlapped(uint _typeid, bool is_memorial = false)
        {

            switch ((IFF_GROUP)getItemGroupIdentify(_typeid))
            {
                case IFF_GROUP.CHARACTER:
                case IFF_GROUP.COURSE:
                case IFF_GROUP.MATCH: 
                case IFF_GROUP.HAIR_STYLE: 
                case IFF_GROUP.QUESTDROP:
                case IFF_GROUP.QUEST:
                default:
                    return false; 
                case IFF_GROUP.CLUBSET:
                    {
                        var cadItem = findClubSet(_typeid);

                        if (cadItem != null && cadItem.Shop.flag_shop.time_shop.active)
                            return true;    // Caddie item pode, se for de tempo para aumentar o tempo dele

                        break;
                    }

                case IFF_GROUP.SKIN:
                    {
                        var cadItem = findSkin(_typeid);

                        if (cadItem != null && cadItem.Shop.flag_shop.time_shop.active)
                            return true;    // Caddie item pode, se for de tempo para aumentar o tempo dele

                        break;
                    }
                case IFF_GROUP.CAD_ITEM:
                    {
                        var cadItem = findCaddieItem(_typeid);

                        if (cadItem != null && cadItem.Shop.flag_shop.time_shop.active)
                            return true;    // Caddie item pode, se for de tempo para aumentar o tempo dele

                        break;
                    }
                case IFF_GROUP.PART:
                    {
                        var part = findPart(_typeid);

                        // Libera os parts para Duplicatas se ele estiver liberado para vender no personal shop
                        if (part != null && (part.type_item == PART_TYPE.UCC_DRAW_ONLY || part.type_item == PART_TYPE.UCC_COPY_ONLY
                            || part.Shop.flag_shop.IsDuplication || part.Shop.flag_shop.can_send_mail_and_personal_shop))
                            return true;

                        break;
                    }
                case IFF_GROUP.ITEM:  // Libera todos item para dub se tiver abilitado no shop 
                case IFF_GROUP.BALL: 
                case IFF_GROUP.CADDIE:
                    if (_typeid == 0x1C000001 || _typeid == 0x1C000002 || _typeid == 0x1C000003 || _typeid == 0x1C000007)
                        return true;
                    break;
                case IFF_GROUP.SET_ITEM:
                    {
                        var tipo_set_item = (SET_ITEM_SUB_TYPE)getItemSubGroupIdentify21(_typeid);
                        var iff = findSetItem(_typeid);

                        if (tipo_set_item == SET_ITEM_SUB_TYPE.BALL
                            || tipo_set_item == SET_ITEM_SUB_TYPE.CHARACTER_SET_DUP_AND_ITEM_PASSIVE_AND_ACTIVE
                            || tipo_set_item == SET_ITEM_SUB_TYPE.CARD || tipo_set_item == SET_ITEM_SUB_TYPE.CHARACTER_SET_NEW)   //olhar um codigo melhor depois
                            return true;

                        if (!is_memorial && (tipo_set_item == SET_ITEM_SUB_TYPE.CHARACTER_SET || tipo_set_item == SET_ITEM_SUB_TYPE.CHARACTER_SET_NEW))
                        {
                            for (int i = 0; i < iff.packege.item_qntd.Length; i++)
                            {
                                if (getItemGroupIdentify(iff.packege.item_typeid[i]) == 2)
                                {
                                    var part = FindCommonItem(iff.packege.item_typeid[i]);
                                    if ((part.Shop.flag_shop.IsDuplication || part.Shop.flag_shop.can_send_mail_and_personal_shop))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        break;
                    }
                case IFF_GROUP.AUX_PART:
                    {
                        var auxPart = findAuxPart(_typeid);

                        if (auxPart != null && auxPart.getQnt())//corrigido....
                            return Convert.ToBoolean(_typeid & ~0x1F0000);

                        break;
                    }   // Fim AuxPart
            }   // Fim Case

            return false;
        }
        public bool IsItemEquipable(uint _typeid)
        {
            var item = findItem(_typeid);

            if (item != null)
                return (getItemSubGroupIdentify24(_typeid) >> 1) == 0;  // Equiável, aqui depois tenho que mudar se mudar lá em cima, para (func() >> 1) == 0

            return false;
        }

        public bool IsTitle(uint _typeid)
        {

            if ((IFF_GROUP)getItemGroupIdentify(_typeid) == IFF_GROUP.SKIN)
            {
                if ((_typeid & 0x3C00000u) != 0x1800000u)
                    return false;   // Não é um title

                return true;
            }

            return false;   // Não é uma skin(bg, frame, sticker, slot, cutin, title)
        }

        public IFFFile<Item> getItem()
        {
            return Item;
        }

        public IFFFile<Skin> getSkin()
        {
            return m_skin;
        }

        public IFFFile<AuxPart> getAuxPart()
        {
            return Aux_part;
        }

        public IFFFile<Ball> getBall()
        {
            return Ball;
        }

        public IFFFile<Character> getCharacter()
        {
            return Character;
        }

        public IFFFile<Caddie> getCaddie()
        {
            return Caddie;
        }

        public IFFFile<CaddieItem> getCaddieItem()
        {
            return Caddie_item;
        }

        public IFFFile<ClubSet> getClubSet()
        {
            return m_club_set;
        }

        public IFFFile<HairStyle> getHairStyle()
        {
            return m_hair_style;
        }

        public IFFFile<Part> getPart()
        {
            return Part;
        }

        public IFFFile<SetItem> getSetItem()
        {
            return Set_item;
        }

        public IFFFile<Desc> getDesc()
        {
            return m_desc;
        }

        public IFFFile<Course> getCourse()
        {
            return m_course;
        }

        public IFFFile<Quest> getQuests()
        {
            return Quests;
        }

        public IFFFile<QuestDrop> getQuestStuff()
        {
            return _QuestDrop;
        }

        public IFFFile<Club> getClub()
        {
            return m_club;
        }

        public IFFFile<Enchant> getEnchant()
        {
            return m_enchant;
        }

        public IFFFile<Match> getMatch()
        {
            return m_match;
        }

        public List<ClubSet> findClubSetOriginal(uint _typeid)
        {

            List<ClubSet> v_clubset = new List<ClubSet>();
            ClubSet clubset = null;

            // Invalid Typeid
            if (_typeid == 0)
                return v_clubset;

            if ((clubset = findClubSet(_typeid)) != null)
            {

                foreach (var el in m_club_set)
                {

                    // Text pangya é o logo da taqueira, como as especiais tem seu proprio logo
                    // então o número do logo vai ser a taqueira base das taqueira que transforma
                    if (el.text_pangya == clubset.text_pangya)
                        v_clubset.Add(el);
                }
            }

            return v_clubset;
        }
         
        public bool isLoad()
        {
            return m_loaded;
        }

        public bool EMPTY_ARRAY_PRICE<T>(T[] price) where T : struct, IComparable
        {
            return !price.Any(el => !el.Equals(default(T)));
        }


        public uint SUM_ARRAY_PRICE_ULONG<T>(T[] price) where T : struct, IComparable
        {
            uint sum = 0;

            foreach (var el in price)
            {
                sum += Convert.ToUInt32(el);
            }

            return sum;
        }
    }
    public class sIff : Singleton<IFFHandle>
    {
    }
}
