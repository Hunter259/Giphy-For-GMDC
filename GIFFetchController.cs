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

        public async Task GetTrendingResultMetaData()
        {
            GiphyDotNet.Model.Results.GiphySearchResult trendingResults = await giphyManager.TrendingGifs(new GiphyDotNet.Model.Parameters.TrendingParameter() { Limit = imagePageLimit * currentTrendingPage });
            Data[] temp = trendingResults.Data;
            foreach (Data result in temp)
            {
                trendingResultData.Enqueue(result);
            }
        }

        public string SearchQuery
        {
            get;
            set;
        }

        public async Task GetSearchResultMetaData()
        {
            GiphyDotNet.Model.Results.GiphySearchResult searchResults = await giphyManager.GifSearch(new GiphyDotNet.Model.Parameters.SearchParameter() { Limit = currentSearchPage * imagePageLimit, Query = SearchQuery });
            Data[] temp = searchResults.Data;
            foreach (Data result in temp)
            {
                searchResultData.Enqueue(result);
            }
        }

        public async Task<GIFData> FetchNextTrendingGif()
        {
            Data data = new Data();
            trendingResultData.TryDequeue(out data);
            if (trendingResultData.Count < 6)
            {
                await GetTrendingResultMetaData();
                if (data == null)
                {
                    trendingResultData.TryDequeue(out data);
                }
            }
            GIFData gifData = await getGIFStreams(data);
            currentTrendingPage++;
            return gifData;
        }

        public async Task<GIFData> FetchNextSearchGif()
        {
            Data data = new Data();
            searchResultData.TryDequeue(out data);
            if (searchResultData.Count < 6)
            {
                await GetSearchResultMetaData();
                if (data == null)
                {
                    searchResultData.TryDequeue(out data);
                }
            }
            GIFData gifData = await getGIFStreams(data);
            currentSearchPage++;
            return gifData;
        }

        private async Task<GIFData> getGIFStreams(Data item)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(item.Images.Downsized.Url);
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
