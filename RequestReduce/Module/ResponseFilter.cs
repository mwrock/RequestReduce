using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RequestReduce.Module
{
    public class ResponseFilter : AbstractFilter
    {
        private readonly Encoding encoding;
        private readonly IResponseTransformer responseTransformer;
        private IList<byte[]> StartStringUpper;
        private bool[] currentStartStringsToSkip;
        private IList<byte[]> StartStringLower;
        private byte[] StartCloseChar;
        private IList<byte[]> EndStringUpper;
        private IList<byte[]> EndStringLower;
        private SearchState state = SearchState.LookForStart;
        private int matchPosition = 0;
        private List<byte> transformBuffer = new List<byte>();
        private int actualOffset = 0;
        private int actualLength = 0;

        private enum SearchState
        {
            LookForStart,
            MatchingStart,
            MatchingStartClose,
            LookForStop,
            MatchingStop
        }

        private enum MatchingEnd
        {
            Start,
            End
        }

        public ResponseFilter(Stream baseStream, Encoding encoding, IResponseTransformer responseTransformer)
        {
            this.encoding = encoding;
            this.responseTransformer = responseTransformer;
            BaseStream = baseStream;

            StartStringUpper = new List<byte[]> { encoding.GetBytes("<HEAD"), encoding.GetBytes("<SCRIPT") };
            StartStringLower = new List<byte[]> { encoding.GetBytes("<head"), encoding.GetBytes("<script") };
            currentStartStringsToSkip =  new bool[StartStringUpper.Count];
            StartCloseChar = encoding.GetBytes("> ");
            EndStringUpper = new List<byte[]>{encoding.GetBytes("</HEAD>"), encoding.GetBytes("</SCRIPT>")};
            EndStringLower = new List<byte[]>{encoding.GetBytes("</head>"), encoding.GetBytes("</script>")};
        }

        protected Stream BaseStream { get; private set; }
        protected bool Closed { get; private set; }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return !Closed; }
        }

        public override void Close()
        {
            Closed = true;
            BaseStream.Close();
        }

        public override void Flush()
        {
            BaseStream.Flush();
            RRTracer.Trace("Flushing Filter");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            RRTracer.Trace("Beginning Filter Write");
            if (Closed) throw new ObjectDisposedException("ResponseFilter");
            actualOffset = offset;
            actualLength = count;

            var startTransformPosition = 0;
            var endTransformPosition = 0;

            for (var idx = offset; idx < (count+offset); idx++)
                matchPosition = HandleMatch(idx, buffer[idx], buffer, ref startTransformPosition, ref endTransformPosition);

            RRTracer.Trace("Response filter state is {0}", state);
            switch (state)
            {
                case SearchState.LookForStart:
                    if (endTransformPosition > 0)
                    {
                        if ((actualOffset + actualLength) - endTransformPosition > 0)
                            BaseStream.Write(buffer, endTransformPosition, (actualOffset + actualLength) - endTransformPosition);
                    }
                    else
                        BaseStream.Write(buffer, actualOffset, actualLength);
                    break;
                case SearchState.MatchingStart:
                case SearchState.MatchingStartClose:
                case SearchState.LookForStop:
                case SearchState.MatchingStop:
                    BaseStream.Write(buffer, actualOffset, startTransformPosition);
                    break;
            }
            RRTracer.Trace("Ending Filter Write");
        }

        private int HandleMatch(int i, byte b, byte[] buffer, ref int startTransformPosition, ref int endTransformPosition)
        {
            switch (state)
            {
                case SearchState.LookForStart:
                    return HandleLookForStartMatch(i, b, ref startTransformPosition);
                case SearchState.MatchingStart:
                    return HandleMatchingStartMatch(b);
                case SearchState.MatchingStartClose:
                    return HandleMatchingStartCloseMatch(i, b);
                case SearchState.LookForStop:
                    return HandleLookForStopMatch(b);
                case SearchState.MatchingStop:
                    return HandleMatchingStopMatch(i, b, buffer, ref endTransformPosition, ref startTransformPosition);
            }

            return matchPosition;
        }

        private int HandleMatchingStartCloseMatch(int i, byte b)
        {
            if (StartCloseChar.Contains(b))
            {
                transformBuffer.Add(b);
                state = SearchState.LookForStop;
                return 0;
            }
            state = SearchState.LookForStart;
            BaseStream.Write(transformBuffer.ToArray(), 0, transformBuffer.Count);
            return 0;
        }

        private int HandleMatchingStopMatch(int i, byte b, byte[] buffer, ref int endTransformPosition, ref int startTransformPosition)
        {
            transformBuffer.Add(b);
            if (IsMatch(MatchingEnd.End, b))
            {
                matchPosition++;
                for (var idx = 0; idx < currentStartStringsToSkip.Length; idx++)
                {
                    if (!currentStartStringsToSkip[idx] && matchPosition == EndStringUpper[idx].Length)
                    {
                        endTransformPosition = ++i;
                        state = SearchState.LookForStart;
                        for (var idx2 = 0; idx2 < currentStartStringsToSkip.Length; idx2++ )
                            currentStartStringsToSkip[idx2] = false;
                        if ((startTransformPosition - actualOffset) >= 0)
                            BaseStream.Write(buffer, actualOffset, startTransformPosition - actualOffset);
                        var transformed =
                            encoding.GetBytes(responseTransformer.Transform(encoding.GetString(transformBuffer.ToArray())));
                        BaseStream.Write(transformed, 0, transformed.Length);
                        actualLength = actualLength - (i - actualOffset);
                        actualOffset = i;
                        startTransformPosition = 0;
                        transformBuffer.Clear();
                        return 0;
                    }
                }
            }
            else
            {
                state = SearchState.LookForStop;
                return 0;
            }
            return matchPosition;
        }

        private int HandleLookForStopMatch(byte b)
        {
            transformBuffer.Add(b);
            if(IsMatch(MatchingEnd.End, b))
            {
                state = SearchState.MatchingStop;
                matchPosition++;
            }
            return matchPosition;
        }

        private int HandleMatchingStartMatch(byte b)
        {
            if(IsMatch(MatchingEnd.Start, b))
            {
                transformBuffer.Add(b);
                matchPosition++;
                for (var i = 0; i < currentStartStringsToSkip.Length; i++)
                {
                    if (!currentStartStringsToSkip[i] && matchPosition == StartStringUpper[i].Length)
                    {
                        matchPosition = 0;
                        state = SearchState.MatchingStartClose;
                    }
                    else if (matchPosition == 0)
                        currentStartStringsToSkip[i] = true;
                }
                return matchPosition;
            }
            state = SearchState.LookForStart;
            for (var idx2 = 0; idx2 < currentStartStringsToSkip.Length; idx2++)
                currentStartStringsToSkip[idx2] = false;
            return 0;
        }

        private int HandleLookForStartMatch(int i, byte b, ref int startTransformPosition)
        {
            if(IsMatch(MatchingEnd.Start, b))
            {
                state = SearchState.MatchingStart;
                transformBuffer.Clear();
                startTransformPosition = i;
                return HandleMatchingStartMatch(b);
            }
            return matchPosition;
        }

        private bool IsMatch(MatchingEnd end, byte b)
        {
            IList<byte[]> upper;
            IList<byte[]> lower;
            if(end == MatchingEnd.Start)
            {
                upper = StartStringUpper;
                lower = StartStringLower;
            }
            else
            {
                upper = EndStringUpper;
                lower = EndStringLower;
            }

            for(var i = 0; i < currentStartStringsToSkip.Length; i++)
            {
                if (!currentStartStringsToSkip[i])
                {
                    var lowerToMatch = lower[i];
                    var upperToMatch = upper[i];
                    if (!(b == lowerToMatch[matchPosition] || b == upperToMatch[matchPosition]))
                    {
                        if (end == MatchingEnd.Start && state == SearchState.MatchingStart)
                            currentStartStringsToSkip[i] = true;
                        else
                            return false;
                    }
                }
            }
            return currentStartStringsToSkip.Any(x => x == false);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
