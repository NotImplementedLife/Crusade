using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Crusade.Data
{
    public static class Temp
    {        
        public static readonly string DirName = "~crusade.tmp";
        public static void SaveEditedImage(BitmapImage img, ImageListItemData data)
        {
            if (!Directory.Exists(DirName)) 
            {
                Directory.CreateDirectory(DirName);
            }
            var fname = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".png";
            var path = Path.Combine(DirName, fname);


            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));        
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                encoder.Save(fileStream);
            }           

            data.TempPath = path;

            BitmapImage thumbnail = new BitmapImage();
            thumbnail.BeginInit();
            thumbnail.UriSource = new Uri(path, UriKind.Relative);
            if (img.Width > img.Height) 
                thumbnail.DecodePixelWidth = 100;
            else
                thumbnail.DecodePixelHeight = 100;
            try
            {
                thumbnail.EndInit();
            }
            catch (ArgumentException)
            {
                // try recreate thumbnail with IgnoreColorProfile option
                thumbnail = new BitmapImage();
                thumbnail.BeginInit();
                thumbnail.UriSource = new Uri(path, UriKind.Absolute);
                if (img.Width > img.Height) 
                    thumbnail.DecodePixelWidth = 100;
                else
                    thumbnail.DecodePixelHeight = 100;
                thumbnail.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                thumbnail.EndInit();
            }
            thumbnail.Freeze();
            data.Thumbnail = thumbnail;
        }
    }
}
