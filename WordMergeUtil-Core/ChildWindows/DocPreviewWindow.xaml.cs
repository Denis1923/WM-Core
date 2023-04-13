using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WordMergeUtil_Core.ChildWindows
{
    public partial class DocPreviewWindow
    {
        public DocPreviewWindow(List<Image> images, DocPreviewWindowoptions opts)
        {
            InitializeComponent();

            ResizeMode = ResizeMode.NoResize;

            if (opts.W != default(double))
                Width = opts.W;

            if (opts.H != default(double))
                Height = opts.H;

            Loaded += (s, e) =>
            {
                if (opts.IsExcel)
                    MyScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

                for (var i = 0; i < images.Count; i++)
                {
                    ImageHolder.Children.Add(images[i]);
                    ImageHolder.Children.Add(new Label { Width = Width, Content = string.Format("Станица {0}", i + 1), HorizontalContentAlignment = HorizontalAlignment.Center });
                }
            };
        }
    }

    public class DocPreviewWindowoptions
    {
        public double H { get; set; }

        public double W { get; set; }

        public bool IsExcel { get; set; }
    }
}
