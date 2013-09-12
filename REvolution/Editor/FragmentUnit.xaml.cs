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
using REvolution.Core.Generator;
using System.Text.RegularExpressions;

namespace REvolution.Editor
{
    /// <summary>
    /// Interaction logic for FragmentViewer.xaml
    /// </summary>
    public partial class FragmentUnit : UserControl
    {
        public FragmentUnit()
        {
            InitializeComponent();
        }

        private FragmentViewer _viewer;

        public FragmentUnit(FragmentViewer viewer)
            : this()
        {
            _viewer = viewer;
        }

        public FragmentUnit(FragmentViewer viewer, string init)
            : this(viewer)
        {
            SetText(init);
        }

        public string Label
        {
            get { return Title.Text; }
            set { Title.Text = value; }
        }

        private TextBox CreateTextBox(string text)
        {
            TextBox box = new TextBox();
            box.Text = text;
            box.Margin = new Thickness(3);
            box.FontSize = 14;
            box.FontFamily = new System.Windows.Media.FontFamily("Courier New");
            box.BorderBrush = Brushes.Transparent;
            box.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            box.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            box.GotFocus += new RoutedEventHandler(box_GotFocus);
            return box;
        }

        private void box_GotFocus(object sender, RoutedEventArgs e)
        {
            _viewer.SetFocus(sender as TextBox, this);
        }

        public TextBox SetText(string text)
        {
            Container.Children.Clear();
            TextBox box = CreateTextBox(text);
            Container.Children.Add(box);
            return box;
        }

        public List<FragmentUnit> Fragments
        {
            get 
            {
                List<FragmentUnit> frags = new List<FragmentUnit>();
                foreach (UIElement element in Container.Children)
                {
                    if (element is FragmentUnit)
                        frags.Add(element as FragmentUnit);
                }
                return frags;
            }
        }

        private Symbol _symbol;

        public Symbol Symbol
        {
            get { return _symbol; }
            set 
            {
                _symbol = value;
                _symbol.Default.ExpressionParsed += new ExpressionParsedHandler(Default_ExpressionParsed);
                _symbol.Default.ExpressionLinked += new ExpressionLinkedHandler(Default_ExpressionLinked);
            }
        }

        void Default_ExpressionLinked(Core.Symbols.Expression exp, bool success, List<Core.Builder.LinkErrorInfo> errors)
        {
            if (!success)
            {
                SetContentError(true);
                return;
            }
            string pattern = exp.Generate(GenerateMode.SingleLine);
            string content = GetContent();
            Regex regex = new Regex("\\A" + pattern + "\\z");
            if (!regex.IsMatch(content))
                SetContentError(true);
            else
                SetContentError(false);
        }

        public string GetContent()
        {
            StringBuilder builder = new StringBuilder();
            foreach (UIElement uie in Container.Children)
            {
                if (uie is TextBox)
                    builder.Append((uie as TextBox).Text);
                else if (uie is FragmentUnit)
                    builder.Append((uie as FragmentUnit).GetContent());
            }
            return builder.ToString();
        }

        void Default_ExpressionParsed(Core.Symbols.Expression exp, bool success, string error)
        {
            if (!success)
                SetContentError(true);
        }

        private bool _edited;

        public bool Edited
        {
            get { return _edited; }
            set { _edited = value; }
        }

        public void SetContentError(bool error)
        {
            if (error)
                (Head.Background as SolidColorBrush).Color = Color.FromRgb(0xFF, 0,0);
            else
                (Head.Background as SolidColorBrush).Color = Color.FromRgb(0x22, 0x44, 0x88);   
        }

        public FragmentUnit CreateFragment(TextBox box)
        {
            int index = Container.Children.IndexOf(box);
            if (box.SelectionLength == 0 || box.SelectionLength == box.Text.Length)
                return null;
            int start = box.SelectionStart;
            int length = box.SelectionLength;
            string text = box.SelectedText;
            FragmentUnit unit = new FragmentUnit(_viewer, text);
            if (start == 0)
            {
                box.Text = box.Text.Substring(length, box.Text.Length - length);
                Container.Children.Insert(index, unit);
            }
            else if (start + length == box.Text.Length)
            {
                box.Text = box.Text.Substring(0, start);
                Container.Children.Insert(index + 1, unit);
            }
            else
            {
                string prefix = box.Text.Substring(0, start);
                string suffix = box.Text.Substring(start + length, box.Text.Length - start - length);
                TextBox newbox = CreateTextBox(suffix);
                box.Text = prefix;
                Container.Children.Insert(index + 1, unit);
                Container.Children.Insert(index + 2, newbox);
            }
            unit.DeleteFragment += new DeleteFragmentHandler(unit_DeleteFragment);
            return unit;
        }

        void unit_DeleteFragment(FragmentUnit sender)
        {
            int position = this.Container.Children.IndexOf(sender);
            int total = this.Container.Children.Count;
            if (position == 0)
            {
                if (total > 1 && this.Container.Children[1] is TextBox)
                {
                    string origin = (this.Container.Children[1] as TextBox).Text;
                    (this.Container.Children[1] as TextBox).Text = sender.GetContent() + origin;
                    this.Container.Children.RemoveAt(0);
                }
                else
                {
                    TextBox tb = CreateTextBox(sender.GetContent());
                    this.Container.Children.RemoveAt(0);
                    this.Container.Children.Insert(0, tb);
                }
            }
            else if (position == total - 1)
            {
                if (total > 1 && this.Container.Children[total - 2] is TextBox)
                {
                    (this.Container.Children[total - 2] as TextBox).Text += sender.GetContent();
                    this.Container.Children.RemoveAt(position);
                }
                else
                {
                    TextBox tb = CreateTextBox(sender.GetContent());
                    this.Container.Children.RemoveAt(position);
                    this.Container.Children.Insert(position, tb);
                }
            }
            else
            {
                if (this.Container.Children[position - 1] is TextBox)
                {
                    if (this.Container.Children[position + 1] is TextBox)
                    {
                        (this.Container.Children[position - 1] as TextBox).Text += sender.GetContent() + (this.Container.Children[position + 1] as TextBox).Text;
                        this.Container.Children.RemoveAt(position + 1);
                        this.Container.Children.RemoveAt(position);
                    }
                    else
                    {
                        (this.Container.Children[position - 1] as TextBox).Text += sender.GetContent();
                        this.Container.Children.RemoveAt(position);
                    }
                }
                else if (this.Container.Children[position + 1] is TextBox)
                {
                    string origin = (this.Container.Children[position + 1] as TextBox).Text;
                    (this.Container.Children[position + 1] as TextBox).Text = sender.GetContent() + origin;
                    this.Container.Children.RemoveAt(position);
                }
                else
                {
                    TextBox tb = CreateTextBox(sender.GetContent());
                    this.Container.Children.RemoveAt(position);
                    this.Container.Children.Insert(position, tb);
                }
            }
            //_viewer.SetFocus(null, this);
        }

        public delegate void DeleteFragmentHandler(FragmentUnit sender);

        public event DeleteFragmentHandler DeleteFragment;

        public void TriggerDeleteFragment()
        {
            DeleteFragmentHandler handler = DeleteFragment;
            if (handler != null)
                handler(this);
        }

        private void Title_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Title.Focusable = true;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewer.SetFocus(null, this);
        }

        private void Submit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Title.Focusable == true)
            {
                Title.Focusable = false;
                try
                {
                    if (Title.Text != _symbol.Name)
                        _symbol.ChangeName(Title.Text);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    Title.Text = _symbol.Name;
                }
            }
        }

        private void Head_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TriggerDeleteFragment();
        }
    }
}
