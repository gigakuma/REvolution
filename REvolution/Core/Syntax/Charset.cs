using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Syntax
{
    public class Charset
    {
        private SetType _type;
        private string _property;

        public SetType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Property
        {
            get { return _property; }
            set { _property = value; }
        }

        public Charset()
        {
            _type = SetType.Wildcard;
            _property = null;
        }

        public Charset(SetType type, bool neg)
        {
            _type = type;
            if (neg)
                _type |= SetType.Not;
            _property = null;
        }

        public Charset(string property, bool neg)
        {
            _type = SetType.Property;
            if (neg)
                _type |= SetType.Not;
            _property = property;
        }

        public Charset(string posix)
        {
            _type = SetType.Posix;
            _property = posix;
        }

        public string Generate()
        {
            StringBuilder builder = new StringBuilder();
            bool not = Type.HasFlag(SetType.Not);
            switch (Type & SetType.Category)
            {
                case SetType.Wildcard:
                    builder.Append('.');
                    break;
                case SetType.Word:
                    if (not)
                        builder.Append("\\W");
                    else
                        builder.Append("\\w");
                    break;
                case SetType.Digit:
                    if (not)
                        builder.Append("\\D");
                    else
                        builder.Append("\\d");
                    break;
                case SetType.Space:
                    if (not)
                        builder.Append("\\S");
                    else
                        builder.Append("\\s");
                    break;
                case SetType.Property:
                    if (not)
                        builder.Append("\\P");
                    else
                        builder.Append("\\p");
                    builder.Append("{").Append(Property).Append("}");
                    break;
                case SetType.Posix:
                    builder.Append("[:").Append(Property).Append(":]");
                    break;
            }
            return builder.ToString();
        }
    }
}
