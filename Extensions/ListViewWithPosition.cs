using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace GMDCGiphyPlugin.Extensions
{
    /// <summary>
    /// <see cref="ListViewWithPosition"/> provides a ListView control that reports on scroll position.
    /// </summary>
    public class ListViewWithPosition : ListView
    {
        /// <summary>
        /// Gets a Dependency Property Key for the IsNotAtBottom Property.
        /// </summary>
        internal static readonly DependencyPropertyKey IsNotAtBottomPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsNotAtBottom",
                typeof(bool),
                typeof(ListViewWithPosition),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets a Dependency Property indicating whether the ListView is scrolled to the bottom.
        /// </summary>
#pragma warning disable SA1202 //Elements should be ordered by access. Initialization for IsNotAtBottomProperty depends on IsNotAtBottomPropertyKey.
        public static readonly DependencyProperty IsNotAtBottomProperty =
            IsNotAtBottomPropertyKey.DependencyProperty;
#pragma warning restore SA1202 // Elements should be ordered by access

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewWithPosition"/> class.
        /// </summary>
        public ListViewWithPosition()
        {
            this.Loaded += this.ListViewWithPosition_Loaded;
            this.ScrollToEnd = new RelayCommand(this.DoScrollToEnd);
        }

        /// <summary>
        /// Gets a value indicating whether the ListView is scrolled to the bottom.
        /// </summary>
        public bool IsNotAtBottom => (bool)this.GetValue(IsNotAtBottomProperty);

        /// <summary>
        /// Gets a command that can be executed to scroll this <see cref="ListView"/> to the bottom.
        /// </summary>
        public ICommand ScrollToEnd { get; }

        private ScrollViewer ScrollViewer { get; set; }

        private bool ShouldSnapToBottom { get; set; } = false;

        private void ListViewWithPosition_Loaded(object sender, RoutedEventArgs e)
        {
            this.ScrollViewer = ListViewExtensions.FindSimpleVisualChild<ScrollViewer>(this);
            this.ScrollViewer.ScrollChanged += this.ScrollViewer_ScrollChanged;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // When DPI scaling is enabled, pixel values may be floating point. Round down to integers to
            // prevent floating-point roundoff error when comparing values.
            var atBottom = (int)e.VerticalOffset == (int)(e.OriginalSource as ScrollViewer).ScrollableHeight;
            this.SetValue(IsNotAtBottomPropertyKey, !atBottom);

            if (this.ShouldSnapToBottom)
            {
                if (atBottom)
                {
                    this.ShouldSnapToBottom = false;
                }
                else
                {
                    this.ScrollViewer.ScrollToBottom();
                }
            }
        }

        private void DoScrollToEnd()
        {
            this.ShouldSnapToBottom = true;
            this.ScrollViewer.ScrollToBottom();
        }
    }
}
