using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestReduce.Utilities;

namespace RequestReduce.Reducer
{
    public class SelectorComparer : IComparer<string>
    {
        private static readonly RegexCache Regex = new RegexCache();
        public int Compare(string x, string y)
        {
            if (x == y)
                return 0;

            var result =  Score(y).CompareTo(Score(x));
            if (result == 0)
                return Int32.Parse(y.Split(new[] {'|'})[1]).CompareTo(Int32.Parse(x.Split(new[] {'|'})[1]));
            return result;
        }

        private int Score(string selector)
        {
            var score = 0;
            var lastChar = ' ';
            foreach (char c in selector)
            {
                switch(c)
                {
                    case '#':
                        score += 100;
                        break;
                    case '.':
                    case '[':
                    case ':':
                        score += 10;
                        break;
                    case '*':
                    case '+':
                    case '>':
                        break;
                    default:
                        if (lastChar == ' ')
                            score += 1;
                        break;
                }
                lastChar = c;
            }
            var matches = Regex.PseudoElementPattern.Matches(selector);
            score -= (matches.Count*9);
            return score;
        }
    }
}
