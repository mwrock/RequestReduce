using System;
using System.Text.RegularExpressions;

namespace RequestReduce.Reducer
{
    public class CssSelectorAnalyzer
    {
        public bool IsInScopeOfTarget(string targetSelector, string comparableSelector)
        {
            var targetOffset = 0;
            var tokens = comparableSelector.Split(new char[] {}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                targetOffset = FindToken(token, targetSelector, targetOffset);
                if(targetOffset == -1)
                    return false;
            }
            return true;
        }

        private int FindToken(string comparableSelector, string targetSelector, int targetOffset)
        {
            while (targetSelector.Length > targetOffset)
            {
                var idx = targetSelector.IndexOf(comparableSelector, targetOffset, StringComparison.OrdinalIgnoreCase);
                if (idx == -1) return idx;
                var endIdx = idx + comparableSelector.Length;
                if (targetSelector.Length <= endIdx || targetSelector[endIdx] == '.' || targetSelector[endIdx] == '#' ||
                    targetSelector.IndexOfAny(new[] {' ', '\n', '\r', '\t'}, endIdx, 1) == endIdx)
                    return idx;
                targetOffset = endIdx;
            }
            return -1;
        }
    }
}
