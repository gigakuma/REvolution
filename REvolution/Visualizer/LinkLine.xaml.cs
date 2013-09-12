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
    /// Interaction logic for LinkLine.xaml
    /// </summary>
    public partial class LinkLine : UserControl
    {
        public LinkLine()
        {
            InitializeComponent();
        }

        public double CenterX
        {
            set
            {
                Center.X1 = Center.X2 = value;
                Left.X2 = Center.X1;
                Right.X1 = Center.X2;
            }
        }

        public void SetPosition(double x1, double y1, double x2, double y2, double center)
        {
            Left.X1 = x1;
            Left.Y1 = y1;
            Right.X2 = x2;
            Right.Y2 = y2;
            Center.X1 = Center.X2 = center;
            Center.Y1 = y1;
            Center.Y2 = y2;
            Left.X2 = center;
            Left.Y2 = y1;
            Right.X1 = center;
            Right.Y1 = y2;
            Canvas.SetLeft(Arrow, x2 - 10);
            Canvas.SetTop(Arrow, y2);
        }

        public void SetStart(double x, double y)
        {
            LeftX = x;
            LeftY = y;
        }

        public void SetEnd(double x, double y)
        {
            RightX = x;
            RightY = y;
            Canvas.SetLeft(Arrow, x - 10);
            Canvas.SetTop(Arrow, y);
        }

        public double LeftX
        {
            set 
            {
                Left.X1 = value;
                Center.X1 = Center.X2 = (Left.X1 + Right.X2) / 2;
                Left.X2 = Center.X1;
            }
        }

        public double RightX
        {
            set 
            {
                Right.X2 = value;
                Center.X1 = Center.X2 = (Left.X1 + Right.X2) / 2;
                Right.X1 = Center.X2;
            }
        }

        public double LeftY
        {
            set 
            {
                Left.Y1 = Left.Y2 = value;
                Center.Y1 = Left.Y2;
            }
        }

        public double RightY
        {
            set
            {
                Right.Y2 = Right.Y1 = value;
                Center.Y2 = Right.Y1;
            }
        }
    }
}
