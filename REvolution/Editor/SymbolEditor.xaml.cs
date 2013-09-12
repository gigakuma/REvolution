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
using Exp = REvolution.Core.Symbols.Expression;
using System.Windows.Media.Animation;

namespace REvolution.Editor
{
    /// <summary>
    /// Interaction logic for SymbolEditor.xaml
    /// </summary>
    public partial class SymbolEditor : UserControl
    {
        public SymbolEditor()
        {
            InitializeComponent();
        }

        private Symbol _symbol;

        public void SetSymbol(Symbol symbol)
        {
            if (_symbol != null)
                _symbol.SymbolNameChanged -= SymbolNameChanged;
            _symbol = symbol;
            _symbol.SymbolNameChanged += SymbolNameChanged;
            Comment.Text = _symbol.Comment;
            Refersh();
        }

        void SymbolNameChanged()
        {
            Title.Text = _symbol.Name;
        }

        private ExpressionItem CreateItem(Exp exp)
        {
            ExpressionItem item = new ExpressionItem(exp);
            item.VisualizeRequest += new ExpressionItem.VisualizeRequestHandler(item_VisualizeRequest);
            item.Margin = new Thickness(5, 0, 5, 0);
            return item;
        }

        public delegate void VisualizeHanlder(Exp exp);
        public event VisualizeHanlder Visualize;
        private void TriggerVisualize(Exp exp)
        {
            VisualizeHanlder hanlder = Visualize;
            if (hanlder != null)
                hanlder(exp);
        }
        void item_VisualizeRequest(Exp exp)
        {
            TriggerVisualize(exp);
        }

        private void Refersh()
        {
            Title.Text = _symbol.Name;
            ExpList.Children.Clear();
            foreach (Exp exp in _symbol.Expressions)
                ExpList.Children.Add(CreateItem(exp));
        }

        private void AddExp_Click(object sender, RoutedEventArgs e)
        {
            _symbol.CreateExpression();
            Refersh();
        }

        private void Comment_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_symbol == null)
                Comment.Text = string.Empty;
            else
                _symbol.Comment = Comment.Text;
            if (string.IsNullOrEmpty(Comment.Text) && Warning.Opacity == 0)
            {
                Storyboard story = Warning.Resources["ShowWarning"] as Storyboard;
                story.Begin();
            }
            else if (!string.IsNullOrEmpty(Comment.Text) && Warning.Opacity == 1)
            {
                Storyboard story = Warning.Resources["HideWarning"] as Storyboard;
                story.Begin();
            }
        }
    }
}
