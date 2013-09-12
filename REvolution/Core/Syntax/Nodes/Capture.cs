using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Capture : NodeGroup
    {
        private Definition _capName = null;

        public override NodeType Type
        {
            get { return NodeType.Capture; }
        }

        public Definition CapName
        {
            get { return _capName; }
        }

        private bool _anonymous;

        public bool Anonymous
        {
            get { return _anonymous; }
        }

        public Capture(NodeParent parent, int anonymous)
            : base(parent)
        {
            _capName = new Definition(anonymous);
            _capName.Source = this;
            _anonymous = true;
        }

        public Capture(NodeParent parent, Definition capname)
            : base(parent)
        {
            _capName = capname;
            _capName.Source = this;
            _anonymous = false;
        }

        public override string GenerateContent(GenerateContext context)
        {
            if (Anonymous && CapName.Number == 0)
                return Child.Generate(context);
            StringBuilder builder = new StringBuilder();
            builder.Append("(");
            if (!Anonymous)
                builder.Append("?<").Append(CapName.Generate()).Append(">");
            builder.Append(Child.Generate(context));
            builder.Append(")");
            return builder.ToString();
        }
    }
}
