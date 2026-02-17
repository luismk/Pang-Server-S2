#define DEBUG
using PangyaAPI.Network.Models;
using PangyaAPI.Utilities.BinaryModels;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pangya_GameServer.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PlayerRoomInfoEx : PlayerRoomInfo
{
    public uint uid;

    public uint guild_uid;
    public byte convidado;

    [field: MarshalAs(UnmanagedType.Struct)]
    public CharacterInfo ci { get; set; }

    public PlayerRoomInfoEx()
    {
        clear();
        ci = new CharacterInfo();
    }

    public byte[] ToArrayEx()
    {
        using PangyaBinaryWriter p = new PangyaBinaryWriter();
        p.WriteBytes(ToArray(), 113);
        p.WriteBytes(ci.ToArray(), 131);
        Debug.Assert(!(p.GetSize != 244), "PlayerRoomInfoEx::BuildEx is error");
        return p.GetBytes;
    }

    public byte[] ToArray()
    {
        using PangyaBinaryWriter p = new PangyaBinaryWriter();
        p.WriteUInt32(uid);
        p.WriteStr(id, 22);
        p.WriteStr(nickname, 22);
        p.WriteStr(guild_name, 20);
        p.WriteByte(position);
        p.WriteInt32(capability.ulCapability);
        p.WriteUInt32(title);
        p.WriteUInt32(char_typeid);
        p.WriteInt32(cad_id);
        p.WriteUInt32(club_typeid);
        p.WriteUInt32(comet_typeid);
        p.WriteUInt32(unknown);//skin_typeid[4]
        p.WriteUInt32(unknown1);//  0x00, 0x00, 0x00, 0x00,
        p.WriteUInt16(state_flag.usFlag);//101 é struct do flags...
        p.WriteByte(level);
        p.WriteByte(icon_angel);
        p.WriteByte(place);
        p.WriteUInt32(guild_uid);
        p.WriteUInt16(flag_item_boost.ulItemBoost);
        p.WriteByte(convidado);
        Debug.Assert(p.GetSize == 113, "PlayerRoomInfo::Build is error");
        return p.GetBytes;
    }
}
