using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GMDCGiphyPlugin
{
    public class GIFData
    {
        private MemoryStream gifStream;

        private string gifStreamURL;

        private string gifName;

        public GIFData()
        {
        }

        public GIFData(MemoryStream stream, string url, string name)
        {
            GIFStream = stream;
            GIFStreamURL = url;
            GIFName = name;
        }

        public MemoryStream GIFStream
        {
            get => this.gifStream;
            private set => this.gifStream = value;
        }

        public string GIFStreamURL
        {
            get => this.gifStreamURL;
            private set => this.gifStreamURL = value;
        }

        public string GIFName
        {
            get => this.gifName;
            private set => this.gifName = value;
        }
    }
}
