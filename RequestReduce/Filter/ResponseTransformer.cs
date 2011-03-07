using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RequestReduce.Filter
{
    public interface IResponseTransformer
    {
        byte[] Transform(byte[] preTransform, Encoding encoding);
    }

    public class ResponseTransformer : IResponseTransformer
    {
        public byte[] Transform(byte[] preTransform, Encoding encoding)
        {
            return new byte[0];
        }
    }
}
