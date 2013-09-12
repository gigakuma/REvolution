using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    [FlagsAttribute]
    public enum QuantifierType
    {
        Invalid = 0x0000,
        Definition = 0x00F0,
        Category = 0x000F,
        Lazy = 0x8000,

        Plus = 0x0001, // +
        Star = 0x0002, // *
        Question = 0x0004, // ?
        Brace = 0x0008, // {1,2}

        SingleDef = 0x0010, // {1}
        RangeDef = 0x0020, // {1,2}
        HalfDef = 0x0040  // {1,}
    }
}
