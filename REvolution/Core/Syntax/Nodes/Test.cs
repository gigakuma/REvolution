using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Test : NodeGroup
    {
        public Test(NodeParent parent, Token token, Definition id)
            : base(parent)
        {
            _condition = new Reference(this, token, id);
        }

        public Test(NodeParent parent)
            : base(parent)
        {
            _condition = new Group(this);
        }

        private Node _condition;

        public override NodeType Type
        {
            get { return NodeType.Test; }
        }

        public Node Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        public override IEnumerable<Node> Children
        {
            get 
            {
                yield return _condition;
                yield return Child;
            }
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(?(");
            if (Condition.Type == NodeType.Reference)
                builder.Append((Condition as Reference).CapName.Generate());
            else
                builder.Append(Condition.Generate(context));
            builder.Append(")").Append(Child.Generate(context)).Append(")");
            return builder.ToString();
        }
    }
}
