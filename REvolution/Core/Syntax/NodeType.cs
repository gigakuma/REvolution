using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    public enum NodeType
    {
        One,
        Set,
        Range,
        CharClass,
        Multi,
        Reference,
        Anchor,

        Group,
        Capture,
        Require,
        Prevent,
        Greedy,
        Test,
        Balance,

        Alternate,
        Concatenate,

        Field,
        Comment,
        Options
    }
}
