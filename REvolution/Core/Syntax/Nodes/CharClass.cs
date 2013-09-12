using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class CharClass : NodeParent
    {
        public CharClass(NodeParent parent, bool neg)
        {
            Parent = parent;
            if (parent != null && parent.Type != NodeType.CharClass)
                parent.AddChild(this);
            _negative = neg;
        }

        public override NodeType Type
        {
            get { return NodeType.CharClass; }
        }

        private bool _negative = false;

        public bool Negative
        {
            get { return _negative; }
        }

        private List<Node> _children = new List<Node>();

        public List<Node> Inners
        {
            get { return _children; }
        }

        public List<One> Characters
        {
            get 
            { 
                List<One> list = new List<One>();
                foreach (Node node in _children)
                    if (node.Type == NodeType.One)
                        list.Add(node as One);
                return list;
            }
        }

        public List<Range> Ranges
        {
            get 
            {
                List<Range> list = new List<Range>();
                foreach (Node node in _children)
                    if (node.Type == NodeType.Range)
                        list.Add(node as Range);
                return list;
            }
        }

        public List<Set> Sets
        {
            get
            {
                List<Set> list = new List<Set>();
                foreach (Node node in _children)
                    if (node.Type == NodeType.Set)
                        list.Add(node as Set);
                return list;
            }
        }

        private CharClass _subtraction;

        public CharClass Subtraction
        {
            get { return _subtraction; }
        }

        public override IEnumerable<Node> Children
        {
            get 
            {
                foreach (Node node in _children)
                    yield return node;
                yield return _subtraction;
            }
        }

        public override void AddChild(Node child)
        {
            Inners.Add(child);
        }

        public void Subtract(CharClass node)
        {
            _subtraction = node;
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            if (Negative)
                builder.Append("^");
            foreach (Node node in Inners)
                builder.Append(node.GenerateContent(context));
            if (Subtraction != null)
                builder.Append("-").Append(Subtraction.GenerateContent(context));
            builder.Append("]");
            return builder.ToString();
        }
    }
}
