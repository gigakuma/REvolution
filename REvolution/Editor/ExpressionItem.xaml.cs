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
using Exp = REvolution.Core.Symbols.Expression;
using REvolution.Core.Builder;

namespace REvolution.Editor
{
    /// <summary>
    /// Interaction logic for ExpressionItem.xaml
    /// </summary>
    public partial class ExpressionItem : UserControl
    {
        public ExpressionItem()
        {
            InitializeComponent();
        }

        public ExpressionItem(Exp exp)
            : this()
        {
            Initialize(exp);
        }

        private Exp _expression;

        public void Initialize(Exp exp)
        {
            _expression = exp;
            if (exp == _expression.Group.Default)
            {
                DefaultLabel.Visibility = System.Windows.Visibility.Visible;
                Delete.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
                DefaultLabel.Visibility = System.Windows.Visibility.Collapsed;
            Title.Text = _expression.Label;
            Expression.Text = _expression.Pattern;
        }

        private void Title_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Title.Focusable = true;
            Submit.Visibility = System.Windows.Visibility.Visible;
        }

        private void Expression_TextChanged(object sender, TextChangedEventArgs e)
        {
            _expression.Pattern = Expression.Text;
            bool parsed = _expression.Parse();
            ErrorInfos.Visibility = System.Windows.Visibility.Collapsed;
            ErrorInfos.Text = _expression.ParserError;
            if (!parsed)
            {
                ErrorInfos.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            try
            {
                bool linked = _expression.Link();
                if (!linked)
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (LinkErrorInfo info in _expression.Errors)
                        builder.Append(info.ToString()).Append(';');
                    ErrorInfos.Text = builder.ToString();
                    ErrorInfos.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                ErrorInfos.Visibility = System.Windows.Visibility.Visible;
                ErrorInfos.Text = ex.Message;
            }
        }

        private void Submit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Title.Focusable == true)
            {
                Title.Focusable = false;
                if (_expression.Label != Title.Text)
                {
                    try
                    {
                        _expression.ChangeLabel(Title.Text);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        Title.Text = _expression.Label;
                    }
                }
            }
        }

        private void Delete_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_expression == _expression.Group.Default)
                return;
            _expression.Group.DestroyExpression(_expression.Label);
            (this.Parent as StackPanel).Children.Remove(this);
        }

        public delegate void VisualizeRequestHandler(Exp exp);
        public event VisualizeRequestHandler VisualizeRequest;
        private void TriggerVisualizeRequest()
        {
            VisualizeRequestHandler hanlder = VisualizeRequest;
            if (hanlder != null)
                hanlder(_expression);
        }
        private void Visualize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TriggerVisualizeRequest();
        }

        private void Generator_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_expression == null || !_expression.Linked)
                return;
            string result = _expression.Generate();
            ProductionResult window = new ProductionResult();
            window.SetExpression(result);
            window.Show();

            SyntaxTreeVisualized vwindow = new SyntaxTreeVisualized();
            vwindow.Show();
            vwindow.Viewer.Expression = _expression;
            vwindow.Viewer.Refresh();
        }

    }
}
