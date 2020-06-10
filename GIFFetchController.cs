using GiphyDotNet.Model.GiphyImage;
using System;
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
        private GiphyDotNet.Manager.Giphy giphyManager;

        private readonly string giphyAPIKey = "RJf1JV2j9TDFocdVWerDaJ5PzZ382F5z";

        private readonly int imagePageLimit = 25;

        private int currentTrendingPage = 1;

        public GIFFetchController()
        {
            giphyManager = new GiphyDotNet.Manager.Giphy(giphyAPIKey);
        }

        public async Task<ObservableCollection<GIFData>> FetchNextTrendingPage()
        {
            GiphyDotNet.Model.Results.GiphySearchResult trendingResults = await giphyManager.TrendingGifs(new GiphyDotNet.Model.Parameters.TrendingParameter() { Limit = imagePageLimit*currentTrendingPage });
            Data[] resultData = trendingResults.Data;
            ObservableCollection<GIFData> gifData = await getGIFStreams(currentTrendingPage, resultData);
            currentTrendingPage++;
            return gifData;
        }

        public async Task<ObservableCollection<GIFData>> FetchNextSearchPage(string searchQuery)
        {
            GiphyDotNet.Model.Results.GiphySearchResult searchResults = await giphyManager.GifSearch(new GiphyDotNet.Model.Parameters.SearchParameter() { Limit = imagePageLimit * currentTrendingPage, Query = searchQuery });
            Data[] resultData = searchResults.Data;
            ObservableCollection<GIFData> gifData = await getGIFStreams(currentTrendingPage, resultData);
            currentTrendingPage++;
            return gifData;
        }

        public async Task<ObservableCollection<GIFData>> getGIFStreams(int currentPage, Data[] resultData)
        {
            ObservableCollection<GIFData> gifData = new ObservableCollection<GIFData>();
            var newArray = resultData.Skip((currentTrendingPage - 1) * imagePageLimit);

            var tasks = newArray.Select(async item =>
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(item.Images.Downsized.Url);
                WebResponse httpWebResponse = await httpWebRequest.GetResponseAsync();
                Stream imageStream = httpWebResponse.GetResponseStream();
                MemoryStream GIFStream = new MemoryStream();
                imageStream.CopyTo(GIFStream);
                GIFStream.Position = 0;

                GIFData fullGIFData = new GIFData(GIFStream, item.Images.Original.Url, item.Username);
                gifData.Add(fullGIFData);
            });

            await Task.WhenAll(tasks);
            return gifData;
        }
    }
}
