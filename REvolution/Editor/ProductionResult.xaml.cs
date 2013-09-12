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

namespace REvolution.Editor
{
    /// <summary>
    /// Interaction logic for ProductionResult.xaml
    /// </summary>
    public partial class ProductionResult : Window
    {
        public ProductionResult()
        {
            InitializeComponent();
        }

        public void SetExpression(string exp)
        {
            Expression.Text = exp;
        }
    }
}
