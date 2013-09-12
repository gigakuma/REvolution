using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class One : Node
    {
        public override NodeType Type
        {
            get { return NodeType.One; }
        }

        private char _character;

        public char Character
        {
            get { return _character; }
        }

        public One(NodeParent parent, Token token)
            : base(parent)
        {
            Tokens.Add(token);
            _character = token.ReadChar();
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            if (context == null || context.Mode == GenerateMode.SingleLine)
                builder.Append(Tokens[0].Text);
            else
            {
                string esc = CharUtils.EscapeWhiteSpace(Tokens[0].Text[0]);
                if (esc == null)
                    builder.Append(Tokens[0].Text);
                else
                    builder.Append(esc);
            }
            return builder.ToString();
        }
    }
}
