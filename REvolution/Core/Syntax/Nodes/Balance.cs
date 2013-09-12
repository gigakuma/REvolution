using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Balance : Capture
    {
        private Definition _uncapName = null;

        public Definition UncapName
        {
            get { return _uncapName; }
        }

        public override NodeType Type
        {
            get { return NodeType.Balance; }
        }

        public Balance(NodeParent parent, Definition cap, Definition uncap)
            : base(parent, cap)
        {
            _uncapName = uncap;
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(?<");
            if (CapName != null)
                builder.Append(CapName.Generate());
            if (UncapName != null)
                builder.Append("-").Append(UncapName.Generate());
            builder.Append(">").Append(Child.Generate(context)).Append(")");
            return builder.ToString();
        }
    }
}
