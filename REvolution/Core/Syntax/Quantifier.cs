using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    public class Quantifier
    {
        public const int Infinity = -1;

        private QuantifierType _type;

        private int _min = Infinity;

        private int _max = Infinity;

        public Quantifier(QuantifierType type)
        {
            _type = type;
        }

        public Quantifier(char special, bool lazy)
        {
            switch (special)
            {
                case '+':
                    _type = QuantifierType.Plus;
                    break;
                case '*':
                    _type = QuantifierType.Star;
                    break;
                case '?':
                    _type = QuantifierType.Question;
                    break;
                default:
                    _type = QuantifierType.Invalid;
                    break;
            }
            if (lazy)
                _type |= QuantifierType.Lazy;
        }

        public Quantifier(int min, int max, bool lazy)
        {
            _type = QuantifierType.Brace;
            if (lazy)
                _type |= QuantifierType.Lazy;
            _min = min;
            _max = max;
            if (min == Infinity)
            {
                if (max == Infinity)
                    throw new ArgumentException("UnknownQuantifierDefinition");
                _type |= QuantifierType.SingleDef;
            }
            else
            {
                if (max == Infinity)
                    _type |= QuantifierType.HalfDef;
                else
                    _type |= QuantifierType.RangeDef;
            }
        }

        public QuantifierType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public bool Lazy
        {
            get 
            { 
                return (_type & QuantifierType.Lazy) != 0; 
            }
            set 
            {
                if (value)
                    _type |= QuantifierType.Lazy;
                else
                    _type &= ~QuantifierType.Lazy;
            }
        }

        public QuantifierType DefinitionType
        {
            get { return _type & QuantifierType.Definition; }
        }

        public QuantifierType CategoryType
        {
            get { return _type & QuantifierType.Category; }
        }

        public int Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public int Max
        {
            get { return _max; }
            set { _max = value; }
        }

        public string Generate()
        {
            StringBuilder builder = new StringBuilder();
            switch (CategoryType)
            {
                case QuantifierType.Plus:
                    builder.Append("+");
                    break;
                case QuantifierType.Star:
                    builder.Append("*");
                    break;
                case QuantifierType.Question:
                    builder.Append("?");
                    break;
                case QuantifierType.Brace:
                    builder.Append("{");
                    switch (DefinitionType)
                    {
                        case QuantifierType.SingleDef:
                            builder.Append(Max);
                            break;
                        case QuantifierType.RangeDef:
                            builder.Append(Min);
                            builder.Append(",");
                            builder.Append(Max);
                            break;
                        case QuantifierType.HalfDef:
                            builder.Append(Min);
                            builder.Append(",");
                            break;
                    }
                    builder.Append("}");
                    break;
            }
            if (Lazy)
                builder.Append("?");
            return builder.ToString();
        }
    }
}
