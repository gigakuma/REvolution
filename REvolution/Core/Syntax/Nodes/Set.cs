using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Set : Node
    {
        public override NodeType Type
        {
            get { return NodeType.Set; }
        }

        private Charset _charset;

        public Charset Charset
        {
            get { return _charset; }
        }

        public Set(NodeParent parent, Token token)
            : base(parent)
        {
            Tokens.Add(token);
            _charset = token.ReadCharset();
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Charset.Generate());
            return builder.ToString();
        }
    }
}
