using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using REvolution.Core.Symbols;

namespace REvolution.Editor
{
    /// <summary>
    /// Interaction logic for FragmentViewer.xaml
    /// </summary>
    public partial class FragmentViewer : UserControl
    {
        public FragmentViewer()
        {
            InitializeComponent();
        }

        public delegate void ViewerFocusChangedHandler(FragmentUnit unit);

        public event ViewerFocusChangedHandler ViewerFocusChanged;

        private void TriggerViewerFocusChanged(FragmentUnit unit)
        {
            ViewerFocusChangedHandler handler = ViewerFocusChanged;
            if (handler != null)
                handler(unit);
        }

        public void Initialize(SymbolManager manager)
        {
            _manager = manager;
            FragmentUnit unit = new FragmentUnit(this);
            TextBox box = unit.SetText("");
            box.Text = "<Please input your sample>";
            unit.Label = "Symbol" + _index++;
            Symbol symbol = _manager.RegisterSymbol(unit.Label);
            symbol.CreateExpression();
            unit.Symbol = symbol;
            Layout.Child = unit;
            //Layout.Children.Add(unit);
            SetFocus(null, unit);
        }

        private int _index = 0;

        private SymbolManager _manager;

        public SymbolManager Manager
        {
            get { return _manager; }
            set { _manager = value; }
        }

        private TextBox _focusTextBox;
        private FragmentUnit _focusUnit;

        public void SetFocus(TextBox box, FragmentUnit unit)
        {
            _focusTextBox = box;
            _focusUnit = unit;
            TriggerViewerFocusChanged(unit);
        }

        public void Partition()
        {
            if (_focusTextBox == null || _focusUnit == null)
                return;
            FragmentUnit unit = _focusUnit.CreateFragment(_focusTextBox);
            if (unit == null)
                return;
            unit.Label = "Symbol" + _index++;
            Symbol symbol = _manager.RegisterSymbol(unit.Label);
            symbol.CreateExpression();
            unit.Symbol = symbol;
        }
    }
}
