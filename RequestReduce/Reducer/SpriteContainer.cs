using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteContainer : ISpriteContainer
    {
        private readonly IList<Bitmap> images = new List<Bitmap>();
        private readonly IWebClientWrapper webClientWrapper;

        public SpriteContainer(IConfigurationWrapper configWrapper, IWebClientWrapper webClientWrapper)
        {
            this.webClientWrapper = webClientWrapper;
            Url = string.Format("{0}/{1}.png", configWrapper.SpriteDirectory, Guid.NewGuid().ToString());
        }

        public void AddImage (BackgroungImageClass image)
        {
            var imageBytes = webClientWrapper.DownloadBytes(image.ImageUrl);
            var bitmap = new Bitmap(new MemoryStream(imageBytes));
            images.Add(bitmap);
            Size += imageBytes.Length;
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
