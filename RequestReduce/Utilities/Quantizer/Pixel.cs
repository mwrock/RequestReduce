namespace RequestReduce.Utilities.Quantizer
{
    public struct Pixel
    {
        public Pixel(byte alpha, byte red, byte green, byte blue) : this()
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
    }
}