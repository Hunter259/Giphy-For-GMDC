using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

        private ICommand clearSearchBoxCommand;

        private string searchQuery;

        private GIFData previewGIF;

        public MainWindowViewModel()
        {
            fetchController = new GIFFetchController();
            trendingGIFIndexImages = new ObservableCollection<GIFData>();
            searchGIFIndexImages = new ObservableCollection<GIFData>();
            currentState = GIFType.Trending;

            LoadButtonCommand = new RelayCommand(async () => await this.FetchGIFs(), true);
            CopyGIFLinkCommand = new RelayCommand<GIFData>(this.onGIFButtonClick);
            SearchCommand = new RelayCommand<string>(async (s) => await this.onSearchCall(s), true);
            TrendingButtonCommand = new RelayCommand(async () => await this.onTrendingButtonClick(), true);
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
        
        private async Task onTrendingButtonClick()
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

        private async Task onSearchCall(string val)
        {
            currentSearchQuery = val;
            onClearButtonClick();
            currentState = GIFType.Search;
            GIFIndexImages = searchGIFIndexImages;
            fetchController.SearchQuery = val;
            await FetchGIFs();
        }

        private async Task FetchGIFs()
        {
            var tasks = new ConcurrentQueue<Task>();
            if (currentState == GIFType.Trending)
            {
                for (int i = 0; i < 25; i++)
                {
                    tasks.Enqueue(new Task(() =>
                    {
                        if (i == 0)
                        {
                            fetchController.trendingCancel = false;
                        }
                        GIFData data = fetchController.FetchNextTrendingGif().Result;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (data != null)
                            {
                                GIFIndexImages.Add(data);
                            }
                        });
                    }));
                }
            }
            else if (currentState == GIFType.Search)
            {
                for (int i = 0; i < 25; i++)
                {
                    tasks.Enqueue(new Task(() =>
                    {
                        if (i == 0)
                        {
                            fetchController.searchCancel = false;
                        }
                        GIFData data = fetchController.FetchNextSearchGif().Result;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (data != null)
                            {
                                GIFIndexImages.Add(data);
                            }
                        });
                    }));
                }
            }

            while (tasks.Count > 0)
            {
                var taskRunningList = new List<Task>();
                for (int i = 0; i < 5; i++)
                {
                    Task task;
                    tasks.TryDequeue(out task);
                    taskRunningList.Add(task);
                    task.Start();
                }
                await Task.WhenAll(taskRunningList.ToArray());
            }
        }

        private void onGIFButtonClick(GIFData data)
        {
            try {
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

        private IMessageContainer MessageContainer { get; }

        private CacheSession CacheSession { get; }
    }

    public enum GIFType
    {
        Trending,
        Search
    }
}
