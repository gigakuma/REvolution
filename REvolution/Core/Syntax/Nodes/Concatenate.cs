using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Concatenate : NodeParent
    {
        private List<Node> _children = new List<Node>();

        public Concatenate(NodeParent parent)
            : base(parent)
        { }

        public override NodeType Type
        {
            get { return NodeType.Concatenate; }
        }

        public override IEnumerable<Node> Children
        {
            get { return _children; }
        }

        public Node this[int index]
        {
            get { return _children[index]; }
            set { _children[index] = value; }
        }

        public int Count
        {
            get { return _children.Count; }
        }

        public Node Reduce()
        {
            if (_children.Count == 1)
                return _children[0];
            else
                return this;
        }

        public override void AddChild(Node child)
        {
            _children.Add(child);
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Node node in _children)
                builder.Append(node.Generate(context));
            return builder.ToString();
        }
    }
}
