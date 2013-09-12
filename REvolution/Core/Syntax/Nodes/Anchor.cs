using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Anchor : Node
    {
        public override NodeType Type
        {
            get { return NodeType.Anchor; }
        }

        private AnchorType _anchorType;

        public AnchorType AnchorType
        {
            get { return _anchorType; }
        }

        public Anchor(NodeParent parent, Token token)
            : base (parent)
        {
            Tokens.Add(token);
            _anchorType = token.ReadAnchorType();
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            switch (_anchorType)
            {
                case AnchorType.Bol:
                    builder.Append("^");
                    break;
                case AnchorType.Eol:
                    builder.Append("$");
                    break;
                case AnchorType.Boundary:
                    builder.Append("\\b");
                    break;
                case AnchorType.Nonboundary:
                    builder.Append("\\B");
                    break;
                case AnchorType.Beginning:
                    builder.Append("\\A");
                    break;
                case AnchorType.Start:
                    builder.Append("\\G");
                    break;
                case AnchorType.EndZ:
                    builder.Append("\\Z");
                    break;
                case AnchorType.End:
                    builder.Append("\\z");
                    break;
            }
            return builder.ToString();
        }
    }
}
