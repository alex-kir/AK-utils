// -f:ak.Email.cs

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;


namespace ak
{
	public static class String
	{
        public static string Quoted(this string self)
        {
            return self.Quoted("\"", "\"");
        }

        public static string Quoted(this string self, string startQuote, string endQuote)
	    {
            if (self.Length >= startQuote.Length + endQuote.Length && self.StartsWith(startQuote) && self.EndsWith(endQuote))
                return self;
            return startQuote + self + endQuote;
        }

        public static string EscapeSpaces(this string self)
        {
            if (string.IsNullOrEmpty(self))
                return self;
            return string.Join("\\ ", self.Split(' '));
        }

        public static string ToSrting(this string self)
        {
            return new string(self.Select(it => (char)(it - 1)).ToArray());
        }

        public static string F(this string self, params object[] args)
        {
            return string.Format(self, args);
        }

        public static string SubstringBetween(this string self, string start, string end)
        {
            return self.SubstringBetween(start, end, null);
        }

        public static string SubstringBetween(this string self, string start, string end, string defolt)
        {
            int n = self.IndexOf(start);
            if (n != -1)
            {
                int m = self.IndexOf(end, n + start.Length);
                if (m != -1)
                {
                    return self.Substring(n + start.Length, m - n - start.Length);
                }
            }
            return defolt;
        }

        public static string SubstringAfter(this string self, string start, string defolt)
        {
            int n = self.IndexOf(start);
            if (n != -1)
            {
                return self.Substring(n + start.Length);
            }
            return defolt;
        }

        public static string SubstringBefore(this string self, string end, string defolt)
        {
            int m = self.IndexOf(end);
            if (m != -1)
            {
                return self.Substring(0, m);
            }
            return defolt;
        }

        public static string JoinStrings<T>(this IEnumerable<T> self)
        {
            return string.Join("", self.Select(it => it + "").ToArray());
        }

        public static string JoinStrings<T>(this IEnumerable<T> self, string separator)
        {
            return string.Join(separator, self.Select(it => it + "").ToArray());
        }

    }
}