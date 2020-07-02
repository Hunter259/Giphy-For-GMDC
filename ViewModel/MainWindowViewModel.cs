using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        private readonly GIFFetchController fetchController;

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

            LoadButtonCommand = new RelayCommand(async () => await this.FetchGIFs(), true);
            CopyGIFLinkCommand = new RelayCommand<string>(this.onGIFButtonClick);
            SearchCommand = new RelayCommand<string>(async (s) => await this.onSearchCall(s), true);
            TrendingButtonCommand = new RelayCommand(async () => await this.onTrendingButtonClick(), true);

            GIFIndexImages = trendingGIFIndexImages;
            this.FetchGIFs();
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

        public async Task onTrendingButtonClick()
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

        public async Task onSearchCall(string val)
        {
            currentSearchQuery = val;
            searchGIFIndexImages.Clear();
            currentState = GIFType.Search;
            GIFIndexImages = searchGIFIndexImages;
            fetchController.SearchQuery = val;
            await FetchGIFs();
        }

        public async Task FetchGIFs()
        {
            if (currentState == GIFType.Trending)
            {
                for (int i = 0; i < 25; i++)
                {
                    GIFData data = await fetchController.FetchNextTrendingGif();
                    await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                    {
                        GIFIndexImages.Add(data);
                    });
                }
            }
            else if (currentState == GIFType.Search)
            {
                for (int i = 0; i < 25; i++)
                {
                    GIFData data = await fetchController.FetchNextSearchGif();
                    await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                    {
                        GIFIndexImages.Add(data);
                    });
                }
            }
        }

        public void onGIFButtonClick(string url)
        {
            try {
                Clipboard.Clear();
                Clipboard.SetText(url); 
            }
            catch (Exception e) { }
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
