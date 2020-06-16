﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GMDCGiphyPlugin.ViewModel
{
    public class MainWindowViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string currentSearchQuery;

        private ObservableCollection<GIFData> gifIndexImages;

        private ObservableCollection<GIFData> trendingGIFIndexImages;

        private ObservableCollection<GIFData> searchGIFIndexImages;

        private GIFFetchController fetchController;

        private GIFType currentState;

        private ICommand trendingButtonCommand;

        private ICommand searchButtonCommand;

        private ICommand loadButtonCommand;

        private ICommand copyGIFLinkCommand;

        private ICommand searchCompletionCommand;

        private string searchQuery;

        public MainWindowViewModel()
        {
            fetchController = new GIFFetchController();
            trendingGIFIndexImages = new ObservableCollection<GIFData>();
            searchGIFIndexImages = new ObservableCollection<GIFData>();
            currentState = GIFType.Trending;
            LoadButtonCommand = new RelayCommand(this.fetchGIFs);
            CopyGIFLinkCommand = new RelayCommand<string>(this.onGIFButtonClick);
            SearchCommand = new RelayCommand<string>(this.onSearchCall);
            TrendingButtonCommand = new RelayCommand(this.onTrendingButtonClick);
            DispatcherHelper.UIDispatcher.InvokeAsync(async () =>
            {
                ObservableCollection<GIFData> results = await fetchController.FetchNextTrendingPage();
                foreach (GIFData ms in results)
                {
                    trendingGIFIndexImages.Add(ms);
                }
                GIFIndexImages = trendingGIFIndexImages;
            });
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

        public void onTrendingButtonClick()
        {
            currentState = GIFType.Trending;
            if (trendingGIFIndexImages.Count == 0)
            {
                fetchGIFs();
            }
            else
            {
                GIFIndexImages = trendingGIFIndexImages;
            }
        }

        public void onSearchCall(string val)
        {
            currentSearchQuery = val;
            searchGIFIndexImages.Clear();
            currentState = GIFType.Search;
            fetchGIFs();
        }

        public void fetchGIFs()
        {
            if (currentState == GIFType.Trending)
            {
                DispatcherHelper.UIDispatcher.InvokeAsync(async () =>
                {
                    var results = await fetchController.FetchNextTrendingPage();
                    foreach (GIFData ms in results)
                    {
                        trendingGIFIndexImages.Add(ms);
                    }
                    GIFIndexImages = trendingGIFIndexImages;
                });
            }
            else if (currentState == GIFType.Search)
            {
                DispatcherHelper.UIDispatcher.InvokeAsync(async () =>
                {
                    var results = await fetchController.FetchNextSearchPage(currentSearchQuery);
                    foreach (GIFData ms in results)
                    {
                        searchGIFIndexImages.Add(ms);
                    }
                    GIFIndexImages = searchGIFIndexImages;
                });
            }
        }

        public void onGIFButtonClick(string url)
        {
            Clipboard.SetText(url);
        }

        private IMessageContainer MessageContainer { get; }

        private CacheSession CacheSession { get; }
    }

    public enum GIFType
    {
        Trending,
        Search
    }
}
