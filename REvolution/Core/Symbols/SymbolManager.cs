using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Symbols
{
    public class SymbolManager
    {
        private Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>();

        public static readonly string FieldSpliter = "/";

        public Expression ParseKey(string key)
        {
            string[] spliter = new string[] { FieldSpliter };
            string[] subs = key.Split(spliter, 2, StringSplitOptions.None);
            string name = subs[0];
            if (!Contains(name))
                throw new ArgumentException("Symbol not exist");
            Symbol sym = _symbols[name];
            if (subs.Length == 1)
                return sym.Default;
            else
            {
                string label = subs[1];
                if (!sym.Contains(label))
                    throw new ArgumentException("Expression not exist");
                return sym[label];
            }
        }

        public static string GenerateKey(string name, string label)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name).Append(FieldSpliter).Append(label);
            return builder.ToString();
        }

        public Symbol RegisterSymbol(string name)
        {
            Symbol.CheckName(name);
            if (_symbols.ContainsKey(name))
                throw new ArgumentException("Symbol has been exist.");
            Symbol sym = new Symbol(name, this);
            _symbols.Add(name, sym);
            return sym;
        }

        public void UnregisterSymbol(string name)
        {
            if (!_symbols.ContainsKey(name))
                return;
            _symbols.Remove(name);
        }

        public Symbol this[string name]
        {
            get { return _symbols[name]; }
        }

        public bool Contains(string name)
        {
            return _symbols.ContainsKey(name);
        }

        internal bool ChangeSymbolName(string oldn, string newn)
        {
            Symbol.CheckName(newn);
            if (!_symbols.ContainsKey(oldn))
                return false;
            else
            {
                Symbol sym = _symbols[oldn];
                _symbols.Remove(oldn);
                _symbols.Add(newn, sym);
                sym.Name = newn;
                return true;
            }
        }

        private Symbol _main;

        public Symbol MainSymbol
        {
            get { return _main; }
            set { _main = value; }
        }
    }
}
