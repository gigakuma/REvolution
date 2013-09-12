using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core.Symbols
{
    public delegate void SymbolNameChangedHandler();
    public class Symbol
    {
        private Symbol()
        { }

        internal Symbol(string name, SymbolManager manager)
        {
            _name = name;
            _manager = manager;
        }

        private SymbolManager _manager;

        private int _counter = 0;

        private string _name;
        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        public event SymbolNameChangedHandler SymbolNameChanged;

        private void TriggerSymbolNameChanged()
        {
            SymbolNameChangedHandler handler = SymbolNameChanged;
            if (handler != null)
                handler();
        }

        private Expression _default;

        public Expression Default
        {
            get { return _default; }
        }

        public Expression CreateExpression()
        {
            Expression exp = new Expression(this);
            exp.Label = _counter++.ToString();
            while (_expressions.ContainsKey(exp.Label))
                exp.Label = _counter++.ToString();
            _expressions.Add(exp.Label, exp);
            if (_expressions.Count == 1)
                _default = exp;
            return exp;
        }

        public Expression CreateExpression(string label)
        {
            if (_expressions.ContainsKey(label))
                throw new ArgumentException("Expression has exist.");
            Expression exp = new Expression(this, label);
            _expressions.Add(label, exp);
            if (_expressions.Count == 1)
                _default = exp;
            return exp;
        }

        public int Count
        {
            get { return _expressions.Count; }
        }

        public void ResetCounter()
        {
            _counter = 0;
        }

        public void AppendExpression(Expression exp)
        {
            if (_expressions.ContainsKey(exp.Label))
                throw new ArgumentException("Expression has been exist.");
            _expressions.Add(exp.Label, exp);
        }

        public void DestroyExpression(string label)
        {
            if (this[label] == this.Default)
                return;
            _expressions.Remove(label);
        }

        private Dictionary<string, Expression> _expressions = new Dictionary<string, Expression>();

        public Expression this[string label]
        {
            get { return _expressions[label]; }
        }

        public List<Expression> Expressions
        {
            get { return new List<Expression>(_expressions.Values); }
        }

        private string comment;
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        public bool ChangeName(string name)
        {
            if (_manager.ChangeSymbolName(_name, name))
            {
                TriggerSymbolNameChanged();
                return true;
            }
            return false;   
        }

        public bool Contains(string label)
        {
            return _expressions.ContainsKey(label);
        }

        internal bool ChangeExpressionLabel(string oldl, string newl)
        {
            if (_expressions.ContainsKey(newl))
                return false;
            else
            {
                Expression exp = _expressions[oldl];
                _expressions.Remove(oldl);
                _expressions.Add(newl, exp);
                exp.Label = newl;
                return true;
            }
        }

        internal static void CheckName(string name)
        {
            foreach (char ch in name)
            {
                if (!CharUtils.IsSymbolsChar(ch))
                    throw new ArgumentException("Has Invalid Character");
            }
        }

        public SymbolManager Manager
        {
            get { return _manager; }
        }
    }
}
