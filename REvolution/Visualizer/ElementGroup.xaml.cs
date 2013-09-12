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
using REvolution.Core.Syntax;
using REvolution.Core.Syntax.Nodes;
using Exp = REvolution.Core.Symbols.Expression;

namespace REvolution.Visualizer
{
    /// <summary>
    /// Interaction logic for ElementGroupO.xaml
    /// </summary>
    public partial class ElementGroup : UserControl, IElement
    {
        public ElementGroup()
        {
            InitializeComponent();
        }

        private void SetQuantifierText(Quantifier quan)
        {
            QuantifierText.Text = quan.Generate();
        }

        public ElementGroup(NodeGroup node)
            : this()
        {
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
            Type.Text = node.Type.ToString();
        }

        public ElementGroup(Field node)
            : this()
        {
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
            SetExpression(node.Expression);
        }

        public void SetExpression(Exp exp)
        {
            Type.Text = "Symbol";
            SetAddition(exp.Key);
            (AdditionBorder.Background as SolidColorBrush).Color = Color.FromRgb(0xff, 0x94, 0x7b);
            (Addition.Foreground as SolidColorBrush).Color = Color.FromRgb(0xce, 0x2d, 0x08);
        }

        private bool _showQuantifier = true;
        public bool ShowQuantifier
        {
            get { return _showQuantifier; }
            set
            {
                _showQuantifier = value;
                if (!_showQuantifier)
                {
                    Quantifier.Visibility = System.Windows.Visibility.Collapsed;
                    OutAnchorL.Width = 1;
                    OutAnchorR.Width = 1;
                    HorizentalL.Visibility = System.Windows.Visibility.Collapsed;
                    HorizentalR.Visibility = System.Windows.Visibility.Collapsed;
                    VerticalL.Visibility = System.Windows.Visibility.Collapsed;
                    VerticalR.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    Quantifier.Visibility = System.Windows.Visibility.Visible;
                    OutAnchorL.Width = 10;
                    OutAnchorR.Width = 10;
                    HorizentalL.Visibility = System.Windows.Visibility.Visible;
                    HorizentalR.Visibility = System.Windows.Visibility.Visible;
                    VerticalL.Visibility = System.Windows.Visibility.Visible;
                    VerticalR.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private ElementState _state;

        internal void AddElement(IElement element)
        {
            Placer.Children.Add(element as UIElement);
            Lines.Children.Add(new LinkLine());
            Lines.Children.Add(new LinkLine());
        }

        public void SetAddition(string content)
        {
            if (string.IsNullOrEmpty(content))
                AdditionBorder.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                AdditionBorder.Visibility = System.Windows.Visibility.Visible;
                Addition.Text = content;
            }
        }

        public Visual LeftAnchor
        {
            get { return OutAnchorL; }
        }

        public Visual RightAnchor
        {
            get { return OutAnchorR; }
        }

        private void Placer_LayoutUpdated(object sender, EventArgs e)
        {
            if (_state == ElementState.Open)
            {
                Point left = OutAnchorL.TransformToVisual(Layout).Transform(new Point());
                Point right = OutAnchorR.TransformToVisual(Layout).Transform(new Point());
                for (int i = 0; i < Placer.Children.Count; i++)
                {
                    UIElement ue = Placer.Children[i];
                    if (ue is IElement)
                    {
                        IElement element = ue as IElement;
                        Point lp = element.LeftAnchor.TransformToVisual(Layout).Transform(new Point());
                        Point rp = element.RightAnchor.TransformToVisual(Layout).Transform(new Point());
                        LinkLine lline = Lines.Children[i * 2] as LinkLine;
                        LinkLine rline = Lines.Children[i * 2 + 1] as LinkLine;
                        lline.SetPosition(left.X + OutAnchorL.Width, left.Y, lp.X + (element.LeftAnchor as Rectangle).Width, lp.Y, left.X + OutAnchorL.Width + 10);
                        rline.SetPosition(rp.X, rp.Y, right.X, right.Y, right.X - 10);
                    }
                }
            }
        }

        private void SwitchOn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _state = ElementState.Open;
        }

        private void SwitchOff_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _state = ElementState.Close;
        }

        private void Body_LayoutUpdated(object sender, EventArgs e)
        {
            Point vl2 = OutAnchorL.TransformToVisual(OutLayout).Transform(new Point());
            Point vr2 = OutAnchorR.TransformToVisual(OutLayout).Transform(new Point());
            VerticalL.X1 = VerticalL.X2 = 0;
            VerticalL.Y2 = vl2.Y;
            VerticalR.X1 = VerticalR.X2 = Body.ActualWidth;
            VerticalR.Y2 = vr2.Y;
            HorizentalL.X1 = 0;
            HorizentalR.X2 = VerticalR.X1;
        }

        private void Quantifier_LayoutUpdated(object sender, EventArgs e)
        {
            Point hl = QAnchorL.TransformToVisual(OutLayout).Transform(new Point());
            Point hr = QAnchorR.TransformToVisual(OutLayout).Transform(new Point());
            HorizentalL.Y1 = HorizentalL.Y2 = hl.Y;
            HorizentalL.X2 = hl.X;
            HorizentalR.Y1 = HorizentalR.Y2 = hr.Y;
            HorizentalR.X1 = hr.X;
            VerticalL.Y1 = HorizentalL.Y1;
            VerticalR.Y1 = HorizentalR.Y1;
        }
    }
}
