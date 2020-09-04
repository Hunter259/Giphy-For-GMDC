using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using GMDCGiphyPlugin.GIF_Control;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using GMDCGiphyPlugin.Views;
using MahApps.Metro.Controls.Dialogs;
using GMDCGiphyPlugin.Settings;
using MahApps.Metro.Controls;

namespace GMDCGiphyPlugin.ViewModels
{
    public class MainWindowViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string currentSearchQuery;

        private ObservableCollection<GIFData> gifIndexImages;

        private ObservableCollection<GIFData> trendingGIFIndexImages;

        private ObservableCollection<GIFData> searchGIFIndexImages;

        private readonly GIFFetchController fetchController;

        private GIFType currentState;

        private string searchQuery;

        private GIFData previewGIF;

        private Settings.SettingsManager SettingsManager;

        public MainWindowViewModel()
        {
            fetchController = new GIFFetchController();
            trendingGIFIndexImages = new ObservableCollection<GIFData>();
            searchGIFIndexImages = new ObservableCollection<GIFData>();
            currentState = GIFType.Trending;

            LoadButtonCommand = new RelayCommand(async () => await this.FetchGIFs(), true);
            LoadMoreInfiniteCommand = new RelayCommand<ScrollViewer>(async (sv) => await this.FetchGIFs(sv), true);
            CopyGIFLinkCommand = new RelayCommand<GIFSizeType?>(async (gs) => await this.onGIFButtonClick(gs), true);
            SearchCommand = new RelayCommand<string>(async (s) => await this.OnSearchCall(s), true);
            TrendingButtonCommand = new RelayCommand(async () => await this.OnTrendingButtonClick(), true);
            ClearSearchBoxCommand = new RelayCommand(this.onClearButtonClick);
            OpenSettingsCommand = new RelayCommand(this.OpenSettings);

            GIFIndexImages = trendingGIFIndexImages;

            var initFetchTask = new Task(async () => await this.FetchGIFs());
            initFetchTask.Start();

            SettingsManager = SimpleIoc.Default.GetInstance<Settings.SettingsManager>();
        }

        public MainWindowViewModel(IMessageContainer messageContainer, CacheSession cacheSession) : this()
        {
            this.MessageContainer = messageContainer;
            this.CacheSession = cacheSession;
        }

        public ICommand SearchCommand
        {
            get;
            private set;
        }

        public ICommand TrendingButtonCommand
        {
            get;
            private set;
        }

        public ICommand LoadButtonCommand
        {
            get;
            private set;
        }

        public ICommand CopyGIFLinkCommand
        {
            get;
            private set;
        }

        public ICommand SearchCompletionCommand
        {
            get;
            private set;
        }

        public ICommand ClearSearchBoxCommand
        {
            get;
            private set;
        }

        public ICommand LoadMoreInfiniteCommand
        {
            get;
            private set;
        }

        public ICommand OpenSettingsCommand
        {
            get;
            private set;
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

            var data = await fetchController.FetchNextGIF(currentState, 10);

            await Application.Current.Dispatcher.InvokeAsync(() =>
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

        private async Task onGIFButtonClick(GIFSizeType? size)
        {
            if (size == null)
            {
                size = SettingsManager.Settings.CopySizeBehavior;
            }
            try
            {
                Clipboard.Clear();
                if (SettingsManager.Settings.CopyBehavior == Settings.CopyBehaviorTypes.Link)
                {
                    if (size == GIFSizeType.Downscaled)
                    {
                        Clipboard.SetText(PreviewGIF.GIFDownsizedStreamURL);
                    }
                    else
                    {
                        Clipboard.SetText(PreviewGIF.GIFOriginalStreamURL);
                    }
                }
                else
                {
                    if (size == GIFSizeType.Downscaled)
                    {
                        PreviewGIF.GIFStream.Position = 0;
                        var convertedGIF = new System.Windows.DataObject();
                        convertedGIF.SetData("PNG", PreviewGIF.GIFStream);
                        convertedGIF.SetData(PreviewGIF.GIFStream);
                        
                        Clipboard.SetDataObject(convertedGIF, true);
                    }
                    else
                    {
                        var gif = await fetchController.DownloadGIFStream(PreviewGIF.GIFOriginalStreamURL);
                        var convertedGIF = new System.Windows.DataObject();
                        convertedGIF.SetData("PNG", gif);
                        convertedGIF.SetData(gif);

                        Clipboard.SetDataObject(convertedGIF, true);

                        gif.Dispose();
                    }
                }
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

        private void OpenSettings()
        {
            ViewModels.SettingsViewModel settingsViewModel = new SettingsViewModel();
            SettingsView settingsView = new SettingsView();
            settingsView.DataContext = settingsViewModel;

            MetroWindow window = new MetroWindow
            {
                Content = settingsView,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };

            window.ShowDialog();
        }

        private IMessageContainer MessageContainer { get; }

        private CacheSession CacheSession { get; }
    }
}
