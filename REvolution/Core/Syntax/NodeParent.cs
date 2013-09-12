using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    abstract public class NodeParent : Node
    {
        public NodeParent(NodeParent parent)
            : base(parent)
        { }

        public NodeParent()
            : this(null)
        { }

        public abstract IEnumerable<Node> Children
        {
            get;
        }

        public abstract void AddChild(Node child);
    }
}
