using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;

namespace MMEditor
{
    public class ScrollViewerCustom : ScrollViewer
    {
        public double HorizontalOffset { get { return ScrollInfo.HorizontalOffset; } }
        public double VerticalOffset { get { return ScrollInfo.VerticalOffset; } }
    }
}
