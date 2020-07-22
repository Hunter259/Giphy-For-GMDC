﻿using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GMDCGiphyPlugin.ViewModel
{
    public class MainWindowViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string currentSearchQuery;

        private ObservableCollection<GIFData> gifIndexImages;

        private ObservableCollection<GIFData> trendingGIFIndexImages;

        private ObservableCollection<GIFData> searchGIFIndexImages;

        private readonly GIFFetchController fetchController;

        private GIFType currentState;

        private ICommand trendingButtonCommand;

        private ICommand searchButtonCommand;

        private ICommand loadButtonCommand;

        private ICommand copyGIFLinkCommand;

        private ICommand searchCompletionCommand;

        private ICommand clearSearchBoxCommand;

        private ICommand loadMoreInfiniteCommand;

        private string searchQuery;

        private GIFData previewGIF;

        public MainWindowViewModel()
        {
            fetchController = new GIFFetchController();
            trendingGIFIndexImages = new ObservableCollection<GIFData>();
            searchGIFIndexImages = new ObservableCollection<GIFData>();
            currentState = GIFType.Trending;

            LoadButtonCommand = new RelayCommand(async () => await this.FetchGIFs(), true);
            LoadMoreInfiniteCommand = new RelayCommand<ScrollViewer>(async (sv) => await this.FetchGIFs(sv), true);
            CopyGIFLinkCommand = new RelayCommand<GIFData>(this.onGIFButtonClick);
            SearchCommand = new RelayCommand<string>(async (s) => await this.OnSearchCall(s), true);
            TrendingButtonCommand = new RelayCommand(async () => await this.OnTrendingButtonClick(), true);
            ClearSearchBoxCommand = new RelayCommand(this.onClearButtonClick);


            GIFIndexImages = trendingGIFIndexImages;

            var initFetchTask = new Task(async () => await this.FetchGIFs());
            initFetchTask.Start();
        }

        public MainWindowViewModel(IMessageContainer messageContainer, CacheSession cacheSession) : this()
        {
            this.MessageContainer = messageContainer;
            this.CacheSession = cacheSession;
        }

        public ICommand SearchCommand
        {
            get => this.searchButtonCommand;
            private set => this.searchButtonCommand = value;
        }

        public ICommand TrendingButtonCommand
        {
            get => this.trendingButtonCommand;
            private set => this.trendingButtonCommand = value;
        }

        public ICommand LoadButtonCommand
        {
            get => this.loadButtonCommand;
            private set => this.loadButtonCommand = value;
        }

        public ICommand CopyGIFLinkCommand
        {
            get => this.copyGIFLinkCommand;
            private set => this.copyGIFLinkCommand = value;
        }

        public ICommand SearchCompletionCommand
        {
            get => this.searchCompletionCommand;
            private set => this.searchCompletionCommand = value;
        }

        public ICommand ClearSearchBoxCommand
        {
            get => this.clearSearchBoxCommand;
            private set => this.clearSearchBoxCommand = value;
        }

        public ICommand LoadMoreInfiniteCommand
        {
            get => this.loadMoreInfiniteCommand;
            private set => this.loadMoreInfiniteCommand = value;
        }

        public string SearchQuery
        {
            get => searchQuery;
            set => this.Set(() => this.SearchQuery, ref this.searchQuery, value);
        }

        public ObservableCollection<GIFData> GIFIndexImages
        {
            get => this.gifIndexImages;
            private set => this.Set(() => this.GIFIndexImages, ref this.gifIndexImages, value);
        }

        public GIFData PreviewGIF
        {
            get => this.previewGIF;
            set => this.Set(() => this.PreviewGIF, ref this.previewGIF, value);
        }

        private async Task OnTrendingButtonClick()
        {
            currentState = GIFType.Trending;
            if (trendingGIFIndexImages.Count == 0)
            {
                await FetchGIFs();
            }
            else
            {
                GIFIndexImages = trendingGIFIndexImages;
            }
        }

        private async Task OnSearchCall(string val)
        {
            currentSearchQuery = val;
            onClearButtonClick();
            currentState = GIFType.Search;
            GIFIndexImages = searchGIFIndexImages;
            fetchController.SearchQuery = val;
            await FetchGIFs();
        }

        private async Task FetchGIFs(ScrollViewer scrollViewer = null)
        {
            double originalOffset = scrollViewer?.VerticalOffset ?? 0.0;

            var data = await fetchController.FetchNextGIF(currentState, 15);

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var d in data)
                {
                    if (d != null)
                    {
                        gifIndexImages.Add(d);
                    }
                }
                if (originalOffset != 0)
                {
                    OffsetCalc(scrollViewer, originalOffset);
                }
            });
        }

        private void onGIFButtonClick(GIFData data)
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetText(data.GIFStreamURL);
                //Clipboard.SetDataObject(data.GIFStream, true);
            }
            catch (Exception e) { }
        }

        private void onClearButtonClick()
        {
            searchGIFIndexImages.Clear();
            fetchController.ClearSearchQueue();
        }

        private void OffsetCalc(ScrollViewer scrollViewer, double originalOffset)
        {
            ScrollChangedEventHandler delayedUpdateHandler = null;
            int skip = 0;
            delayedUpdateHandler = (s, e) =>
            {
                scrollViewer.ScrollToVerticalOffset(originalOffset);

                if ((int)e.VerticalOffset == (int)originalOffset && skip > 1)
                {
                    scrollViewer.ScrollChanged -= delayedUpdateHandler;
                }

                skip++;
            };

            scrollViewer.ScrollChanged += delayedUpdateHandler;
        }

        private IMessageContainer MessageContainer { get; }

        private CacheSession CacheSession { get; }
    }
}
