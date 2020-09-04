using GalaSoft.MvvmLight;
using GiphyDotNet.Model.GiphyImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GMDCGiphyPlugin.GIF_Control
{
    public class GIFData
    {
        private ObservableObject observableObject = new ObservableObject();

        public GIFData()
        {
        }

        public GIFData(MemoryStream stream, Original orignal, Downsized downsized, string name)
        {
            GIFStream = stream;
            GIFOriginalStreamURL = orignal.Url;
            GIFOriginalStreamSize = (Convert.ToDouble(orignal.Size) / 1024d) / 1024d;
            GIFDownsizedStreamURL = downsized.Url;
            GIFDownsizedStreamSize = (Convert.ToDouble(downsized.Size) / 1024d) / 1024d;
            GIFName = name;
        }

        public MemoryStream GIFStream
        {
            get;
            private set;
        }

        public string GIFOriginalStreamURL
        {
            get;
            private set;
        }

        public double GIFOriginalStreamSize
        {
            get;
            private set;
        }

        public string GIFDownsizedStreamURL
        {
            get;
            private set;
        }

        public double GIFDownsizedStreamSize
        {
            get;
            private set;
        }

        public string GIFName
        {
            get;
            private set;
        }
    }
}
