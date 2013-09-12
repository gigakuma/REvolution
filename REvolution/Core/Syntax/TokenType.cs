using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    public enum TokenType
    {
        Empty,
        // 0-char
        Char,       // a, \a, \xFF, \u00FF, \cX
        // Charset
        Set,        // ., \w, \p{name}
        BracketL, // [
        BracketR, // ]
        Negative, // ^
        Hyphen,     // -
        Subtract,   // -

        // Identifier
        Reference,  // \k<name>, \k'name', \<name>, \'name', \1

        // 0-AnchorType
        Anchor,    // \B, \b, ^, $

        // null
        VerticalBar,// |

        //Group,      // (?, (, )
        ParenL,
        ParenR,
        Question,
        Name,       // <name>, 'name', <name1-name2>, <-name>  Identifier[2]
        Colon,      // :
        Options,    // imnsx-imnsx  Options[2]
        Require,    // =, <=     AssertDirection
        Prevent,    // !, <!     AssertDirection
        Greedy,     // >
        Comment,    // #
        Field,      // @

        Quantifier, // +, *, ?, +?, *?, ??, {,}, {}, {}?

        // Extension Info
        Extention,
        CommentText
    }
}
