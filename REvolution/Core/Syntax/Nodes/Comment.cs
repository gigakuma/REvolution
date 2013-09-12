using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Comment : Node
    {
        public Comment(NodeParent parent)
            : base(parent)
        { }

        public override NodeType Type
        {
            get { return NodeType.Comment; }
        }

        public string _text;

        public string Text
        {
            get { return _text; }
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(?#").Append(Text).Append(")");
            return builder.ToString();
        }
    }
}
