using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using XamlAnimatedGif;

namespace GMDCGiphyPlugin.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : MetroWindow
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var animator = XamlAnimatedGif.AnimationBehavior.GetAnimator(((e.OriginalSource as Button).Content as Image));
            animator?.Play();
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var animator = XamlAnimatedGif.AnimationBehavior.GetAnimator(((e.OriginalSource as Button).Content as Image));
            animator?.Pause();
        }

        private void GIFIndexListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView.Padding.Top > 0 && e.VerticalChange > 0)
            {
               // listView.Padding = { 0,0,0,0};
            }
            else
            {
                //listView.Padding.Top += 5;
            }
        }
    }
}
