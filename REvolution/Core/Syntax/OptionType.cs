﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    [FlagsAttribute]
    public enum OptionType
    {
        None                    = 0x0000,
        IgnoreCase              = 0x0001,      // "i"
        Multiline               = 0x0002,      // "m"
        ExplicitCapture         = 0x0004,      // "n"
        Compiled                = 0x0008,      // "c"
        Singleline              = 0x0010,      // "s"
        IgnorePatternWhitespace = 0x0020,      // "x"
        RightToLeft             = 0x0040,      // "r"
        ECMAScript              = 0x0100,      // "e"
        CultureInvariant        = 0x0200
    }
}
