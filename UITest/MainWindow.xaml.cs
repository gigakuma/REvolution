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
using System.Windows.Controls.Primitives;

namespace UITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Rectangle rect = new Rectangle();
            rect.Width = 10;
            rect.Height = 10;
            rect.Fill = Brushes.Red;
            Container.Children.Add(rect);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Container.LayoutUpdated += new EventHandler(Container_LayoutUpdated);
        }

        void Container_LayoutUpdated(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Layout Update");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Container.Children.Count != 0)
                Container.Children.RemoveAt(Container.Children.Count - 1);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("start:" + textBox1.SelectionStart + ";text:" + textBox1.SelectedText);
        }
    }
}
