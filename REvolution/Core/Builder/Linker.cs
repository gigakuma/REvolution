using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Syntax;
using REvolution.Core.Symbols;
using REvolution.Core.Syntax.Nodes;

namespace REvolution.Core.Builder
{
    internal class Linker
    {
        private Expression _exp;
        public Expression Expression
        {
            get { return _exp; }
        }

        public Linker(Expression exp)
        {
            _exp = exp;
        }

        private Stack<TraceState> _traceback = new Stack<TraceState>();

        /// <summary>
        /// Forcely named capture's expression slots.
        /// </summary>
        private Dictionary<int, Expression> _capnums = new Dictionary<int,Expression>();
        /// <summary>
        /// Forcely named capture's expression slots.
        /// </summary>
        private Dictionary<string, Expression> _capnames = new Dictionary<string,Expression>();

        private Dictionary<Capture, int> _anonys = new Dictionary<Capture,int>();
        public Dictionary<Capture, int> AnonyCaptures
        {
            get { return _anonys; }
        }

        private List<LinkErrorInfo> _errorinfos = new List<LinkErrorInfo>();

        public List<LinkErrorInfo> ErrorInfos
        {
            get { return _errorinfos; }
        }

        /// <summary>
        /// When link happen exception, you can get trackback list by this property.
        /// </summary>
        private List<Expression> Traceback
        {
            get
            {
                if (_traceback == null)
                    return null;
                List<Expression> list = new List<Expression>(_traceback.Count);
                foreach (TraceState state in _traceback)
                    list.Add(state.Exp);
                return list;
            }
        }

        private HashSet<Expression> _visiting = new HashSet<Expression>();

        private void Assign(Definition def, Expression exp)
        {
            if (def.IsName)
            {
                if (_capnames.ContainsKey(def.Name))
                    _capnames[def.Name] = exp;
                else
                    _capnames.Add(def.Name, exp);
            }
            else
            {
                if (_capnums.ContainsKey(def.Number))
                    _capnums[def.Number] = exp;
                else
                    _capnums.Add(def.Number, exp);
            }
        }

        private void StartVisit(Expression exp)
        {
            _traceback.Push(new TraceState(exp));
            _visiting.Add(exp);
        }

        private void ChangeState(TraceState state)
        {
            _traceback.Pop();
            _traceback.Push(state);
        }

        private void EndVisit(Expression exp)
        {
            _traceback.Pop();
            _visiting.Remove(exp);
        }

        public void Link()
        {
            // this must be cleared at first, but at last, for being needed after linking.
            _anonys.Clear();
            _errorinfos.Clear();
            StartVisit(_exp);
            while (_traceback.Count != 0)
            {
                TraceState state = _traceback.Peek();
                // visit expression's sequence. If capture, assign slot;
                // if reference, check slot; if expression, stop visit.
                // and Index point to expression' position.
                Visit(ref state);
                if (state.Index < state.Exp.SyntaxTree.Sequence.Count)
                {
                    Expression exp = state.Exp.SyntaxTree.Sequence[state.Index] as Expression;
                    // point to next
                    state.Index++;
                    // push state back to continue visiting.
                    ChangeState(state);
                    // if there's ring in visiting traceback, save this error.
                    if (_visiting.Contains(exp))
                        _errorinfos.Add(new RingErrorInfo(Traceback, exp));
                    // push new state
                    else
                    {
                        StartVisit(exp);
                        continue;
                    }
                }
                // finish visiting.
                if (state.Index == state.Exp.SyntaxTree.Sequence.Count)
                    EndVisit(state.Exp);
            }
            // clear temp data
            _traceback.Clear();
            _capnums.Clear();
            _capnames.Clear();
        }

        /// <summary>
        /// check capture & reference error
        /// </summary>
        /// <param name="state"></param>
        private void Visit(ref TraceState state)
        {
            List<object> seq = state.Exp.SyntaxTree.Sequence;
            for (int i = state.Index; i < seq.Count; i++)
            {
                if (seq[i] is Capture)
                {
                    Capture cap = seq[i] as Capture;
                    if (!cap.Anonymous)
                        Assign(cap.CapName, state.Exp);
                    else // annoy cap number start at 1
                        _anonys.Add(cap, _anonys.Keys.Count + 1);
                }
                else if (seq[i] is Reference)
                {
                    Reference refer = seq[i] as Reference;
                    // handle rename of annoy refer
                    if (refer.CapName.IsNumber)
                    {
                        // get real cap number for annoy cap.
                        int capnum;
                        if (refer.CapName.Source.Anonymous)
                            capnum = _anonys[refer.CapName.Source];
                        else
                            capnum = refer.CapName.Number;
                        if (_capnums.ContainsKey(capnum) && _capnums[capnum] != state.Exp)
                            _errorinfos.Add(new ReferenceErrorInfo(Traceback, refer, _capnums[capnum]));
                    }
                    else
                    {
                        string capname = refer.CapName.Name;
                        if (_capnames.ContainsKey(capname) && _capnames[capname] != state.Exp)
                            _errorinfos.Add(new ReferenceErrorInfo(Traceback, refer, _capnames[capname]));
                    }
                }
                else if (seq[i] is Expression)
                    return;
                state.Index++;
            }
        }

        private struct TraceState
        {
            public Expression Exp;
            public int Index;
            public TraceState(Expression exp)
            {
                Exp = exp;
                Index = 0;
            }
        }
    }

    public class LinkErrorInfo
    {
        private List<Expression> _traceback;

        public List<Expression> Traceback
        {
            get { return _traceback; }
        }

        public LinkErrorInfo(List<Expression> traceback)
        {
            _traceback = traceback;
        }
    }

    public class ReferenceErrorInfo : LinkErrorInfo
    {
        private Expression _definer;
        public Expression Definer
        {
            get { return _definer; }
        }

        private Reference _source;
        public Reference Source
        {
            get { return _source; }
        }

        public ReferenceErrorInfo(List<Expression> traceback, Reference source, Expression definer)
            : base(traceback)
        {
            _source = source;
            _definer = definer;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("ERROR<Back Reference>: Expression [").Append(_definer.Key).Append("] has defined ").Append(_source.CapName.Generate()).Append(".");
            builder.Append("Traceback:");
            foreach (Expression exp in Traceback)
            {
                builder.Append('<').Append(exp.Key);
            }
            return builder.ToString();
        }
    }

    public class RingErrorInfo : LinkErrorInfo
    {
        private Expression _source;
        /// <summary>
        /// The expression which triggers the error.
        /// </summary>
        public Expression Source
        {
            get { return _source; }
        }

        /// <summary>
        /// The expression which refers the parent-trace expression.
        /// And that is at end of Traceback.
        /// </summary>
        public Expression Referer
        {
            get { return Traceback[Traceback.Count - 1]; }
        }

        public RingErrorInfo(List<Expression> traceback, Expression source)
            : base(traceback)
        {
            _source = source;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("ERROR<Refering Symbol>: Expression [").Append(Referer.Key).Append("] has been refered by [").Append(Traceback[Traceback.Count-2].Key).Append("].");
            builder.Append("Traceback:");
            foreach (Expression exp in Traceback)
            {
                builder.Append('<').Append(exp.Key);
            }
            return builder.ToString();
        }
    }
}
