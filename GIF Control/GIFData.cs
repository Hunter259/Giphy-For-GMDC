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
        private MemoryStream gifStream;

        private string gifStreamURL;

        private double gifStreamSize;

        private string gifDownsizedStreamURL;

        private double gifDownsizedStreamSize;

        private string gifName;

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
            get => this.gifStream;
            private set => this.gifStream = value;
        }

        public string GIFOriginalStreamURL
        {
            get => this.gifStreamURL;
            private set => this.gifStreamURL = value;
        }

        public double GIFOriginalStreamSize
        {
            get => this.gifStreamSize;
            private set => this.gifStreamSize = value;
        }

        public string GIFDownsizedStreamURL
        {
            get => this.gifDownsizedStreamURL;
            private set => this.gifDownsizedStreamURL = value;
        }

        public double GIFDownsizedStreamSize
        {
            get => this.gifDownsizedStreamSize;
            private set => this.gifDownsizedStreamSize = value;
        }

        public string GIFName
        {
            get => this.gifName;
            private set => this.gifName = value;
        }
    }
}
