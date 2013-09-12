using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Syntax.Nodes;

namespace REvolution.Core.Syntax
{
    public class Definition
    {
        private int _capnum;
        private string _capname;

        private const int Nothing = -1;

        public Definition(int capnum)
            : this(capnum, DefinitionType.Record)
        { }

        public Definition(string capname)
            : this(capname, DefinitionType.Record)
        { }

        public Definition(int capnum, string capname)
            : this(capnum, capname, DefinitionType.Record)
        { }

        public Definition(int capnum, DefinitionType type)
        {
            _capnum = capnum;
            _capname = null;
            _type = type;
        }

        public Definition(string capname, DefinitionType type)
        {
            _capnum = Nothing;
            _capname = capname;
            _type = type;
        }

        public Definition(int capnum, string capname, DefinitionType type)
        {
            if (capnum < 0)
            {
                _capnum = Nothing;
                _capname = capname;
            }
            else
            {
                _capnum = capnum;
                _capname = null;
            }
            _type = type;
        }

        private DefinitionType _type;

        public DefinitionType Type
        {
            get { return _type; }
        }

        public bool IsNumber
        {
            get { return _capnum != Nothing; }
        }

        public bool IsName
        {
            get { return _capnum == Nothing; }
        }

        public int Number
        {
            get { return _capnum; }
            set 
            {
                if (value < 0)
                    throw new ArgumentException("InvalidCaptureNumber");
                _capnum = value;
                _capname = null;
            }
        }

        public string Name
        {
            get { return _capname; }
            set 
            {
                if (value == null || value == string.Empty)
                    throw new ArgumentException("InvalidCaptureName");
                _capnum = Nothing;
                _capname = value;
            }
        }

        public string Generate()
        {
            if (IsName)
                return Name;
            else
                return Number.ToString();
        }

        private Capture _source;

        public Capture Source
        {
            get { return _source; }
            set { _source = value; }
        }
    }
}
