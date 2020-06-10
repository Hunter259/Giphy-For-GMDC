using GalaSoft.MvvmLight.Command;
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
        private int indexPageNumberData;

        private string currentSearchQuery;

        private string currentGIFAddress;

        private string currentGIFID;

        private ObservableCollection<GIFData> gifIndexImages;

        private ObservableCollection<GIFData> trendingGIFIndexImages;

        private ObservableCollection<GIFData> searchGIFIndexImages;

        private GIFFetchController fetchController;

        private GIFType currentState;

        private ICommand trendingButtonCommand;

        private ICommand searchButtonCommand;

        private ICommand loadButtonCommand;

        private ICommand copyGIFLinkCommand;

        private ICommand gifMouseOverCommand;

        public MainWindowViewModel()
        {
            indexPageNumberData = 1;
            fetchController = new GIFFetchController();
            trendingGIFIndexImages = new ObservableCollection<GIFData>();
            searchGIFIndexImages = new ObservableCollection<GIFData>();
            currentState = GIFType.Trending;
            LoadButtonCommand = new RelayCommand(this.fetchGIFs);
            CopyGIFLinkCommand = new RelayCommand<string>(this.onGIFButtonClick);
            GIFMouseOverCommand = new RelayCommand<object>(this.onGIFMouseOver);
            SearchButtonCommand = new RelayCommand(this.onSearchButtonClick);
            TrendingButtonCommand = new RelayCommand(this.onTrendingButtonClick);
            Task.Run(() =>
            {
                var results = fetchController.FetchNextTrendingPage();
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (GIFData ms in results.Result)
                    {
                        trendingGIFIndexImages.Add(ms);
                    }
                    GIFIndexImages = trendingGIFIndexImages;
                });
            });
        }

        public MainWindowViewModel(IMessageContainer messageContainer, CacheSession cacheSession) : this()
        {
            this.MessageContainer = messageContainer;
            this.CacheSession = cacheSession;
        }

        public ICommand SearchButtonCommand
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

        public ICommand GIFMouseOverCommand
        {
            get => this.gifMouseOverCommand;
            private set => this.gifMouseOverCommand = value;
        }

        public int IndexPageNumberData
        {
            get => this.indexPageNumberData;
            private set => this.Set(() => this.IndexPageNumberData, ref this.indexPageNumberData, value);
        }

        public ObservableCollection<GIFData> GIFIndexImages
        {
            get => this.gifIndexImages;
            private set => this.Set(() => this.GIFIndexImages, ref this.gifIndexImages, value);
        }

        public string CurrentGIFAddress
        {
            get => this.currentGIFAddress;
            private set => this.Set(() => this.CurrentGIFAddress, ref this.currentGIFAddress, value);
        }

        public string CurrentGIFID
        {
            get => this.currentGIFID;
            private set => this.Set(() => this.CurrentGIFID, ref this.currentGIFID, value);
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

        public void onSearchButtonClick()
        {
            SearchView search = new SearchView();
            SearchViewModel vm = new SearchViewModel();
            search.DataContext = vm;
            search.ShowDialog();

            currentSearchQuery = vm.SearchQuery;
            if (currentSearchQuery != null)
            {
                currentState = GIFType.Search;
                searchGIFIndexImages.Clear();
                fetchGIFs();
            }
        }

        public void fetchGIFs()
        {
            if (currentState == GIFType.Trending)
            {
                Task.Run(() =>
                {
                    var results = fetchController.FetchNextTrendingPage();
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (GIFData ms in results.Result)
                        {
                            trendingGIFIndexImages.Add(ms);
                        }
                        GIFIndexImages = trendingGIFIndexImages;
                    });
                });
            }
            else if (currentState == GIFType.Search)
            {
                Task.Run(() =>
                {
                    var results = fetchController.FetchNextSearchPage(currentSearchQuery);
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        foreach (GIFData ms in results.Result)
                        {
                            searchGIFIndexImages.Add(ms);
                        }
                        GIFIndexImages = searchGIFIndexImages;
                    });
                });
            }
        }

        public void onGIFMouseOver(object dataParams)
        {
            var values = (object[])dataParams;
            string name = (string)values[0];
            string url = (string)values[1];

            CurrentGIFID = name;
            CurrentGIFAddress = url;
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
