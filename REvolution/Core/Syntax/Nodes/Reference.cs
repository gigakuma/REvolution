using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Reference : Node
    {
        private Definition _capname;

        public override NodeType Type
        {
            get { return NodeType.Reference; }
        }

        public Definition CapName
        {
            get { return _capname; }
        }

        public Reference(NodeParent parent, Token token, Definition capname)
            : base(parent)
        {
            Tokens.Add(token);
            _capname = capname;
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            // if is (?:...) then the capnum maybe changed, so use source's identifier to generate this content.
            if (CapName.Source.Anonymous == true)
            {
                int capnum = context.FindCaptureIndex(CapName.Source);
                if (capnum == -1)
                    throw new ArgumentException("Internal Error");
                builder.Append("\\").Append(capnum);
            }
            else
                builder.Append("\\k<").Append(CapName.Generate()).Append(">");
            return builder.ToString();
        }
    }
}
