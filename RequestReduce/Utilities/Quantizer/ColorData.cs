using System.Collections.Generic;

namespace RequestReduce.Utilities.Quantizer
{
    public class ColorData
    {
        public ColorData(int dataGranularity)
        {
            dataGranularity++;
            Weights = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            MomentsAlpha = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            MomentsRed = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            MomentsGreen = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            MomentsBlue = new long[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            Moments = new float[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
            QuantizedPixels = new List<int>();
            Pixels = new List<Pixel>();
        }

        public long[, , ,] Weights { get; private set; }
        public long[, , ,] MomentsAlpha { get; private set; }
        public long[, , ,] MomentsRed { get; private set; }
        public long[, , ,] MomentsGreen { get; private set; }
        public long[, , ,] MomentsBlue { get; private set; }
        public float[, , ,] Moments { get; private set; }
        public IList<int> QuantizedPixels { get; private set; }
        public IList<Pixel> Pixels { get; private set; }
    }
}