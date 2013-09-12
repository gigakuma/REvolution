using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Syntax;
using REvolution.Core.Syntax.Nodes;
using REvolution.Core.Symbols;
using System.Diagnostics;

namespace REvolution.Core.Builder
{
    internal class Parser
    {
        private NodeGroup _group = null;
        private Alternate _alternate = null;
        private Concatenate _concatenate = null;
        private Node _unit = null;

        private Lexer _lexer;

        private SymbolManager _manager;

        private Queue<Token> _tokenBuffer = new Queue<Token>();
        private List<Token> _tokens = new List<Token>();
        private HashSet<Expression> _exps = new HashSet<Expression>();
        private List<object> _sequence = new List<object>();

        private Slots _slots = new Slots();

        public Parser(string pattern)
        {
            _lexer = new Lexer(pattern);
            _lexer.ExtentionHandlers += new Lexer.ExtentionHandler(ParseExtention);
        }

        public Parser(string pattern, SymbolManager manager)
            : this(pattern)
        {
            _manager = manager;
        }

        private Expression ParseExtention(Lexer lexer, string text)
        {
            Expression exp = _manager.ParseKey(text);
            return exp;
        }

        #region Token Read Stream
        public Token GetToken()
        {
            Token buff;
            if (_tokenBuffer.Count != 0)
                buff = _tokenBuffer.Dequeue();
            else
                buff = _lexer.GetNextToken();
            _tokens.Add(buff);
            return buff;
        }

        public Token PeekToken()
        {
            if (_tokenBuffer.Count == 0)
                _tokenBuffer.Enqueue(_lexer.GetNextToken());
            return _tokenBuffer.Peek();
        }

        public Token PeekToken(int skip)
        {
            if (skip < 0)
                throw MakeException("ArgumentOutOfRange");
            if (skip >= _tokenBuffer.Count)
            {
                int record = skip;
                int target = _tokenBuffer.Count;
                while (record-- >= target)
                    _tokenBuffer.Enqueue(_lexer.GetNextToken());
            }
            Queue<Token>.Enumerator en = _tokenBuffer.GetEnumerator();
            for (int i = 0; i <= skip; i++)
                en.MoveNext();
            return en.Current;
        }
        #endregion

        #region Parse
        public Tree Parse()
        {
            Initial();
            Token token = null;
            while ((token = GetToken()) != Token.Empty)
            {
                switch (token.Type)
                {
                    case TokenType.Char:
                        ParseMulti(token);
                        if (PeekToken().Type == TokenType.Char)
                            AddUnitChar(GetToken());
                        break;
                    case TokenType.Set:
                        AddUnitSet(token);
                        break;
                    case TokenType.Anchor:
                        AddUnitAnchor(token);
                        break;
                    case TokenType.Quantifier:
                        AddQuantifier(token);
                        break;
                    case TokenType.ParenL:
                        StartGroup(token);
                        PushGroup();
                        break;
                    case TokenType.ParenR:
                        PopGroup();
                        FinishGroup(token);
                        break;
                    case TokenType.BracketL:
                        ParseCharClass(token);
                        break;
                    case TokenType.Reference:
                        AddUnitReference(token);
                        break;
                    case TokenType.VerticalBar:
                        AddBranch();
                        // VerticalBar token belongs to group node
                        _group.Tokens.Add(token);
                        break;
                    default:
                        throw MakeException("InternalError");
                }
            }
            Debug.Assert(_group != null);
            // just clear
            _slots.Clear();
            return new Tree(_group, _tokens, _exps, _sequence);
        }

        private void Initial()
        {
            _sequence.Clear();
            _tokenBuffer.Clear();
            _tokens.Clear();
            _exps.Clear();
            _lexer.Reset();
            _group = new Capture(null, _lexer.AnonymousCapCount);
            _alternate = new Alternate(_group);
            _concatenate = new Concatenate(_alternate);
        }

        #region group

        // call this before push group
        private void StartGroup(Token paren)
        {
            Token token = PeekToken();
            if (token.Type == TokenType.Question)
            {
                Token spec = PeekToken(1);
                switch (spec.Type)
                {
                    // (?:exp)
                    case TokenType.Colon:
                        _unit = new Group(_concatenate);
                        _unit.Tokens.Add(paren);
                        // question
                        _unit.Tokens.Add(GetToken());
                        // colon
                        _unit.Tokens.Add(GetToken());
                        break;
                    // (?'name'exp)
                    case TokenType.Name:
                        _unit = CreateCapture(_concatenate, spec.ReadCaptureDef(), spec.ReadUncaptureDef());
                        // question
                        _unit.Tokens.Add(GetToken());
                        // name
                        _unit.Tokens.Add(GetToken());
                        break;
                    // (?=exp)
                    case TokenType.Require:
                        _unit = new Require(_concatenate, spec.ReadAssertDirection());
                        // question
                        _unit.Tokens.Add(GetToken());
                        // require
                        _unit.Tokens.Add(GetToken());
                        break;
                    // (?!exp)
                    case TokenType.Prevent:
                        _unit = new Prevent(_concatenate, spec.ReadAssertDirection());
                        // question
                        _unit.Tokens.Add(GetToken());
                        // prevent
                        _unit.Tokens.Add(GetToken());
                        break;
                    // (?>exp)
                    case TokenType.Greedy:
                        _unit = new Greedy(_concatenate);
                        // question
                        _unit.Tokens.Add(GetToken());
                        // greedy
                        _unit.Tokens.Add(GetToken());
                        break;
                    // (?(name)yes|no)
                    case TokenType.Reference:
                        Definition id = spec.ReadReference();
                        Definition source = _slots.FindAssigned(id);
                        if (source != null)
                            id.Source = source.Source;
                        else
                            throw new ArgumentException("Internal Error");
                        _unit = new Test(_concatenate, spec, id);
                        _unit.Tokens.Add(paren);
                        // question
                        _unit.Tokens.Add(GetToken());
                        // just skip (ref)
                        GetToken();
                        break;
                    // (?#...)
                    case TokenType.Comment:
                        // CANNOT set unit
                        Node com = new Comment(_concatenate);
                        com.Tokens.Add(paren);
                        // question
                        _unit.Tokens.Add(GetToken());
                        // sharp
                        _unit.Tokens.Add(GetToken());
                        if (PeekToken().Type == TokenType.CommentText)
                            com.Tokens.Add(GetToken());
                        // ParenR
                        com.Tokens.Add(GetToken());
                        break;
                    // (?@...)
                    case TokenType.Field:
                        _unit = new Field(_concatenate);
                        _unit.Tokens.Add(paren);
                        // question
                        _unit.Tokens.Add(GetToken());
                        // special ext (@)
                        _unit.Tokens.Add(GetToken());
                        if (PeekToken().Type != TokenType.Extention)
                            throw MakeException("FieldNoSymbol");
                        // Extention (Symbol)
                        Expression exp = PeekToken().ReadExpression();
                        (_unit as Field).Expression = exp;
                        // Recording to the list
                        _exps.Add(exp);
                        // add this exp to sequence
                        _sequence.Add(exp);
                        // extention content
                        _unit.Tokens.Add(GetToken());
                        // ParenR
                        _unit.Tokens.Add(GetToken());
                        break;
                    case TokenType.Options:
                        // (?i-m:exp)
                        if (PeekToken(2).Type == TokenType.Colon)
                        {
                            _unit = new Group(_concatenate, new Options((_unit as NodeGroup), spec));
                            _unit.Tokens.Add(paren);
                            // quesion
                            _unit.Tokens.Add(token);
                            // options (skip)
                            GetToken();
                            // colon
                            _unit.Tokens.Add(GetToken());
                        }
                        // (?i-m)
                        else if (PeekToken(2).Type == TokenType.ParenR)
                        {
                            // CANNOT set unit
                            Node op = new Options(_concatenate, spec.ReadOnOptions(), spec.ReadOffOptions());
                            op.Tokens.Add(paren);
                            // question
                            op.Tokens.Add(GetToken());
                            // options
                            op.Tokens.Add(GetToken());
                            // paren
                            op.Tokens.Add(GetToken());
                        }
                        else
                            throw MakeException("InvalidGroupOptions");
                        break;
                    // (?(exp)yes|no)
                    case TokenType.ParenL:
                        _unit = new Test(_concatenate);
                        _unit.Tokens.Add(paren);
                        // question
                        _unit.Tokens.Add(GetToken());
                        // go into (skip Test Node) condition child
                        _unit = (_unit as Test).Condition;
                        // paren L
                        _unit.Tokens.Add(GetToken());
                        throw new ArgumentException("The Test Group hasn't been supported well!");
                        //break;
                }
                return;
            }
            else
            {
                _unit = CreateCapture(_concatenate);
                _unit.Tokens.Add(paren);
            }

        }

        // call this after pop group
        private void FinishGroup(Token paren)
        {
            _unit.Tokens.Add(paren);
            if (_unit.Parent.Type == NodeType.Test)
                _unit = null;
        }

        private void PushGroup()
        {
            if (_unit.Type == NodeType.Comment || _unit.Type == NodeType.Field)
                return;
            _group = _unit as NodeGroup;
            _alternate = new Alternate(_group);
            _concatenate = new Concatenate(_alternate);
            _unit = null;
        }

        private void PopGroup()
        {
            _unit = _group;
            // Test Node's exp condition
            if (_unit.Parent.Type == NodeType.Test)
            {
                _group = _unit.Parent as NodeGroup;
                _alternate = new Alternate(_group);
                _concatenate = new Concatenate(_alternate);
            }
            else
            {
                _concatenate = _unit.Parent as Concatenate;
                _alternate = _concatenate.Parent as Alternate;
                _group = _alternate.Parent as NodeGroup;
            }
        }

        #endregion

        #region nodes

        private void ParseCharClass(Token bracket)
        {
            CharClass _charclass;
            NodeParent _parent = _concatenate;
            Token next = PeekToken();
            if (next.Type == TokenType.Negative)
            {
                _charclass = new CharClass(_parent, true);
                _charclass.Tokens.Add(bracket);
                // negative
                _charclass.Tokens.Add(GetToken());
            }
            else
            {
                _charclass = new CharClass(_parent, false);
                _charclass.Tokens.Add(bracket);
            }
            _parent = _charclass;
            while (_parent.Type == NodeType.CharClass)
            {
                Token token = GetToken();
                switch (token.Type)
                {
                    case TokenType.Char:
                        if (PeekToken().Type == TokenType.Hyphen)
                        {
                            // error has been handled in lexer, so just get token!LOL
                            new Range(_parent, token, GetToken(), GetToken());
                        }
                        else
                            new One(_parent, token);
                        break;
                    case TokenType.Set:
                        new Set(_parent, token);
                        break;
                    case TokenType.Subtract:
                        _parent.Tokens.Add(token);
                        CharClass _sub;
                        // check token after bracket L
                        if (PeekToken(1).Type == TokenType.Negative)
                        {
                            _sub = new CharClass(_parent, true);
                            // bracket L
                            _sub.Tokens.Add(GetToken());
                            // negative
                            _sub.Tokens.Add(GetToken());
                        }
                        else
                        {
                            _sub = new CharClass(_parent, false);
                            // bracket L
                            _sub.Tokens.Add(GetToken());
                        }
                        (_parent as CharClass).Subtract(_sub);
                        _parent = _sub;
                        break;
                    case TokenType.BracketR:
                        _parent.Tokens.Add(token);
                        // finish this level charclass
                        _parent = _parent.Parent;
                        break;
                    default:
                        throw MakeException("InternalError");
                }
            }
            _unit = _charclass;
        }

        private void ParseMulti(Token start)
        {
            Token next = PeekToken();
            if (next.Type != TokenType.Char)
            {
                AddUnitChar(start);
                return;
            }
            Multi node = new Multi(_concatenate);
            node.AddChar(start);
            while (PeekToken().Type == TokenType.Char && PeekToken(1).Type != TokenType.Quantifier)
                node.AddChar(GetToken());
        }

        private void AddUnitChar(Token token)
        {
            _unit = new One(_concatenate, token);
        }

        private void AddUnitSet(Token token)
        {
            _unit = new Set(_concatenate, token);
        }

        private void AddUnitReference(Token token)
        {
            _unit = CreateReference(_concatenate, token);
            _sequence.Add(_unit);
        }

        private Reference CreateReference(NodeParent parent, Token token)
        {
            Definition id = token.ReadReference();
            Definition source = _slots.FindAssigned(id);
            if (source != null)
                id.Source = source.Source;
            else
                throw new ArgumentException("Internal Error");
            return new Reference(parent, token, id);
        }

        private void AddQuantifier(Token token)
        {
            if (_unit == null)
                throw MakeException("QuantifyAfterNothing");
            _unit.Tokens.Add(token);
            _unit.Quantifier = token.ReadQuantifier();
        }

        private void AddUnitAnchor(Token token)
        {
            _unit = new Anchor(_concatenate, token);
        }

        private void AddBranch()
        {
            if (_group.Type == NodeType.Test && _alternate.Count == 2)
                    throw MakeException("TooManyAlternates");
            _concatenate = new Concatenate(_alternate);
            _unit = null;
        }

        private Capture CreateCapture(NodeParent parent, Definition cap, Definition uncap)
        {
            if (uncap != null)
            {
                Balance balance = new Balance(_concatenate, cap, uncap);
                _slots.Assign(balance);
                _sequence.Add(balance);
                return balance;
            }
            else
            {
                Capture capture = new Capture(_concatenate, cap);
                _slots.Assign(capture);
                _sequence.Add(capture);
                return capture;
            }
        }

        private Capture CreateCapture(NodeParent parent)
        {
            Capture capture = new Capture(_concatenate, _lexer.AnonymousCapCount);
            _slots.Assign(capture);
            _sequence.Add(capture);
            return capture;
        }
        #endregion

        #endregion

        private static ArgumentException MakeException(String message)
        {
            return new ArgumentException(message);
        }
    }
}
