using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace REvolution.Visualizer
{
    internal interface IElement
    {
        Visual LeftAnchor { get; }
        Visual RightAnchor { get; }
    }

    internal enum ElementState
    {
        Open,
        Close
    }
}
