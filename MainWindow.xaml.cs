﻿using System;
using System.Windows;
using System.Windows.Controls;
using XamlAnimatedGif;

namespace GMDCGiphyPlugin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
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
    }
}
