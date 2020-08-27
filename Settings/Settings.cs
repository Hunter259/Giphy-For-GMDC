using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMDCGiphyPlugin.Settings
{
    public class Settings
    {
        public CopyBehaviorTypes CopyBehavior
        {
            get;
            set;
        } = CopyBehaviorTypes.Link;

        public GIF_Control.GIFSizeType CopySizeBehavior
        {
            get;
            set;
        } = GIF_Control.GIFSizeType.Downscaled;
    }
}
