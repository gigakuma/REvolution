using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Symbols;

namespace REvolution.Core.Syntax
{
    public class Token
    {
        private TokenType _type;
        private int _startPos;
        private string _text;

        private object _tag;

        public static readonly Token Empty = new Token(TokenType.Empty, null, -1);

        public Token(TokenType type, string text, int start, object tag)
        {
            _type = type;
            _text = text;
            _startPos = start;
            _tag = tag;
        }

        public Token(TokenType type, char ch, int start, object tag)
            : this(type, ch.ToString(), start, tag) { }

        public Token(TokenType type, char ch, int start)
            : this(type, ch.ToString(), start, null) { }

        public Token(TokenType type, string text, int start)
            : this(type, text, start, null) { }

        public int StartPosition
        { get { return _startPos; } }

        public string Text
        { get { return _text; } }

        public int Length
        { get { return _text.Length; } }

        public int EndPosition
        { get { return StartPosition + Length - 1; } }

        public TokenType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public override string ToString()
        {
            return Text;
        }

        public char ReadChar()
        {
            if (_type != TokenType.Char)
                throw new MethodAccessException("NotCharToken");
            return (char)_tag;
        }

        public AssertDirection ReadAssertDirection()
        {
            if (_type != TokenType.Require && _type != TokenType.Prevent)
                throw new MethodAccessException("NotAssertToken");
            return (AssertDirection)_tag;
        }

        public Quantifier ReadQuantifier()
        {
            if (_type != TokenType.Quantifier)
                throw new MethodAccessException("NotQuantifierToken");
            return (Quantifier)_tag;
        }

        public AnchorType ReadAnchorType()
        {
            if (_type != TokenType.Anchor)
                throw new MethodAccessException("NotAnchorToken");
            return (AnchorType)_tag;
        }

        public Definition ReadReference()
        {
            if (_type != TokenType.Reference)
                throw new MethodAccessException("NotReferenceToken");
            return (Definition)_tag;
        }

        public Definition ReadCaptureDef()
        {
            if (_type != TokenType.Name)
                throw new MethodAccessException("NotNameToken");
            return ((Definition[])_tag)[0];
        }

        public Definition ReadUncaptureDef()
        {
            if (_type != TokenType.Name)
                throw new MethodAccessException("NotNameToken");
            return ((Definition[])_tag)[1];
        }

        public OptionType ReadOnOptions()
        {
            if (_type != TokenType.Options)
                throw new MethodAccessException("NotOptionsToken");
            return ((OptionType[])_tag)[0];
        }

        public OptionType ReadOffOptions()
        {
            if (_type != TokenType.Options)
                throw new MethodAccessException("NotOptionsToken");
            return ((OptionType[])_tag)[1];
        }

        public Charset ReadCharset()
        {
            if (_type != TokenType.Set)
                throw new MethodAccessException("NotSetToken");
            return (Charset)_tag;
        }

        public Expression ReadExpression()
        {
            if (_type != TokenType.Extention)
                throw new MethodAccessException("NotSetToken");
            return (Expression)_tag;
        }
    }
}
