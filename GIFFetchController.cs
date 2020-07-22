using GiphyDotNet.Model.GiphyImage;
using GroupMeClientApi.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GMDCGiphyPlugin
{
    public enum GIFType
    {
        Trending,
        Search
    }
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

        private bool trendingCancel { get; set; }

        private bool searchCancel { get; set; }

        public GIFFetchController()
        {
            giphyManager = new GiphyDotNet.Manager.Giphy(giphyAPIKey);
            trendingResultData = new ConcurrentQueue<Data>();
            searchResultData = new ConcurrentQueue<Data>();
        }

        public string SearchQuery { get; set; }

        private void GetTrendingResultMetaData()
        {
            try
            {
                Data[] temp = Task.Run(async () => await giphyManager.TrendingGifs(new GiphyDotNet.Model.Parameters.TrendingParameter() { Limit = imagePageLimit * currentTrendingPage })).Result.Data;
                if (temp.Length == 0)
                {
                    trendingCancel = true;
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

        private void GetSearchResultMetaData()
        {
            try
            {
                Data[] temp = Task.Run(async () => await giphyManager.GifSearch(new GiphyDotNet.Model.Parameters.SearchParameter() { Limit = imagePageLimit, Offset = (currentSearchPage - 1) * imagePageLimit, Query = SearchQuery })).Result.Data;
                if (temp.Length == 0)
                {
                    searchCancel = true;
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

        public async Task<ConcurrentQueue<GIFData>> FetchNextGIF(GIFType gifType, int count = 1)
        {
            ConcurrentQueue<GIFData> gifDataOut = new ConcurrentQueue<GIFData>();
            var data = new GIFData();

            while (count > 0)
            {
                if (gifType == GIFType.Trending)
                {
                    data = await FetchNextTrendingGif();
                }
                else if (gifType == GIFType.Search)
                {
                    data = await FetchNextSearchGif();
                }
                else
                {
                    data = null;
                }
                gifDataOut.Enqueue(data);
                count--;
            }

            return gifDataOut;
        }

        private async Task<GIFData> FetchNextTrendingGif()
        {
            Data data = new Data();
            if (trendingResultData.Count < 6)
            {
                lock (trendingLock)
                {
                    if (trendingCancel == false)
                    {
                        if (trendingResultData.Count < 6)
                        {
                            GetTrendingResultMetaData();
                            currentTrendingPage++;
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

        private async Task<GIFData> FetchNextSearchGif()
        {
            Data data = new Data();
            if (searchResultData.Count < 6)
            {
                lock (searchLock)
                {
                    if (searchCancel == false)
                    {
                        if (searchResultData.Count < 6)
                        {
                            GetSearchResultMetaData();
                            currentSearchPage++;
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
            currentSearchPage = 1;
        }
    }
}
