using System;
using System.Linq;
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
            var tokens = comparableSelector.Split(new[]{'.'}, StringSplitOptions.RemoveEmptyEntries);
            while (targetSelector.Length > targetOffset)
            {
                var idx = targetSelector.IndexOf(tokens[0], targetOffset, StringComparison.OrdinalIgnoreCase);
                if (idx == -1) return idx;
                var endIdx = idx + tokens[0].Length;
                if ((idx == 0 || targetSelector[idx-1] == '.' ||
                    targetSelector.IndexOfAny(new[] { ' ', '\n', '\r', '\t' }, idx - 1, 1) == idx - 1) &&
                    (targetSelector.Length <= endIdx || targetSelector[endIdx] == '.' || targetSelector[endIdx] == '#' ||
                    targetSelector.IndexOfAny(new[] {' ', '\n', '\r', '\t'}, endIdx, 1) == endIdx))
                {
                    var startTargetIdx = targetSelector.LastIndexOfAny(new[] {' ', '\n', '\r', '\t'}, idx) + 1;
                    if(targetSelector[startTargetIdx] ==  comparableSelector[0])
                    {
                        var endTargetdx = targetSelector.IndexOfAny(new[] { ' ', '\n', '\r', '\t' }, idx);
                        endTargetdx = endTargetdx == -1 ? targetSelector.Length - 1 : endTargetdx - 1;
                        var targetTokens = targetSelector.Substring(startTargetIdx, endTargetdx - startTargetIdx + 1).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.All(x => targetTokens.Contains(x)))
                            return idx;
                    }
                }
                targetOffset = endIdx;
            }
            return -1;
        }
    }
}
