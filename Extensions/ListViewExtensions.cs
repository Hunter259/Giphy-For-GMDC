﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GMDCGiphyPlugin.Extensions
{
    /// <summary>
    /// Provides MVVM support for scrolling to the bottom of a list or allowing
    /// a user to lock the current scroll position.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/23561679.
    /// </remarks>
    public static class ListViewExtensions
    {
        /// <summary>
        /// Gets a property indicating if Auto Scroll is enabled.
        /// </summary>
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",
                typeof(bool),
                typeof(ListViewExtensions),
                new PropertyMetadata(false, HookupAutoScrollToEnd));

        /// <summary>
        /// Gets a property containing the Auto Scroll handler.
        /// </summary>
        public static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEndHandler",
                typeof(ListViewAutoScrollToEndHandler),
                typeof(ListViewExtensions));

        /// <summary>
        /// Gets a property indicating if Scroll To Top notifications are enabled.
        /// </summary>
        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToTop",
                typeof(ICommand),
                typeof(ListViewExtensions),
                new FrameworkPropertyMetadata(null, OnScrollToTopBottomPropertyChanged));

        /// <summary>
        /// Gets a property indicating if Scroll To Bottom notifications are enabled.
        /// </summary>
        public static readonly DependencyProperty ScrollToBottomProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToBottom",
                typeof(ICommand),
                typeof(ListViewExtensions),
                new FrameworkPropertyMetadata(null, OnScrollToTopBottomPropertyChanged));

        /// <summary>
        /// Gets a value indicating whether Auto Scrolling in enabled.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static bool GetAutoScrollToEnd(ListView instance)
        {
            return (bool)instance.GetValue(AutoScrollProperty);
        }

        /// <summary>
        /// Sets a value indicating whether Auto Scrolling is enabled.
        /// </summary>
        /// <param name="instance">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether scroll to end is enabled. </param>
        public static void SetAutoScrollToEnd(ListView instance, bool value)
        {
            if (value)
            {
                instance.Loaded += Loaded;
            }
        }

        /// <summary>
        /// Gets a value indicating whether notifications when the control is scrolled to the top are enabled.
        /// </summary>
        /// <param name="ob">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static ICommand GetScrollToTop(DependencyObject ob)
        {
            return (ICommand)ob.GetValue(ScrollToTopProperty);
        }

        /// <summary>
        /// Sets a value indicating whether Scroll to Top notications are enabled.
        /// </summary>
        /// <param name="ob">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether scroll to top notifications are enabled. </param>
        public static void SetScrollToTop(DependencyObject ob, ICommand value)
        {
            ob.SetValue(ScrollToTopProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether notifications when the control is scrolled to the bottom are enabled.
        /// </summary>
        /// <param name="ob">The dependency object to retreive the property from.</param>
        /// <returns>A boolean indicating whether enabled.</returns>
        public static ICommand GetScrollToBottom(DependencyObject ob)
        {
            return (ICommand)ob.GetValue(ScrollToBottomProperty);
        }

        /// <summary>
        /// Sets a value indicating whether Scroll to Bottom notications are enabled.
        /// </summary>
        /// <param name="ob">The dependency object to retreive the property from.</param>
        /// <param name="value">Whether scroll to top notifications are enabled. </param>
        public static void SetScrollToBottom(DependencyObject ob, ICommand value)
        {
            ob.SetValue(ScrollToBottomProperty, value);
        }

        /// <summary>
        /// Retreives a child element of a control.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="element">The control to search in.</param>
        /// <returns>The child control if found.</returns>
        public static T FindSimpleVisualChild<T>(DependencyObject element)
          where T : class
        {
            while (element != null)
            {
                if (element is T)
                {
                    return element as T;
                }

                element = VisualTreeHelper.GetChild(element, 0);
            }

            return null;
        }

        private static void Loaded(object sender, EventArgs e)
        {
            var instance = sender as ListView;

            var oldHandler = (ListViewAutoScrollToEndHandler)instance.GetValue(AutoScrollHandlerProperty);
            if (oldHandler != null)
            {
                oldHandler.Dispose();
                instance.SetValue(AutoScrollHandlerProperty, null);
            }

            instance.SetValue(AutoScrollProperty, true);
            instance.SetValue(AutoScrollHandlerProperty, new ListViewAutoScrollToEndHandler(instance));
        }

        private static void HookupAutoScrollToEnd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ListView ListView))
            {
                return;
            }

            SetAutoScrollToEnd(ListView, (bool)e.NewValue);
        }

        private static void OnScrollToTopBottomPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var ListView = obj as ListView;

            ListView.Loaded -= OnListViewLoaded;
            ListView.Loaded += OnListViewLoaded;
        }

        private static void OnListViewLoaded(object sender, RoutedEventArgs e)
        {
            (sender as ListView).Loaded -= OnListViewLoaded;

            var scrollViewer = FindSimpleVisualChild<ScrollViewer>(sender as ListView);

            scrollViewer.ScrollChanged += OnListViewScrollChanged;
        }

        private static void OnListViewScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            var ListView = scrollViewer.TemplatedParent as ListView;

            // Check to see if scrolled to top
            if (scrollViewer.VerticalOffset == 0)
            {
                var command = GetScrollToTop(ListView);
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(scrollViewer);
                }
            }
            else if ((int)scrollViewer.VerticalOffset == (int)scrollViewer.ScrollableHeight)
            {
                if (e.VerticalChange < 1000)
                {
                    var command = GetScrollToBottom(ListView);
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(scrollViewer);
                    }
                }
            }
        }

        /// <summary>
        /// Handler class to maintain user scroll information for a scrollable <see cref="ListView"/>.
        /// </summary>
        public class ListViewAutoScrollToEndHandler : DependencyObject, IDisposable
        {
            private readonly ScrollViewer scrollViewer;
            private bool doScroll = true;
            private bool disposedValue = false; // To detect redundant calls, for IDisposable

            /// <summary>
            /// Initializes a new instance of the <see cref="ListViewAutoScrollToEndHandler"/> class.
            /// </summary>
            /// <param name="ListView">The ListView to bind to.</param>
            public ListViewAutoScrollToEndHandler(ListView ListView)
            {
                this.scrollViewer = FindSimpleVisualChild<ScrollViewer>(ListView);
                this.scrollViewer.ScrollToEnd();
                this.scrollViewer.ScrollChanged += this.ScrollChanged;
                this.scrollViewer.SizeChanged += this.SizeChanged;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                // This code added to correctly implement the disposable pattern.
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                this.Dispose(true);
            }

            private void ScrollChanged(object sender, ScrollChangedEventArgs e)
            {
                // User scroll event : set or unset autoscroll mode
                if (e.ExtentHeightChange == 0)
                {
                    // Trucate to integers before comparing to prevent round-off errors when high-DPI scaling is used.
                    this.doScroll = (int)this.scrollViewer.VerticalOffset == (int)this.scrollViewer.ScrollableHeight;
                }

                // Content scroll event : autoscroll eventually
                if (this.doScroll && e.ExtentHeightChange != 0)
                {
                    this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.ExtentHeight);
                }
            }

            private void SizeChanged(object sender, SizeChangedEventArgs e)
            {
                // If autoscroll is enabled and scrolled to the bottom, keep the bottom tracked if the size changes.
                if (this.doScroll)
                {
                    this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.ExtentHeight);
                }
            }

            private void Dispose(bool disposing)
            {
                if (!this.disposedValue)
                {
                    if (disposing)
                    {
                        this.scrollViewer.ScrollChanged -= this.ScrollChanged;
                    }

                    this.disposedValue = true;
                }
            }
        }
    }
}