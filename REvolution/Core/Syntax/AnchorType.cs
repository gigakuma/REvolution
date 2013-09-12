using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    public enum AnchorType
    {
        Bol,            // ^
        Eol,            // $
        Boundary,       // \b
        Nonboundary,    // \B
        Beginning,      // \A
        Start,          // \G
        EndZ,           // \Z
        End             // \z
    }
}