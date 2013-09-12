using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    abstract public class NodeGroup : NodeParent
    {
        private Node _child;

        public Node Child
        {
            get { return _child; }
            set { _child = value; }
        }

        public NodeGroup(NodeParent parent)
            : base(parent)
        { }

        public NodeGroup()
            : base()
        { }

        public override IEnumerable<Node> Children
        {
            get { yield return Child; }
        }

        public override void AddChild(Node child)
        {
            Child = child;
        }
    }
}
