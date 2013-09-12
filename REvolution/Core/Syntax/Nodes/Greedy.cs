using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Greedy : NodeGroup
    {
        public Greedy(NodeParent parent)
            : base(parent)
        { }

        public override NodeType Type
        {
            get { return NodeType.Greedy; }
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(?>").Append(Child.Generate(context)).Append(")");
            return builder.ToString();
        }
    }
}
