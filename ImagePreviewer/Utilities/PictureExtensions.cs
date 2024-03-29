﻿using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImagePreviewer.Utilities
{
    public static class PictureExtensions
    {
        public static byte[] ToByteArray(this BitmapSource bitmapSource)
        {

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        public static BitmapSource ToBitmapSource(this byte[] bytes)
        {

            using (var stream = new MemoryStream(bytes))
            {
                var decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                return decoder.Frames.First();
            }
        }
    }
}
