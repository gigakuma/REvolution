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
using REvolution.Core.Syntax.Nodes;
using REvolution.Core.Syntax;
using REvolution.Core;

namespace REvolution.Visualizer
{
    /// <summary>
    /// Interaction logic for ElementO.xaml
    /// </summary>
    public partial class Element : UserControl, IElement
    {
        public Element()
        {
            InitializeComponent();
        }

        private bool _showQuantifier = true;
        private bool ShowQuantifier
        {
            get { return _showQuantifier; }
            set
            {
                _showQuantifier = value;
                if (!_showQuantifier)
                {
                    Quantifier.Visibility = System.Windows.Visibility.Collapsed;
                    AnchorL.Width = 1;
                    AnchorR.Width = 1;
                    HorizentalL.Visibility = System.Windows.Visibility.Collapsed;
                    HorizentalR.Visibility = System.Windows.Visibility.Collapsed;
                    VerticalL.Visibility = System.Windows.Visibility.Collapsed;
                    VerticalR.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    Quantifier.Visibility = System.Windows.Visibility.Visible;
                    AnchorL.Width = 10;
                    AnchorR.Width = 10;
                    HorizentalL.Visibility = System.Windows.Visibility.Visible;
                    HorizentalR.Visibility = System.Windows.Visibility.Visible;
                    VerticalL.Visibility = System.Windows.Visibility.Visible;
                    VerticalR.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public Element(Anchor node)
            : this()
        {
            this.TypeText = "Anchor";
            this.ExpText = GetExpText(node);
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
        }

        public Element(Multi node)
            : this()
        {
            this.TypeText = "String";
            this.ExpText = GetExpText(node);
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
        }

        public Element(One node)
            : this()
        {
            this.TypeText = "Character";
            this.ExpText = GetExpText(node);
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
        }

        public Element(Options node)
            : this()
        {
            this.TypeText = "Options";
            this.ExpText = GetExpText(node);
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
        }

        public Element(Set node)
            : this()
        {
            this.TypeText = "Character Set";
            this.ExpText = GetExpText(node);
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
        }

        public Element(Reference node)
            : this()
        {
            this.TypeText = "Reference";
            this.ExpText = GetExpText(node);
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
        }

        public Element(Reference node, int capnum)
            : this()
        {
            this.TypeText = "Reference";
            this.ExpText = "Anony:" + capnum;
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
        }

        public Element(CharClass node)
            : this()
        {
            this.TypeText = "Character Class";
            this.ExpText = GetExpText(node);
            if (node.Quantifier == null)
                ShowQuantifier = false;
            else
            {
                SetQuantifierText(node.Quantifier);
                ShowQuantifier = true;
            }
        }

        private void SetQuantifierText(Quantifier quan)
        {
            QuantifierText.Text = quan.Generate();
        }

        public string TypeText
        {
            get { return Type.Text; }
            set { Type.Text = value; }
        }

        public string ExpText
        {
            get { return Exp.Text; }
            set { Exp.Text = value; }
        }

        private string GetExpText(Anchor node)
        {
            string tname;
            switch (node.AnchorType)
            {
                case AnchorType.Beginning:
                    tname = "Beginning";
                    break;
                case AnchorType.Start:
                    tname = "Start";
                    break;
                case AnchorType.Boundary:
                    tname = "Boundary";
                    break;
                case AnchorType.Nonboundary:
                    tname = "Not boundary";
                    break;
                case AnchorType.End:
                    tname = "End";
                    break;
                case AnchorType.EndZ:
                    tname = "EndZ??";
                    break;
                case AnchorType.Bol:
                    tname = "Beginning of line";
                    break;
                case AnchorType.Eol:
                    tname = "End of line";
                    break;
                default:
                    tname = string.Empty;
                    break;
            }
            return tname + " (" + node.Tokens[0].Text + ")";
        }

        private string GetExpText(CharClass node)
        {
            return node.GenerateContent(null);
        }

        private string GetExpText(Multi node)
        {
            return node.GenerateContent(null);
        }

        private string GetExpText(One node)
        {
            string esc = CharUtils.EscapeWhiteSpace(node.Tokens[0].Text[0]);
            if (esc == null)
                return node.Tokens[0].Text;
            else
                return esc;
        }

        private string GetExpText(Options node)
        {
            StringBuilder builder = new StringBuilder();
            if (node.OnOptions != OptionType.None)
                builder.Append("On:").Append(Options.OptionsToString(node.OnOptions));
            if (node.OnOptions != OptionType.None && node.OffOptions != OptionType.None)
                builder.Append("|");
            if (node.OffOptions != OptionType.None)
                builder.Append("Off:").Append(Options.OptionsToString(node.OffOptions));
            return builder.ToString();
        }

        private string GetExpText(Reference node)
        {
            if (node.CapName.IsName)
                return "Number:" + node.CapName.Number.ToString();
            else
                return "Name:" + node.CapName.Name;
        }

        private string GetExpText(Set node)
        {
            if (node.Charset.Type == SetType.Wildcard)
                return "Wildchard(all)";
            StringBuilder builder = new StringBuilder();
            if ((node.Charset.Type & SetType.Not) == SetType.Not)
                builder.Append("NOT ");
            switch (node.Charset.Type & SetType.Category)
            {
                case SetType.Digit:
                    builder.Append("Digit");
                    break;
                case SetType.Word:
                    builder.Append("Word");
                    break;
                case SetType.Space:
                    builder.Append("Space");
                    break;
                case SetType.Property:
                    builder.Append("Property:").Append(node.Charset.Property);
                    break;
            }
            return builder.ToString();
        }

        private void Body_LayoutUpdated(object sender, EventArgs e)
        {
            Point vl2 = AnchorL.TransformToVisual(Layout).Transform(new Point());
            Point vr2 = AnchorR.TransformToVisual(Layout).Transform(new Point());
            VerticalL.X1 = VerticalL.X2 = 0;
            VerticalL.Y2 = vl2.Y;
            VerticalR.X1 = VerticalR.X2 = Body.ActualWidth;
            VerticalR.Y2 = vr2.Y;
            HorizentalL.X1 = 0;
            HorizentalR.X2 = VerticalR.X1;
        }

        private void Quantifier_LayoutUpdated(object sender, EventArgs e)
        {
            Point hl = QAnchorL.TransformToVisual(Layout).Transform(new Point());
            Point hr = QAnchorR.TransformToVisual(Layout).Transform(new Point());
            HorizentalL.Y1 = HorizentalL.Y2 = hl.Y;
            HorizentalL.X2 = hl.X;
            HorizentalR.Y1 = HorizentalR.Y2 = hr.Y;
            HorizentalR.X1 = hr.X;
            VerticalL.Y1 = HorizentalL.Y1;
            VerticalR.Y1 = HorizentalR.Y1;
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
