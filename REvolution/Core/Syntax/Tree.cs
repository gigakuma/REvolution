using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Symbols;
using REvolution.Core.Syntax.Nodes;
using REvolution.Core.Builder;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax
{
    public class Tree
    {
        private List<object> _sequence;

        internal Tree(Node root, List<Token> tokens, HashSet<Expression> exps, List<object> sequence)
        {
            _root = root;
            _tokens = tokens;
            _exps = exps;
            _sequence = sequence;
        }

        private Node _root;

        public Node Root
        {
            get { return _root; }
        }

        private List<Token> _tokens;

        public List<Token> Tokens
        {
            get { return _tokens; }
        }

        private HashSet<Expression> _exps;

        public HashSet<Expression> Expressions
        {
            get { return _exps; }
        }

        public List<object> Sequence
        {
            get { return _sequence; }
        }
    }
}
