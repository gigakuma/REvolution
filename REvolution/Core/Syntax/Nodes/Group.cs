using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Group : NodeGroup
    {
        public Group(NodeParent parent)
            : base(parent)
        { }

        public Group(NodeParent parent, Options options)
            : base(parent)
        {
            _optionChild = options;
        }

        private Options _optionChild;

        public Options Options
        {
            get { return _optionChild; }
        }

        public override NodeType Type
        {
            get { return NodeType.Group; }
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                yield return Options;
                yield return Child;
            }
        }

        public override string GenerateContent(GenerateContext context)
        {
            if (Parent.Type == NodeType.Test)
                return Child.Generate(context);
            StringBuilder builder = new StringBuilder();
            builder.Append("(?");
            if (Options != null)
                builder.Append(Options.GetString());
            builder.Append(":").Append(Child.Generate(context)).Append(")");
            return builder.ToString();
        }
    }
}
