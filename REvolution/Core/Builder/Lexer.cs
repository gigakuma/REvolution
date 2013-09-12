using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Syntax;
using REvolution.Core.Symbols;

namespace REvolution.Core.Builder
{
    internal class Lexer
    {
        private String _pattern;
        private int _currentPos;

        private bool isInCharClass;
        private bool isFirstCharInCharClass;
        private bool isPrevOpenGroup;
        private bool isPrevQuantifier;
        private bool isInComment;
        private bool isInField;

        private int groupNestedLevel;

        private bool isInRange;
        private bool isPrevSubtract;
        private int classNestedLevel;

        private Token prevChar;
        private Token tokenBuffer;

        private int anonymousCapCount;

        private const int MaxValueDiv10 = Int32.MaxValue / 10;
        private const int MaxValueMod10 = Int32.MaxValue % 10;

        private HashSet<int> _capnumlist;
        private HashSet<string> _capnamelist;

        private object _infos;

        public delegate Expression ExtentionHandler(Lexer lexer, string text);

        public event ExtentionHandler ExtentionHandlers;

        private Expression TriggerExtentionHandlers(string text)
        {
            ExtentionHandler handler = ExtentionHandlers;
            if (handler != null)
                return handler(this, text);
            else
                return null;
        }

        public Lexer(String pattern)
        {
            _pattern = pattern;
            _capnumlist = new HashSet<int>();
            _capnamelist = new HashSet<string>();
            Reset();
        }

        internal void Reset()
        {
            _currentPos = 0;
            _capnumlist.Clear();
            _capnamelist.Clear();
            anonymousCapCount = 0;
            isInCharClass = false;
            isFirstCharInCharClass = false;
            isPrevOpenGroup = false;
            isPrevQuantifier = false;
            isInComment = false;
            isInField = false;
            groupNestedLevel = 0;
            isInRange = false;
            isPrevSubtract = false;
            classNestedLevel = 0;
            prevChar = null;
            tokenBuffer = null;
            _infos = null;
        }

        public Token GetNextToken()
        {
            return ScanToken();
        }

        public bool ScaningCharClass
        {
            get { return isInCharClass; }
        }

        public int AnonymousCapCount
        {
            get { return anonymousCapCount; }
        }

        #region Scanner
        private Token ScanToken()
        {
            if (CharsRight() == 0)
            {
                if (isInCharClass)
                    throw MakeException("LackOfCloseBracket");
                if (groupNestedLevel != 0)
                    throw MakeException("LackOfCloseGroup");
                return Token.Empty;
            }
            if (tokenBuffer != null)
            {
                Token token = tokenBuffer;
                tokenBuffer = null;
                MoveRight(token.Length);
                return token;
            }
            char ch;
            int start = Textpos();
            if (isInComment)
            {
                isInComment = false;
                while (CharsRight() > 0 && MoveRightGetChar() != ')') ;
                if (CharsRight() == 0 && RightChar(-1) != ')')
                    throw MakeException("IncompletedComment");
                MoveLeft();
                if (Textpos() != start)
                    return new Token(TokenType.CommentText, GetSubstring(start, Textpos() - start), start);
                else
                {
                    MoveRight();
                    return ScanCloseGroup();
                }
            }
            else if (isInField)
            {
                isInField = false;
                while (CharsRight() > 0 && MoveRightGetChar() != ')') ;
                if (CharsRight() == 0 && RightChar(-1) != ')')
                    throw MakeException("IncompletedComment");
                MoveLeft();
                if (Textpos() != start)
                {
                    string ext = GetSubstring(start, Textpos() - start);
                    _infos = TriggerExtentionHandlers(ext);
                    return new Token(TokenType.Extention, ext, start, _infos);
                }
                else
                {
                    MoveRight();
                    return ScanCloseGroup();
                }
            }
            else if (isInCharClass)
            {
                return ScanCharClass();
            }
            else if (isPrevOpenGroup)
            {
                ch = MoveRightGetChar();
                isPrevOpenGroup = false;
                switch (ch)
                {
                    case ':':
                        return new Token(TokenType.Colon, ":", start);
                    case '=':
                        return new Token(TokenType.Require, "=", start, AssertDirection.Forward);
                    case '!':
                        return new Token(TokenType.Prevent, "!", start, AssertDirection.Forward);
                    case '>':
                        return new Token(TokenType.Greedy, ">", start);
                    case '#':
                        isInComment = true;
                        return new Token(TokenType.Comment, "#", start);
                    case '@':
                        isInField = true;
                        return new Token(TokenType.Field, "@", start);
                    case '<':
                        if (CharsRight() > 0)
                        {
                            if (RightChar() == '=')
                            {
                                MoveRight();
                                return new Token(TokenType.Require, "<=", start, AssertDirection.Backward);
                            }
                            else if (RightChar() == '!')
                            {
                                MoveRight();
                                return new Token(TokenType.Prevent, "<!", start, AssertDirection.Backward);
                            }
                            else
                            {
                                MoveLeft();
                                return new Token(TokenType.Name, ScanGroupName(), start, _infos);
                            }
                        }
                        else
                            throw MakeException("InvalidOpenGroup");
                    case '\'':
                        MoveLeft();
                        return new Token(TokenType.Name, ScanGroupName(), start, _infos);
                    case '(':
                        if (CharsRight() <= 0)
                            throw MakeException("InvalidOpenGroup");
                        int backpos = Textpos();
                        string name;
                        int capnum = -1;
                        bool needBack = false;
                        if (IsWordChar(RightChar()))
                        {
                            name = ScanCapname();
                            needBack = !IsCaptureSlot(name);
                        }
                        else if (IsDecimalChar(RightChar()))
                        {
                            name = ScanDecimal();
                            capnum = DecimalValue(name);
                            needBack = !IsCaptureSlot(capnum);
                        }
                        else
                        {
                            if (groupNestedLevel < 0)
                                throw MakeException("IncorrectParen");
                            //groupNestedLevel++;
                            //return new Token(TokenType.ParenL, "(", start);
                            return ScanOpenGroup();
                        }
                        if (CharsRight() <= 0)
                            throw MakeException("UndefinedOpenGroup");
                        if (MoveRightGetChar() != ')')
                            needBack = true;

                        if (needBack)
                        {
                            Textto(backpos);
                            //groupNestedLevel++;
                            //return new Token(TokenType.ParenL, "(", start);
                            return ScanOpenGroup();
                        }
                        else
                            return new Token(TokenType.Reference, "(" + name + ")", start, new Definition(capnum, name));

                    default:
                        if (IsWordChar(ch) || ch == '+' || ch == '-')
                        {
                            MoveLeft();
                            return ScanOption();
                        }
                        throw MakeException("UndefinedOpenGroup");
                }
            }
            else
            {
                ch = MoveRightGetChar();
                if (!IsQuantifier(ch))
                    isPrevQuantifier = false;
                switch (ch)
                {
                    case '\\':
                        return ScanBackslash();
                    case '^':
                        return new Token(TokenType.Anchor, "^", start, AnchorType.Bol);
                    case '$':
                        return new Token(TokenType.Anchor, "$", start, AnchorType.Eol);
                    case '[':
                        return ScanOpenBracket();
                    case '.':
                        return new Token(TokenType.Set, ".", start, new Charset());
                    case '(':
                        return ScanOpenGroup();
                    case ')':
                        return ScanCloseGroup();
                    case '|':
                        return new Token(TokenType.VerticalBar, "|", start);
                    case '*':
                    case '+':
                    case '?':
                        if (isPrevQuantifier)
                            throw MakeException("NestedQuantify");
                        MoveLeft();
                        isPrevQuantifier = true;
                        return ScanSpecialQuatifier();
                    case '{':
                        MoveLeft();
                        if (!IsTrueQuantifier())
                        {
                            MoveRight();
                            return new Token(TokenType.Char, "{", start);
                        }
                        if (isPrevQuantifier)
                            throw MakeException("NestedQuantify");
                        isPrevQuantifier = true;
                        return ScanBracketQuatifier();
                    default:
                        return new Token(TokenType.Char, ch, start, ch);
                }
            }
        }

        private Token ScanOpenBracket()
        {
            isInCharClass = true;
            isFirstCharInCharClass = true;
            classNestedLevel++;
            int start = Textpos() - 1;
            if (CharsRight() > 0 && RightChar() == '^')
            {
                tokenBuffer = new Token(TokenType.Negative, "^", start + 1);
                return new Token(TokenType.BracketL, "[", start);
            }
            else
                return new Token(TokenType.BracketL, "[", start);
        }

        private Token ScanCloseBracket()
        {
            isInCharClass = false;
            return new Token(TokenType.BracketR, "]", Textpos() - 1);
        }

        private Token ScanCharClass()
        {
            Token charToken = null;
            int start = Textpos();
            char ch = MoveRightGetChar();
            if (ch == ']')
            {
                if (!isFirstCharInCharClass)
                {
                    isInRange = false;
                    isPrevSubtract = false;
                    classNestedLevel--;
                    if (classNestedLevel != 0)
                    {
                        // [a-[a]]
                        if (CharsRight() > 0 && RightChar() != ']')
                            throw MakeException("SubtractionMustBeLast");
                    }
                    else
                        isInCharClass = false;
                    return new Token(TokenType.BracketR, "]", start);
                }
                else
                {
                    isFirstCharInCharClass = false;
                    return new Token(TokenType.Char, "]", start, ']');
                }
            }
            else if (ch == '\\' && CharsRight() > 0)
            {
                switch (ch = MoveRightGetChar())
                {
                    case 'd':
                        if (isInRange)
                            throw MakeException("BadClassInCharRange");
                        isFirstCharInCharClass = false;
                        return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Digit, false));
                    case 'D':
                        if (isInRange)
                            throw MakeException("BadClassInCharRange");
                        isFirstCharInCharClass = false;
                        return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Digit, true));
                    case 's':
                        if (isInRange)
                            throw MakeException("BadClassInCharRange");
                        isFirstCharInCharClass = false;
                        return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Space, false));
                    case 'S':
                        if (isInRange)
                            throw MakeException("BadClassInCharRange");
                        isFirstCharInCharClass = false;
                        return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Space, true));
                    case 'w':
                        if (isInRange)
                            throw MakeException("BadClassInCharRange");
                        isFirstCharInCharClass = false;
                        return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Word, false));
                    case 'W':
                        if (isInRange)
                            throw MakeException("BadClassInCharRange");
                        isFirstCharInCharClass = false;
                        return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Word, true));
                    case '-':
                        charToken = new Token(TokenType.Char, "\\-", start, '-');
                        break;
                    default:
                        MoveLeft();
                        charToken = ScanCharEscape();
                        break;
                }
            }
            else if (ch == '[')
            {
                if (CharsRight() > 0 && RightChar() == ':' && !isPrevSubtract)
                {
                    string name;
                    int savePos = Textpos();
                    MoveRight();
                    name = ScanCapname();
                    if (CharsRight() < 2 || MoveRightGetChar() != ':' || MoveRightGetChar() != ']')
                        Textto(savePos);
                    else
                    {
                        isFirstCharInCharClass = false;
                        return new Token(TokenType.Set, "[:" + name + ":]", start, new Charset(name));
                    }
                }
            }

            if (isPrevSubtract)
            {
                isPrevSubtract = false;
                // starting a nested char class [ab-[a]]
                return ScanOpenBracket();
            }
            if (charToken == null)
                charToken = new Token(TokenType.Char, ch, start, ch);
            if (isInRange)
            {
                isInRange = false;
                if (!IsValidCharSeq(prevChar, charToken))
                    throw MakeException("ReversedCharRange");
                prevChar = null;
            }
            if (CharsRight() >= 2 && RightChar() == '-' && RightChar(1) != ']')
            {
                if (RightChar(1) == '[')
                {
                    tokenBuffer = new Token(TokenType.Subtract, "-", Textpos());
                    isPrevSubtract = true;
                }
                else
                {
                    tokenBuffer = new Token(TokenType.Hyphen, "-", Textpos());
                    isInRange = true;
                    prevChar = charToken;
                }
            }
            isFirstCharInCharClass = false;
            return charToken;
        }

        private static bool IsValidCharSeq(Token char1, Token char2)
        {
            char ch1 = char1.ReadChar();
            char ch2 = char2.ReadChar();
            if (ch1 <= ch2)
                return true;
            else
                return false;
        }

        private Token ScanOpenGroup()
        {
            if (groupNestedLevel < 0)
                throw MakeException("IncorrectParen");
            groupNestedLevel++;
            int start = Textpos() - 1;
            if (CharsRight() > 0 && RightChar() == '?')
            {
                isPrevOpenGroup = true;
                tokenBuffer = new Token(TokenType.Question, "?", start + 1);
                return new Token(TokenType.ParenL, "(", start);
            }
            else
            {
                AddAnonymousCap();
                return new Token(TokenType.ParenL, "(", start);
            }
        }

        private Token ScanCloseGroup()
        {
            groupNestedLevel--;
            if (groupNestedLevel < 0)
                throw MakeException("IncorrectParen");
            isPrevOpenGroup = false;
            return new Token(TokenType.ParenR, ")", Textpos() - 1);
        }

        private Token ScanSpecialQuatifier()
        {
            int start = Textpos();
            char ch = MoveRightGetChar();
            if (CharsRight() > 0 && RightChar() == '?')
            {
                MoveRight();
                return new Token(TokenType.Quantifier, ch + "?", start, new Quantifier(ch, true));
            }
            else
                return new Token(TokenType.Quantifier, ch, start, new Quantifier(ch, false));
        }

        private Token ScanBracketQuatifier()
        {
            int start = Textpos();
            MoveRight();
            string min = ScanDecimal();
            int minv = DecimalValue(min);
            if (MoveRightGetChar() == '}')
            {
                if (RightChar() == '?')
                {
                    MoveRight();
                    return new Token(TokenType.Quantifier, "{" + min + "}?", start, new Quantifier(Quantifier.Infinity, minv, true));
                }
                else
                    return new Token(TokenType.Quantifier, "{" + min + "}", start, new Quantifier(Quantifier.Infinity, minv, false));
            }
            string max = ScanDecimal();
            int maxv = DecimalValue(max);
            MoveRight();
            if (RightChar() == '?')
            {
                MoveRight();
                return new Token(TokenType.Quantifier, "{" + min + "," + max + "}?", start, new Quantifier(minv, maxv, true));
            }
            else
                return new Token(TokenType.Quantifier, "{" + min + "," + max + "}", start, new Quantifier(minv, maxv, false));
        }

        private Token ScanOption()
        {
            int start = Textpos();
            char ch;
            OptionType option;
            OptionType pos = OptionType.None;
            OptionType neg = OptionType.None;
            bool off = false;
            for (; CharsRight() > 0; MoveRight())
            {
                ch = RightChar();
                if (ch == '+')
                {
                    off = false;
                    continue;
                }
                else if (ch == '-')
                {
                    off = true;
                    continue;
                }
                else
                {
                    option = OptionChar(ch);
                    if (option == 0)
                        break;
                    else
                    {
                        if (off)
                            neg |= option;
                        else
                            pos |= option;
                    }
                }
            }
            if (Textpos() == start)
                throw MakeException("UnrecognizedGroup");
            if (CharsRight() > 0 && RightChar() == ':')
                tokenBuffer = new Token(TokenType.Colon, ":", Textpos());
            return new Token(TokenType.Options, GetSubstring(start, Textpos() - start), start, new OptionType[]{pos, neg});
        }

        internal static OptionType OptionChar(char ch)
        {
            // case-insensitive
            if (ch >= 'A' && ch <= 'Z')
                ch += (char)('a' - 'A');

            switch (ch)
            {
                case 'c':
                    return OptionType.Compiled;
                case 'i':
                    return OptionType.IgnoreCase;
                case 'r':
                    return OptionType.RightToLeft;
                case 'm':
                    return OptionType.Multiline;
                case 'n':
                    return OptionType.ExplicitCapture;
                case 's':
                    return OptionType.Singleline;
                case 'x':
                    return OptionType.IgnorePatternWhitespace;
                case 'e':
                    return OptionType.ECMAScript;
                default:
                    return 0;
            }
        }

        /*
         * Scans chars following a '\' (not counting the '\'), and returns
         * a RegexNode for the type of atom scanned.
         */
        private Token ScanBackslash()
        {
            if (CharsRight() == 0)
                throw MakeException("IllegalEndEscape");
            int start = Textpos() - 1;
            char ch = MoveRightGetChar();
            switch (ch)
            {
                case 'b':
                    return new Token(TokenType.Anchor, "\\" + ch, start, AnchorType.Boundary);
                case 'B':
                    return new Token(TokenType.Anchor, "\\" + ch, start, AnchorType.Nonboundary);
                case 'A':
                    return new Token(TokenType.Anchor, "\\" + ch, start, AnchorType.Beginning);
                case 'G':
                    return new Token(TokenType.Anchor, "\\" + ch, start, AnchorType.Start);
                case 'Z':
                    return new Token(TokenType.Anchor, "\\" + ch, start, AnchorType.EndZ);
                case 'z':
                    return new Token(TokenType.Anchor, "\\" + ch, start, AnchorType.End);
                case 'w':
                    return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Word, false));
                case 'W':
                    return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Word, true));
                case 's':
                    return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Space, false));
                case 'S':
                    return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Space, true));
                case 'd':
                    return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Digit, false));
                case 'D':
                    return new Token(TokenType.Set, "\\" + ch, start, new Charset(SetType.Digit, true));
                case 'p':
                    string property = ScanProperty();
                    return new Token(TokenType.Set, "\\" + ch + "{" + property + "}", start, new Charset(property, false));
                case 'P':
                    property = ScanProperty();
                    return new Token(TokenType.Set, "\\" + ch + "{" + property + "}", start, new Charset(property, true));
                
                case 'k':
                    string res0 = ScanRefName();
                    if (res0 == null)
                        throw MakeException("UnrecognizedEscape");
                    else
                        return new Token(TokenType.Reference, "\\k" + res0, start, _infos);
                case '\'':
                case '<':
                    int backpos = Textpos();
                    MoveLeft();
                    string res1 = ScanRefName();
                    if (res1 == null)
                    {
                        Textto(backpos);
                        return new Token(TokenType.Char, "\\" + ch, start);
                    }
                    else
                        return new Token(TokenType.Reference, "\\" + res1, start, _infos);
                default:
                    MoveLeft();
                    return ScanBasicBackslash();
            }
        }

        /*
         * Scans number backreferences and character escapes
         */
        private Token ScanBasicBackslash()
        {
            char ch = RightChar();
            int start = Textpos() - 1;
            if (IsDecimalChar(ch))
            {
                int backpos = Textpos();
                bool isOctal = IsOctalChar(ch);
                string name = ScanDecimal();
                int capnum = DecimalValue(name);
                if (IsCaptureSlot(capnum))
                    return new Token(TokenType.Reference, "\\" + name, start, new Definition(capnum));
                else if (isOctal)
                    Textto(backpos);
                else
                    throw MakeException("UndefinedBackRef");
            }
            return ScanCharEscape();
        }

        /*
         * Scans \ code for escape codes that map to single unicode chars.
         */
        private Token ScanCharEscape()
        {
            int start = Textpos() - 1;
            char ch = MoveRightGetChar();
            if (IsOctalChar(ch))
            {
                MoveLeft();
                string oct = ScanOctal();
                return new Token(TokenType.Char, "\\" + ScanOctal(), start, (char)OctalValue(oct));
            }
            switch (ch)
            {
                case 'a':
                    return new Token(TokenType.Char, "\\" + ch, start, '\u0007');
                case 'b':
                    return new Token(TokenType.Char, "\\" + ch, start, '\b');
                case 'e':
                    return new Token(TokenType.Char, "\\" + ch, start, '\u001B');
                case 'f':
                    return new Token(TokenType.Char, "\\" + ch, start, '\f');
                case 'n':
                    return new Token(TokenType.Char, "\\" + ch, start, '\n');
                case 'r':
                    return new Token(TokenType.Char, "\\" + ch, start, '\r');
                case 't':
                    return new Token(TokenType.Char, "\\" + ch, start, '\t');
                case 'v':
                    return new Token(TokenType.Char, "\\" + ch, start, '\u000B');
                case 'x':
                    string hex = ScanHex(2);
                    return new Token(TokenType.Char, "\\x" + hex, start, (char)HexValue(hex));
                case 'u':
                    hex = ScanHex(4);
                    return new Token(TokenType.Char, "\\u" + hex, start, (char)HexValue(hex));
                case 'c': 
                    char raw = RightChar();
                    char control = ScanControl();
                    return new Token(TokenType.Char, "\\c" + raw, start, control);
                default:
                    if (IsWordChar(ch))
                        throw MakeException("UnrecognizedEscape");
                    return new Token(TokenType.Char, "\\" + ch, start, ch);
            }
        }

        private string ScanProperty()
        {
            if (CharsRight() < 3)
            {
                throw MakeException("IncompleteSlashP");
            }
            char ch = MoveRightGetChar();
            int start = Textpos();
            if (ch != '{')
            {
                throw MakeException("MalformedSlashP");
            }
            while (CharsRight() > 0)
            {
                ch = MoveRightGetChar();
                if (!(IsWordChar(ch) || ch == '-'))
                {
                    MoveLeft();
                    break;
                }
            }
            String capname = GetSubstring(start, Textpos() - start);

            if (CharsRight() == 0 || MoveRightGetChar() != '}')
                throw MakeException("IncompleteSlashP");
            return capname;
        }

        private string ScanGroupName()
        {
            char ch = RightChar();
            int capnum = -1;
            int capnum2 = -1;
            string name = null;
            string name2 = null;
            if (ch != '<' && ch != '\'')
                throw MakeException("MalformedName");
            char close = ch == '\'' ? '\'' : '>';
            if (CharsRight() > 0)
                MoveRight();
            else
                throw MakeException("IncompleteName");
            StringBuilder builder = new StringBuilder();
            builder.Append(ch);
            ch = RightChar();
            if (IsDecimalChar(ch))
            {
                name = ScanDecimal();
                capnum = DecimalValue(name);
            }
            else if (IsWordChar(ch))
            {
                name = ScanCapname();
            }
            else if (ch != '-')
                throw MakeException("InvalidGroupName");

            if (CharsRight() == 0)
                throw MakeException("IncompleteCapName");

            if (name != null)
            {
                if (capnum == -1)
                    AssignCaptureSlot(name);
                else
                    AssignCaptureSlot(capnum);
                builder.Append(name);
            }
            ch = RightChar();
            if (ch == '-')
            {
                builder.Append('-');
                MoveRight();
                if (CharsRight() == 0)
                    throw MakeException("InvalidGroupName");
                ch = RightChar();
                if (IsDecimalChar(ch))
                {
                    name2 = ScanDecimal();
                    capnum = DecimalValue(name2);
                    if (!IsCaptureSlot(capnum))
                        throw MakeException("UndefinedBackRef");
                }
                else if (IsWordChar(ch))
                {
                    name2 = ScanCapname();
                    if (!IsCaptureSlot(name2))
                        throw MakeException("UndefinedNameRef");
                }
                else
                    throw MakeException("InvalidGroupName");
                builder.Append(name2);
                if (CharsRight() == 0)
                    throw MakeException("IncompleteCapName");
            }

            if (CharsRight() == 0 || RightChar() != close)
                throw MakeException("InvalidGroupName");
            else
            {
                builder.Append(close);
                MoveRight();
                Definition id1 = null;
                Definition id2 = null;
                if (name != null)
                {
                    if (name2 != null)
                        id1 = new Definition(capnum, name, DefinitionType.BalanceRecord);
                    else
                        id1 = new Definition(capnum, name, DefinitionType.Record);
                }
                if (name2 != null)
                    id2 = new Definition(capnum2, name2, DefinitionType.BalanceDelete);
                _infos = new Definition[] { id1, id2 };
                return builder.ToString();
            }

        }

        private string ScanRefName()
        {
            char ch = RightChar();
            int capnum = -1;
            string name;
            if (ch != '<' && ch != '\'')
                throw MakeException("MalformedNameRef");
            char close = ch == '\'' ? '\'' : '>';
            if (CharsRight() > 0)
                MoveRight();
            else
                throw MakeException("IncompleteNameRef");
            StringBuilder builder = new StringBuilder();
            builder.Append(ch);
            ch = RightChar();
            if (IsDecimalChar(ch))
            {
                name = ScanDecimal();
                capnum = DecimalValue(name);
            }
            else if (IsWordChar(ch))
            {
                name = ScanCapname();
            }
            else
                return null;
            if (CharsRight() == 0)
                return null;
            ch = RightChar();
            if (ch != close)
                return null;
            else
            {
                if (capnum == -1)
                {
                    if (!IsCaptureSlot(name))
                        throw MakeException("UndefinedNameRef");
                }
                else
                {
                    if (!IsCaptureSlot(capnum))
                        throw MakeException("UndefinedBackRef");
                }
                builder.Append(name);
                builder.Append(close);
                MoveRight();
                _infos = new Definition(capnum, name);
                return builder.ToString();
            }
        }

        private string ScanHex(int c)
        {
            int start = Textpos();
            if (CharsRight() >= c)
            {
                while (c > 0 && IsHexChar(MoveRightGetChar()))
                    c--;
            }
            if (c > 0)
                throw MakeException("TooFewHex");
            return GetSubstring(start, Textpos() - start);
        }

        private static int HexDigit(char ch)
        {
            int d;

            if ((uint)(d = ch - '0') <= 9)
                return d;

            if ((uint)(d = ch - 'a') <= 5)
                return d + 0xa;

            if ((uint)(d = ch - 'A') <= 5)
                return d + 0xa;

            return -1;
        }

        private static int HexValue(string hex)
        {
            if (hex.Length == 0)
                return -1;
            int h = 0;
            for (int i = 0; i < hex.Length; i++)
            {
                h *= 10;
                h += HexDigit(hex[i]);
            }
            return h;
        }

        private string ScanOctal()
        {
            int start = Textpos();
            char first = RightChar();
            int c = 3;
            while (CharsRight() > 0 && c > 0 && IsOctalChar(RightChar()))
            {
                MoveRight();
                c--;
                if (c == 1)
                {
                    if ('0' <= first && first <= '3')
                        continue;
                    else
                        break;
                }
            }
            return GetSubstring(start, Textpos() - start);
        }

        private static int OctalValue(string oct)
        {
            if (oct.Length == 0)
                return -1;
            int o = 0;
            for (int i = 0; i < oct.Length; i++)
            {
                o *= 10;
                o += oct[i] - '0';
            }
            return o;
        }

        private string ScanDecimal()
        {
            int start = Textpos();
            while (CharsRight() > 0 && IsDecimalChar(RightChar()))
                MoveRight();
            return GetSubstring(start, Textpos() - start);
        }

        private static int DecimalValue(string dec)
        {
            if (dec.Length == 0)
                return -1;
            int d = 0;
            int v;
            for (int i = 0; i < dec.Length; i++)
            {
                v = dec[i] - '0';
                if (d > (MaxValueDiv10) || (d == (MaxValueDiv10) && v > (MaxValueMod10)))
                    throw MakeException("DecimalNumberOutOfRange");
                d *= 10;
                d += v;
            }
            return d;
        }

        private string ScanCapname()
        {
            int start = Textpos();
            while (CharsRight() > 0)
            {
                if (!IsWordChar(MoveRightGetChar()))
                {
                    MoveLeft();
                    break;
                }
            }
            return GetSubstring(start, Textpos() - start);
        }

        private char ScanControl()
        {
            //char raw = RightChar();

            if (CharsRight() <= 0)
                throw MakeException("MissingControl");

            char ch = MoveRightGetChar();
            // \ca interpreted as \cA
            if (ch >= 'a' && ch <= 'z')
                ch = (char)(ch - ('a' - 'A'));
            if ((ch = (char)(ch - '@')) < ' ')
                return ch;

            throw MakeException("UnrecognizedControl");
        }

        private static bool IsHexChar(char ch)
        {
            return ((uint)(ch - '0') <= 9 || (uint)(ch - 'a') <= 5 || (uint)(ch - 'A') <= 5);
        }

        private static bool IsOctalChar(char ch)
        {
            return ((uint)(ch - '0') <= 7);
        }

        private static bool IsDecimalChar(char ch)
        {
            return ((uint)(ch - '0') <= 9);
        }

        private static bool IsWordChar(char ch)
        {
            return ((uint)(ch - '0') <= 9 || (uint)(ch - 'a') <= 25 || (uint)(ch - 'A') <= 25 || ch == '_');
        }
        #endregion

        /*
         * Fills in an ArgumentException
         */
        private static ArgumentException MakeException(String message)
        {
            return new ArgumentException(message);
        }

        #region Capture Slot
        private bool IsCaptureSlot(int i)
        {
            if (i == -1)
                return false;
            return _capnumlist.Contains(i);
        }

        private bool IsCaptureSlot(string name)
        {
            if (name.Length == 0)
                return false;
            return _capnamelist.Contains(name);
        }

        internal void AssignCaptureSlot(int i)
        {
            if (i == -1)
                throw MakeException("InvalidBackRef");
            if (i == 0)
                throw MakeException("CapnumNotZero");
            if (!_capnumlist.Contains(i))
                _capnumlist.Add(i);
        }

        internal void AssignCaptureSlot(string name)
        {
            if (name == null)
                return;
            if (name.Length == 0)
                throw MakeException("InvalidNameRef");
            if (!_capnamelist.Contains(name))
                _capnamelist.Add(name);
        }

        internal void AddAnonymousCap()
        {
            AssignCaptureSlot(++anonymousCapCount);
        }
        #endregion

        #region Character Category

        /*
         * Returns true for those characters that terminate a string of ordinary chars.
         */
        private static bool IsSpecial(char ch)
        {
            return CharUtils.IsSpecial(ch);
        }

        /*
         * Returns true for those characters that terminate a string of ordinary chars.
         */
        private static bool IsStopperX(char ch)
        {
            return CharUtils.IsStopperX(ch);
        }

        /*
         * Returns true for those characters that begin a quantifier.
         */
        private static bool IsQuantifier(char ch)
        {
            return CharUtils.IsQuantifier(ch);
        }

        /*
         * Returns true for whitespace.
         */
        private static bool IsSpace(char ch)
        {
            return CharUtils.IsSpace(ch);
        }

        /*
         * Returns true for chars that should be escaped.
         */
        private static bool IsMetachar(char ch)
        {
            return CharUtils.IsMetachar(ch);
        }

        private bool IsTrueQuantifier()
        {
            int nChars = CharsRight();
            if (nChars == 0)
                return false;
            int startpos = Textpos();
            char ch = CharAt(startpos);
            int pos = startpos;
            while (--nChars > 0 && (ch = CharAt(++pos)) >= '0' && ch <= '9') ;
            if (nChars == 0 || pos - startpos == 1)
                return false;
            if (ch == '}')
                return true;
            if (ch != ',')
                return false;
            while (--nChars > 0 && (ch = CharAt(++pos)) >= '0' && ch <= '9') ;
            return nChars > 0 && ch == '}';
        }

        #endregion

        #region Text Flow Controller
        /*
         * Returns the current parsing position.
         */
        protected int Textpos()
        {
            return _currentPos;
        }

        /*
         * Zaps to a specific parsing position.
         */
        protected void Textto(int pos)
        {
            //CheckPos(pos);
            _currentPos = pos;
        }

        /*
         * Returns the char at the right of the current parsing position and advances to the right.
         */
        protected char MoveRightGetChar()
        {
            //CheckPos(_currentPos + 1);
            return _pattern[_currentPos++];
        }

        /*
         * Moves the current position to the right. 
         */
        protected void MoveRight()
        {
            //CheckPos(_currentPos + 1);
            MoveRight(1);
        }

        protected void MoveRight(int i)
        {
            //CheckPos(_currentPos + i);
            _currentPos += i;
        }

        /*
         * Moves the current parsing position one to the left.
         */
        protected void MoveLeft()
        {
            //CheckPos(_currentPos - 1);
            --_currentPos;
        }

        /*
         * Returns the char left of the current parsing position.
         */
        protected char CharAt(int i)
        {
            //CheckPos(i);
            return _pattern[i];
        }

        /*
         * Returns the char right of the current parsing position.
         */
        protected char RightChar()
        {
            if (_currentPos >= _pattern.Length)
                return '\0';
            return _pattern[_currentPos];
        }

        /*
         * Returns the char i chars right of the current parsing position.
         */
        protected char RightChar(int i)
        {
            //CheckPos(_currentPos + i);
            return _pattern[_currentPos + i];
        }

        /*
         * Number of characters to the right of the current parsing position.
         */
        protected int CharsRight()
        {
            return _pattern.Length - _currentPos;
        }

        private string GetSubstring(int start, int length)
        {
            return _pattern.Substring(start, length);
        }
        #endregion

    }
}
