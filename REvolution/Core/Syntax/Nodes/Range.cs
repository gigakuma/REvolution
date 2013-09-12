using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Range : Node
    {
        public override NodeType Type
        {
            get { return NodeType.Range; }
        }

        private char _start;
        private char _end;

        public char StartChar
        {
            get { return _start; }
        }

        public char EndChar
        {
            get { return _end; }
        }

        public Range(NodeParent parent, Token start, Token subtract, Token end)
            : base(parent)
        {
            Tokens.Add(start);
            Tokens.Add(subtract);
            Tokens.Add(end);
            _start = start.ReadChar();
            _end = end.ReadChar();
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            string esc1 = CharUtils.EscapeWhiteSpace(Tokens[0].Text[0]);
            if (esc1 == null)
                builder.Append(Tokens[0].Text);
            else
                builder.Append(esc1);
            builder.Append("-");
            string esc2 = CharUtils.EscapeWhiteSpace(Tokens[2].Text[0]);
            if (esc2 == null)
                builder.Append(Tokens[2].Text);
            else
                builder.Append(esc2);
            return builder.ToString();
        }
    }
}
