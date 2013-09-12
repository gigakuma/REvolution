using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Symbols;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Field : Node
    {
        public Field(NodeParent parent)
            : base(parent)
        { }

        public override NodeType Type
        {
            get { return NodeType.Field; }
        }

        private Expression _expression;
        public Expression Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }
        
        // TODO
        public override string GenerateContent(GenerateContext context)
        {
            context.TraceIn(_expression);
            StringBuilder builder = new StringBuilder();
            if (context.Mode == GenerateMode.SingleLine)
            {
                Capture root = _expression.SyntaxTree.Root as Capture;
                Alternate alter = root.Child as Alternate;
                if (alter.Count == 1 && (alter[0] as Concatenate).Count > 1 || alter.Count > 1)
                    builder.Append("(?:").Append(_expression.Generate(context)).Append(")");
                else
                    builder.Append(_expression.Generate(context));
            }
            else
            {
                // prepare the indentation space
                StringBuilder indent = new StringBuilder();
                for (int i = 0; i < context.DeepLevel; i++)
                    indent.Append(context.IndentSpace);
                // [indent]#symbol/label : comment
                builder.Append('\n').Append(indent).Append('#').Append(_expression.Key).Append(" : ").Append(_expression.Group.Comment).Append('\n');
                builder.Append(indent);
                Capture root = _expression.SyntaxTree.Root as Capture;
                Alternate alter = root.Child as Alternate;
                // if not single element or have exp reference, create a group
                if (alter.Count == 1 && (alter[0] as Concatenate).Count > 1 || alter.Count > 1 || _expression.SyntaxTree.Expressions.Count > 0)
                {
                    builder.Append("(?:").Append(_expression.Generate(context));
                    // if not refer other exps, don't new line
                    if (_expression.SyntaxTree.Expressions.Count > 0)
                        builder.Append('\n').Append(indent);
                    builder.Append(')').Append('\n');
                }
                else
                    builder.Append(_expression.Generate(context)).Append('\n');
                
                // add indentation for super level
                for (int i = 0; i < context.DeepLevel - 1; i++)
                    builder.Append(context.IndentSpace);
            }
            context.TraceOut();
            return builder.ToString();
        }
    }
}
