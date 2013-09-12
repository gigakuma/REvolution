using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    [FlagsAttribute]
    public enum SetType
    {
        None = 0x0000,
        Wildcard = 0x0020,
        Not = 0x8000,
        Category = 0x00FF,
        Word = 0x0001,
        Digit = 0x0002,
        Space = 0x0004,
        Property = 0x0008,
        Posix = 0x0010
    }
}
