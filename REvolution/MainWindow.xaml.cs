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
using System.Windows.Shapes;
using REvolution.Core.Symbols;

namespace REvolution
{
    /// <summary>
    /// Interaction logic for MainWindowTest.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private SymbolManager _manager;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _manager = new SymbolManager();
            Partitioner.ViewerFocusChanged += new REvolution.Editor.FragmentViewer.ViewerFocusChangedHandler(Partitioner_ViewerFocusChanged);
            Partitioner.Initialize(_manager);
            Editor.Visualize += new REvolution.Editor.SymbolEditor.VisualizeHanlder(Editor_Visualize);
        }

        void Editor_Visualize(Core.Symbols.Expression exp)
        {
            Visualizer.Expression = exp;
            Visualizer.Refresh();
        }

        void Partitioner_ViewerFocusChanged(Editor.FragmentUnit unit)
        {
            Editor.SetSymbol(unit.Symbol);
        }

        private void PatitionButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Partitioner.Partition();
        }
    }
}
