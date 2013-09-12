using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Builder;
using System.Windows;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax
{
    public abstract class Node
    {
        public Node(NodeParent parent)
            : this(true)
        {
            
            Parent = parent;
            if (parent != null)
                Parent.AddChild(this);
        }

        public Node()
            : this(null)
        { }

        private Node(bool tokenize)
        {
            if (tokenize)
                _tokens = new List<Token>();
        }

        protected NodeParent _parent;

        protected Quantifier _quantifier;

        /// <summary>
        /// The parent of this node in syntax tree.
        /// </summary>
        public NodeParent Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// The type of this node.
        /// </summary>
        abstract public NodeType Type
        {
            get;
        }

        /// <summary>
        /// The quantifier of node. This could be null eternally because that 
        /// some type of nodes dosen't need quantifier.
        /// </summary>
        public Quantifier Quantifier
        {
            get { return _quantifier; }
            set { _quantifier = value; }
        }

        private List<Token> _tokens;

        /// <summary>
        /// The tokens of node contains.
        /// </summary>
        public List<Token> Tokens
        {
            get { return _tokens; }
        }

        /// <summary>
        /// Generate regex with quantifier.
        /// </summary>
        /// <returns></returns>
        public string Generate(GenerateContext context)
        {
            if (Quantifier == null)
                return GenerateContent(context);
            else
                return GenerateContent(context) + GenerateQuantifier();
        }

        /// <summary>
        /// Generate regex without quantifier.
        /// </summary>
        /// <param name="context">Context when generating</param>
        /// <returns></returns>
        public abstract string GenerateContent(GenerateContext context);
        /// <summary>
        /// Generate quantifier.
        /// </summary>
        /// <returns>If Quantifier is null, return empty string.</returns>
        public string GenerateQuantifier()
        {
            if (this.Quantifier == null)
                return string.Empty;
            return this.Quantifier.Generate();
        }
    }
}
