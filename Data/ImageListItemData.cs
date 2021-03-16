using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Crusade.Data
{
    public class ImageListItemData
    {
        public ImageListItemData(string path, BitmapImage thumbnail)
        {
            Path = path;
            Filename = System.IO.Path.GetFileName(path);
            Thumbnail = thumbnail;
        }

        public string Path { get; }
        public string Filename { get; }
        public BitmapImage Thumbnail { get; set; } = null;
        public string TempPath = "";
    }
}
