using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using RequestReduce.Configuration;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SpriteManager : ISpriteManager
    {
        protected IList<Bitmap> unflushedImages = new List<Bitmap>();
        private IWebClientWrapper webClientWrapper = null;
        private string currentSpriteUrl = null;
        private IConfigurationWrapper configWrapper = null;
        private int currentPosition = 0;

        public SpriteManager(IWebClientWrapper webClientWrapper, IConfigurationWrapper configWrapper)
        {
            this.webClientWrapper = webClientWrapper;
            this.configWrapper = configWrapper;
            currentSpriteUrl = string.Format("{0}/{1}.png", configWrapper.SpriteDirectory, Guid.NewGuid().ToString());
        }

        public virtual bool Contains(string imageUrl)
        {
            throw new NotImplementedException();
        }

        public virtual Sprite this[string imageUrl]
        {
            get { throw new NotImplementedException(); }
        }

        public virtual Sprite Add(string imageUrl)
        {
            var bitmap = webClientWrapper.DownloadImage(imageUrl);
            unflushedImages.Add(bitmap);
            var currentPositionToReturn = currentPosition;
            currentPosition += bitmap.Width;
            return new Sprite(currentPositionToReturn, currentSpriteUrl);
        }

        public virtual void Flush()
        {
            throw new NotImplementedException();
        }
    }
}