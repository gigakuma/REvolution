using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Prevent : NodeGroup
    {
        public Prevent(NodeParent parent, AssertDirection direction)
            : base(parent)
        {
            _direction = direction;
        }

        public override NodeType Type
        {
            get { return NodeType.Prevent; }
        }

        private AssertDirection _direction;

        public AssertDirection Direction
        {
            get { return _direction; }
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(?");
            switch (_direction)
            {
                case AssertDirection.Backward:
                    builder.Append("<!");
                    break;
                case AssertDirection.Forward:
                    builder.Append("!");
                    break;
            }
            builder.Append(Child.Generate(context)).Append(")");
            return builder.ToString();
        }
    }
}
