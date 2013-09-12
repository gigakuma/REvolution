using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Syntax.Nodes;
using REvolution.Core.Symbols;

namespace REvolution.Core.Generator
{
    public class GenerateContext
    {
        public GenerateContext(Dictionary<Capture, int> anonys)
        {
            _level = 0;
            _anonys = anonys;
            _mode = GenerateMode.MultiLine;
        }

        public GenerateContext(Dictionary<Capture, int> anonys, GenerateMode mode)
        {
            _level = 0;
            _anonys = anonys;
            _mode = mode;
        }

        private Dictionary<Capture, int> _anonys;

        public int FindCaptureIndex(Capture capture)
        {
            if (!capture.Anonymous)
                throw new ArgumentException("Not Anonymous Definition Capture");
            if (!_anonys.ContainsKey(capture))
                return -1;
            return _anonys[capture];
        }

        private uint _level;
        public uint DeepLevel
        {
            get { return _level; }
        }

        public void Initialize(Expression exp)
        {
            _current = exp;
        }

        public void TraceIn(Expression exp)
        {
            _level++;
            _current = exp;
        }

        public void TraceOut()
        {
            _level--;
        }

        private GenerateMode _mode;
        public GenerateMode Mode
        {
            get { return _mode; }
        }

        private Expression _current;
        public Expression Current
        {
            get { return _current; }
        }

        private int _indentation = 4;
        public int Indentation
        {
            get { return _indentation; }
            set 
            {
                if (_indentation == value)
                    return;
                _indentation = value;
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < _indentation; i++)
                    builder.Append(' ');
                _indentSpace = builder.ToString();
            }
        }

        private string _indentSpace = "    ";
        public string IndentSpace
        {
            get { return _indentSpace; }
        }
    }

    public enum GenerateMode
    {
        MultiLine,
        SingleLine
    }
}
