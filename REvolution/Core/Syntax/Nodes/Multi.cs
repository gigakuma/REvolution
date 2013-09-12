using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Multi : Node
    {
        private StringBuilder _text = new StringBuilder();

        public override NodeType Type
        {
            get { return NodeType.Multi; }
        }

        public string Text
        {
            get { return _text.ToString(); }
        }

        public Multi(NodeParent parent)
            : base(parent)
        { }

        public void AddChar(Token token)
        {
            _text.Append(token.ReadChar());
            Tokens.Add(token);
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            if (context == null || context.Mode == GenerateMode.SingleLine)
            {
                foreach (Token token in Tokens)
                    builder.Append(token.Text);
            }
            else
            {
                foreach (Token token in Tokens)
                {
                    string esc = CharUtils.EscapeWhiteSpace(token.Text[0]);
                    if (esc == null)
                        builder.Append(token.Text);
                    else
                        builder.Append(esc);
                }
            }
            return builder.ToString();
        }
    }
}
