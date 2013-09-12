using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Syntax;
using REvolution.Core.Builder;
using REvolution.Core.Generator;

namespace REvolution.Core.Symbols
{
    public delegate void PatternChangedHandler();
    public delegate void ExpressionParsedHandler(Expression exp, bool success, string error);
    public delegate void ExpressionLinkedHandler(Expression exp, bool success, List<LinkErrorInfo> errors);
    public class Expression
    {
        private Expression()
        { }

        public event PatternChangedHandler PatternChanged;

        private void TriggerPatternChanged()
        {
            PatternChangedHandler handler = PatternChanged;
            if (handler != null)
                handler();
        }

        public event ExpressionParsedHandler ExpressionParsed;

        private void TriggerExpressionParsed(bool success, string error)
        {
            ExpressionParsedHandler handler = ExpressionParsed;
            if (handler != null)
                handler(this, success, error);
        }

        public event ExpressionLinkedHandler ExpressionLinked;

        private void TriggerExpressionLinked()
        {
            ExpressionLinkedHandler handler = ExpressionLinked;
            if (handler != null)
                handler(this, _linker.ErrorInfos.Count == 0, _linker.ErrorInfos);
        }

        #region Constructor
        internal Expression(Symbol group)
        {
            if (group == null)
                throw new ArgumentException("Null Symbol");
            _group = group;
            _linker = new Linker(this);
        }

        internal Expression(Symbol group, string label)
            : this(group)
        {
            _label = label;
        }
        #endregion

        #region Symbol System

        private Symbol _group;

        public Symbol Group
        {
            get { return _group; }
        }

        private string _label;
        public string Label
        {
            get { return _label; }
            internal set { _label = value; }
        }

        public bool ChangeLabel(string label)
        {
            return _group.ChangeExpressionLabel(_label, label);
        }

        public string Key
        {
            get { return SymbolManager.GenerateKey(_group.Name, _label); }
        }
        #endregion

        #region Pattern & Syntax
        private string _pattern;

        public string Pattern
        {
            get { return _pattern; }
            set
            {
                _pattern = value;
                _syntaxTree = null;
                _linked = false;
                TriggerPatternChanged();
            }
        }

        private Tree _syntaxTree;

        public Tree SyntaxTree
        {
            get { return _syntaxTree; }
        }

        public bool Parse()
        {
            if (String.IsNullOrEmpty(_pattern))
                return true;
            Parser parser = new Parser(_pattern, _group.Manager);
            try
            {
                _syntaxTree = parser.Parse();
                foreach (Expression exp in _syntaxTree.Expressions)
                    if (exp == this)
                        throw new ArgumentException("Cannot refer self.");
                TriggerExpressionParsed(true, string.Empty);
                _parserError = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                TriggerExpressionParsed(false, ex.Message);
                _parserError = ex.Message;
                return false;
            }
        }

        public bool Parsed
        {
            get { return _syntaxTree != null; }
        }

        private string _parserError = string.Empty;
        public string ParserError
        {
            get { return _parserError; }
        }

        private Linker _linker;
        private bool _linked = false;

        public bool Link()
        {
            // just for empty pattern
            if (_syntaxTree == null)
                return true;
            foreach (Expression exp in _syntaxTree.Expressions)
            {
                if (exp == this)
                    throw new ArgumentException("Cannot refer self.");
                if (!exp.Parsed)
                {
                    exp.Parse();
                    if (exp.SyntaxTree == null)
                        throw new ArgumentException("Still not support empty expression.");
                }
                if (!exp.Linked)
                    exp.Link();
            }
            _linker.Link();
            _linked = true;
            TriggerExpressionLinked();
            return _linker.ErrorInfos.Count == 0;
        }

        public List<LinkErrorInfo> Errors
        {
            get { return _linker.ErrorInfos; }
        }

        public bool Linked
        {
            get { return _linked; }
        }

        #endregion

        #region Generate

        public string Generate(GenerateMode mode)
        {
            if (!_linked)
                throw new ArgumentException("Please Link First");
            if (_linker.ErrorInfos.Count != 0)
                throw new ArgumentException("There are link errors");

            GenerateContext context = new GenerateContext(_linker.AnonyCaptures, mode);
            if (context.Mode == GenerateMode.MultiLine)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('#').Append(Key).Append(" : ").Append(_group.Comment).Append('\n').Append(_syntaxTree.Root.Generate(context));
                return builder.ToString();
            }
            return _syntaxTree.Root.Generate(context);
        }

        public string Generate()
        {
            return Generate(GenerateMode.MultiLine);
        }

        public GenerateContext CreateContext()
        {
            if (!_linked)
                return null;
            return new GenerateContext(_linker.AnonyCaptures, GenerateMode.SingleLine);
        }

        public string Generate(GenerateContext context)
        {
            return _syntaxTree.Root.Generate(context);
        }
        #endregion
    }

}
