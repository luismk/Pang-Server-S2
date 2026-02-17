using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangya_LoginServer.PangyaEnums
{
    public enum SubLoginCode
    {
  Success = 0x00,
    AuthFail = 0x01,
    ProcFail = 0x02,
    AlreadyConnected = 0x03,
    ProhibitedID = 0x04,
    IncorrectPassword = 0x05,
    TempBlocked = 0x06,
    Timeout = 0x07,
    NotAuthorized = 0x08,
    NotAdult = 0x09,
    Unknown = 0x0A,
    IdentityFormatIncorrect = 0x0B,
    Abandoned = 0x0C,
    NotQualified = 0x0D,
    Announcement = 0x0E,
    NotAllowedArea = 0x0F,
    CharacterSelect = 0x11,
    InvalidInformation = 0x12,
    NickNameUsed = 0x13
    }
}
