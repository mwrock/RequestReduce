using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Utilities;

namespace RequestReduce.Module
{
    public class ResponseFilter : AbstractFilter
    {
        private readonly HttpContextBase context;
        private readonly Encoding encoding;
        private readonly IResponseTransformer responseTransformer;
        private byte[][] startStringUpper;
        private bool[] currentStartStringsToSkip;
        private byte[][] startStringLower;
        private readonly byte[] startCloseChar;
        private readonly byte[] whiteSpaceChar;
        private byte[][] endStringUpper;
        private byte[][] endStringLower;
        private readonly byte[][] okWhenAdjacentToScriptStartUpper;
        private readonly byte[][] okWhenAdjacentToScriptStartLower;
        private readonly byte[][] okWhenAdjacentToScriptEndUpper;
        private readonly byte[][] okWhenAdjacentToScriptEndLower;
        private SearchState state = SearchState.LookForStart;
        private int matchPosition;
        private readonly List<byte> transformBuffer = new List<byte>();
        private int actualOffset;
        private int actualLength;
        private bool isAdjacent;
        private int originalOffset;
        private readonly Stopwatch watch = new Stopwatch();
        public const string ContextKey = "HttpOnlyFilteringModuleInstalled";

        private enum SearchState
        {
            LookForStart,
            MatchingStart,
            MatchingStartClose,
            LookForStop,
            MatchingStop,
            LookForAdjacentScript
        }

        private enum MatchingEnd
        {
            Start,
            End
        }

        public ResponseFilter(HttpContextBase context, Stream baseStream, Encoding encoding, IResponseTransformer responseTransformer)
        {
            this.context = context;
            this.encoding = encoding;
            this.responseTransformer = responseTransformer;
            BaseStream = baseStream;
            InitSearchArrays();
            okWhenAdjacentToScriptStartUpper = new[] { encoding.GetBytes("<NOSCRIPT"), encoding.GetBytes("<!--") };
            okWhenAdjacentToScriptStartLower = new[] { encoding.GetBytes("<noscript"), encoding.GetBytes("<!--") };
            okWhenAdjacentToScriptEndUpper = new[] { encoding.GetBytes("</NOSCRIPT>"), encoding.GetBytes("-->") };
            okWhenAdjacentToScriptEndLower = new[] { encoding.GetBytes("</noscript>"), encoding.GetBytes("-->") };
            currentStartStringsToSkip = new bool[startStringUpper.Length];
            startCloseChar = encoding.GetBytes("> ");
            whiteSpaceChar = encoding.GetBytes("\t\n\r ");
        }

        private void InitSearchArrays()
        {
            startStringUpper = new[] { encoding.GetBytes("<HEAD"), encoding.GetBytes("<SCRIPT") };
            startStringLower = new[] { encoding.GetBytes("<head"), encoding.GetBytes("<script") };
            endStringUpper = new[] { encoding.GetBytes("</HEAD>"), encoding.GetBytes("</SCRIPT>") };
            endStringLower = new[] { encoding.GetBytes("</head>"), encoding.GetBytes("</script>") };
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
            watch.Start();
            if (isAdjacent)
            {
                var transformed =
                    encoding.GetBytes(responseTransformer.Transform(encoding.GetString(transformBuffer.ToArray())));
                BaseStream.Write(transformed, 0, transformed.Length);
                transformBuffer.Clear();
            }
            BaseStream.Flush();
            RRTracer.Trace("Flushing Filter");
            var filterQs = context != null && context.Request != null ? context.Request.QueryString["rrfilter"] : null;
            watch.Stop();
            if (filterQs == "time") context.Response.Headers["X-RequestReduce-Time"] = watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            watch.Start();

            RRTracer.Trace("Beginning Filter Write");
            if (Closed) throw new ObjectDisposedException("ResponseFilter");
            originalOffset = actualOffset = offset;
            actualLength = count;

            var startTransformPosition = 0;
            var endTransformPosition = 0;

            for (var idx = offset; idx < (count+offset); idx++)
                matchPosition = HandleMatch(ref idx, buffer[idx], buffer, ref startTransformPosition, ref endTransformPosition);

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
                case SearchState.LookForAdjacentScript:
                    if (startTransformPosition > actualOffset)
                        BaseStream.Write(buffer, actualOffset, startTransformPosition - actualOffset);
                    break;
            }
            RRTracer.Trace("Ending Filter Write");
            watch.Stop();
        }

        private int HandleMatch(ref int i, byte b, byte[] buffer, ref int startTransformPosition, ref int endTransformPosition)
        {
            switch (state)
            {
                case SearchState.LookForStart:
                    return HandleLookForStartMatch(ref i, b, buffer, ref endTransformPosition, ref startTransformPosition);
                case SearchState.MatchingStart:
                    return HandleMatchingStartMatch(b, ref i, buffer, ref endTransformPosition, ref startTransformPosition);
                case SearchState.MatchingStartClose:
                    return HandleMatchingStartCloseMatch(i, b);
                case SearchState.LookForStop:
                    return HandleLookForStopMatch(b);
                case SearchState.MatchingStop:
                    return HandleMatchingStopMatch(i, b, buffer, ref endTransformPosition, ref startTransformPosition);
                case SearchState.LookForAdjacentScript:
                    return HandleLookForAdjacentScriptMatch(buffer, i, b, ref endTransformPosition, ref startTransformPosition);
            }

            return matchPosition;
        }

        private int HandleMatchingStartCloseMatch(int i, byte b)
        {
            if (startCloseChar.Contains(b) || (currentStartStringsToSkip.Length == 4 && !currentStartStringsToSkip[3]))
            {
                transformBuffer.Add(b);
                state = SearchState.LookForStop;
                return 0;
            }
            if (i - originalOffset < transformBuffer.Count)
                BaseStream.Write(transformBuffer.ToArray(), 0, (transformBuffer.Count - (i - originalOffset)));
            transformBuffer.Clear();
            state = SearchState.LookForStart;
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
                    if (!currentStartStringsToSkip[idx] && matchPosition == endStringUpper[idx].Length)
                    {
                        endTransformPosition = ++i;
                        if (idx == 0) //head
                        {
                            DoTransform(buffer, ref startTransformPosition);
                            state = SearchState.LookForStart;
                            actualLength = actualLength - (i - actualOffset); actualOffset = i;
                        }
                        else
                        {
                            state = SearchState.LookForAdjacentScript;
                            SetAdjacent(true);
                            currentStartStringsToSkip[1] = false; //script tag
                        }
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

        private void DoTransform(byte[] buffer, ref int startTransformPosition)
        {
            currentStartStringsToSkip = new bool[currentStartStringsToSkip.Length];
            if ((startTransformPosition - actualOffset) >= 0)
                BaseStream.Write(buffer, actualOffset, startTransformPosition - actualOffset);
            try
            {
                var transformed =
                    encoding.GetBytes(responseTransformer.Transform(encoding.GetString(transformBuffer.ToArray())));
                BaseStream.Write(transformed, 0, transformed.Length);
            }
            catch (Exception ex)
            {
                var message = string.Format("There were errors transforming {0}", encoding.GetString(transformBuffer.ToArray()));
                var wrappedException =
                    new ApplicationException(message, ex);
                RRTracer.Trace(message);
                RRTracer.Trace(ex.ToString());
                if (Registry.CaptureErrorAction != null)
                    Registry.CaptureErrorAction(wrappedException);
                BaseStream.Write(transformBuffer.ToArray(), 0, transformBuffer.Count);
            }
            startTransformPosition = 0;
            transformBuffer.Clear();
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

        private int HandleMatchingStartMatch(byte b, ref int i, byte[] buffer, ref int endTransformPosition, ref int startTransformPosition)
        {
            if(IsMatch(MatchingEnd.Start, b))
            {
                transformBuffer.Add(b);
                matchPosition++;
                for (var idx = 0; idx < currentStartStringsToSkip.Length; idx++)
                {
                    if (!currentStartStringsToSkip[idx] && matchPosition == startStringUpper[idx].Length)
                    {
                        matchPosition = 0;
                        state = SearchState.MatchingStartClose;
                    }
                    else if (matchPosition == 0)
                        currentStartStringsToSkip[idx] = true;
                }
                return matchPosition;
            }

            if (isAdjacent)
            {
                SetAdjacent(false);
                var numToTrim = i - endTransformPosition;
                if (numToTrim > 0)
                    transformBuffer.RemoveRange(transformBuffer.Count - numToTrim, numToTrim);
                DoTransform(buffer, ref startTransformPosition);
                i = i - numToTrim - 1;
                actualLength = actualLength - (i - actualOffset) - 1; 
                actualOffset = i + 1;
            }
            else
            {
                if(i-originalOffset < transformBuffer.Count)
                    BaseStream.Write(transformBuffer.ToArray(), 0, (transformBuffer.Count - (i - originalOffset)));
                transformBuffer.Clear();
            }
            state = SearchState.LookForStart;
            currentStartStringsToSkip = new bool[currentStartStringsToSkip.Length];
            return 0;
        }

        private int HandleLookForStartMatch(ref int i, byte b, byte[] buffer, ref int endTransformPosition, ref int startTransformPosition)
        {
            if(IsMatch(MatchingEnd.Start, b))
            {
                state = SearchState.MatchingStart;
                transformBuffer.Clear();
                startTransformPosition = i;
                return HandleMatchingStartMatch(b, ref i, buffer, ref endTransformPosition, ref startTransformPosition);
            }
            return matchPosition;
        }

        private int HandleLookForAdjacentScriptMatch(byte[] buffer, int i, byte b, ref int endTransformPosition, ref int startTransformPosition)
        {
            if (IsMatch(MatchingEnd.Start, b))
            {
                state = SearchState.MatchingStart;
                return HandleMatchingStartMatch(b, ref i, buffer, ref endTransformPosition, ref startTransformPosition);
            }

            if (!whiteSpaceChar.Contains(b))
            {
                SetAdjacent(false);
                state = SearchState.LookForStart;
                var numToTrim = i - endTransformPosition;
                if(numToTrim > 0)
                    transformBuffer.RemoveRange(transformBuffer.Count - numToTrim, numToTrim);
                DoTransform(buffer, ref startTransformPosition);
                actualLength = actualLength - (i - actualOffset); actualOffset = i;
                return 0;
            }

            transformBuffer.Add(b);
            return matchPosition;
        }


        private void SetAdjacent(bool value)
        {
            if (value && !isAdjacent)
            {
                startStringUpper = startStringUpper.Concat(okWhenAdjacentToScriptStartUpper).ToArray();
                startStringLower = startStringLower.Concat(okWhenAdjacentToScriptStartLower).ToArray();
                endStringUpper = endStringUpper.Concat(okWhenAdjacentToScriptEndUpper).ToArray();
                endStringLower = endStringLower.Concat(okWhenAdjacentToScriptEndLower).ToArray();
                currentStartStringsToSkip = currentStartStringsToSkip.Concat(okWhenAdjacentToScriptStartUpper.Select(x => false)).ToArray();
            }
            else if(!value)
            {
                InitSearchArrays();
                currentStartStringsToSkip = new bool[startStringUpper.Length];
            }
            isAdjacent = value;
            if(isAdjacent)
            {
                for (var i = 1; i < currentStartStringsToSkip.Length; i++)
                    currentStartStringsToSkip[i] = false;
            }
        }

        private bool IsMatch(MatchingEnd end, byte b)
        {
            byte[][] upper;
            byte[][] lower;
            if(end == MatchingEnd.Start)
            {
                upper = startStringUpper;
                lower = startStringLower;
            }
            else
            {
                upper = endStringUpper;
                lower = endStringLower;
            }

            for(var i = 0; i < currentStartStringsToSkip.Length; i++)
            {
                if (currentStartStringsToSkip[i]) continue;
                var lowerToMatch = lower[i];
                var upperToMatch = upper[i];
                if ((b == lowerToMatch[matchPosition] || b == upperToMatch[matchPosition])) continue;
                if (end == MatchingEnd.Start && state == SearchState.MatchingStart)
                    currentStartStringsToSkip[i] = true;
                else
                    return false;
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

        public static void InstallFilter(HttpContextBase context)
        {
            var request = context.Request;
            var config = RRContainer.Current.GetInstance<IRRConfiguration>();
            if (context.Items.Contains(ContextKey) ||
                (request.QueryString["RRFilter"] != null && request.QueryString["RRFilter"].Equals("disabled", StringComparison.OrdinalIgnoreCase)) ||
                (config.CssProcessingDisabled && config.JavaScriptProcessingDisabled) ||
                context.Response.StatusCode == 302 ||
                context.Response.StatusCode == 301 ||
                RRContainer.Current.GetAllInstances<IFilter>().Where(x => x is PageFilter).FirstOrDefault(y => y.IgnoreTarget(new PageFilterContext(context.Request))) != null)
                return;

            var hostingEnvironment = RRContainer.Current.GetInstance<IHostingEnvironmentWrapper>();
            if (string.IsNullOrEmpty(config.ResourcePhysicalPath))
                config.ResourcePhysicalPath = hostingEnvironment.MapPath(config.ResourceVirtualPath);

            var oldFilter = context.Response.Filter; //suppresses a asp.net3.5 bugg 
            context.Response.Filter = RRContainer.Current.GetInstance<AbstractFilter>();
            context.Items.Add(ContextKey, new object());
            RRTracer.Trace("Attaching Filter to {0}", request.RawUrl);
        }
    }
}
