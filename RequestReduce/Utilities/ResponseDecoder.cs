using System.IO;
using System.IO.Compression;

namespace RequestReduce.Utilities
{
    public interface IResponseDecoder
    {
        Stream GetDecodableStream(Stream encodedStream, string encoding);
    }

    public class ResponseDecoder : IResponseDecoder
    {
        public Stream GetDecodableStream(Stream encodedStream, string encoding)
        {
            if (encoding == null)
                return encodedStream;

            encoding = encoding.ToLower();
            if(encoding.Contains("gzip"))
                return new GZipStream(encodedStream, CompressionMode.Decompress);
            if (encoding.Contains("deflate"))
                return new DeflateStream(encodedStream, CompressionMode.Decompress);
            return encodedStream;
        }
    }
}
