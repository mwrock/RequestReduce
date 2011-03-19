using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class Reducer
    {
        private readonly IUrlSplitter urlSplitter;

        public Reducer(IUrlSplitter urlSplitter)
        {
            this.urlSplitter = urlSplitter;
        }

        public string Process(string urls)
        {
            var urlList = urlSplitter.Split(urls);
            /*
             - Iterate each css
	            - Load CSS
	            - Iterate each background image
		            - Load image
		            - Has this image been loaded?
			            - Yes?(fetch position)
			            - No? (Add to new image and mark position)
		            - Replace background image with sprite
	            - Add css to new css
             - save sprited image to disk
             - minify css
             - save css to disk
             - add css url to lookup keyed on hash of old CSS urls             
             */
            return null;
        }
    }
}
