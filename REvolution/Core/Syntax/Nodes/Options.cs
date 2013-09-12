using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Generator;

namespace REvolution.Core.Syntax.Nodes
{
    public class Options : Node
    {
        public Options(NodeParent parent, Token token)
            : base(parent)
        {
            Tokens.Add(token);
            _onOptions = token.ReadOnOptions();
            _offOptions = token.ReadOffOptions();
        }

        public Options(NodeParent parent, OptionType on, OptionType off)
            : base(parent)
        {
            _onOptions = on;
            _offOptions = off;
        }

        private OptionType _onOptions;

        private OptionType _offOptions;

        public OptionType OnOptions
        {
            get { return _onOptions; }
        }

        public OptionType OffOptions
        {
            get { return _offOptions; }
        }

        public override NodeType Type
        {
            get { return NodeType.Options; }
        }

        public static string OptionsToString(OptionType options)
        {
            if (options == OptionType.None)
                return string.Empty;
            StringBuilder builder = new StringBuilder();
            if (options.HasFlag(OptionType.IgnoreCase))
                builder.Append("i");
            if (options.HasFlag(OptionType.Multiline))
                builder.Append("m");
            if (options.HasFlag(OptionType.ExplicitCapture))
                builder.Append("n");
            if (options.HasFlag(OptionType.Compiled))
                builder.Append("c");
            if (options.HasFlag(OptionType.Singleline))
                builder.Append("s");
            if (options.HasFlag(OptionType.IgnorePatternWhitespace))
                builder.Append("x");
            if (options.HasFlag(OptionType.RightToLeft))
                builder.Append("r");
            if (options.HasFlag(OptionType.ECMAScript))
                builder.Append("e");
            return builder.ToString();
        }

        public string GetString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(OptionsToString(OnOptions));
            if (OffOptions != OptionType.None)
                builder.Append("-").Append(OptionsToString(OffOptions));
            return builder.ToString();
        }

        public override string GenerateContent(GenerateContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(?").Append(GetString()).Append(")");
            return builder.ToString();
        }
    }
}
