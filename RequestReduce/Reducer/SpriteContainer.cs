using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteContainer : IEnumerable<Bitmap>
    {
        private readonly IList<Bitmap> images = new List<Bitmap>();

        public void AddImage (string imageUrl)
        {
            var webClientWrapper = RRContainer.Current.GetInstance<IWebClientWrapper>();
            int size;
            var bitmap = webClientWrapper.DownloadImage(imageUrl, out size);
            images.Add(bitmap);
            Size += size;
            Width += bitmap.Width;
            if (Height < bitmap.Height) Height = bitmap.Height;
        }

        public string Url { get; set; }
        public int Size { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IEnumerator<Bitmap> GetEnumerator()
        {
            return images.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
