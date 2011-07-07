using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RequestReduce.Utilities
{
    public class Hasher
    {
        private static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

        public static Guid Hash(string input)
        {
            return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        public static Guid Hash(byte[] bytes)
        {
            return new Guid(md5.ComputeHash(bytes));
        }
    }
}
