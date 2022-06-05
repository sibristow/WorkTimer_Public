using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WorkTimer4.Assets
{
    internal class WPFAssets
    {
        public static ImageSource? GetImageResource(string? resource)
        {
            if (string.IsNullOrWhiteSpace(resource))
                return null;

            var packUri = ToPackUri(resource);
            var uri = new Uri(packUri, UriKind.Absolute);
            //var uri = new Uri(resource, UriKind.Relative);
            return new BitmapImage(uri);
        }

        public static ImageSource FromImage(System.Drawing.Image? winImage)
        {
            BitmapImage bitmap = new BitmapImage();

            if (winImage == null)
                return bitmap;

            using (MemoryStream stream = new MemoryStream())
            {
                // Save to the stream
                winImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                // Rewind the stream
                stream.Seek(0, SeekOrigin.Begin);

                // Tell the WPF BitmapImage to use this stream
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }

        internal static string ToPackUri(string resource)
        {
            return string.Format("pack://application:,,,{0}", resource.TrimStart('.'));
        }
    }
}
