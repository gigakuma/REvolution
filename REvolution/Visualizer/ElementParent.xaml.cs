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

namespace REvolution.Visualizer
{
    /// <summary>
    /// Interaction logic for ElementParent.xaml
    /// </summary>
    public partial class ElementParent : UserControl, IElement
    {
        public ElementParent()
        {
            InitializeComponent();
        }

        internal void AddElement(IElement element)
        {
            Placer.Children.Add(element as UIElement);
            Lines.Children.Add(new LinkLine());
        }

        private void Container_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < Placer.Children.Count; i++)
            {
                UIElement ue = Placer.Children[i];
                if (ue is IElement)
                {
                    IElement element = ue as IElement;
                    Point lp = element.LeftAnchor.TransformToVisual(Layout).Transform(new Point());
                    Point rp = element.RightAnchor.TransformToVisual(Layout).Transform(new Point());
                    LinkLine lline = Lines.Children[i] as LinkLine;
                    LinkLine rline = Lines.Children[i + 1] as LinkLine;
                    lline.SetEnd(lp.X + (element.LeftAnchor as Rectangle).Width, lp.Y);
                    rline.SetStart(rp.X, rp.Y);
                }
            }
            Point la = AnchorL.TransformToVisual(Layout).Transform(new Point());
            Point ra = AnchorR.TransformToVisual(Layout).Transform(new Point());
            LinkLine left = Lines.Children[0] as LinkLine;
            LinkLine right = Lines.Children[Lines.Children.Count - 1] as LinkLine;
            left.SetStart(la.X, la.Y);
            right.SetEnd(ra.X, ra.Y);
        }

        public Visual LeftAnchor
        {
            get { return AnchorL; }
        }

        public Visual RightAnchor
        {
            get { return AnchorR; }
        }
    }
}
