using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace WorkTimer4.Assets
{
    internal class WinFormsAssets
    {
        internal static readonly string CLOCK_BLUE = "./Assets/clock_blue.ico";
        internal static readonly string CLOCK_RED = "./Assets/clock_red.ico";

        public static Icon GetApplicationIcon()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static Icon GetResourceIcon(string? resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return WinFormsAssets.GetApplicationIcon();
            }

            if (resourceUri == CLOCK_RED || resourceUri == CLOCK_BLUE)
            {
                var uri = new Uri(resourceUri, UriKind.Relative);
                using (Stream iconStream = System.Windows.Application.GetResourceStream(uri).Stream)
                {
                    return new Icon(iconStream);
                }
            }

            return WinFormsAssets.EncodedImageToIcon(resourceUri);
        }

        internal static Image? GetResourceImage(string? resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            var wpfResource = WPFAssets.GetImageResource(resourceUri);
            if (wpfResource is null)
                return null;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapImage)wpfResource));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Flush();
                return new Bitmap(stream);
            }
        }

        public static Color ColourFromCategory(string? colourCode)
        {
            if (colourCode is null || string.IsNullOrWhiteSpace(colourCode))
                return SystemColors.ControlText;

            if (Enum.TryParse<KnownColor>(colourCode, out var knownColour))
            {
                return Color.FromKnownColor(knownColour);
            }

            try
            {
                return ColorTranslator.FromHtml(colourCode);
            }
            catch
            {
                return Color.Gray;
            }
        }

        internal static Image? FromEncodedImage(string? encodedImage)
        {
            if (string.IsNullOrWhiteSpace(encodedImage))
                return null;

            byte[] data = Convert.FromBase64String(encodedImage);
            using (var ms = new MemoryStream(data))
            {
                return Image.FromStream(ms);
            }
        }

        internal static string? ToEncodedImage(string? filename)
        {
            if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
                return null;

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var bytes = new byte[fs.Length];
                fs.Read(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        internal static Icon EncodedImageToIcon(string? encodedImage)
        {
            if (string.IsNullOrWhiteSpace(encodedImage))
                return GetResourceIcon(CLOCK_RED);

            try
            {
                // Convert the base64 string to a byte array
                byte[] bytes = Convert.FromBase64String(encodedImage);

                // Create a memory stream from the byte array
                using (var ms = new MemoryStream(bytes))
                {
                    // Create an image from the stream
                    using (var image = Image.FromStream(ms))
                    {
                        // Create an icon from the image
                        return Icon.FromHandle(((Bitmap)image).GetHicon());
                    }
                }
            }
            catch
            {
                return GetResourceIcon(CLOCK_RED);
            }
        }
    }
}
