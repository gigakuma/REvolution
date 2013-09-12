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
using REvolution.Core;
using System.Threading;
using REvolution.Core.Generator;
using Exp = REvolution.Core.Symbols.Expression;

namespace REvolution.Visualizer
{
    /// <summary>
    /// Interaction logic for SyntaxViewer.xaml
    /// </summary>
    public partial class SyntaxViewer : UserControl
    {
        public SyntaxViewer()
        {
            InitializeComponent();
        }

        private Exp _expression;

        public Exp Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        public void Refresh()
        {
            Container.Children.Clear();
            if (_expression == null || !_expression.Linked)
                return;
            //ElementGroup group = new ElementGroup();
            //group.SetExpression(_expression);
            //group.ShowQuantifier = false;
            //group.OutLayout.Margin = new Thickness();
            GenerateContext context = _expression.CreateContext();
            context.TraceIn(_expression);
            IElement ve = GenerateElement(_expression.SyntaxTree.Root, context);
            context.TraceOut();
            //group.AddElement(ve);
            this.Container.Children.Add(ve as UIElement);
        }

        private IElement GenerateElement(Node node, GenerateContext context)
        {
            if (!(node is NodeParent) && node.Type != NodeType.Field)
            {
                Element e = null;
                switch (node.Type)
                {
                    case NodeType.Anchor:
                        e = new Element(node as Anchor);
                        break;
                    case NodeType.Multi:
                        e = new Element(node as Multi);
                        break;
                    case NodeType.One:
                        e = new Element(node as One);
                        break;
                    case NodeType.Options:
                        e = new Element(node as Options);
                        break;
                    case NodeType.Set:
                        e = new Element(node as Set);
                        break;
                    case NodeType.Reference:
                        if ((node as Reference).CapName.Source.Anonymous)
                        {
                            int capnum = context.FindCaptureIndex((node as Reference).CapName.Source);
                            e = new Element(node as Reference, capnum);
                        }
                        else
                            e = new Element(node as Reference);
                        break;
                    case NodeType.Comment:
                        e = null;
                        break;
                }
                return e;
            }
            else
            {
                if (node.Type == NodeType.CharClass)
                {
                    return new Element(node as CharClass);
                }
                else if (node.Type == NodeType.Concatenate)
                {
                    Concatenate conca = node as Concatenate;
                    if (conca.Count == 0)
                        return null;
                    if (conca.Count == 1)
                        return GenerateElement(conca[0], context);

                    ElementParent group = new ElementParent();
                    foreach (Node child in (node as Concatenate).Children)
                    {
                        if (child == null)
                            continue;
                        IElement ve = GenerateElement(child, context);
                        group.AddElement(ve);
                    }
                    return group;
                }
                else if (node.Type == NodeType.Test)
                {
                    ElementGroup group = new ElementGroup(node as NodeGroup);
                    foreach (Node child in (node as Test).Children)
                    {
                        if (child == null)
                            continue;
                        IElement ve = GenerateElement(child, context);
                        group.AddElement(ve);
                    }
                    return group;
                }
                else if (node is NodeGroup)
                {
                    ElementGroup group = new ElementGroup(node as NodeGroup);
                    if (node.Type == NodeType.Capture)
                    {
                        Capture capture = node as Capture;
                        if (capture.Anonymous)
                        {
                            int capnum;
                            if (capture.CapName.Number != 0)
                            {
                                capnum = context.FindCaptureIndex(capture);
                                group.SetAddition("Anony:" + capnum.ToString());
                                (group.AdditionBorder.Background as SolidColorBrush).Color = Color.FromRgb(0xd5, 0xb7, 0xff);
                                (group.Addition.Foreground as SolidColorBrush).Color = Color.FromRgb(0x80, 0x22, 0xff);
                            }
                            else
                            {
                                if (context.DeepLevel == 1)
                                    group.OutLayout.Margin = new Thickness();
                                group.SetExpression(context.Current);
                            }
                        }
                        else
                        {
                            if (capture.CapName.IsName)
                                group.SetAddition("Name:" + capture.CapName.Name);
                            else
                                group.SetAddition("Number:" + capture.CapName.Number);
                        }
                    }
                    Alternate alter = (node as NodeGroup).Child as Alternate;

                    foreach (Node child in alter.Children)
                    {
                        if (child == null)
                            continue;
                        IElement ve = GenerateElement(child, context);
                        group.AddElement(ve);
                    }
                    return group;
                }
                else if (node.Type == NodeType.Field)
                {
                    context.TraceIn((node as Field).Expression);
                    IElement ve = GenerateElement((node as Field).Expression.SyntaxTree.Root, context);
                    context.TraceOut();
                    return ve;
                }
                // unfinished : field comment
                else
                    throw new Exception("Internal Node Type Error.");
            }
        }
    }
}
