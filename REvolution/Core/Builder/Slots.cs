using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Syntax.Nodes;
using REvolution.Core.Syntax;
using REvolution.Core.Symbols;

namespace REvolution.Core.Builder
{
    public class Slots
    {
        
        private Dictionary<int, Definition> _capnums;
        private Dictionary<string, Definition> _capnames;

        public Slots()
        {
            _capnums = new Dictionary<int, Definition>();
            _capnames = new Dictionary<string, Definition>();
        }

        /// <summary>
        /// Sequence will be saved, so if you want restart parsing, clear sequence.
        /// </summary>
        public void Clear()
        {
            _capnums.Clear();
            _capnames.Clear();
        }

        private void Assign(Definition id)
        {
            if (id.IsName)
            {
                if (_capnames.ContainsKey(id.Name))
                    _capnames[id.Name] = id;
                else
                    _capnames.Add(id.Name, id);
            }
            else
            {
                if (_capnums.ContainsKey(id.Number))
                    _capnums[id.Number] = id;
                else
                    _capnums.Add(id.Number, id);
            }
        }

        public void Assign(Capture capture)
        {
            Assign(capture.CapName);
        }

        public Definition FindAssigned(Definition id)
        {
            if (id.IsName)
                return FindAssigned(id.Name);
            else
                return FindAssigned(id.Number);
        }

        public Definition FindAssigned(int capnum)
        {
            return _capnums.ContainsKey(capnum) ? _capnums[capnum] : null;
        }

        public Definition FindAssigned(string capname)
        {
            return _capnames.ContainsKey(capname) ? _capnames[capname] : null;
        }

        public void Assign(Balance balance)
        {
            Definition cap = balance.CapName;
            Definition uncap = balance.UncapName;
            if (cap != null)
                Assign(cap);
            if (uncap != null)
                Assign(uncap);
        }
    }
}
