using GiphyDotNet.Model.GiphyImage;
using GroupMeClientApi.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GMDCGiphyPlugin
{
    public class GIFFetchController
    {
        private readonly GiphyDotNet.Manager.Giphy giphyManager;

        private readonly string giphyAPIKey = "RJf1JV2j9TDFocdVWerDaJ5PzZ382F5z";

        private readonly int imagePageLimit = 100;

        private static readonly Object trendingLock = new Object();

        private static readonly Object searchLock = new Object();

        private ConcurrentQueue<Data> trendingResultData;

        private ConcurrentQueue<Data> searchResultData;

        private int currentTrendingPage = 1;

        private int currentSearchPage = 1;

        public GIFFetchController()
        {
            giphyManager = new GiphyDotNet.Manager.Giphy(giphyAPIKey);
            trendingResultData = new ConcurrentQueue<Data>();
            searchResultData = new ConcurrentQueue<Data>();
        }

        public bool noTrendingGifs { get; set; }

        public bool noSearchGifs { get; set; }

        public bool trendingCancel { get; set; }

        public bool searchCancel { get; set; }

        public string SearchQuery { get; set; }

        public void GetTrendingResultMetaData()
        {
            try
            {
                Data[] temp = Task.Run(async () => await giphyManager.TrendingGifs(new GiphyDotNet.Model.Parameters.TrendingParameter() { Limit = imagePageLimit * currentTrendingPage })).Result.Data;
                if (temp.Length == 0)
                {
                    noTrendingGifs = true;
                    MessageBox.Show("No GIFs found", "Trending Metadata");
                    return;
                }
                for (int i = imagePageLimit * (currentTrendingPage - 1); i < temp.Length; i++)
                {
                    trendingResultData.Enqueue(temp[i]);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.InnerException.Message, "Trending Metadata");
                trendingCancel = true;
            }
        }

        public void GetSearchResultMetaData()
        {
            try
            {
                Data[] temp = Task.Run(async () => await giphyManager.GifSearch(new GiphyDotNet.Model.Parameters.SearchParameter() { Limit = imagePageLimit, Offset = (currentSearchPage - 1) * imagePageLimit, Query = SearchQuery })).Result.Data;
                if (temp.Length == 0)
                {
                    noSearchGifs = true;
                    MessageBox.Show("No GIFs found", "Search Metadata");
                    return;
                }
                foreach (Data result in temp)
                {
                    searchResultData.Enqueue(result);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.InnerException.Message, "Search Metadata");
                searchCancel = true;
            }
        }

        public async Task<GIFData> FetchNextTrendingGif()
        {
            Data data = new Data();
            if (trendingResultData.Count < 6)
            {
                lock (trendingLock)
                {
                    if (trendingCancel == false)
                    {
                        if (trendingResultData.Count < 6 && noTrendingGifs == false)
                        {
                            GetTrendingResultMetaData();
                            currentTrendingPage++;
                        }
                        else if (noTrendingGifs == true)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            trendingResultData.TryDequeue(out data);
            GIFData gifData;
            if (data != null)
            {
                gifData = await getGIFStreams(data);
            }
            else
            {
                gifData = null;
            }
            return gifData;
        }

        public async Task<GIFData> FetchNextSearchGif()
        {
            Data data = new Data();
            if (searchResultData.Count < 6)
            {
                lock (searchLock)
                {
                    if (searchCancel == false)
                    {
                        if (searchResultData.Count < 6 && noSearchGifs == false)
                        {
                            GetSearchResultMetaData();
                            currentSearchPage++;
                        }
                        else if (noSearchGifs == true)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            searchResultData.TryDequeue(out data);
            GIFData gifData;
            if (data != null)
            {
                gifData = await getGIFStreams(data);
            }
            else
            {
                gifData = null;
            }
            return gifData;
        }

        private async Task<GIFData> getGIFStreams(Data item)
        {
            string url;
            if (item.Images.Downsized.Url == null)
            {
                url = item.Images.Original.Url;
            }
            else
            {
                url = item.Images.Downsized.Url;
            }
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            WebResponse httpWebResponse = await httpWebRequest.GetResponseAsync();
            Stream imageStream = httpWebResponse.GetResponseStream();
            MemoryStream GIFStream = new MemoryStream();
            imageStream.CopyTo(GIFStream);
            GIFStream.Position = 0;

            GIFData fullGIFData = new GIFData(GIFStream, item.Images.Original.Url, item.Username);

            return fullGIFData;
        }

        public void ClearSearchQueue()
        {
            searchResultData = new ConcurrentQueue<Data>();
        }
    }
}
