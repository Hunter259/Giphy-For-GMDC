using GiphyDotNet.Model.GiphyImage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
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

        public void GetTrendingResultMetaData()
        {
            GiphyDotNet.Model.Results.GiphySearchResult trendingResults = giphyManager.TrendingGifs(new GiphyDotNet.Model.Parameters.TrendingParameter() { Limit = imagePageLimit * currentTrendingPage }).Result;
            Data[] temp = trendingResults.Data;
            for (int i = imagePageLimit * (currentTrendingPage-1); i < temp.Length; i++)
            {
                trendingResultData.Enqueue(temp[i]);
            }
        }

        public string SearchQuery
        {
            get;
            set;
        }

        public void GetSearchResultMetaData()
        {
            GiphyDotNet.Model.Results.GiphySearchResult searchResults = giphyManager.GifSearch(new GiphyDotNet.Model.Parameters.SearchParameter() { Limit = currentSearchPage * imagePageLimit, Query = SearchQuery }).Result;
            Data[] temp = searchResults.Data;
            for (int i = imagePageLimit * (currentSearchPage - 1); i < temp.Length; i++)
            {
                searchResultData.Enqueue(temp[i]);
            }
        }

        public async Task<GIFData> FetchNextTrendingGif()
        {
            Data data = new Data();
            lock (trendingLock)
            {
                if (trendingResultData.Count < 6)
                {
                    currentTrendingPage++;
                    GetTrendingResultMetaData();
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
            lock (searchLock)
            {
                if (searchResultData.Count < 6)
                {
                    currentSearchPage++;
                    GetSearchResultMetaData();
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
    }
}
